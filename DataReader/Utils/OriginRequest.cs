
using GUI.Data;
using IoTClient.Clients.Modbus;
using libplctag;
using libplctag.DataTypes;
using System.Xml;
using System.Xml.Linq;

namespace GUI.Utils
{
    public class OriginRequest : Observable
    {
        private Task _backgroundWorker;
        private DataTag tag;
        public string ReturnValue;
        private CancellationTokenSource _token;

        public OriginRequest() : base()
        {
        }


        public void ReadOriginTagAsync(DataTag tag)
        {

            this.tag = tag;
            if (_token != null)
            {
                _token.Cancel();
            }
            _token = new CancellationTokenSource();
            _backgroundWorker = new Task(() => ReadOriginTag(), _token.Token);
            _backgroundWorker.Start();
        }

        public string ReadOriginTagSync(DataTag tag)
        {
            this.tag = tag;
            ReadOriginTag();
            return ReturnValue;
        }

        private void ReadOriginTag()
        {
            try
            {
                ReturnValue = "";
                switch (tag.DataOrigin.Type)
                {
                    case Constants.MODBUS:
                        ModbusTcpClient modbusClient = new(tag.DataOrigin.Ip, 502);
                        int start = short.Parse(tag.Address[0..1]);
                        int end = int.Parse(tag.Address[1..]);
                        switch (tag.Type)
                        {
                            case Constants.BOOLEAN:
                                IoTClient.Result<ushort>? connBool = modbusClient.ReadUInt16(end + "", Convert.ToByte(tag.DataOrigin.Path), Convert.ToByte(start));
                                ReturnValue = connBool.Value.ToString();
                                break;

                            case Constants.SHORT:
                                IoTClient.Result<short>? connShort = modbusClient.ReadInt16(end + "", Convert.ToByte(tag.DataOrigin.Path), Convert.ToByte(start));
                                ReturnValue = connShort.Value.ToString();
                                break;

                            case Constants.INTEGER:
                                IoTClient.Result<int>? connInt = modbusClient.ReadInt32(end + "", Convert.ToByte(tag.DataOrigin.Path), Convert.ToByte(start));
                                ReturnValue = connInt.Value.ToString();
                                break;

                            case Constants.LONG:
                                IoTClient.Result<long>? connLong = modbusClient.ReadInt64(end + "", Convert.ToByte(tag.DataOrigin.Path), Convert.ToByte(start));
                                ReturnValue = connLong.Value.ToString();
                                break;

                            case Constants.FLOAT:
                                IoTClient.Result<float>? connFloat = modbusClient.ReadFloat(end + "", Convert.ToByte(tag.DataOrigin.Path), Convert.ToByte(start));
                                ReturnValue = connFloat.Value.ToString();
                                break;

                            case Constants.DOUBLE:
                                IoTClient.Result<double>? connDouble = modbusClient.ReadDouble(end + "", Convert.ToByte(tag.DataOrigin.Path), Convert.ToByte(start));
                                ReturnValue = connDouble.Value.ToString();
                                break;

                        }
                        _ = modbusClient.Close();

                        break;

                    case Constants.XML:

                        HttpClient client = new();
                        string downloadString = client.GetStringAsync(tag.DataOrigin.Ip).Result;
                        XmlDocument xdoc = new();
                        xdoc.LoadXml(downloadString);
                        XmlNode xmlnode = xdoc;
                        bool Found = false;
                        foreach (string index in tag.Address.Split(Constants.SPLITTER))
                        {
                            bool isNumeric = int.TryParse(index, out int n);
                            if (isNumeric)
                            {
                                xmlnode = xmlnode.ChildNodes[n];
                            }
                            else
                            {
                                if (index.Equals(Constants.XML_VALUE_STD))
                                {
                                    if (xmlnode.Value != null)
                                    {
                                        Found = true;
                                        ReturnValue = xmlnode.Value;
                                    }

                                }
                                foreach (XmlAttribute att in xmlnode.Attributes)
                                {
                                    if (att.Name.Equals(index))
                                    {
                                        Found = true;
                                        ReturnValue = att.Value;
                                    }
                                }
                                if (!Found)
                                {
                                    xmlnode = null;
                                }
                            }
                        }


                        if (!Found)
                        {
                            if (xmlnode != null)
                            {
                                string? xml = xmlnode.OuterXml;
                                bool toRemove = false;
                                if (!xml.Contains("<root>") && !xml.Contains("?xml"))
                                {
                                    xml = "<root>" + xml + "</root>";
                                    toRemove = true;
                                }
                                if (!xml.Contains("<root>") && xml.Contains("?xml"))
                                {
                                    xml += "<root></root>";
                                    toRemove = true;
                                }
                                ReturnValue = XElement.Parse(xml).ToString();
                                if (xml.Contains("?xml"))
                                {
                                    ReturnValue = "<xml>\r\n" + ReturnValue;
                                }
                                if (toRemove)
                                {

                                    ReturnValue = ReturnValue.Replace("<root>\r\n", "");
                                    ReturnValue = ReturnValue.Replace("\r\n</root>", "");
                                    ReturnValue = ReturnValue.Replace("\r\n<root>", "");
                                    ReturnValue = ReturnValue.Replace("</root>", "");
                                }
                                _ = ReturnValue.Trim();
                            }
                            else
                            {
                                ReturnValue = "Error in Address!<W>";
                            }
                        }


                        break;

                    default:
                        switch (tag.Type)
                        {
                            case Constants.BOOLEAN:
                                Tag<BoolPlcMapper, bool>? boolLong = new()
                                {
                                    Name = tag.Address,
                                    Gateway = tag.DataOrigin.Ip,
                                    Path = tag.DataOrigin.Path,
                                    PlcType = PlcType.ControlLogix,
                                    Protocol = Protocol.ab_eip,
                                    Timeout = TimeSpan.FromSeconds(5)
                                };
                                _ = boolLong.Read();
                                ReturnValue = boolLong.Value.ToString();
                                break;


                            case Constants.SHORT:
                                Tag<IntPlcMapper, short>? connShort = new()
                                {
                                    Name = tag.Address,
                                    Gateway = tag.DataOrigin.Ip,
                                    Path = tag.DataOrigin.Path,
                                    PlcType = PlcType.ControlLogix,
                                    Protocol = Protocol.ab_eip,
                                    Timeout = TimeSpan.FromSeconds(5)
                                };

                                _ = connShort.Read();
                                ReturnValue = connShort.Value.ToString();
                                break;


                            case Constants.INTEGER:
                                Tag<DintPlcMapper, int>? connInt = new()
                                {
                                    Name = tag.Address,
                                    Gateway = tag.DataOrigin.Ip,
                                    Path = tag.DataOrigin.Path,
                                    PlcType = PlcType.ControlLogix,
                                    Protocol = Protocol.ab_eip,
                                    Timeout = TimeSpan.FromSeconds(5)
                                };

                                _ = connInt.Read();
                                ReturnValue = connInt.Value.ToString();
                                break;


                            case Constants.LONG:
                                Tag<LintPlcMapper, long>? connLong = new()
                                {
                                    Name = tag.Address,
                                    Gateway = tag.DataOrigin.Ip,
                                    Path = tag.DataOrigin.Path,
                                    PlcType = PlcType.ControlLogix,
                                    Protocol = Protocol.ab_eip,
                                    Timeout = TimeSpan.FromSeconds(5)
                                };
                                _ = connLong.Read();
                                ReturnValue = connLong.Value.ToString();
                                break;


                            case Constants.DOUBLE:
                                Tag<LrealPlcMapper, double>? connDouble = new()
                                {
                                    Name = tag.Address,
                                    Gateway = tag.DataOrigin.Ip,
                                    Path = tag.DataOrigin.Path,
                                    PlcType = PlcType.ControlLogix,
                                    Protocol = Protocol.ab_eip,
                                    Timeout = TimeSpan.FromSeconds(5)
                                };
                                _ = connDouble.Read();
                                ReturnValue = connDouble.Value.ToString();
                                break;


                            case Constants.FLOAT:
                                Tag<RealPlcMapper, float>? connFloat = new()
                                {
                                    Name = tag.Address,
                                    Gateway = tag.DataOrigin.Ip,
                                    Path = tag.DataOrigin.Path,
                                    PlcType = PlcType.ControlLogix,
                                    Protocol = Protocol.ab_eip,
                                    Timeout = TimeSpan.FromSeconds(0.1)
                                };
                                _ = connFloat.Read();
                                ReturnValue = connFloat.Value.ToString();
                                break;

                        }

                        break;
                }

            }
            catch (Exception ex)
            {
                ReturnValue = ex.Message + "<e>";
                if (tag.DataOrigin.Type.Equals(Constants.MODBUS))
                {
                    ReturnValue = "1: ReadCoils, 2: Read Discrete Inputs, 3:Read Holding Registers, 4:Read Input Registers\n" + ReturnValue;
                }
            }

            SendToSubscriber(Constants.LIVE_READ, ReturnValue);

        }



        public static ITag getTag(DataTag tag)
        {
            ITag retI = new Tag<BoolPlcMapper, bool>();
            if (tag.Type == Constants.FLOAT)
            {
                retI = new Tag<LrealPlcMapper, double>();
            }

            if (tag.Type is Constants.INTEGER or Constants.LONG)
            {
                retI = new Tag<LintPlcMapper, long>();
            }


            retI.Name = tag.Address;
            retI.Gateway = tag.DataOrigin.Ip;
            retI.Path = tag.DataOrigin.Path;
            retI.PlcType = (PlcType?)Filler.TypeOrigin().IndexOf(tag.DataOrigin.Type);
            retI.Protocol = Protocol.ab_eip;

            return retI;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
