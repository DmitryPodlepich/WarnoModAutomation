using NDFSerialization;
using NDFSerialization.Enums;
using NDFSerialization.Models;
using NDFSerialization.NDFDataTypes;
using NDFSerialization.NDFDataTypes.Interfaces;
using NDFSerialization.NDFDataTypes.Primitive;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace WarnoModeAutomation.Logic
{
    public static class NDFSerializer
    {
        private const string IS_KEYWORD = "is";

        public static async Task Initialize() 
        {
            await Task.Run(() => FileDescriptor<Descriptor>.Initialize());
        }

        public static string Serialize<T>(FileDescriptor<T> fileDescriptor, Action<string> outputLogs = null)
            where T : Descriptor
        {
            var sb = new StringBuilder(fileDescriptor.RawLines.Count);

            foreach (var rawLine in fileDescriptor.RawLines)
            {
                try
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
                        //Logic for vectors
                        if (mapInstantce.PropertyInfo.PropertyType.GetInterface(nameof(INDFVector)) is not null)
                        {
                            var vectorCollection = mapInstantce.PropertyInfo.GetValue(mapInstantce.Object) as INDFVector;

                            var vectorItem = vectorCollection.Get(mapInstantce.Index.Value);

                            modifiedLine = rawLine.Value.Replace(rawLine.Value.Trim(), vectorItem.ItemToString(rawLine.Value));
                        }
                        else
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
                        }

                        sb.AppendLine(modifiedLine);
                        continue;
                    }

                    var splitted = rawLine.Value.Split('=');

                    var rawValue = splitted.Last().TrimEnd();

                    var modifiedValue = rawValue.Replace(rawValue, propertyValue.ItemToString(rawValue));

                    modifiedLine = rawLine.Value.Replace(rawValue.Trim(), modifiedValue);

                    sb.AppendLine(modifiedLine);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    outputLogs?.Invoke($"Serialization error: {ex.Message}");
                    outputLogs?.Invoke(ex.StackTrace);
                }
            }

            return sb.ToString();
        }

        public static FileDescriptor<T> Deserialize<T>(string filePath, CancellationToken cancellationToken, Action<string> outputLogs = null) where T : Descriptor
        {
            if (!File.Exists(filePath))
                outputLogs?.Invoke($"File not found by path: {filePath}");

            var fileDescriptior = new FileDescriptor<T>(filePath);
            var descriptorsStack = new Stack<Descriptor>();

            const int bufferSize = 512;
            using (var fileStream = File.OpenRead(filePath))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, bufferSize))
            {
                string line;
                var i = 0;
                while ((line = streamReader.ReadLine()) != null)
                {
                    //if (line.Contains("SupplyCost"))
                        //Debugger.Break();

                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

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

                            descriptorsStack.Push(newDescriptor);

                            continue;
                        }

                        if (line.Trim().Equals("]")
                            && (currentDescriptor?.LastSettedProperyNDFType == NDFPropertyTypes.Vector || currentDescriptor?.LastSettedProperyNDFType == NDFPropertyTypes.VectorGeneric))
                        {
                            currentDescriptor.LastSettedPropery = null;
                            currentDescriptor.LastSettedProperyNDFType = NDFPropertyTypes.Primitive;
                            continue;
                        }

                        var lineHasKeyAndValue = line.Contains('=');

                        if (lineHasKeyAndValue && currentDescriptor is not null)
                        {
                            var key = line.Split('=')[0].Trim();
                            var value = line.Split('=')[1].Trim();

                            SetDescriptorProperty(fileDescriptior, currentDescriptor, rawLineKey, key, value);
                            continue;
                        }

                        if (currentDescriptor?.LastSettedProperyNDFType == NDFPropertyTypes.MAP)
                        {
                            if (NDFRegexes.TryExtractMapItem(line, out var item))
                            {
                                SetMapItem(fileDescriptior, currentDescriptor, rawLineKey, item);
                                continue;
                            }
                        }

                        if (currentDescriptor?.LastSettedProperyNDFType == NDFPropertyTypes.Vector || currentDescriptor?.LastSettedProperyNDFType == NDFPropertyTypes.VectorGeneric)
                        {
                            SetVectorItem(fileDescriptior, currentDescriptor, rawLineKey, line);
                            continue;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        outputLogs?.Invoke($"Deserialization error: {ex.Message}");
                        outputLogs?.Invoke(ex.StackTrace);
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

            if (!FileDescriptor<T>.TypesMap.TryGetValue(typeName, out Type definedType))
                definedType = typeof(UnknownDescriptor);

            var descriptor = Activator.CreateInstance(definedType) as Descriptor;

            descriptor.EntityNDFType = entityNDFType;

            //Nested objects logic
            if (currentDescriptor is not null && currentDescriptor.LastSettedPropery is not null && currentDescriptor.LastSettedPropery.GetValue(currentDescriptor) is INDFVector)
            {
                if (currentDescriptor.LastSettedProperyNDFType == NDFPropertyTypes.Vector)
                {
                    if (definedType != typeof(UnknownDescriptor))
                    {
                        var collectionPropertyValue = currentDescriptor.LastSettedPropery.GetValue(currentDescriptor) as INDFVector;
                        collectionPropertyValue.Add(descriptor);
                    }

                    return descriptor;
                }
            }

            if (currentDescriptor is not null && currentDescriptor.LastSettedPropery is not null && currentDescriptor.LastSettedProperyNDFType == NDFPropertyTypes.Descriptor)
            {
                if (definedType != typeof(UnknownDescriptor))
                {
                    return currentDescriptor.LastSettedPropery.GetValue(currentDescriptor) as Descriptor;
                }
            }

            if(descriptor is T entityDescriptor)
                fileDescriptor.RootDescriptors.Add(entityDescriptor);

            return descriptor;
        }

        private static void SetDescriptorProperty<T>(FileDescriptor<T> fileDescriptor, Descriptor descriptor, Guid rawLineKey, string key, string value)
            where T : Descriptor
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            var applicableProperty = descriptor.PropertiesInfo.SingleOrDefault(p => p.Name == key);

            if (applicableProperty is null)
                return;

            if (applicableProperty.PropertyType.GetInterface(nameof(IEnumerable)) is not null && applicableProperty.PropertyType != typeof(string))
            {
                var lastSettedProperyNDFType = DefineNDFPropertyType(applicableProperty);

                applicableProperty.SetValue(descriptor, Activator.CreateInstance(applicableProperty.PropertyType));

                descriptor.LastSettedPropery = applicableProperty;

                descriptor.LastSettedProperyNDFType = lastSettedProperyNDFType;

                return;
            }

            if (applicableProperty.PropertyType.BaseType == typeof(Descriptor))
            {
                applicableProperty.SetValue(descriptor, Activator.CreateInstance(applicableProperty.PropertyType));

                descriptor.LastSettedPropery = applicableProperty;

                descriptor.LastSettedProperyNDFType = NDFPropertyTypes.Descriptor;

                return;
            }

            fileDescriptor.RawLineToObjectPropertyMap.Add(rawLineKey, new PropertyToObject() { Object = descriptor, PropertyInfo = applicableProperty });

            if (string.IsNullOrEmpty(value))
                return;

            if (applicableProperty.PropertyType == typeof(DistanceMetre))
            {
                var distanceMetre = new DistanceMetre(value);

                applicableProperty.SetValue(descriptor, distanceMetre);
            }
            else
            {
                var parsedValue = Convert.ChangeType(value, applicableProperty.PropertyType, CultureInfo.InvariantCulture);

                if (applicableProperty.PropertyType == typeof(string))
                    parsedValue = (parsedValue as string).Replace("\'", "").Replace("\"", "");

                applicableProperty.SetValue(descriptor, parsedValue);
            }

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

            var key = Convert.ChangeType(mapItem.Key, keyType, CultureInfo.InvariantCulture);

            object value = valueType == typeof(DistanceMetre) 
                ? new DistanceMetre(mapItem.Value)
                : Convert.ChangeType(mapItem.Value, valueType, CultureInfo.InvariantCulture);

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

        private static void SetVectorItem<T>(FileDescriptor<T> fileDescriptor, Descriptor descriptor, Guid rawLineKey, string line) 
            where T : Descriptor
        {
            try
            {
                if (string.IsNullOrWhiteSpace(line))
                    return;

                //Skip all objects
                if (line.TrimStart()[0] == 'T' || line.TrimStart()[0] == '~' || line.Contains(" is"))
                    return;

                if (descriptor.LastSettedProperyNDFType != NDFPropertyTypes.Vector && descriptor.LastSettedProperyNDFType != NDFPropertyTypes.VectorGeneric)
                    return;

                var lastSettedPropertyValue = descriptor.LastSettedPropery.GetValue(descriptor);

                var vectorCollection = lastSettedPropertyValue as INDFVector;

                vectorCollection.Add(line);

                fileDescriptor.RawLineToObjectPropertyMap.Add(rawLineKey, new PropertyToObject() { Object = descriptor, PropertyInfo = descriptor.LastSettedPropery, Index = vectorCollection.CurrentIndex - 1 });
                
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static NDFPropertyTypes DefineNDFPropertyType(PropertyInfo propertyInfo) 
        {
            var type = NDFPropertyTypes.Primitive;

            if (propertyInfo.PropertyType == typeof(NDFVector))
                return NDFPropertyTypes.Vector;

            if (propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(NDFVectorGeneric<>))
                return NDFPropertyTypes.VectorGeneric;


            var mapAttribute = propertyInfo.GetCustomAttribute<NDFMAPAttribute>();

            if (mapAttribute is not null)
                return NDFPropertyTypes.MAP;

            return type;
        }

        private static object EnshureItemQuotes(object original, object current)
        {
            if (original.GetType() == typeof(string) && current.GetType() == typeof(string))
            {
                if (original.ToString().Contains('\''))
                    return "'" + current + "'";

                if (original.ToString().Contains("\""))
                    return "\"" + current + "\"";
            }

            return current;
        }

        private static string ItemToString(this object item, object original)
        {
            if(item.GetType() == typeof(float))
                return ((float)item).ToString(CultureInfo.InvariantCulture);

            else if (item.GetType() == typeof(double))
                return ((double)item).ToString(CultureInfo.InvariantCulture);

            else if (item.GetType() == typeof(decimal))
                return ((decimal)item).ToString(CultureInfo.InvariantCulture);

            else if (original.GetType() == typeof(string) && item.GetType() == typeof(string))
            {
                var comma = original.ToString().TrimEnd().Last() == ',' ? "," : string.Empty;

                if (original.ToString().Contains('\''))
                    return "'" + item + "'" + comma;

                if (original.ToString().Contains('"'))
                    return "\"" + item + "\"" + comma;
            }

            return item.ToString();
        }
    }
}
