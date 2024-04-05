using System.IO;
using System.Reflection;

namespace UzlePlugins.Settings
{
    internal class FileReader
    {
        //private string _jsonFileContain;


        public string ReadFile(string filename)
        {
            string assemblyFolder =
                "C:\\Users\\yusufzhon.marasulov\\source\\repos\\UzlePlugins\\UzlePlugins.Models.Revit2022\\bin\\Debug\\net48";

#if !DEBUG
                assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#endif

            var file = Path.Combine(assemblyFolder, filename);
            return File.ReadAllText(file);
        }
        //public string JsonFileContain => _jsonFileContain;
    }
}
