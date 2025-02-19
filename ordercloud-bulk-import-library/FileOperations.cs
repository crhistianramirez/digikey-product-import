using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using System.Globalization;

namespace ordercloud_bulk_import_library
{
    public static class FileOperations
    {

        public static string? GetProjectDataPath()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            var directoryInfo = new DirectoryInfo(currentDirectory);

            while (directoryInfo != null && !directoryInfo.GetFiles("*.sln").Any()) { 
                directoryInfo = directoryInfo.Parent;
            }

            return Path.Combine(directoryInfo?.FullName, "Data");
        }

        public static string GetDataFile(string fileName)
        {
            return Path.Combine(GetProjectDataPath(), fileName);
        }
        
        public static IEnumerable<T> ReadCsv<T, T1>(string filename) where T1 : ClassMap
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                PrepareHeaderForMatch = args => args.Header
                    .Replace(":", "_").Replace("/", "_").Replace(".", "_").Replace("-", "_")
                    .Replace(")", "").Replace("(", "")
            };

            using var csv = new CsvReader(new StreamReader(filename), config);
            csv.Context.RegisterClassMap<T1>();

            while (csv.Read())
                yield return csv.GetRecord<T>();
        }

        public static List<T> FileReadJson<T>(string filepath)
        {
            var file = File.ReadAllText(filepath);
            return JsonConvert.DeserializeObject<List<T>>(file) ?? [];
        }

        public static void FileWriteAllText(string filepath, object item) =>
            File.WriteAllText(filepath, JsonConvert.SerializeObject(item));

        public static void FileAppendTextLogError(string filepath, Exception ex, string message = "")
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message ?? message);
                Console.ForegroundColor = ConsoleColor.White;
                FileOpenWriteObject(filepath, new
                {
                    Message = message ?? "",
                    Errors = "",
                    RequestMessage = ex?.Message ?? ""
                });
            }
            catch (Exception)
            {
                Console.WriteLine($"Error writing to log file");
            }
        }

        public static void FileOpenWriteObject(string filepath, object item)
        {
            using var stream = File.OpenWrite(filepath);
            using var writer = new StreamWriter(stream);
            JsonSerializer serializer = new();
            serializer.Serialize(writer, item);
        }

        public static void FileCreateTextObject(string filepath, object item)
        {
            using StreamWriter file = File.CreateText(filepath);
            JsonSerializer serializer = new();
            serializer.Serialize(file, item);
        }
    }
}
