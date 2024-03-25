using JsonDatabase.DTO;
using System.Text.Json;

namespace JsonDatabase
{
    public static class JsonDatabase
    {
        private const string SETTINGS_FILE_NAME = "Settings.json";

        private static string _settingsFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SETTINGS_FILE_NAME);

        private const string AMMO_FIRE_RANGE_FILE_NAME = "AmmoFireRange.json";
        private static string _ammoFireRangeFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AMMO_FIRE_RANGE_FILE_NAME);

        private static readonly Lazy<List<AmmoRangeDTO>> _ammoRange = new(GetAllAmmoRange);
        public static List<AmmoRangeDTO> AmmoRange => _ammoRange.Value;

        private static readonly string[] _unitNamesExceptions = ["Unit", "Ammo", "SAM"];

        static JsonDatabase()
        {
        }

        public static string[] GetDuplicatedAmmoNames() 
        {
            return AmmoRange
                .GroupBy(x => x.AmmoName)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key).ToArray();
        }

        public static AmmoRangeDTO FindAmmoRange(string ammoName)
        {
            if (ammoName.Contains("~/"))
                ammoName = ammoName.Replace("~/", string.Empty);

            var unitRealAmmoRange = AmmoRange.SingleOrDefault(a => a.AmmoName == ammoName);

            return unitRealAmmoRange;
        }

        public static void AddOrUpdateAmmoRangeByWebName(AmmoRangeDTO item)
        {
            var existingToUpdate = AmmoRange.FirstOrDefault(x => x.AmmoName == item.AmmoName);

            if (existingToUpdate is not null)
            {
                existingToUpdate.FireRangeInMeters = item.FireRangeInMeters;
                return;
            }

            AmmoRange.Add(item);
        }

        private static List<AmmoRangeDTO> GetAllAmmoRange()
        {
            var text = File.ReadAllText(_ammoFireRangeFilePath);

            if (text.Length == 0)
                return [];

            return JsonSerializer.Deserialize<List<AmmoRangeDTO>>(text);
        }

        public static async Task SaveAmmoAsync()
        {
            await File.WriteAllTextAsync(_ammoFireRangeFilePath, JsonSerializer.Serialize(AmmoRange));
        }

        public static SettingsDTO LoadSettings()
        {
            var text = File.ReadAllText(_settingsFilePath);

            return JsonSerializer.Deserialize<SettingsDTO>(text);
        }

        public static async Task SaveSettings(SettingsDTO settingsDTO)
        {
            var text = JsonSerializer.Serialize(settingsDTO);

            await File.WriteAllTextAsync(_settingsFilePath, text);
        }
    }
}
