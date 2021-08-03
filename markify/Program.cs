using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace markify
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length < 2)
            {
                Console.WriteLine("Provide an input and output folder");
                return;
            }

            string inputDirectory = args[0];
            string outputDirectory = args[1];
            if (!outputDirectory.EndsWith("\\"))
                outputDirectory += "\\";

            List<string> inputFiles = GetAllFilesInDirectory(inputDirectory, "*.cs");

            int i = 0;
            foreach (string filepath in inputFiles)
            {
                i++;
                string text = File.ReadAllText(filepath);
                string output = Parser.ParseFile(text);

                string outputName = filepath.Remove(0, inputDirectory.Length + 1).Replace("\\", ".").Split('.')[0] + ".md"; //a mess
                File.WriteAllText(outputDirectory + outputName, output);

                Console.WriteLine("Completed {0} ({1}/{2})", outputName, i, inputFiles.Count);

            }
            //string fileText = File.ReadAllText(@"C:\Users\Matthew\Source\Repos\IsoEngine\IsoEngine\Animation.cs");
            //string output = Parser.ParseFile(fileText);

            //Console.WriteLine(output);
        }

        static List<string> GetAllFilesInDirectory(string directory, string searchPattern = "*")
        {
            if (directory.Contains("\\obj\\")) //skip obj directory
                return new List<string>();

            List<string> files = Directory.GetFiles(directory, searchPattern).ToList<string>();
            List<string> directories = Directory.GetDirectories(directory).ToList<string>();

            foreach (string d in directories)
            {
                files.AddRange(GetAllFilesInDirectory(d, searchPattern));
            }

            return files;

        }

    }
}
