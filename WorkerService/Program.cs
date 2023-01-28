using GUI.Data;
using Microsoft.EntityFrameworkCore;




namespace WorkerService
{

    internal class Program
    {
        public const string DBNAME = "bucket";
        public const string IP = "http://127.0.0.1";

        public static void Main()
        {
            SQLConnector db = new();
            List<DataTag> tags = db.Tags.Include(e => e.DataOrigin).ToList();


            IHost host = Host.CreateDefaultBuilder()
                .UseWindowsService()
                .UseSystemd()
                .ConfigureServices(services =>
                {
                    _ = services.AddHostedService(worker => new Worker(worker.GetService<ILogger<Worker>>(), tags));

                })
                .Build();

            _ = host.RunAsync();

        }
    }
}
