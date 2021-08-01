using System;
using System.IO;

namespace markify
{
    class Program
    {
        static void Main(string[] args)
        {
            //temporary
            string fileText = File.ReadAllText(@"C:\Users\Matthew\Source\Repos\IsoEngine\IsoEngine\EntityManager.cs");
            string output = Generator.ParseFile(fileText);

            Console.WriteLine(output);
        }
    }
}
