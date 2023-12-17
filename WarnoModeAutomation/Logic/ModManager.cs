using Microsoft.Maui.Storage;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using WarnoModeAutomation.DTO;

namespace WarnoModeAutomation.Logic
{
    public static class ModManager
    {
        private const string _createNewModBatFileName = "CreateNewMod.bat";
        private const string _generateModBatFileName = "GenerateMod.bat";

        private static readonly string _buildingDescriptorsPath
            = Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModeName, 
                "GameData", "Generated", "Gameplay", "Gfx", "BuildingDescriptors.ndf");

        public delegate void Outputter(string data);
        public static event Outputter OnOutput;

        public static async Task<bool> CreateModAsync()
        {
            var batFullPath = Path.Combine(Storage.ModeSettings.ModsDirectory, _createNewModBatFileName);

            if (!File.Exists(batFullPath))
            {
                OnOutput?.Invoke($"{batFullPath} file does not exist!");
                return false;
            }

            using var cmdProvier = new CMDProvider(Storage.ModeSettings.ModsDirectory);

            cmdProvier.OnOutput += OnCMDProviderOutput;

            return await cmdProvier.PerformCMDCommand($"{_createNewModBatFileName} {Storage.ModeSettings.ModeName}");
        }

        public static bool DeleteMod() 
        {
            var modDirectory = Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModeName);

            if (!Directory.Exists(modDirectory))
            {
                OnOutput?.Invoke($"{modDirectory} not found!");
                return false;
            }

            if (!FileManager.DeleteDirectoryWithFiles(modDirectory, out string error))
            {
                OnOutput?.Invoke(error);
                return false;
            }

            OnOutput?.Invoke($"{modDirectory} has been deleted.");

            return true;
        }

        public static async Task<bool> GenerateModAsync() 
        {
            var batFullPath = Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModeName, _generateModBatFileName);

            var modDirectory = Path.Combine(Storage.ModeSettings.ModsDirectory, Storage.ModeSettings.ModeName);

            if (!File.Exists(batFullPath))
            {
                OnOutput?.Invoke($"{batFullPath} file does not exist!");
                return false;
            }

            using var cmdProvier = new CMDProvider(modDirectory);

            cmdProvier.OnOutput += OnCMDProviderOutput;

            return await cmdProvier.PerformCMDCommand(_generateModBatFileName);
        }

        public static void Run()
        {
            if (!File.Exists(_buildingDescriptorsPath))
            {
                OnOutput?.Invoke($"{_buildingDescriptorsPath} not found!.");
                return;
            }

            var fileDescriptior = new FileDescriptor();
            var descriptorsStack = new Stack<Descriptor>();

            const int bufferSize = 128;
            using (var fileStream = File.OpenRead(_buildingDescriptorsPath))
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

                        if(newDescriptor is not null)
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

            foreach (var item in fileDescriptior.EntityDescriptors)
            {
                //item.ClassNameForDebug = item.ClassNameForDebug + "1";

                var tSupplyModuleDescriptors = item.ModulesDescriptors
                    .Where(x => x.Value.Type.Equals(typeof(TSupplyModuleDescriptor)))
                    .Select(x => x.Value.DescriptorObject as TSupplyModuleDescriptor);

                foreach (var tSupplyModuleDescriptor in tSupplyModuleDescriptors)
                {
                    tSupplyModuleDescriptor.SupplyCapacity = 100000;
                }
            }

            var stringToSave = fileDescriptior.Serialize();

            File.WriteAllText(_buildingDescriptorsPath, stringToSave);
        }

        private static Descriptor CreateDescriptor(FileDescriptor fileDescriptor, int currentIndex, Descriptor currentDescriptor) 
        {
            var previousLine = fileDescriptor.RawLines.ElementAt(currentIndex - 2);

            var splittedName = previousLine.Value.Split(' ');

            var name = splittedName.Last().TrimEnd();

            var definedType = FileDescriptor.TypesMap.ContainsKey(name) ? FileDescriptor.TypesMap[name] : typeof(Descriptor);

            var descriptor = FileDescriptor.TypesMap.ContainsKey(name) 
                ? Activator.CreateInstance(definedType) as Descriptor
                : Activator.CreateInstance(definedType) as Descriptor;

            if (descriptor is TEntityDescriptor)
            {
                fileDescriptor.EntityDescriptors.Add(descriptor as TEntityDescriptor);
            }

            if (currentDescriptor is not null && currentDescriptor.LastSettedPropery is not null && currentDescriptor.LastSettedPropery.GetValue(currentDescriptor) is IDictionary)
            {
                var collectionPropertyValue = currentDescriptor.LastSettedPropery.GetValue(currentDescriptor) as IDictionary;

                var typeToObject = new TypeToObject(name, definedType, descriptor);

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

            if(applicableProperty.PropertyType == typeof(string))
                parsedValue = (parsedValue as string).Replace("\'", "").Replace("\"", "");

            applicableProperty.SetValue(descriptor, parsedValue);

            descriptor.LastSettedPropery = applicableProperty;
        }

        public static void Save()
        {
            
        }

        private static void OnCMDProviderOutput(string data)
        {
            OnOutput?.Invoke(data);
        }
    }
}
