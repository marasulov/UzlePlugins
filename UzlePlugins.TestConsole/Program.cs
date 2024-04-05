// See https://aka.ms/new-console-template for more information

using System.IO;
using System.Reflection;
using System.Text.Json;
using UzlePlugins.Contracts.DTOs;
using UzlePlugins.Settings;

var reader = new SettingsReader<OffsetsDto>();
var r = reader.Read("Settings.json");

var manager = new OffsetManagerService();

var dto = manager.Read();

Console.ReadLine();

namespace UzlePlugins.TestConsole
{
    public class JsonDataManager
    {
        private string filePath;

        public JsonDataManager()
        {
            string assemblyFolder =
                "C:\\Users\\yusufzhon.marasulov\\holeSource\\repos\\UzlePlugins\\UzlePlugins.Settings\\bin\\Debug\\net48\\";

#if !DEBUG
                assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#endif

            filePath = Path.Combine(assemblyFolder, "Offset.json");

        }

        public Offsets? ReadData()
        {
            try
            {
                // Проверяем, существует ли файл
                if (File.Exists(filePath))
                {
                    // Читаем JSON из файла
                    string jsonString = File.ReadAllText(filePath);

                    // Десериализуем JSON в объект
                    var data = JsonSerializer.Deserialize<Offsets>(jsonString);

                    Console.WriteLine("Duct:");
                    foreach (var range in data.Duct)
                    {
                        Console.WriteLine($"Диапазон от {range.From} до {range.To}, смещение: {range.Offset}");
                    }

                    Console.WriteLine("\nPipe:");
                    foreach (var range in data.Pipe)
                    {
                        Console.WriteLine($"Диапазон от {range.From} до {range.To}, смещение: {range.Offset}");
                    }

                    return data;
                }

                Console.WriteLine("Файл не существует.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении из файла: {ex.Message}");
                return null;
            }
        }

        public void WriteData(Offsets data)
        {
            try
            {
                // Сериализуем объект в JSON
                string jsonString = JsonSerializer.Serialize(data);

                // Записываем JSON в файл
                File.WriteAllText(filePath, jsonString);

                Console.WriteLine("Данные успешно записаны в файл.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при записи в файл: {ex.Message}");
            }
        }
    }






    public class Offsets
    {
        public List<OffsetTypes> Duct { get; set; }
        public List<OffsetTypes> Pipe { get; set; }
    }

    public class OffsetTypes
    {
        public int From { get; set; }
        public int To { get; set; }
        public int Offset { get; set; }
    }
}