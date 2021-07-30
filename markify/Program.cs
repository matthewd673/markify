using System;
using System.IO;

namespace markify
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //temporary
            string fileText = File.ReadAllText(@"C:\Users\Matthew\Source\Repos\IsoEngine\IsoEngine\Animation.cs");
            string output = Generator.ParseFile(fileText);

            Console.WriteLine(output);

            Console.ReadKey();
        }
    }
}
