using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace SecureFileTransfer.Network.TrivialEntityBasedProtocol
{
    /// <summary>
    /// This is the implementation of a trivial entity based protocol.
    /// 
    /// An entity consists of a name, an id and multiple parameters. While TEBP
    /// is active, the two clients will communicate using entities.
    /// 
    /// There are multiple types of entities: Request, which requires an
    /// answer from the receiver; Response, which answers a Request; and
    /// Notice, which is independent and does not require any acknowledgment
    /// from the receiver.
    /// </summary>
    public class TEBPProvider
    {
        public IConnection Connection { get; private set; }

        public bool IsShutDown { get; private set; }

        public bool UseDefaultEntities { get; set; }


        public const int RequestTimeoutMilliseconds = 10000;


        public delegate void EntityResponseDelegate(Response res);

        public delegate void ReceivedRequestHandler(Request req);
        public delegate void ReceivedNoticeHandler(Notice not);

        /// <summary>
        /// Is raised when a request was received.
        /// </summary>
        public event ReceivedRequestHandler ReceivedRequest;
        /// <summary>
        /// Is raised when a notice was received.
        /// </summary>
        public event ReceivedNoticeHandler ReceivedNotice;

        private Dictionary<Entity, EntityResponseDelegate> Unanswered = new Dictionary<Entity, EntityResponseDelegate>();

        private Thread ListenerThread;

        private ITEBPPlatformDependent PlatformDependentFunctions;

        /// <summary>
        /// Creates a new TEBP Provider
        /// </summary>
        /// <param name="conn">The connection the provider will use</param>
        /// <param name="platformDependent">Platform dependent functions</param>
        public TEBPProvider(IConnection conn, ITEBPPlatformDependent platformDependent)
        {
            PlatformDependentFunctions = platformDependent;

            Connection = conn;
            IsShutDown = false;

            UseDefaultEntities = true;
        }

        static int CurrentIdentifier = 1;

        /// <summary>
        /// Gets a unique identifier for a request
        /// </summary>
        /// <returns>An unique identifier</returns>
        public static int GetNextIdentifier()
        {
            return CurrentIdentifier++;
        }

        /// <summary>
        /// Claim the connection, enter TEBP mode.
        /// Do not use the connection directly after this call.
        /// </summary>
        public void Init()
        {
            ListenerThread = new Thread(() =>
            {
                while (!IsShutDown)
                {
                    ListenLoop();
                }
            });
            ListenerThread.Start();
        }

        private void ShutdownWithoutSendingDisconnect(bool shutDownConnection = true)
        {
            IsShutDown = true;

            if (shutDownConnection)
                Connection.Shutdown();
        }

        /// <summary>
        /// Ends the TEBP mode. If UseDefaultEntities is enabled, this method
        /// will send a disconnect notice.
        /// </summary>
        /// <param name="shutDownConnection">Sets whether the connection should automatically be shut down.</param>
        public void Shutdown(bool shutDownConnection = true)
        {
            if (UseDefaultEntities)
            {
                try
                {
                    Send(new DefaultEntities.DisconnectNotice());
                }
                catch (Exception)
                {
                    // Connection may already be closed.
                }
            }

            ShutdownWithoutSendingDisconnect(shutDownConnection);
        }

        private void ListenLoop()
        {
            string entityString, entityType;
            try
            {
                entityType = Connection.Receive();
                entityString = Connection.Receive();
            }
            catch (Exception ex)
            {
                if (ex is ObjectDisposedException || ex is SocketException)
                {
                    // Socket disconnected
                    Shutdown();
                    return;
                }
                else
                {
                    throw;
                }
            }

            Type type = Type.GetType(entityType);
            if (type == null)
            {
                Console.WriteLine("TEBPProvider received an invalid entity (ignored).");
                return;
            }

            Entity ent = (Entity)JsonConvert.DeserializeObject(entityString, type);
            ent.Provider = this;

            HandleReceiveEntity(ent);
        }

        private void HandleReceiveEntity(Entity ent)
        {
            if (ent is Response)
                HandleResponse(ent as Response);
            else if (ent is Request)
                HandleRequest(ent as Request);
            else if (ent is Notice)
                HandleNotice(ent as Notice);
            else
                Console.WriteLine("TEBPProvicer received an invalid entity (ignored).");

            if (ent.RequiresAnswer && !ent.Responded)
                throw new NotSupportedException("The entity " + ent.ToString() + " has not been responded to.");
        }

        private void HandleRequest(Request req)
        {
            if (ReceivedRequest != null)
                ReceivedRequest(req);
        }

        private void HandleNotice(Notice not)
        {
            if (ReceivedNotice != null)
                ReceivedNotice(not);

            if (not is DefaultEntities.DisconnectNotice && UseDefaultEntities)
            {
                ShutdownWithoutSendingDisconnect();
            }
        }

        private void HandleResponse(Response res)
        {
            if (!Unanswered.Any(val => val.Key.Identifier == res.Identifier))
            {
                Console.WriteLine("TEBPProvider received a response which it did not expect (ignored).");
                return;
            }

            KeyValuePair<Entity, EntityResponseDelegate> waiting = Unanswered.SingleOrDefault(val => val.Key.Identifier == res.Identifier);
            Unanswered.Remove(waiting.Key);

            waiting.Value(res);
        }

        private void CheckIfRequestTimedOut(Entity ent)
        {
            if (Unanswered.Any(val => val.Key.Identifier == ent.Identifier))
            {
                var responseDelegate = Unanswered[ent];
                Unanswered.Remove(ent);

                responseDelegate(new DefaultEntities.NoResponse(this));
            }
        }

        private void StartTimeoutCheckForEntity(Entity ent)
        {
            PlatformDependentFunctions.RunAfterDelay(
                () => CheckIfRequestTimedOut(ent), 
                RequestTimeoutMilliseconds
                );
        }

        /// <summary>
        /// Sends an entity to the TEBP receiver
        /// </summary>
        /// <param name="ent">The entity to send</param>
        /// <param name="responseDelegate">
        /// If the entity requires an answer (for example a request), this delegate is called when a response was received.
        /// In case the request timed out, the response delegate will contain a NoResponse object, so make sure to check
        /// for the correct response object type before using it.
        /// </param>
        public void Send(Entity ent, EntityResponseDelegate responseDelegate = null)
        {
            if (IsShutDown)
            {
                if (ent is DefaultEntities.DisconnectNotice)
                    return; // Disconnect is discarded if connection is already down.

                throw new ObjectDisposedException(this.ToString(), "The provider was shut down.");
            }
            if (!Connection.CanSend())
            {
                throw new ConnectionBrokenException();
            }
            if (ent.RequiresAnswer)
            {
                if (responseDelegate == null)
                    throw new ArgumentException("You have to set a response delegate for this entity.");
                if (Unanswered.Any(val => val.Key.Identifier == ent.Identifier))
                    throw new NotSupportedException("Already waiting for a response for this entity.");

                Unanswered[ent] = responseDelegate;

                StartTimeoutCheckForEntity(ent);
            }

            string entityString = JsonConvert.SerializeObject(ent, Formatting.None, new JsonSerializerSettings() { });

            Connection.Send(ent.GetType().AssemblyQualifiedName);
            Connection.Send(entityString);
        }

    }

    public class ConnectionBrokenException : Exception
    { }
}
