using WarnoModeAutomation.DTO.NDFFiles;

namespace WarnoModeAutomation.Logic
{
    public static class NDFFormatInteractions
    {
        public static void SetRealUnitName(this TEntityDescriptor entityDescriptor) 
        {
            var vehicleApparenceModelModuleDescriptor = entityDescriptor.ModulesDescriptors
                    .OfType<VehicleApparenceModelModuleDescriptor>()
                    .SingleOrDefault();

            if (vehicleApparenceModelModuleDescriptor is null)
                throw new InvalidOperationException($"Cannot find VehicleApparenceModelModuleDescriptor in TEntityDescriptor: {entityDescriptor.ClassNameForDebug}");

            var file = FileManager.TryGetFileWithFullSearch(FileManager.DepictionResourcesPath, vehicleApparenceModelModuleDescriptor.BlackHoleIdentifier+".ndf");

            var templateMeshDescriptors = NDFSerializer.Deserialize<TemplateMeshDescriptor>(file.FullName);

            var firstTemplateMeshDescriptor = templateMeshDescriptors.RootDescriptors.First();

            var gameUIUnitName = firstTemplateMeshDescriptor.FileName.Split('/').Last().Split('.')[0].Replace('_',' ');

            entityDescriptor.GameUIUnitName = gameUIUnitName;
        }
    }
}
