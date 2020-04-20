using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using MultiConverterLib;

namespace MultiConverter_Console
{
    class Converter
    {
        private List<string> files_to_convert;
        public bool FixHelm { get; set; } = false;
        public bool AdtWater { get; set; } = true;
        public bool AdtModels { get; set; } = true;

        public Converter(string[] list)
        {
            files_to_convert = new List<string>();

            bool print_help = false;

            foreach (var s in list)
            {
                if (s[0] == '-')
                {
                    switch (s.ToLower())
                    {
                        case "-nomodel": AdtModels = false; break;
                        case "-nowater": AdtWater = false; break;
                        case "-fixhelm": FixHelm = true; break;
                        default: print_help = true; break;
                    }
                }
                if (Directory.Exists(s))
                {
                    foreach (string file in Directory.EnumerateFiles(s, "*.*", SearchOption.AllDirectories))
                    {
                        if (Utils.IsCorrectFile(file) && !files_to_convert.Contains(file))
                        {
                            files_to_convert.Add(file.ToLower());
                        }
                    }

                }
                else if (File.Exists(s) && Utils.IsCorrectFile(s) && !files_to_convert.Contains(s))
                {
                    files_to_convert.Add(s.ToLower());
                }
            }

            // print the help of there's no files
            print_help |= files_to_convert.Count == 0;

            if (print_help)
            {
                PrintHelp();
            }
        }

        private void PrintHelp()
        {
            Console.WriteLine("---");
            Console.WriteLine("Multi Converter Legion > Wotlk (console)");
            Console.WriteLine("Convert: m2/skin, wmo, adt and wdt");
            Console.WriteLine("");
            Console.WriteLine("How to use: execute with the models/folders you want to convert as arguments");
            Console.WriteLine("");
            Console.WriteLine("Other args:");
            Console.WriteLine("   '-nomodel': remove the models from the adts");
            Console.WriteLine("   '-nowater': remove the water from the adts");
            Console.WriteLine("   '-fixhelm': fix legion helm position for older character models");
            Console.WriteLine("   '-help': display this message");
            Console.WriteLine("---");
        }

        public void Run()
        {
            if (files_to_convert.Count == 0)
            {
                Console.WriteLine("No files to convert.");
                return;
            }

            Console.WriteLine("Converting {0} files...", files_to_convert.Count);

            int errors = 0;
            using (StreamWriter sw = new StreamWriter("error.log", false))
            {
                foreach (string s in files_to_convert)
                {
                    IConverter converter = null;

                    if (s.EndsWith("m2"))
                    {
                        converter = new M2Converter(s, FixHelm);
                    }
                    else if (s.EndsWith("adt"))
                    {
                        converter = new AdtConverter(s, AdtWater, AdtModels);
                    }
                    else if (s.EndsWith("wdt"))
                    {
                        converter = new WDTConverter(s);
                    }
                    else if (Regex.IsMatch(s, @".*_[0-9]{3}(_(lod[0-9]))?\.(wmo)"))
                    {
                        converter = new WMOGroupConverter(s, false);
                    }
                    else if (s.EndsWith("wmo"))
                    {
                        converter = new WMORootConverter(s);
                    }
                    else if (s.EndsWith("anim"))
                    {
                        converter = new AnimConverter(s);
                    }

                    try
                    {
                        // ? -> in case a file with a wrong extension/pattern was in the list
                        if (converter?.Fix() ?? false)
                        {
                            converter.Save();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error fixing: " + s);
                        sw.WriteLine("Error fixing: " + s);
                        sw.WriteLine(e.Message);
                        sw.WriteLine(e.StackTrace);
                        errors++;
                    }
                }
            }

            files_to_convert.Clear();

            if (errors > 0)
            {
                Console.WriteLine("{0} error(s) while converting files, please send the error.log file to the developper so this issue can be fixed", errors);
            }

            Console.WriteLine("Done.");
        }
    }
}
