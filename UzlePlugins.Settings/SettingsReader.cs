using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using UzlePlugins.Contracts;

namespace UzlePlugins.Settings
{
    public class SettingsReader : ISettingsReader
    {
        private readonly string _jsonFileName;

        public SettingsReader()
        {


            _jsonFileName = @"C:\Users\y.marasulov\source\repos\marasulov\UzlePlugins\UzlePlugins.Models.Revit2022\bin\Debug\net48\Settings.json";

        }

        public HoleFamilyTypes? GetFamilyTypes()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };

            using FileStream fs = new FileStream(_jsonFileName, FileMode.Open);
            return JsonSerializer.Deserialize<HoleFamilyTypes>(fs, options);
        }

        public List<string> GetFamilyNames()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };

            using FileStream fs = new FileStream(_jsonFileName, FileMode.Open);
            var names = JsonSerializer.Deserialize<HoleFamilyNames>(fs, options);

            return names.FamilyNames;

        }

        static string GetAssemblyDirectory(System.Reflection.Assembly assembly)
        {
            return System.IO.Path.GetDirectoryName(assembly.Location);
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
