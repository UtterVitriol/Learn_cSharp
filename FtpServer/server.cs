using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using FileTransfer.FileLib;
using FileTransfer.Serializer;
using FileTransfer.Types;
using FileTransfer.Types.FileTransferEnums;
using FileTransfer.Types.FileTransferStructs;

namespace Server
{
    class Program
    {
        public static TcpListener? server;
        static void Main(string[] args)
        {
            server = new TcpListener(IPAddress.Parse("0.0.0.0"), 23669);

            Console.WriteLine($"Local: {server.LocalEndpoint}");

            server.Start(10);

            while (true)
            {
                TcpClient client = server!.AcceptTcpClient();
                Console.WriteLine($"Remote: {client.Client.RemoteEndPoint}");

                FileTransferMessage msg = RecvMsg(client);

                if (msg.GetTypeCode() == MessageType.Get)
                {
                    GetFile((GetMessage)msg, client);
                }
                else if (msg.GetTypeCode() == MessageType.Put)
                {
                    PutFile((PutMessage)msg, client);
                }
                else
                {
                    throw new Exception("Bad Message");
                }
            }

        }

        public static FileTransferMessage RecvMsg(TcpClient client)
        {
            byte[] bytes = new byte[4096];
            string data = "";
            bool done = false;

            NetworkStream stream = client.GetStream();

            int i = 0;
            int bytesProcessed = 0;

            string message = "";
            MessageType type = MessageType.Unknown;
            var ser = new MyJsonSerializer();

            while ((i = stream.Read(bytes, 0, 4096)) != 0)
            {
                data += System.Text.Encoding.UTF8.GetString(bytes);

                while (bytesProcessed < data.Length)
                {

                    Array.Clear(bytes, 0, bytes.Length);

                    int lBracket = data.IndexOf("{");
                    int rBracket = data.IndexOf("}");

                    if (rBracket == -1)
                    {
                        bytesProcessed += data.Length;
                        continue;
                    }

                    string d = new string(data.Take(rBracket + 1).ToArray());
                    bytesProcessed = 0; 
                    MessageChunk chunk = ser.Deserialize<MessageChunk>(d);
                    data = new string(data.Skip(rBracket + 1).ToArray());
                    message += Encoding.UTF8.GetString(Convert.FromBase64String(chunk.Data));

                    Console.WriteLine($"Chunk {chunk.ChunkNumber} - Total {chunk.TotalChunks}");

                    if (chunk.ChunkNumber == chunk.TotalChunks)
                    {
                        type = chunk.Type;
                        done = true;
                        break;
                    }
                    else if (chunk.ChunkNumber > chunk.TotalChunks)
                    {
                        throw new Exception("RecvMsg Error");
                    }



                }

                bytesProcessed = 0;

                if(done)
                {
                    break;
                }
            }

            byte[] bMessage = Encoding.UTF8.GetBytes(message);
            return ser.DeserializeMessage(bMessage, type);
        }

        static void SendMsg(FileTransferMessage message, TcpClient client)
        {
            var ser = new MyJsonSerializer();

            NetworkStream stream = client!.GetStream();
            MessageChunk[] chunks = ser.SerializeMessage(message);

            foreach (var chunk in chunks)
            {
                byte[] mBytes = Encoding.UTF8.GetBytes(ser.Serialize(chunk));
                stream.Write(mBytes, 0, mBytes.Length);
            }
        }


        public static int GetFile(GetMessage msg, TcpClient client)
        {
            Console.WriteLine($"{msg.Type} - {msg.Source}");

            long sz = FileLib.GetFileSize(msg.Source);

            byte[] buffer;
            if (sz > 0)
            {
                buffer = new byte[sz];
                FileLib.ReadFile(msg.Source, buffer, 0);
            }
            else
            {
                throw new Exception("GetFile error filesz");
            }


            msg = new GetMessage()
            {
                Type = MessageType.GetUpdate,
                Source = Encoding.UTF8.GetString(buffer),
            };

            SendMsg(msg, client);



            return 0;
        }

        public static int PutFile(PutMessage msg, TcpClient client)
        {



            //Console.WriteLine($"{msg.Type} - {msg.Location} - {msg.Destination}");


            FileLib.WriteFile(msg.Location, Encoding.UTF8.GetBytes(msg.Destination), 0);

            msg = new PutMessage()
            {
                Type = MessageType.PutUpdate,
                Location = "All",
                Destination = "Okay",

            };

            SendMsg(msg, client);

            return 0;
        }
    }
}