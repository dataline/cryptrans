using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureFileTransfer.Windows.TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Connecting.");

            Network.ClientConnection conn = Network.ClientConnection.ConnectTo("192.168.0.139", 23956);

            Console.WriteLine("Connection successful.");
        }
    }
}
