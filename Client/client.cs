// A C# program for Client
using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace ClientApplication
{

    class Client
    {
        private static IPEndPoint? RemoteEndPoint { get; set; }
        private static IPAddress? RemoteAddress { get; set; }
        private static int Port { get; set; } = 0;
        private static Socket? Sender { get; set; }

        static void StartClient(string ipAddr, string port)
        {
            try
            {
                RemoteAddress = IPAddress.Parse(ipAddr);

                Port = int.Parse(port);

                RemoteEndPoint = new IPEndPoint(RemoteAddress, Port);

                // Creation TCP/IP Socket using
                // Socket Class Constructor
                Sender = new Socket(RemoteAddress.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);

                // Connect Socket to the remote
                // endpoint using method Connect()
                Sender.Connect(RemoteEndPoint);
            }
            // Manage of Socket's Exceptions
            catch (ArgumentNullException ane)
            {

                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }

            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.Message);
            }

            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }
        }

        // ExecuteClient() Method
        static void ExecuteClient()
        {

            try
            {
                // We print EndPoint information
                // that we are connected
                Console.WriteLine($"Socket connected to -> {Sender!.RemoteEndPoint!}");

                // Creation of message that
                // we will send to Server
                byte[] messageSent = Encoding.ASCII.GetBytes("Test Client<EOF>");
                int byteSent = Sender.Send(messageSent);

                // Data buffer
                byte[] messageReceived = new byte[1024];

                // We receive the message using
                // the method Receive(). This
                // method returns number of bytes
                // received, that we'll use to
                // convert them to string
                int byteRecv = Sender.Receive(messageReceived);
                Console.WriteLine("Message from Server -> {0}",
                    Encoding.ASCII.GetString(messageReceived,
                                                0, byteRecv));

                // Close Socket using
                // the method Close()
                Sender.Shutdown(SocketShutdown.Both);
                Sender.Close();
            }

            // Manage of Socket's Exceptions
            catch (ArgumentNullException ane)
            {

                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }

            catch (SocketException se)
            {

                Console.WriteLine("SocketException : {0}", se.ToString());
            }

            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

        }
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("usage: Client.exe IP PORT");
                return;
            }

            byte[] r = new byte[1024];
            r[1023] = (byte)'r';
            r.Resize();

            Console.WriteLine(r[1023]);

            //StartClient(args[0], args[1]);
            //ExecuteClient();
        }

    }


}

