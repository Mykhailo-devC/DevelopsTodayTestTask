using DevelopsTodayTestTask.Models;

namespace DevelopsTodayTestTask
{
    public class CsvParser
    {
        private int _batchSize;
        public CsvParser(int batchSize = 1000)
        {
            _batchSize = batchSize;
        }
        public async Task<List<ImportData>> Parse(string filename)
        {
            var result = new List<ImportData>();
            var tasks = new List<Task<List<ImportData>>>();

            try
            {
                using (var reader = new StreamReader(filename))
                {
                    string[] buffer = new string[_batchSize];
                    string line;
                    int index = 0, n = 0;

                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        n++;
                        if (n == 1)
                        {
                            continue;
                        }

                        buffer[index++] = line;

                        if (index == _batchSize)
                        {
                            tasks.Add(RetrieveDataFromBatch(buffer));

                            buffer = new string[_batchSize];
                            index = 0;
                        }
                    }

                    if (index > 0)
                    {
                        tasks.Add(RetrieveDataFromBatch(buffer));
                    }

                    var resultData = await Task.WhenAll(tasks);
                    result = resultData.SelectMany(batch => batch).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured while parsing csv file.");
                Console.WriteLine(ex.Message);

                result = [];
            }

            return result;
        }
        public async Task WriteCsv(string fileName, IEnumerable<ImportData> data)
        {
            var csvData = new List<string>();

            csvData.Add(string.Join(",",
                "tpep_pickup_datetime",
                "tpep_dropoff_datetime",
                "passenger_count",
                "trip_distance",
                "store_and_fwd_flag",
                "PULocationID",
                "DOLocationID",
                "fare_amount",
                "tip_amount"
            ));

            foreach (var item in data)
            {
                csvData.Add(string.Join(",",
                        item.tpep_pickup_datetime,
                        item.tpep_dropoff_datetime,
                        item.passenger_count,
                        item.trip_distance,
                        item.store_and_fwd_flag,
                        item.PULocationID,
                        item.DOLocationID,
                        item.fare_amount,
                        item.tip_amount
                    ));
            }

            await File.WriteAllLinesAsync(fileName, csvData);
        }

        private async Task<List<ImportData>> RetrieveDataFromBatch(string[] lines)
        {
            var result = new List<ImportData>();

            foreach (var line in lines)
            {
                if (line == null) break;

                var fields = line.Split(",");
                if (fields.Length == 18)
                {
                    var data = new ImportData();

                    data.tpep_pickup_datetime = ConvertToUTC(DateTime.Parse(fields[1].Trim()));
                    data.tpep_dropoff_datetime = ConvertToUTC(DateTime.Parse(fields[2].Trim()));

                    data.passenger_count = int.TryParse(fields[3].Trim(), out int passenger_count) ? passenger_count : null;
                    data.trip_distance = float.TryParse(fields[4].Trim(), out float trip_distance) ? trip_distance : null;

                    var store_and_fwd_flag = fields[6].Trim();
                    if (store_and_fwd_flag == "N")
                    {
                        data.store_and_fwd_flag = "No";
                    }
                    else if (store_and_fwd_flag == "Y")
                    {
                        data.store_and_fwd_flag = "Yes";
                    }

                    data.PULocationID = int.Parse(fields[7].Trim());
                    data.DOLocationID = int.Parse(fields[8].Trim());
                    data.fare_amount = float.Parse(fields[10].Trim());
                    data.tip_amount = float.Parse(fields[13].Trim());

                    result.Add(data);
                }
                else
                {
                    Console.WriteLine("Incorrect number of fields. Line: " + line);
                }
            }

            return result;
        }

        private DateTime ConvertToUTC(DateTime est)
        {
            TimeZoneInfo estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            return TimeZoneInfo.ConvertTimeToUtc(est, estZone);
        }


    }

}
