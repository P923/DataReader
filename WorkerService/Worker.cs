using GUI.Data;
using GUI.Utils;
using Questdb.Net;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _Logger;
        private readonly int WAIT_SLEEP_MAIN = 5000;
        private readonly List<DataTag> _Tags;
        private readonly QuestDBClient _QuestDBClient;
        private readonly SessionTracker _sessions;

        //private InfluxDBClient TSDbClient;

        public Worker(ILogger<Worker> Logger, List<DataTag> Tags)
        {
            _Logger = Logger;
            _Tags = Tags;
            _QuestDBClient = new QuestDBClient(Program.IP);
        }

        protected override async Task ExecuteAsync(CancellationToken StoppingToken)
        {
            bool close = false;
            List<ReaderService> readers = new();
            foreach (DataTag tag in _Tags)
            {
                readers.Add(new ReaderService(tag, _Logger, _QuestDBClient, _sessions));
            }

            foreach (ReaderService backgroundService in readers)
            {
                backgroundService.StartRead(StoppingToken);
            }

            while (!close)
            {
                Thread.Sleep(WAIT_SLEEP_MAIN);
                close = StoppingToken.IsCancellationRequested;
            }

            _sessions.CloseAllSessions();

        }
    }
}