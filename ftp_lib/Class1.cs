using System.Text;

namespace ftp_lib
{
    public class Class1
    {

    }
}


using System;
using System.IO;
using System.Text;
using System.Net;

class FileOpener
{


    public static void Main(string[] args)
    {
        string path = @".\beans.txt";

        if (File.Exists(path))
        {
            using (FileStream fs = File.OpenWrite(path))
            {
                AddText(fs, "REEE\n");
            }
        }
        else
        {
            using (FileStream fs = File.Create(path))
            {
                AddText(fs, "Fuck you\n");
            }
        }

        using (FileStream fs = File.OpenRead(path))
        {
            byte[] buffer = new byte[1024];
            UTF8Encoding temp = new UTF8Encoding(true);
            while (fs.Read(buffer, 0, buffer.Length) > 0)
            {
                Console.WriteLine(temp.GetString(buffer));
            }
        }
    }

    private static void AddText(FileStream fs, string value)
    {
        byte[] info = new UTF8Encoding(true).GetBytes(value);
        fs.Seek(0, SeekOrigin.End);
        fs.Write(info, 0, info.Length);
    }
}

