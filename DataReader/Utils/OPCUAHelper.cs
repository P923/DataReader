using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System.Security.Cryptography.X509Certificates;

namespace GUI.Utils
{
    /// <summary>
    /// Singleton to keep track of all connexion to the OPC Servers
    /// </summary>
    public class SessionTracker
    {
        private List<OPCUAHelper> sessions = new List<OPCUAHelper>();

        public OPCUAHelper AddOrCreateSession(string UriToScan, string Username = null, string Password = null)
        {
            OPCUAHelper opc = new OPCUAHelper("DataReader", "Reader", @"C:\Users\Matteo\Documents\TestOPC\OPC.xml");
            opc.UriToScan = UriToScan;

            if (sessions.Contains(opc))
            {
                return sessions.ElementAt(sessions.IndexOf(opc));
            }
            else
            {
                opc.Connect(UriToScan, "Sessione1", Username, Password);
                sessions.Add(opc);
                return opc;
            }

        }

        public void CloseAllSessions()
        {
            foreach (var opc in sessions)
            {
                opc.CloseSession();
            }
            sessions.Clear();
        }

    }

    /// <summary>
    /// Helper for the library OPC Ua.
    /// </summary>
    public class OPCUAHelper
    {
        private string PathXML;
        private Session session;
        private ApplicationInstance application;
        internal string UriToScan;

        public string SessionName { get; private set; }

        public OPCUAHelper(string ApplicationName, string ConfigSectionName, string PathXML)
        {
            this.PathXML = PathXML;

            application = new ApplicationInstance();
            application.ApplicationName = ApplicationName;
            application.ConfigSectionName = ConfigSectionName;
            application.ApplicationType = ApplicationType.Client;
            LoadXML();

        }

        public static NodeId ToNode(string node)
        {
            return new NodeId(node);
        }

