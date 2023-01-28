
using GUI.Data;
using GUI.Utils;
using Questdb.Net;
using Questdb.Net.Write;

namespace WorkerService
{
    public class ReaderService
    {
        public DataTag Tag;
        private readonly ILogger<Worker> _logger;
        private readonly OriginRequest _originRequest;
        private readonly QuestDBClient _DB;
        private CancellationToken _stoppingToken;

        private Task _t;
        private long _startTime;
        private long _endTime;

        public ReaderService(DataTag Tag, ILogger<Worker> Logger, QuestDBClient Client)
        {
            this.Tag = Tag;
            _logger = Logger;
            _originRequest = new OriginRequest();
            _DB = Client;
        }


        public void StartRead(CancellationToken StoppingToken)
        {
            _stoppingToken = StoppingToken;
            _t = new Task(Read);
            _t.Start();
        }

        public void Kill()
        {
            _t.Dispose();
        }

        private void Read()
        {
            bool close = false;
            IWriteLineApi? writeApiAsync = _DB.GetWriteApi();
            _logger.LogInformation(Tag.ToString() + ":" + "Starting!");


            while (!close)
            {

                _startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                string? ret = _originRequest.ReadOriginTagSync(Tag);
                ret = checkRet(ret);
                if (ret != null && !ret.Equals(""))
                {
                    PointData? point =
                         PointData.Measurement(Tag.DataOrigin.Name + "_" + Tag.DataOrigin.Ip + Tag.ID + Tag.Name)
                        .Field("value", ret)
                        .Timestamp(DateTime.UtcNow, WritePrecision.Nanoseconds);

                    switch (Tag.Type)
                    {
                        case Constants.BOOLEAN:
                            point =
                               PointData.Measurement(Tag.ID + Tag.Name)
                               .Field("value", bool.Parse(ret))
                                .Timestamp(DateTime.UtcNow, WritePrecision.Nanoseconds);
                            break;


                        case Constants.SHORT:
                            point =
                               PointData.Measurement(Tag.ID + Tag.Name)
                               .Field("value", short.Parse(ret))
                               .Timestamp(DateTime.UtcNow, WritePrecision.Nanoseconds);
                            break;


                        case Constants.INTEGER:
                            point =
                               PointData.Measurement(Tag.ID + Tag.Name)
                               .Field("value", int.Parse(ret))
                               .Timestamp(DateTime.UtcNow, WritePrecision.Nanoseconds);
                            break;


                        case Constants.LONG:
                            point =
                                PointData.Measurement(Tag.ID + Tag.Name)
                                .Field("value", long.Parse(ret))
                                .Timestamp(DateTime.UtcNow, WritePrecision.Nanoseconds);
                            break;


                        case Constants.FLOAT:
                            point =
                                PointData.Measurement(Tag.ID + Tag.Name)
                                .Field("value", float.Parse(ret))
                                .Timestamp(DateTime.UtcNow, WritePrecision.Nanoseconds);
                            break;


                        case Constants.DOUBLE:
                            point =
                                PointData.Measurement(Tag.ID + Tag.Name)
                                .Field("value", double.Parse(ret))
                                .Timestamp(DateTime.UtcNow, WritePrecision.Nanoseconds);
                            break;

                    }
                    writeApiAsync.WritePoint(point);

                }
                else
                {
                    _logger.LogWarning(Tag.ToString() + ":" + "Error in Request...");
                }

                _endTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                long waitTime = Tag.ScanClass - (_endTime - _startTime);
                if (waitTime > 0)
                {
                    Thread.Sleep((int)waitTime);
                }
                else
                {
                    _logger.LogError(Tag.ToString() + ":" + "Too slow!(" + waitTime + ")");
                }


                close = _stoppingToken.IsCancellationRequested;
            }
            _logger.LogInformation(Tag.ToString() + ":Goodbye!");
        }

        private string checkRet(string ret)
        {
            return ret.Contains("<e>") ? null : ret.Contains("<W>") ? Tag.DefaultValue : ret;
        }

        public bool isStuck()
        {
            long diff = _endTime - _startTime;
            diff = (Tag.ScanClass * 5) - Math.Abs(diff);
            return diff < 0;
        }
    }
}