using JsonDatabase.DTO;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace JsonDatabase
{
    public static class JsonDatabase
    {
        private const string SETTINGS_FILE_NAME = "Settings.json";
        private const string AMMO_FIRE_RANGE_FILE_NAME = "AmmoFireRange.json";
        private const string DIVISION_RULES_FILE_NAME = "DivisionRules.json";
        private static string SettingsFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SETTINGS_FILE_NAME);
        private static string AmmoFireRangeFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AMMO_FIRE_RANGE_FILE_NAME);
        private static string DivisionRulesFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DIVISION_RULES_FILE_NAME);

        private static readonly Lazy<List<AmmoRangeDTO>> _ammoRange = new(GetAllAmmoRange);
        public static List<AmmoRangeDTO> AmmoRange => _ammoRange.Value;

        private static readonly string[] _unitNamesExceptions = ["Unit", "Ammo", "SAM"];

        static JsonDatabase()
        {
            EnsureFilesCreated();
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

        public static async Task SaveAmmoAsync()
        {
            await File.WriteAllTextAsync(AmmoFireRangeFilePath, JsonSerializer.Serialize(AmmoRange));
        }

        public static async Task<SettingsDTO> LoadSettingsAsync()
        {
            var text = await File.ReadAllTextAsync(SettingsFilePath);

            return JsonSerializer.Deserialize<SettingsDTO>(text);
        }

        public static async Task SaveSettingsasync(SettingsDTO settingsDTO)
        {
            var text = JsonSerializer.Serialize(settingsDTO);

            await File.WriteAllTextAsync(SettingsFilePath, text);
        }

        public static async Task<List<DivisionRuleDTO>> LoadDivisionRulesAsync()
        {
            var text = await File.ReadAllTextAsync(DivisionRulesFilePath);

            if (text.Length == 0)
                return [];

            return JsonSerializer.Deserialize<List<DivisionRuleDTO>>(text);
        }

        public static async Task SaveDivisionRulesAsync(DivisionRuleDTO[] divisionRules)
        {
            var text = JsonSerializer.Serialize(divisionRules);

            await File.WriteAllTextAsync(DivisionRulesFilePath, text);
        }

        private static List<AmmoRangeDTO> GetAllAmmoRange()
        {
            var text = File.ReadAllText(AmmoFireRangeFilePath);

            if (text.Length == 0)
                return [];

            return JsonSerializer.Deserialize<List<AmmoRangeDTO>>(text);
        }

        private static async Task EnsureFilesCreated()
        {
            if (!File.Exists(SettingsFilePath))
                File.Create(SettingsFilePath);
            if(!File.Exists(AmmoFireRangeFilePath))
                await File.WriteAllTextAsync(AmmoFireRangeFilePath, JsonSerializer.Serialize(new List<AmmoRangeDTO>()));
            if(!File.Exists(DivisionRulesFilePath))
                await File.WriteAllTextAsync(DivisionRulesFilePath, JsonSerializer.Serialize(new List<DivisionRuleDTO>()));
        }
    }
}