        #region Connect to a distant OPC Server
        /// <summary>
        /// Load the configuration XML in Path.
        /// </summary>
        private void LoadXML()
        {
            try
            {
                application.LoadApplicationConfiguration(PathXML, false).Wait();
                bool certOK = application.CheckApplicationInstanceCertificate(false, 0).Result;
                if (!certOK)
                {
                    throw new Exception("Application instance certificate invalid!");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Given an URI, get all EndpointDescriptionCollection 
        /// </summary>
        /// <param name="UriToScan"> Uri to scan</param>
        /// <returns></returns>
        public EndpointDescriptionCollection ScanUri(string UriToScan)
        {
            this.UriToScan = UriToScan;
            Uri uri = new Uri(UriToScan);
            EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(application.ApplicationConfiguration);
            endpointConfiguration.OperationTimeout = 12000;

            DiscoveryClient client = DiscoveryClient.Create(
            uri,
                EndpointConfiguration.Create(application.ApplicationConfiguration),
                application.ApplicationConfiguration);

            EndpointDescriptionCollection endpoints = client.GetEndpoints(null);
            return endpoints;
        }


        /// <summary>
        /// Create a session to an URI.
        /// </summary>
        /// <param name="UriToScan"></param>
        public void Connect(string UriToScan, string SessionName, string Username, string Password)
        {

            this.SessionName = SessionName;
            // Scan the endpoint anf generate a ConfiguredEndpoint
            EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(application.ApplicationConfiguration);
            EndpointDescriptionCollection endpoints = ScanUri(UriToScan);
            ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, endpoints[0], endpointConfiguration);

            ApplicationConfiguration configuration = application.ApplicationConfiguration;

            // Load the Client CERT.
            X509Certificate2 clientCertificate = null;
            X509Certificate2Collection clientCertificateChain = null;

            // Wait to load the certificate.
            clientCertificate = configuration.SecurityConfiguration.ApplicationCertificate.Find(true).Result;
            clientCertificateChain = new X509Certificate2Collection(clientCertificate);
            List<CertificateIdentifier> issuers = new List<CertificateIdentifier>();
            bool done = configuration.CertificateValidator.GetIssuers(clientCertificate, issuers).Result;
            for (int i = 0; i < issuers.Count; i++)
            {
                clientCertificateChain.Add(issuers[i].Certificate);
            }

            // Inizialize the trasport channel.
            ServiceMessageContext m_messageContext = configuration.CreateMessageContext(true);
            ITransportChannel channel = SessionChannel.Create(
                configuration,
                endpoint.Description,
                endpoint.Configuration,
                clientCertificate,
                configuration.SecurityConfiguration.SendCertificateChain ? clientCertificateChain : null,
                m_messageContext);

            // Add the username and password.
            IUserIdentity identity = new UserIdentity();
            if (Username != null && Password != null)
            {
                identity = new UserIdentity(Username, Password);
            }

            session = new Session(channel, configuration, endpoint, null);
            session.ReturnDiagnostics = DiagnosticsMasks.All;
            try
            {
                session.Open(SessionName, identity);
            }
            catch (Exception ex)
            {
                session.Close();
                Console.WriteLine(ex.ToString());
            }

        }
        #endregion

        #region Browse a distant OPC Server
        /// <summary>
        /// Explore a distant OPC Server.
        /// </summary>
        /// <param name="nodeToBrowse">
        ///   The node to explore.
        /// 
        ///   Browse(new NodeId(Opc.Ua.Objects.Server));
        //    Browse(new NodeId(Opc.Ua.Objects.ObjectsFolder));
        //    Browse(new NodeId("ns=2;s=Channel1"));
        /// 
        /// </param>
        /// <returns> ReferenceDescriptionCollection </returns>
        public ReferenceDescriptionCollection Browse(NodeId nodeToBrowse)
        {
            if (session == null || session.Connected == false)
            {
                Console.WriteLine("Session not connected!");
                return null;
            }

            try
            {
                // Create a Browser object
                Browser browser = new Browser(session);

                // Set browse parameters
                browser.BrowseDirection = BrowseDirection.Both;
                browser.NodeClassMask = (int)NodeClass.Object | (int)NodeClass.Variable;
                browser.ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences;
                browser.IncludeSubtypes = true;

                // Call Browse service
                // Console.WriteLine("Browsing {0} node...", nodeToBrowse);
                ReferenceDescriptionCollection browseResults = browser.Browse(nodeToBrowse);


                /* Display the results
                Console.WriteLine("Browse returned {0} results:", browseResults.Count);
                foreach (ReferenceDescription result in browseResults)
                {
                    Console.WriteLine("     DisplayName = {0}, NodeClass = {1}, NodeID =  {2}", result.DisplayName.Text, result.NodeClass, result.NodeId);
                }
                */
                return browseResults;
            }
            catch (Exception ex)
            {
                // Log Error
                Console.WriteLine($"Browse Error : {ex.Message}.");
                return null;
            }
        }
        #endregion

        #region Read single or multiple Tags
        /// <summary>
        /// Read a Node
        /// </summary>
        /// <param name="node">Node to be readen</param>
        /// <param name="AttributeId">For example Attributes.NodeId, Attributes.Value, Attributes.ValueRank</param>
        public DataValueCollection Read(NodeId node, uint AttributeId)
        {
            ReadValueIdCollection nodesToRead = new ReadValueIdCollection()
                {
                    new ReadValueId() { NodeId = node , AttributeId = AttributeId }
                };

            DataValueCollection resultsValues = ReadMultiple(nodesToRead);

            /*
            // Display the results.
            foreach (DataValue result in resultsValues)
            {
                Console.WriteLine("Value = {0}, Type = {1},  StatusCode = {2}", result.Value, result.Value.GetType(), result.StatusCode);
            }
            */
            return resultsValues;
        }

        public DataValueCollection ReadMultiple(ReadValueIdCollection nodesToRead)
        {
            session.Read(
                null,
                0,
                TimestampsToReturn.Both,
                nodesToRead,
                out DataValueCollection resultsValues,
                out DiagnosticInfoCollection diagnosticInfos);

            return resultsValues;
        }
        #endregion

        #region Write single or multiple Tags
        public StatusCodeCollection Write<T>(NodeId node, T value)
        {

            WriteValueCollection nodesToWrite = new WriteValueCollection();
            WriteValue GenericWriteVal = new WriteValue();
            GenericWriteVal.NodeId = node;
            GenericWriteVal.AttributeId = Attributes.Value;
            GenericWriteVal.Value = new DataValue();
            GenericWriteVal.Value.Value = (T)value;
            nodesToWrite.Add(GenericWriteVal);

            return WriteMultiple(nodesToWrite);
        }

        public StatusCodeCollection WriteMultiple<T>(List<NodeId> node, List<T> value)
        {

            WriteValueCollection nodesToWrite = new WriteValueCollection();
            for (int i = 0; i < node.Count; i++)
            {
                WriteValue GenericWriteVal = new WriteValue();
                GenericWriteVal.NodeId = node.ElementAt(i);
                GenericWriteVal.AttributeId = Attributes.Value;
                GenericWriteVal.Value = new DataValue();
                GenericWriteVal.Value.Value = (T)value.ElementAt(i);
                nodesToWrite.Add(GenericWriteVal);
            }

            return WriteMultiple(nodesToWrite);


        }

        public StatusCodeCollection WriteMultiple(WriteValueCollection nodesToWrite)
        {
            StatusCodeCollection results = null;
            DiagnosticInfoCollection diagnosticInfos;

            // Call Write Service
            session.Write(null,
                            nodesToWrite,
                            out results,
                            out diagnosticInfos);

            /*foreach (StatusCode writeResult in results)
            {
                Console.WriteLine("     {0}", writeResult);
            }*/
            return results;
        }
        #endregion

        #region Subscribe to a distant tag

        public void SubscribeToDataChanges(int PublishingInterval,
            NodeId node,
            Action<MonitoredItem,
            MonitoredItemNotificationEventArgs> methodToCall)
        {
            session.DeleteSubscriptionsOnClose = true;

            // Event Delegate to launch the method
            MonitoredItemNotificationEventHandler eventHandler = null;
            eventHandler = (s, e) =>
            {
                methodToCall(s, e);
            };

            try
            {
                Subscription subscription = new Subscription(session.DefaultSubscription)
                {
                    DisplayName = "Console ReferenceClient Subscription",
                    PublishingEnabled = true,
                    PublishingInterval = PublishingInterval,
                    LifetimeCount = 0,
                    MinLifetimeInterval = 100,
                };

                session.AddSubscription(subscription);

                // Create the subscription on Server side
                subscription.Create();
                // Console.WriteLine("New Subscription created with SubscriptionId = {0}.", subscription.Id);

                // Create MonitoredItems for data changes (Reference Server)
                MonitoredItem intMonitoredItem = new MonitoredItem(subscription.DefaultItem);
                intMonitoredItem.StartNodeId = node;
                intMonitoredItem.AttributeId = Attributes.Value;
                intMonitoredItem.DisplayName = "Variable";
                intMonitoredItem.SamplingInterval = PublishingInterval;
                intMonitoredItem.QueueSize = 100;
                intMonitoredItem.DiscardOldest = false;
                intMonitoredItem.Notification += eventHandler;
                subscription.AddItem(intMonitoredItem);



                // Create the monitored items on Server side
                subscription.ApplyChanges();
                // Console.WriteLine("MonitoredItems created for SubscriptionId = {0}.", subscription.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Subscribe error: {0}", ex.Message);
            }
        }


        public static void OnMonitoredItemNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            try
            {
                // Log MonitoredItem Notification event
                MonitoredItemNotification notification = e.NotificationValue as MonitoredItemNotification;
                Console.WriteLine("Notification: {0} \"{1}\" and Value = {2}.", notification.Message.SequenceNumber, monitoredItem.ResolvedNodeId, notification.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine("OnMonitoredItemNotification error: {0}", ex.Message);
            }
        }
        #endregion


        public void CloseSession()
        {
            session.Close();
        }

        public override bool Equals(object? obj)
        {
            return obj is OPCUAHelper helper &&
                   UriToScan == helper.UriToScan;
        }



    }
}
