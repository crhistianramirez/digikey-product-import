using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using System.Globalization;
using System.Xml.Serialization;

namespace ordercloud_bulk_import_library
{
    public class Commands
    {
        public static (T file, Status status, Tracker tracker) StartXmlOperation<T>(string sourceFile, Func<T, int> countItems)
        {
            var status = StartStatusOperation(sourceFile);
            var tracker = ConfigureTrackerOperation(10.Seconds());
            var catalog = LoadXmlFile<T>(sourceFile);

            var itemCount = countItems(catalog);
            ValidateOperation(itemCount, status);
            tracker.ItemsDiscovered(itemCount);
            status.Start();

            return new(catalog, status, tracker);
        }

        public static (IEnumerable<T> items, Status status, Tracker tracker) StartJsonOperation<T>(string sourceFile)
        {
            var status = StartStatusOperation(sourceFile);
            var tracker = ConfigureTrackerOperation(1.Seconds());
            var items = LoadJsonFile<T>(sourceFile, status.Processed);

            ValidateOperation(items.Count(), status);

            tracker.ItemsDiscovered(items.Count());
            status.Start();

            return new(items, status, tracker);
        }

        public static (IEnumerable<T>? items, Status status, Tracker tracker) StartCsvOperation<T, T1>(string sourceFile) where T1 : ClassMap
        {
            var status = StartStatusOperation(sourceFile);
            var tracker = ConfigureTrackerOperation(1.Seconds());
            var items = LoadCsvFile<T, T1>(sourceFile).Skip(status.Processed);

            ValidateOperation(items.Count(), status);

            tracker.ItemsDiscovered(items.Count());
            status.Start();
            return new(items, status, tracker);
        }

        public static async Task BatchOperation<T>(int batchSize, Tracker tracker, Status status, IEnumerable<T> items, Func<T, Task> op)
        {
            while (true)
            {
                var batch = items.Take(batchSize).ToList();
                if (batch.Count == 0)
                    break;

                await Throttler.RunAsync(batch, ThrottlerConfig.minPause, ThrottlerConfig.maxConcurrent, async row =>
                {
                    try
                    {
                        tracker.ItemStarted();
                        await op(row);
                        tracker.ItemSucceeded();
                    }
                    catch (OrderCloudException ex)
                    {
                        tracker.ItemFailed();
                        status.LogError(new
                        {
                            Message = $"Error: {ex.Message}",
                            Object = JsonConvert.SerializeObject(row)
                        });
                    }
                    catch (Exception ex)
                    {
                        tracker.ItemFailed();
                        status.LogError(new
                        {
                            Message = $"Error: {ex.Message}",
                            Object = JsonConvert.SerializeObject(row)
                        });
                    }
                    status.Update();
                });

                items = items.Skip(batchSize);
            }
        }

        public static void EndOperation(Status status, Tracker tracker)
        {
            tracker.Stop();
            status.Stop();
        }

        // add other validation here as needed
        private static void ValidateOperation(int count, Status status)
        {
            if (count == 0) throw new Exception($"Items discovered = {count}. " +
                $"Items processed = ({status.Processed}). " +
                $"On file load, items discovered skips the items processed. If the processed count matches the total items discovered there will be none to process" +
                $"To start over with import, delete progress.json file or manually set the processed count to 0.");
        }

        private static Status StartStatusOperation(string filename)
        {
            return new Status(filename);
        }

        private static Tracker ConfigureTrackerOperation(TimeSpan span)
        {
            var tracker = new Tracker();
            tracker.Every(span, LogProgressToConsole);
            tracker.OnComplete(LogProgressToConsole);
            tracker.Start();
            return tracker;
        }

        private static IEnumerable<T> LoadCsvFile<T, T1>(string filename) where T1 : ClassMap
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                Mode = CsvMode.NoEscape, //possible solution to the JSON columns
                // map headers to c# class
                PrepareHeaderForMatch = args => args.Header
                    .Replace(":", "_").Replace("/", "_").Replace(".", "_").Replace("-", "_") // replace most special characters with underscore
                    .Replace(")", "").Replace("(", ""), // remove parenthese
            };

            using var reader = new StreamReader(FileOperations.GetDataFile(filename));
            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<T1>();

            while (csv.Read())
                yield return csv.GetRecord<T>();
        }

        private static IEnumerable<T> LoadJsonFile<T>(string filename, int skip)
        {
            return FileOperations.FileReadJson<T>(FileOperations.GetDataFile(filename))
                .Skip(skip);
        }

        private static T LoadXmlFile<T>(string filename)
        {
            XmlSerializer serializer = new(typeof(T)); //, new XmlRootAttribute { Namespace = "http://ordercloud.io" });
            using StreamReader reader = new(FileOperations.GetDataFile(filename));
            return (T)serializer.Deserialize(reader);
        }

        private static void LogProgressToConsole(Progress progress)
        {
            Console.WriteLine($"{progress.ElapsedTime:hh\\:mm\\:ss} elapsed. {progress.ItemsDone} of {progress.TotalItems} complete ({progress.PercentDone}%)");
        }
    }
}
