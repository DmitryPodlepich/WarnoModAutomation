using Microsoft.Maui.Storage;
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

            TEntityDescriptor instance = null;
            object childInstanse = null;

            bool isReadingEntiryDescriptorClass = false;
            bool isReadingModulesDescriptorsCollection = false;
            bool isReadingModuleDescriptor = false;

            //try
            //{
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

                    if (!isReadingEntiryDescriptorClass && line.Trim().Equals("("))
                    {
                        instance = new TEntityDescriptor();
                        isReadingEntiryDescriptorClass = true;
                        continue;
                    }

                    if (isReadingEntiryDescriptorClass && !isReadingModulesDescriptorsCollection && line.Trim().Equals(")"))
                    {
                        fileDescriptior.EntityDescriptors.Add(instance);

                        instance = null;
                        isReadingEntiryDescriptorClass = false;
                        continue;
                    }

                    if (isReadingModulesDescriptorsCollection && line.Trim().Equals("("))
                    {
                        var previousLine = fileDescriptior.RawLines.ElementAt(i - 2);

                        var moduleDescriptorTypeName = previousLine.Value.Trim();

                        if (FileDescriptor.ModulesDescriptorTypesMap.ContainsKey(moduleDescriptorTypeName))
                        {
                            var type = FileDescriptor.ModulesDescriptorTypesMap[moduleDescriptorTypeName];
                            childInstanse = Activator.CreateInstance(type);
                        }

                        isReadingModuleDescriptor = true;

                        continue;
                    }

                    if (isReadingModulesDescriptorsCollection && !isReadingModuleDescriptor && line.Trim().Equals("]"))
                    {
                        isReadingModulesDescriptorsCollection = false;
                        continue;
                    }

                    if (isReadingModulesDescriptorsCollection && isReadingModuleDescriptor && line.Trim().Equals("),"))
                    {
                        if(childInstanse is not null)
                            instance.ModulesDescriptors.Add(childInstanse);

                        childInstanse = null;

                        isReadingModuleDescriptor = false;
                        continue;
                    }

                    if (isReadingModuleDescriptor)
                    {
                        if (lineHasKeyAndValue)
                        {
                            var applicableProperty = childInstanse?.GetType().GetProperties().SingleOrDefault(p => p.Name == key);

                            if (applicableProperty is null)
                                continue;

                            if (value.Equals(string.Empty))
                                continue;

                            fileDescriptior.Map.Add(rawLineKey, new PropertyToObject() { ParentObject = childInstanse, PropertyInfo = applicableProperty });

                            var parsedValue = Convert.ChangeType(value, applicableProperty.PropertyType, CultureInfo.InvariantCulture);

                            applicableProperty.SetValue(childInstanse, parsedValue);
                        }

                        continue;
                    }

                    if (isReadingEntiryDescriptorClass)
                    {
                        var applicableProperty = instance.PropertiesInfo.SingleOrDefault(p => p.Name == key);

                        if (applicableProperty is not null)
                        {
                            if (value.Equals(string.Empty))
                                continue;

                            //For Collections
                            if (value.Equals("["))
                            {
                                isReadingModulesDescriptorsCollection = true;
                                continue;
                            }

                            if (isReadingModulesDescriptorsCollection)
                                continue;

                            fileDescriptior.Map.Add(rawLineKey, new PropertyToObject() { ParentObject = instance, PropertyInfo = applicableProperty });

                            var parsedValue = Convert.ChangeType(value, applicableProperty.PropertyType);
                                    
                            applicableProperty.SetValue(instance, parsedValue);

                            continue;
                        }
                    }
                }
            }

            foreach (var item in fileDescriptior.EntityDescriptors)
            {
                item.ClassNameForDebug = item.ClassNameForDebug + "1";
            }

            var stringToSave = fileDescriptior.Serialize();

            File.WriteAllText(_buildingDescriptorsPath, stringToSave);
            //}
            //catch (Exception ex) 
            //{
            //    OnOutput?.Invoke(ex.Message);
            //}
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
