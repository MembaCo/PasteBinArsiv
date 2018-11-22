using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace PasteBinArsivci
{
    internal class Program
    {
        internal const string PASTEBIN_URL = "https://pastebin.com/";
        private const string PASTEBIN_ARCHIVE = PASTEBIN_URL + "archive";
        private static string SAVE_LOCATION = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\PasteBin Arşiv\\";
        private static int TIMER = 100;

        internal static WebClient client = null;
        private static List<Paste> pastes = new List<Paste>();

        private static void InitClient()
        {
            if (client == null)
            {
                client = new WebClient();
                client.Proxy = null;
            }
        }

        private static string ParseArg(string[] args, string term)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-" + term)
                {
                    return args[i + 1];
                }
            }
            return null;
        }

        private static void Main(string[] args)
        {
            InitClient();
            Paste.categoryFilter = ParseArg(args, "c");
            Paste.contentFilter = ParseArg(args, "s");

            while (true)
            {
                try
                {
                    string dump = DumpRawArchive();
                    if (dump.Contains("You are scraping our site way too fast"))
                    {
                        Console.WriteLine("Sanırım Engellendiniz !!");
                    }
                    else
                    {
                        string tbl = TextHelper.StrBetweenStr(dump, "<table class=" + @"""" + "maintable" + @"""" + ">", "</table>");

                        string[] records = tbl.Split(new string[] { "<td>" }, StringSplitOptions.None);

                        foreach (string record in records)
                        {
                            try
                            {
                                new Paste(record);
                            }
                            catch
                            {
                                continue;
                            }
                        }
                        Console.WriteLine("Pasteler: " + Paste.pastes.Count);

                        foreach (Paste paste in Paste.pastes)
                        {
                            string buildPath = SAVE_LOCATION + paste.category + "\\";
                            string buildFile = buildPath + paste.pid + ".txt";
                            if (!Directory.Exists(buildPath))
                            {
                                Directory.CreateDirectory(buildPath);
                            }

                            if (!File.Exists(buildFile))
                            {
                                File.WriteAllText(buildFile, paste.content);
                            }
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Bilinmeyen Bir Hata Oluştu ;)");
                }

                for (int i = TIMER; i >= 0; i--)
                {
                    if (i != 0)
                    {
                        for (int ii = 0; ii < 3; ii++)
                        {
                            Console.Write(".");
                            Thread.Sleep(333);
                        }
                    }
                    else
                    {
                        Console.WriteLine(i);
                    }
                }
            }
        }

        private static string DumpRawArchive()
        {
            return client.DownloadString(PASTEBIN_ARCHIVE);
        }
    }
}