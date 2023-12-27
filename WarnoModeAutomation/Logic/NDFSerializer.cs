using BlazorBootstrap;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WarnoModeAutomation.DTO;
using static System.Net.Mime.MediaTypeNames;

namespace WarnoModeAutomation.Logic
{
    public static class NDFSerializer
    {
        #region NDFKeywords

        private const string IS_KEYWORD = "is";

        #endregion

        public static string Serialize<T>(FileDescriptor<T> fileDescriptor)
            where T : Descriptor
        {
            var sb = new StringBuilder(fileDescriptor.RawLines.Count);

            foreach (var rawLine in fileDescriptor.RawLines)
            {
                if (!fileDescriptor.RawLineToObjectPropertyMap.ContainsKey(rawLine.Key))
                {
                    sb.AppendLine(rawLine.Value);
                    continue;
                }

                var modifiedLine = string.Empty;

                var mapInstantce = fileDescriptor.RawLineToObjectPropertyMap[rawLine.Key];

                var propertyValue = mapInstantce.PropertyInfo.GetValue(mapInstantce.Object);

                //Logic for collections
                if (mapInstantce.Index is not null)
                {
                    //Logic for MAPs
                    var dictionary = mapInstantce.PropertyInfo.GetValue(mapInstantce.Object) as IDictionary;

                    var index = 0;
                    foreach (var value in dictionary.Values)
                    {
                        if (index == mapInstantce.Index)
                        {
                            modifiedLine = NDFRegexes.ReplaceMapItemValue(rawLine.Value, value);
                            break;
                        }
                        index++;
                    }

                    sb.AppendLine(modifiedLine);
                    continue;
                }

                var splitted = rawLine.Value.Split('=');

                var rawValue = splitted.Last().TrimEnd();

                if (mapInstantce.PropertyInfo.PropertyType == typeof(string))
                    propertyValue = "'" + propertyValue + "'";

                var modifiedValue = rawValue.Replace(rawValue, propertyValue.ToString());

                modifiedLine = rawLine.Value.Replace(rawValue.Trim(), modifiedValue);

                sb.AppendLine(modifiedLine);
            }

            return sb.ToString();
        }

        public static FileDescriptor<T> Deserialize<T>(string filePath) where T : Descriptor
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            var fileDescriptior = new FileDescriptor<T>(filePath);
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

                    var lineHasKeyAndValue = line.Contains("=");

                    if (lineHasKeyAndValue && currentDescriptor is not null)
                    {
                        var key = line.Split('=')[0].Trim();
                        var value = line.Split("=")[1].Trim();

                        SetDescriptorProperty(fileDescriptior, currentDescriptor, rawLineKey, key, value);
                    }

