using MultiConverter.Lib.Format;
using MultiConverter.Lib.Interface;
using MultiConverter.Lib.Shared;
using System.Collections.Generic;
using System;

namespace MultiConverter
{
    class Program
    {
        private static List<string> FilesToConvert = new List<string>();
        private static bool FixHelm { get; set; } = false;
        private static bool AdtWater { get; set; } = true;
        private static bool AdtModels { get; set; } = true;

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("| First time converting might take a while since it is downloading + loading the listfile..");
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.WriteLine("Loading listfile...");
            Listfile.Initialize();

            // var printHelp = false;

            // foreach (var arg in args)
            // {
            //     if (arg[0] == '-')
            //     {
            //         switch (arg.ToLower())
            //         {
            //             case "-nomodel": AdtModels = false; break;
            //             case "-nowater": AdtWater = false; break;
            //             case "-fixhelm": FixHelm = true; break;
            //             default: printHelp = true; break;
            //         }
            //     }
            // }

            // Print help if there are no files to convert.
            // printHelp |= FilesToConvert.Count == 0;
            // 
            // if (printHelp)
            //     PrintHelp();

            string m2 = "nzoth - Copy.m2";
            IConverter converter = new M2(m2, FixHelm);

            if (converter.Fix())
                converter.Save();
        }

        private static void PrintHelp()
        {
            Console.WriteLine("|------------------------------------------------------------------------------");
            Console.WriteLine("| MultiConverter BFA > Wotlk)");
            Console.WriteLine("| Convert: m2/skin, wmo, adt and wdt");
            Console.WriteLine("|");
            Console.WriteLine("| How to use: execute with the models/folders you want to convert as arguments");
            Console.WriteLine("|");
            Console.WriteLine("| Other args:");
            Console.WriteLine("|    '-nomodel': remove the models from the adts");
            Console.WriteLine("|    '-nowater': remove the water from the adts");
            Console.WriteLine("|    '-fixhelm': fix legion helm position for older character models");
            Console.WriteLine("|    '-help': display this message");
            Console.WriteLine("|------------------------------------------------------------------------------");
        }
    }
}
