using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Rug.Osc;

namespace AZW.FacialOSC.Service
{
    internal class OSCService : IDisposable
    {
        public const long LOCALHOST = 0x7F000001;
        public const int VRC_PORT = 9000;

        public const string ADDRESS = "/avatar/parameters/";
        public string addressRoot = ADDRESS;


        private OscSender sender = new OscSender(new IPAddress(LOCALHOST), VRC_PORT);

        public void Send(FaceKey key, double value)
        {
            sender.Send(new OscMessage(addressRoot + key, value));
        }

        public void ChangeAddress(string ipAddress, int port)
        {
            try
            {
                IPAddress.Parse(ipAddress);
                var newSender = new OscSender(IPAddress.Parse(ipAddress), port);
                var oldSender = sender;
                sender = newSender;
            }
            catch (FormatException ex)
            {
                // A wrong format of the IP address
            }
            catch (SocketException ex)
            {
                // When another process is using the port
            }

        }

        public void Dispose()
        {
            sender?.Dispose();
        }
    }
}