                    if (currentDescriptor?.LastSettedProperyNDFType == NDFPropertyTypes.MAP)
                    {
                        if (NDFRegexes.TryExtractMapItem(line, out var item))
                        {
                            SetMapItem(fileDescriptior, currentDescriptor, rawLineKey, item);
                        }
                    }
                }
            }

            return fileDescriptior;
        }

        private static Descriptor CreateDescriptor<T>(FileDescriptor<T> fileDescriptor, int currentIndex, Descriptor currentDescriptor)
            where T : Descriptor
        {
            var previousLine = fileDescriptor.RawLines.ElementAt(currentIndex - 2);

            var splittedName = previousLine.Value.Split(' ');

            var typeName = splittedName.Last().TrimEnd();

            var entityNDFType = Array.Exists(splittedName, x => x.Equals(IS_KEYWORD))
                ? splittedName[Array.IndexOf(splittedName, IS_KEYWORD) - 1] : typeName;

            var definedType = FileDescriptor<T>.TypesMap.ContainsKey(typeName) ? FileDescriptor<T>.TypesMap[typeName] : typeof(Descriptor);

            var descriptor = FileDescriptor<T>.TypesMap.ContainsKey(typeName)
                ? Activator.CreateInstance(definedType) as Descriptor
                : Activator.CreateInstance(definedType) as Descriptor;

            descriptor.EntityNDFType = entityNDFType;

            if (descriptor is T)
            {
                fileDescriptor.EntityDescriptors.Add(descriptor as T);
            }

            if (currentDescriptor is not null && currentDescriptor.LastSettedPropery is not null && currentDescriptor.LastSettedPropery.GetValue(currentDescriptor) is ICollection)
            {
                if (currentDescriptor.LastSettedProperyNDFType == NDFPropertyTypes.BlockOfCode)
                {
                    var collectionPropertyValue = currentDescriptor.LastSettedPropery.GetValue(currentDescriptor) as IDictionary;

                    var typeToObject = new TypeToObject(typeName, definedType, descriptor);

                    collectionPropertyValue.Add(Guid.NewGuid(), typeToObject);
                }
            }

            return descriptor;
        }

        private static void SetDescriptorProperty<T>(FileDescriptor<T> fileDescriptor, Descriptor descriptor, Guid rawLineKey, string key, string value)
            where T : Descriptor
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

                var lastSettedPropertyType = NDFPropertyTypes.BlockOfCode;

                if (value.Contains("MAP"))
                    lastSettedPropertyType = NDFPropertyTypes.MAP;

                descriptor.LastSettedProperyNDFType = lastSettedPropertyType;

                return;
            }

            fileDescriptor.RawLineToObjectPropertyMap.Add(rawLineKey, new PropertyToObject() { Object = descriptor, PropertyInfo = applicableProperty });

            var parsedValue = Convert.ChangeType(value, applicableProperty.PropertyType, CultureInfo.InvariantCulture);

            if (applicableProperty.PropertyType == typeof(string))
                parsedValue = (parsedValue as string).Replace("\'", "").Replace("\"", "");

            applicableProperty.SetValue(descriptor, parsedValue);

            descriptor.LastSettedPropery = applicableProperty;

            descriptor.LastSettedProperyNDFType = NDFPropertyTypes.Primitive;
        }

        private static void SetMapItem<T>(FileDescriptor<T> fileDescriptor, Descriptor descriptor, Guid rawLineKey, KeyValuePair<string, string> mapItem)
            where T : Descriptor
        {
            if (descriptor?.LastSettedPropery is null)
                return;

            if (descriptor.LastSettedProperyNDFType != NDFPropertyTypes.MAP)
                return;

            if (descriptor.LastSettedPropery.GetValue(descriptor) is not IDictionary)
                return;

            var keyType = descriptor.LastSettedPropery.PropertyType.GenericTypeArguments[0];

            var valueType = descriptor.LastSettedPropery.PropertyType.GenericTypeArguments[1];

            var collectionPropertyValue = descriptor.LastSettedPropery.GetValue(descriptor) as IDictionary;

            var key = Convert.ChangeType(mapItem.Key, keyType);

            var value = Convert.ChangeType(mapItem.Value, valueType);

            if (!collectionPropertyValue.Contains(key))
            {
                collectionPropertyValue.Add(key, value);

                var index = 0;
                foreach (var entry in collectionPropertyValue.Keys)
                {
                    if (entry == key)
                        break;
                    index++;
                }

                fileDescriptor.RawLineToObjectPropertyMap.Add(rawLineKey, new PropertyToObject() { Object = descriptor, PropertyInfo = descriptor.LastSettedPropery, Index = index});
            }
        }
    }

    public static partial class NDFRegexes
    {
        public static bool IsMapItem(string value)
        {
            return MapItemRegex().IsMatch(value);
        }

        public static bool TryExtractMapItem(string value, out KeyValuePair<string, string> item) 
        {
            item = new KeyValuePair<string, string>();

            if (!IsMapItem(value))
                return false;

            var matchCollection = MapItemRegex().Matches(value).FirstOrDefault();

            item = new KeyValuePair<string, string>(matchCollection.Groups["key"].Value, matchCollection.Groups["value"].Value);

            return true;
        }

        public static string ReplaceMapItemValue(string rawLine, object value)
        {
            if (!IsMapItem(rawLine))
                throw new InvalidDataException($"Raw line: {rawLine} does not match MapItemRegex pattern!");

            var matchCollection = MapItemRegex().Matches(rawLine).FirstOrDefault();

            var t = MapItemRegex().Replace(rawLine, m => $"({m.Groups["key"].Value}, {value})");

            return t;
        }

        [GeneratedRegex(@"\((?<key>.{1,}),(?<value>.{1,})\)", RegexOptions.Compiled)]
        private static partial Regex MapItemRegex();
    }
}
