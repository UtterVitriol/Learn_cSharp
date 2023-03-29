// A C# program for Client
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Runtime.Serialization.Json;

namespace ClientApplication
{

    class Client :IDisposable
    {
        private static IPEndPoint? RemoteEndPoint { get; set; }
        private static IPAddress? RemoteAddress { get; set; }
        private static int Port { get; set; } = 0;
        private static Socket? Sender { get; set; }

        public void StartClient(IPAddress ipAddr, int port)
        {
            try
            {
                RemoteAddress = ipAddr;

                Port = port;

                RemoteEndPoint = new IPEndPoint(RemoteAddress, Port);

                Sender = new Socket(RemoteAddress.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);

                Sender.Connect(RemoteEndPoint);
                Console.WriteLine($"Socket connected to -> {Sender.RemoteEndPoint}");

            }
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

        static int SendMsg(byte[] msg)
        {
            int byteSent = 0;

            try
            {
                byteSent = Sender!.Send(msg);
            }
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

            if (byteSent != msg.Length)
            {
                throw new Exception($"RecvMsg ToSend: {msg.Length} Sent: {byteSent}");
            }

            return byteSent;
        }

        static int RecvMsg(byte[] msg)
        {
            int byteRead = 0;
            try
            {
                byteRead = Sender!.Receive(msg);
            }
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

            if (byteRead == 0)
            {
                throw new Exception($"RecvMsg Read: {byteRead}");
            }


            return byteRead;
        }
             

        public void Close()
        {
            Sender!.Shutdown(SocketShutdown.Both);
            Sender.Close();
        }

        public void Dispose()
        {
            Console.WriteLine("Disposing!!!");
            Sender!.Shutdown(SocketShutdown.Both);
            Sender.Close();
        }

    }

    class Program
    {
        public static Client ?client;

        static int Main(string[] args)
        {


            var ipArg = new Argument<IPAddress>(
                name: "ip",
                description: "Remote Address");

            var portArg = new Argument<int>(
                name: "port",
                description: "Remote Port");

            var get = new Command("get", "Retrieve file from remote.")
            {
                new Argument<string>("Source", "Remote file to get."),
                new Argument<string>("Destination", "Local location to retrieve."),
            };

            get.Handler = CommandHandler.Create<IPAddress, int, string, string, IConsole>(GetFile);

            var put = new Command("put", "Send file to remote.")
            {
                new Argument<string>("Source", "Local file to put."),
                new Argument<string>("Destination", "Remote location to send."),
            };

            put.Handler = CommandHandler.Create<IPAddress, int, string, string, IConsole>(PutFile);

            var cmd = new RootCommand
            {
               ipArg,
               portArg,
               get,
               put
            };

            int ret = 0;

            using (client = new())
            {
                ret = cmd.Invoke(args);
            }


            
            return ret;
        }


        static void GetFile(IPAddress ip, int port, string source, string destination, IConsole console)
        {
            client!.StartClient(ip, port);
        }
        static void PutFile(IPAddress ip, int port, string source, string destination, IConsole console)
        {
            client!.StartClient(ip, port);


            
        }

    }

}

