using System;
using System.IO;
using System.Text;
using System.Net;
using System.Security.Cryptography;
using FileTransfer.Types.FileTransferEnums;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using FileTransfer.Types.FileTransferStructs;
using FileTransfer.Types;

namespace FileTransfer.FileLib
{
    //TODO: Add Error checking.
    public class FileLib
    {

        public static long GetFileSize(string path)
        {
            if (File.Exists(path))
            {
                return new FileInfo(path).Length;
            }
            else
            {
                return -1;
            }
        }
        public static long ReadFile(string path, byte[] buffer, int offset)
        {
            int bytesRead = 0;

            if (File.Exists(path))
            {
                try
                {
                    using FileStream fs = File.OpenRead(path);
                    bytesRead = fs.Read(buffer, 0, buffer.Length);
                }
                catch (Exception ex) { throw new Exception(ex.Message); }
            }
            else
            {
                bytesRead = -1;
            }



            return bytesRead;
        }

        public static long WriteFile(string path, byte[] buffer, int offset)
        {
            int bytesWritten = 0;

            if (File.Exists(path))
            {
                using (FileStream fs = File.OpenWrite(path))
                {
                    fs.Write(buffer, offset, buffer.Length);
                }
            }
            else
            {
                using (FileStream fs = File.Create(path))
                {
                    fs.Write(buffer, offset, buffer.Length);
                }
            }

            return bytesWritten;
        }
               
    }
}

namespace FileTransfer.Types
{
    namespace FileTransferStructs
    {
        public interface FileTransferMessage
        {
            MessageType GetTypeCode();
        }

        [DataContract]
        public struct GetMessage : FileTransferMessage
        {


            [DataMember(Name = "message_type")]
            public MessageType Type;
            [DataMember(Name = "source")]
            public string Source;

            public GetMessage(MessageType type, string source)
            {
                Type = type;
                Source = source;
            }
            public MessageType GetTypeCode()
            {
                return MessageType.Get;
            }
        }

        [DataContract]
        public struct PutMessage : FileTransferMessage
        {
            [DataMember(Name = "type")]
            public MessageType Type;
            [DataMember(Name = "location")]
            public string Location;
            [DataMember(Name = "destination")]
            public string Destination;

            public PutMessage(MessageType type, string location, string destination)
            {
                Type = type;
                Location = location;
                Destination = destination;
            }

            public MessageType GetTypeCode()
            {
                return MessageType.Put;
            }
        }

        [DataContract]
        public struct PeerMessage : FileTransferMessage
        {
            [DataMember(Name = "message_type")]
            public MessageType Type;
            [DataMember(Name = "message")]
            public string Message;

            public PeerMessage(MessageType type, string message)
            {
                Type = type;
                byte[] temp = Encoding.UTF8.GetBytes(message);
                Message = Convert.ToBase64String(temp);
            }


            public MessageType GetTypeCode()
            {
                return Type;
            }

        }

        [DataContract]
        public struct MessageChunk
        {
            [DataMember(Name = "message_type")]
            public MessageType Type;
            [DataMember(Name = "chunk_number")]
            public int ChunkNumber;
            [DataMember(Name = "total_chunks")]
            public int TotalChunks;
            [DataMember(Name = "data")]
            public string Data;

            public MessageChunk(MessageType mt = 0, int chunkNum = 0, int totalChunks = 1, byte[]? data = null)
            {
                Type = mt;
                ChunkNumber = chunkNum;
                TotalChunks = totalChunks;
                Data = Convert.ToBase64String(data!);
            }
        }
    }

    namespace FileTransferEnums
    {
        public enum MessageType
        {
            Unknown = 0,
            Get,
            GetUpdate,
            Put,
            PutUpdate
        }

        public enum BaseSize
        {
            MessageChunk = 12
        }
    }

    public static class FileTransferTypes
    {
        public static Type GetMessageType(MessageType mt)
        {
            if (mt == MessageType.Get)
            {
                return typeof(GetMessage);
            }
            else if (mt == MessageType.Put)
            {
                return typeof(PutMessage);
            }
            else
            {
                throw new Exception("GetFileTrasferType invalid MessageType");
            }
        }
    }
}

namespace FileTransfer.Serializer
{
    public class MyJsonSerializer
    {

        public string Serialize(object obj)
        {
            using var ms = new MemoryStream();
            var ser = new DataContractJsonSerializer(obj.GetType());
            ser.WriteObject(ms, obj);
            ms.Position = 0;
            using var sr = new StreamReader(ms);
            string res = sr.ReadToEnd();
            return res;
        }

        public virtual T? Deserialize<T>(string msg)
        {
            using var ms = new MemoryStream(Encoding.Unicode.GetBytes(msg));
            var deserializer = new DataContractJsonSerializer(typeof(T));
            return (T)deserializer.ReadObject(ms)!;
        }

        public virtual object Deserialize(string msg, Type t)
        {
            using var ms = new MemoryStream(Encoding.Unicode.GetBytes(msg));
            var deserializer = new DataContractJsonSerializer(t);
            return deserializer.ReadObject(ms)!;
        }

        public MessageChunk[] SerializeMessage(FileTransferMessage message, int chunksz = 4096)
        {
            string msg = Serialize(message);
            byte[] bMsg = Encoding.UTF8.GetBytes(msg);
            int numMessages = bMsg.Length / chunksz + 1;

            MessageChunk[] chunks = new MessageChunk[numMessages];

            for (int i = 0; i < numMessages; i++)
            {
                byte[] part = bMsg.Skip(i * chunksz).Take(chunksz).ToArray();
                chunks[i] = new MessageChunk(message.GetTypeCode(), i + 1, numMessages, part);
            }

            return chunks;
        }


        public virtual FileTransferMessage DeserializeMessage(byte[] data, MessageType mt)
        {
            string msg = Encoding.UTF8.GetString(data);
            Type t = FileTransferTypes.GetMessageType(mt);
            return (FileTransferMessage)Deserialize(msg, t);
        }

    }
}
