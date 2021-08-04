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
            if (!outputDirectory.EndsWith("\\") && !outputDirectory.StartsWith("--"))
                outputDirectory += "\\";

            List<string> inputFiles = new List<string>();
            if (File.Exists(inputDirectory)) //its not a directory, its a file
                inputFiles.Add(inputDirectory);
            else
                inputFiles = GetAllFilesInDirectory(inputDirectory, "*.cs");

            int i = 0;
            Parser parser = new Parser(new MarkdownGenerator());
            foreach (string filepath in inputFiles)
            {
                i++;
                string text = File.ReadAllText(filepath);
                string output = parser.ParseFile(text);

                if (outputDirectory.Equals("--print")) //special flag to print to console
                {
                    Console.WriteLine(output);
                    continue;
                }

                string outputName = filepath.Remove(0, inputDirectory.Length).Split('.')[0] + ".md"; //a mess
                outputName = outputName.Replace('\\', '.');
                File.WriteAllText(outputDirectory + outputName, output);

                Console.WriteLine("Completed {0} ({1}/{2})", outputName, i, inputFiles.Count);
            }
        }

        /// <summary>
        /// Find the filepaths of all files located in the given directory, and any subdirectories.
        /// </summary>
        /// <param name="directory">The directory to search in.</param>
        /// <param name="searchPattern">The search pattern to apply.</param>
        /// <returns>A list of all filepaths matching the search pattern through all subdirectories.</returns>
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
