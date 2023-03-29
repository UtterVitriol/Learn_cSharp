using System;
using System.IO;
using System.Text;
using System.Net;
using System.Security.Cryptography;
using FileTransfer.Types.FileTransferEnums;

namespace FileTransfer.FileLib
{
    public class FileLib
    {
        public int ReadFile(string path, byte[] buffer, int offset)
        {
            int bytesRead = 0;

            using (FileStream fs = File.OpenRead(path))
            {
                UTF8Encoding temp = new UTF8Encoding(true);
                while (fs.Read(buffer, 0, buffer.Length) > 0)
                {
                    Console.WriteLine(temp.GetString(buffer));
                }
            }


            return bytesRead;
        }

        public int WriteFile(string path, byte[] buffer, int offset)
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

        //private static void AddText(FileStream fs, string value)
        //{
        //    byte[] info = new UTF8Encoding(true).GetBytes(value);
        //    fs.Seek(0, SeekOrigin.End);
        //    fs.Write(info, 0, info.Length);
        //}
    }
}

namespace FileTransfer.Types
{
    namespace FileTransferStructs
    {
        public struct GetMessage
        {
            public MessageType Type;
            public string Source;
            public string Destination;
        }

        public struct PutMessage
        {
            public MessageType Type;
            public string Source;
            public string Destination;
        }
    }

    namespace FileTransferEnums
    {
        public enum MessageType
        {
            Unknown = 0,
            Get,
            Put
        }
    }
}

