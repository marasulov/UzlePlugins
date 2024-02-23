using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using UzlePlugins.Settings;

namespace UzlePlugins.RevitCore.Settings
{
    public class SettingsReader
    {
        private readonly string _jsonFileName;

        public SettingsReader()
        {
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            _jsonFileName = Path.Combine(assemblyFolder,"Settings.json");
        }

        public HoleFamilyTypes GetFamilyTypes( )
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            
            using (FileStream fs = new FileStream(_jsonFileName, FileMode.Open))
            {
                return JsonSerializer.Deserialize<HoleFamilyTypes>(fs,options);
                
            }
        }
    }
}
