using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;
using System.Net.Sockets;

namespace SecureFileTransfer.Network.TrivialEntityBasedProtocol
{
    public class TEBPProvider
    {
        public IConnection Connection { get; private set; }

        public bool IsShutDown { get; private set; }

        public bool UseDefaultEntities { get; set; }

        public delegate void EntityResponseDelegate(Response res);

        public delegate void ReceivedRequestHandler(Request req);
        public delegate void ReceivedNoticeHandler(Notice not);

        public event ReceivedRequestHandler ReceivedRequest;
        public event ReceivedNoticeHandler ReceivedNotice;

        private Dictionary<Entity, EntityResponseDelegate> Unanswered = new Dictionary<Entity, EntityResponseDelegate>();

        private Thread ListenerThread;

        public TEBPProvider(IConnection conn)
        {
            Connection = conn;
            IsShutDown = false;

            UseDefaultEntities = true;
        }

        static int CurrentIdentifier = 1;

        public static int GetNextIdentifier()
        {
            return CurrentIdentifier++;
        }

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

        public void Shutdown(bool shutDownConnection = true)
        {
            if (UseDefaultEntities)
            {
                Send(new DefaultEntities.DisconnectNotice());
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
                    // Socket unterbrochen
                    Shutdown();
                    return;
                }
                else
                {
                    throw ex;
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

        public void Send(Entity ent, EntityResponseDelegate responseDelegate = null)
        {
            if (IsShutDown)
                throw new ObjectDisposedException(this.ToString(), "The provider was shut down.");
            if (ent.RequiresAnswer)
            {
                if (responseDelegate == null)
                    throw new ArgumentException("You have to set a response delegate for this entity.");
                if (Unanswered.Any(val => val.Key.Identifier == ent.Identifier))
                    throw new NotSupportedException("Already waiting for a response for this entity.");

                Unanswered[ent] = responseDelegate;
            }

            string entityString = JsonConvert.SerializeObject(ent, Formatting.None, new JsonSerializerSettings() { });

            Connection.Send(ent.GetType().AssemblyQualifiedName);
            Connection.Send(entityString);
        }

    }
}
