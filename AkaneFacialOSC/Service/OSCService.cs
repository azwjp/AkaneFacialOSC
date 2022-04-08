using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Azw.FacialOsc.Tracking;
using Rug.Osc;

namespace Azw.FacialOsc.Service
{
    internal class OSCService : IDisposable
    {
        public const long LOCALHOST = 0x0100007F;
        public const int VRC_PORT = 9000;

        public const string ADDRESS = "/avatar/parameters/";
        public string addressRoot = ADDRESS;


        private OscSender sender = new OscSender(new IPAddress(LOCALHOST), 9535, VRC_PORT);

        public OSCService()
        {
            sender.Connect();
        }

        public void Send(FaceKey key, double value)
        {
            if (sender.State != OscSocketState.Connected) sender.Connect();
            sender.Send(new OscMessage(addressRoot + key, (float)value));
            sender.WaitForAllMessagesToComplete();
        }
        public void Send(OSCData data)
        {
            Send(data.key, data.value);
        }
        public void Send(IEnumerable<OSCData> data)
        {
            if (sender.State != OscSocketState.Connected) sender.Connect();

            foreach (var message in data.Select(d => new OscMessage(addressRoot + d.key, (float)d.value)))
            {
                sender.Send(message);
            }

            sender.WaitForAllMessagesToComplete();
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
