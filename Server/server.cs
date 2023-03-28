using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Server
{

    class Program
    {
        public static IPAddress? IpAddr { get; set; }
        public static IPEndPoint? LocalEndPoint { get; set; }
        public static Socket? Listener { get; set; }

        public static void StartSever()
        {
            try
            {
                //IpAddr = System.Net.IPAddress.Parse("127.0.0.1");
                IpAddr = GetLocalIPv4(NetworkInterfaceType.Ethernet);

                if (IpAddr == null)
                {
                    throw new Exception("Cannot Aqcire IPv4 Address");
                }

                LocalEndPoint = new IPEndPoint(IpAddr, 23669);
                Listener = new Socket(IpAddr.AddressFamily,
                             SocketType.Stream, ProtocolType.Tcp);

                Console.WriteLine("IP:" + LocalEndPoint.Address + " PORT:" + LocalEndPoint.Port);

                Listener.Bind(LocalEndPoint);
                Listener.Listen(10);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        public static void CloseServer()
        {
            Listener?.Close();
        }

        public static System.Net.IPAddress ?GetLocalIPv4(NetworkInterfaceType _type)
        {
            System.Net.IPAddress ?ipAddr = null;
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            if(ip.Address.ToString().EndsWith(".1"))
                            {
                                continue;
                            }
                            else
                            {
                                ipAddr = ip.Address;
                                break;
                            }
                        }
                    }
                }
            }
            return ipAddr;
        }


        public static void ExecuteServer()
        {
            if(Listener == null)
            {
                return;
            }

            try
            {

                while (true)
                {

                    Console.WriteLine("Waiting connection ... ");

                    Socket clientSocket = Listener.Accept();

                    // Data buffer
                    byte[] bytes = new Byte[1024];
                    string? data = null;

                    while (true)
                    {

                        int numByte = clientSocket.Receive(bytes);

                        data += Encoding.ASCII.GetString(bytes,
                                                   0, numByte);

                        if (data.IndexOf("<EOF>") > -1)
                            break;
                    }

                    Console.WriteLine("Text received -> {0} ", data);
                    byte[] message = Encoding.ASCII.GetBytes("Test Server");

                    clientSocket.Send(message);

                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // Main Method
        static void Main(string[] args)
        {
            StartSever();
            ExecuteServer();
            CloseServer();
        }
    }
}