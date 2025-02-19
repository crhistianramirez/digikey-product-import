using Newtonsoft.Json;
using System.Text;

namespace ordercloud_bulk_import_library
{
    public class Status
    {
        private readonly string _meta; // File length + last modified timestamp
        private int _processed;
        private readonly string _filename;
        private readonly string _progressFile;

        private class SavedProgress
        {
            public string Meta { get; set; }
            public int Processed { get; set; }
        }

        public int Processed => _processed;

        /// <summary>
        /// Initializes status tracking and ensures progress.json is correctly set up.
        /// </summary>
        public Status(string sourceFile)
        {
            try
            {
                _meta = GetFileMeta(GetDataFile(sourceFile));
                _filename = sourceFile;
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException($"Source file not found: {sourceFile}");
            }

            _progressFile = GetDataFile("progress.json");

            var savedProgress = ReadProgress();

            if (savedProgress == null)
            {
                WriteProgress(new SavedProgress { Meta = _meta, Processed = 0 });
                _processed = 0;
            }
            else
            {
                if (_meta != savedProgress.Meta)
                    throw new Exception($"{sourceFile} appears to have changed since last run. " +
                        $"To start over, delete progress.json. " +
                        $"If you’re sure you don’t need to start over, manually update FileMeta in progress.json to {_meta}.");

                _processed = savedProgress.Processed;
            }
        }

        private SavedProgress? ReadProgress()
        {
            if (!File.Exists(_progressFile)) return null;

            for (int i = 0; i < 5; i++) // Retry logic
            {
                try
                {
                    using (var stream = new FileStream(_progressFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var reader = new StreamReader(stream))
                    {
                        var content = reader.ReadToEnd();
                        return JsonConvert.DeserializeObject<SavedProgress>(content);
                    }
                }
                catch (IOException)
                {
                    Thread.Sleep(100); // Wait before retrying
                }
            }
            throw new IOException("Failed to read progress file after multiple attempts.");
        }

        private void WriteProgress(SavedProgress progress)
        {
            int maxRetries = 5;
            int delay = 1000; // Milliseconds
            for (int i = 0; i < maxRetries; i++) // Retry logic
            {
                try
                {
                    using (var stream = new FileStream(_progressFile, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(JsonConvert.SerializeObject(progress));
                    }
                    return;
                }
                catch (IOException)
                {
                    Thread.Sleep(delay); // Wait before retrying
                    delay *= 2; // Exponential backoff
                }
            }
            throw new IOException("Failed to write progress file after multiple attempts.");
        }

        public void Report() => Console.WriteLine($"Processed {_processed} in file {_filename}");

        public void Start() => Console.WriteLine($"Starting operation at {_processed} in file {_filename}");

        public void Stop() => Console.WriteLine($"End of operation. Processed {_processed} in file {_filename}");

        public void Increment(int count = 1) => _processed += count;

        public void Update(int count = 1)
        {
            Increment(count);
            WriteProgress(new SavedProgress { Meta = _meta, Processed = _processed });
        }

        internal void LogError(object obj)
        {
            string filePath = GetDataFile("errors.json") ?? throw new Exception("Failed to determine error log path.");
            string logEntry = JsonConvert.SerializeObject(obj) + Environment.NewLine;
            int maxRetries = 5;
            int delay = 1000; // Milliseconds

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    using (var writer = new StreamWriter(stream, Encoding.UTF8))
                    {
                        writer.WriteLine(logEntry);
                        return; // Success
                    }
                }
                catch (IOException) when (attempt < maxRetries)
                {
                    Thread.Sleep(delay);
                    delay *= 2; // Exponential backoff
                }
            }
            throw new IOException("Failed to write error log after multiple attempts.");
        }

        public static string? GetDataFile(string filename)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            var directoryInfo = new DirectoryInfo(currentDirectory);

            while (directoryInfo != null && !directoryInfo.GetFiles("*.sln").Any())
            {
                directoryInfo = directoryInfo.Parent;
            }

            return Path.Combine(directoryInfo?.FullName, "Data", filename);
        }

        private static string GetFileMeta(string filePath)
        {
            FileInfo fileInfo = new(filePath);
            return $"{fileInfo.Length}-{fileInfo.LastWriteTimeUtc.Ticks}";
        }
    }
}
