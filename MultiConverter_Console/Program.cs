using System;

namespace MultiConverter_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Converter c = new Converter(args);
            c.Run();
            Console.Read();
        }
    }
}
