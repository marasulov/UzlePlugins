using System;
using System.IO;
using System.Text.Json;
using UzlePlugins.Contracts;

namespace UzlePlugins.Settings
{
    public class SettingsReader<T> : ISettingsReader<T>
    {
        private string _settings;
        private JsonSerializerOptions _options;
        
        public void ReadSettings(string filename)
        {
            Filename = filename;
            FileReader reader = new FileReader();
            _settings = reader.ReadFile(filename);

            _options = new JsonSerializerOptions { WriteIndented = true };
        }

        public T Read(string filename)
        {
            try
            {
                ReadSettings(filename);
                if (_settings != null)
                {
                    var jsonObject = JsonSerializer.Deserialize<T>(_settings, _options);
                    return jsonObject;
                }

                return default;
            }
            catch (Exception e)
            {
                return default;
            }

        }

        public void WriteData<T>(T data, string filename)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(data, options);

                File.WriteAllText(filename, jsonString);

                Console.WriteLine($"Данные успешно записаны в файл {filename}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при записи в файл: {ex.Message}");
            }
        }


        public string Filename { get; private set; }
    }
}
