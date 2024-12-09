using DevelopsTodayTestTask;
using DevelopsTodayTestTask.DB;
using DevelopsTodayTestTask.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text;

internal class Program
{
    private async static Task Main(string[] args)
    {
        if(args.Length != 1)
        {
            Console.WriteLine("Specify import data file name.");
            return;
        }

        var fileName = args[0];
        if (!File.Exists(fileName))
        {
            Console.WriteLine("No such file in working directory.");
            return;
        }

        var csvParser = new CsvParser();

        var parsedData = await csvParser.Parse(fileName);

        if (!parsedData.Any())
        {
            Console.WriteLine("Any data present in the import file.");
            return;
        }

        var duplicatedObjects = parsedData
            .GroupBy(x => new { x.tpep_pickup_datetime, x.tpep_dropoff_datetime, x.passenger_count })
            .Where(x => x.Count() > 1)               
            .SelectMany(x => x.Skip(1))       
            .ToList();

        if (duplicatedObjects.Any())
        {
            await csvParser.WriteCsv("duplicates.csv", duplicatedObjects);
            foreach( var duplicatedObject in duplicatedObjects)
            {
                parsedData.Remove(duplicatedObject);
            }
        }

        await SaveRecords(parsedData);

    }

    protected static async Task SaveRecords(List<ImportData> data)
    {
        try
        {
            using (var connection = new ClientDbContext().Database.GetDbConnection())
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandTimeout = 60 * 60;

                    StringBuilder cmdBuilder = new StringBuilder();
                    int batchSize = 1000;

                    for (int i = 0; i < data.Count; i += batchSize)
                    {
                        cmdBuilder.Clear();

                        cmdBuilder.AppendLine("INSERT INTO import_integration (tpep_pickup_datetime, tpep_dropoff_datetime, passenger_count, trip_distance, store_and_fwd_flag, PULocationID, DOLocationID, fare_amount, tip_amount)");
                        cmdBuilder.AppendFormat("VALUES ");

                        var batch = data.GetRange(i, Math.Min(batchSize, data.Count - i));

                        foreach (var item in batch)
                        {
                            if (item.store_and_fwd_flag is null)
                            {
                                item.store_and_fwd_flag = "NULL";
                            }
                            else
                            {
                                item.store_and_fwd_flag = string.Format("'{0}'", item.store_and_fwd_flag);
                            }
                            cmdBuilder.AppendFormat("('{0}', '{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8})",
                                item.tpep_pickup_datetime,
                                item.tpep_dropoff_datetime,
                                item.passenger_count?.ToString() ?? "NULL",
                                item.trip_distance?.ToString() ?? "NULL",
                                item.store_and_fwd_flag,
                                item.PULocationID,
                                item.DOLocationID,
                                item.fare_amount,
                                item.tip_amount);
                            cmdBuilder.Append(',');
                        }

                        cmdBuilder.Remove(cmdBuilder.Length - 1, 1);
                        cmdBuilder.Append(';');
                        cmd.CommandText = cmdBuilder.ToString();

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error occured while saving records to database.");
            Console.WriteLine(ex.Message);
        }
    }
}

