
using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;

class FileOpener
{


    public static void Main(string[] args)
    {
        string path = @".\beans.txt";

        if(File.Exists(path))
        {
            File.Delete(path);
        }

        using(FileStream fs = File.Create(path))
        {
            AddText(fs, "Fuck you");
        }
    }

    private static void AddText(FileStream fs, string value)
    {
        byte[] info = new UTF8Encoding(true).GetBytes(value);
        fs.Write(info, 0, info.Length);
    }
}
