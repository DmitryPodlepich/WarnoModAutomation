using JsonDatabase.DTO;
using System.Text.Json;

namespace JsonDatabase
{
    public static class JsonDatabase
    {
        private const string FOLDER_NAME = "JsonDatabase";
        private static string FolderPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FOLDER_NAME);

        private const string AMMO_FIRE_RANGE_FILE_NAME = "AmmoFireRange.json";
        private static string AmmoFireRangeFilePath => Path.Combine(FolderPath, AMMO_FIRE_RANGE_FILE_NAME);

        private static readonly Lazy<List<AmmoRangeDTO>> _ammoRange = new(GetAllAmmoRange);
        public static List<AmmoRangeDTO> AmmoRange => _ammoRange.Value;

        private static readonly string[] unitNamesExceptions = ["Unit", "Ammo", "SAM"];

        static JsonDatabase()
        {
            if(!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);

            if (!File.Exists(AmmoFireRangeFilePath))
                File.Create(AmmoFireRangeFilePath);
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

        public static async Task SaveAsync()
        {
            await File.WriteAllTextAsync(AmmoFireRangeFilePath, JsonSerializer.Serialize(AmmoRange));
        }

        private static List<AmmoRangeDTO> GetAllAmmoRange()
        {
            var text = File.ReadAllText(AmmoFireRangeFilePath);

            if (text.Length == 0)
                return [];

            return JsonSerializer.Deserialize<List<AmmoRangeDTO>>(text);
        }
    }
}
