using System;
using System.IO;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: DayakLang <file.dyk>");
            return 1;
        }

        var path = args[0];
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"File tidak ditemukan: {path}");
            return 2;
        }

        var src = File.ReadAllText(path);
        var lang = new RitoLang(Console.WriteLine);
        try
        {
            lang.Run(src);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error eksekusi: " + ex.Message);
            return 3;
        }
    }
}

