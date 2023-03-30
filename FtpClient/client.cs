// A C# program for Client
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using FileTransfer.Types.FileTransferStructs;
using FileTransfer.Types.FileTransferEnums;
using FileTransfer.Serializer;
using FileTransfer.Types;
using System.Security.Cryptography;
using FileTransfer.FileLib;

namespace ClientApplication
{
        
    class Program
    {
        public static TcpClient? client;

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

            ret = cmd.Invoke(args);

            return ret;
        }

        public static FileTransferMessage RecvMsg()
        {
            byte[] bytes = new byte[1024];
            string data = "";

            NetworkStream stream = client!.GetStream();

            int i = 0;

            string message = "";
            MessageType type = MessageType.Unknown;
            var ser = new MyJsonSerializer();

            while ((i = stream.Read(bytes, i, bytes.Length)) != 0)
            {
                // Translate data bytes to a ASCII string.
                data += System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                int lBracket = data.IndexOf("{");
                int rBracket = data.IndexOf("}");

                if (lBracket != -1)
                {
                    Console.WriteLine("No lbracket");
                }
                else
                {
                    Console.WriteLine("lBracket found");
                }

                if (rBracket > 0)
                {
                    Console.WriteLine("rBracket found");

                }
                else
                {
                    continue;
                }


                MessageChunk chunk = ser.Deserialize<MessageChunk>(data);

                message += Encoding.UTF8.GetString(Convert.FromBase64String(chunk.Data));

                if (chunk.ChunkNumber == chunk.TotalChunks)
                {
                    Console.WriteLine(message);
                    type = chunk.Type;
                    break;
                }
                else
                {
                    throw new Exception("RecvMsg Error");
                }

            }


            byte[] bMessage = Encoding.UTF8.GetBytes(message);
            return ser.DeserializeMessage(bMessage, type);
        }

        static void SendMsg(FileTransferMessage message)
        {
            var ser = new MyJsonSerializer();

            NetworkStream stream = client!.GetStream();
            MessageChunk[] chunks = ser.SerializeMessage(message);


            foreach (var chunk in chunks)
            {
                byte[] mBytes = Encoding.UTF8.GetBytes(ser.Serialize(chunk));
                Console.WriteLine($"Chunk {chunk.ChunkNumber} - Total {chunk.TotalChunks}");
                stream.Write(mBytes, 0, mBytes.Length);
            }
        }

        static void GetFile(IPAddress ip, int port, string source, string destination, IConsole console)
        {
            using (client = new(ip.ToString(), port))
            {

                GetMessage request = new GetMessage()
                {
                    Type = MessageType.Get,
                    Source = source,
                };

                SendMsg(request);

                request = (GetMessage)RecvMsg();

                Console.WriteLine($"{request.Type} - {request.Source}");


                FileLib.WriteFile(destination, Encoding.UTF8.GetBytes(request.Source), 0);


            }
            
        }
        static void PutFile(IPAddress ip, int port, string source, string destination, IConsole console)
        {
            using (client = new(ip.ToString(), port))
            {

                long sz = FileLib.GetFileSize(source);
                long read = 0;
                byte[] buffer;
                if (sz > 0)
                {
                    buffer = new byte[sz];
                    read = FileLib.ReadFile(source, buffer, 0);
                }
                else
                {
                    throw new Exception("GetFile error filesz");
                }

                if (read < 1)
                {
                    throw new Exception("Readfile Error");
                }


                PutMessage msg = new PutMessage()
                {
                    Type = MessageType.Put,
                    Location = destination,
                    Destination = Encoding.UTF8.GetString(buffer),
                };

                SendMsg(msg);

                msg = (PutMessage)RecvMsg();

                Console.WriteLine($"{msg.Type} - {msg.Location}" +
                    $" - {msg.Destination}");
            }



        }

    }

}

