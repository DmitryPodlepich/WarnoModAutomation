using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarnoModeAutomation.DTO;

namespace WarnoModeAutomation.Logic
{
    public static class NDFSerializer
    {
        #region NDFKeywords

        private const string IS_KEYWORD = "is";

        #endregion

        public static string Serialize(FileDescriptor fileDescriptor)
        {
            var sb = new StringBuilder(fileDescriptor.RawLines.Count);

            foreach (var rawLine in fileDescriptor.RawLines)
            {
                if (!fileDescriptor.RawLineToObjectPropertyMap.ContainsKey(rawLine.Key))
                {
                    sb.AppendLine(rawLine.Value);
                    continue;
                }

                var mapInstantce = fileDescriptor.RawLineToObjectPropertyMap[rawLine.Key];

                var splitted = rawLine.Value.Split('=');

                var rawValue = splitted.Last().TrimEnd();

                var propertyValue = mapInstantce.PropertyInfo.GetValue(mapInstantce.Object);

                if (mapInstantce.PropertyInfo.PropertyType == typeof(string))
                    propertyValue = "'" + propertyValue + "'";

                var modifiedValue = rawValue.Replace(rawValue, propertyValue.ToString());

                var modifiedLine = rawLine.Value.Replace(rawValue.Trim(), modifiedValue);

                sb.AppendLine(modifiedLine);
            }

            return sb.ToString();
        }

        public static FileDescriptor Deserialize(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            var fileDescriptior = new FileDescriptor(filePath);
            var descriptorsStack = new Stack<Descriptor>();

            const int bufferSize = 128;
            using (var fileStream = File.OpenRead(filePath))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, bufferSize))
            {
                string line;
                var i = 0;
                while ((line = streamReader.ReadLine()) != null)
                {
                    var rawLineKey = Guid.NewGuid();

                    fileDescriptior.RawLines.Add(rawLineKey, line);
                    i++;

                    var key = string.Empty;
                    var value = string.Empty;
                    var lineHasKeyAndValue = line.Contains("=");

                    if (lineHasKeyAndValue)
                    {
                        key = line.Split('=')[0].Trim();
                        value = line.Split("=")[1].Trim();
                    }

                    _ = descriptorsStack.TryPeek(out var currentDescriptor);

                    if (line.Trim().Equals(")") || line.Trim().Equals("),"))
                    {
                        descriptorsStack.TryPop(out var _);
                        continue;
                    }

                    if (line.Trim().Equals("("))
                    {
                        var newDescriptor = CreateDescriptor(fileDescriptior, i, currentDescriptor);

                        if (newDescriptor is not null)
                            descriptorsStack.Push(newDescriptor);

                        continue;
                    }

                    if (!lineHasKeyAndValue)
                        continue;

                    if (currentDescriptor is not null)
                    {
                        SetDescriptorProperty(fileDescriptior, currentDescriptor, rawLineKey, key, value);
                    }
                }
            }

            return fileDescriptior;
        }

        private static Descriptor CreateDescriptor(FileDescriptor fileDescriptor, int currentIndex, Descriptor currentDescriptor)
        {
            var previousLine = fileDescriptor.RawLines.ElementAt(currentIndex - 2);

            var splittedName = previousLine.Value.Split(' ');

            var typeName = splittedName.Last().TrimEnd();

            var entityName = Array.Exists(splittedName, x => x.Equals(IS_KEYWORD))
                ? splittedName[Array.IndexOf(splittedName, IS_KEYWORD) - 1] : typeName;

            var definedType = FileDescriptor.TypesMap.ContainsKey(typeName) ? FileDescriptor.TypesMap[typeName] : typeof(Descriptor);

            var descriptor = FileDescriptor.TypesMap.ContainsKey(typeName)
                ? Activator.CreateInstance(definedType) as Descriptor
                : Activator.CreateInstance(definedType) as Descriptor;

            descriptor.EntityName = entityName;

            if (descriptor is TEntityDescriptor)
            {
                fileDescriptor.EntityDescriptors.Add(descriptor as TEntityDescriptor);
            }

            if (currentDescriptor is not null && currentDescriptor.LastSettedPropery is not null && currentDescriptor.LastSettedPropery.GetValue(currentDescriptor) is IDictionary)
            {
                var collectionPropertyValue = currentDescriptor.LastSettedPropery.GetValue(currentDescriptor) as IDictionary;

                var typeToObject = new TypeToObject(typeName, definedType, descriptor);

                collectionPropertyValue.Add(Guid.NewGuid(), typeToObject);
            }

            return descriptor;
        }

        private static void SetDescriptorProperty(FileDescriptor fileDescriptor, Descriptor descriptor, Guid rawLineKey, string key, string value)
        {
            var applicableProperty = descriptor.PropertiesInfo.SingleOrDefault(p => p.Name == key);

            if (applicableProperty is null)
                return;

            if (value.Equals(string.Empty))
                return;

            if (applicableProperty.GetValue(descriptor) is ICollection)
            {
                applicableProperty.SetValue(descriptor, Activator.CreateInstance(applicableProperty.PropertyType));

                descriptor.LastSettedPropery = applicableProperty;

                return;
            }

            fileDescriptor.RawLineToObjectPropertyMap.Add(rawLineKey, new PropertyToObject() { Object = descriptor, PropertyInfo = applicableProperty });

            var parsedValue = Convert.ChangeType(value, applicableProperty.PropertyType, CultureInfo.InvariantCulture);

            if (applicableProperty.PropertyType == typeof(string))
                parsedValue = (parsedValue as string).Replace("\'", "").Replace("\"", "");

            applicableProperty.SetValue(descriptor, parsedValue);

            descriptor.LastSettedPropery = applicableProperty;
        }
    }
}
