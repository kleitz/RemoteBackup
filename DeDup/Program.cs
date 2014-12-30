using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace DeDup
{
    class Program
    {
        static void Main(string[] args) {
            while (true) {
                Console.Write("Enter the path to hash\t");
                string path = Console.ReadLine();
                //string path = "G:/Pathfinder Roleplaying Game";
                DirectoryInfo directory;
                try {
                    directory = new DirectoryInfo(path);
                    if (!directory.Exists) {
                        throw new DirectoryNotFoundException();
                    }
                } catch (DirectoryNotFoundException) {
                    Console.WriteLine("Unable to locate directory, please try again");
                    continue;
                }
                Console.WriteLine(String.Format("Processing Directory: {0}", path));
                var progressIndicator = new Progress<string>(FileHashed);
                FileSystemHasher.HashDirectory(directory, progressIndicator).Wait();
                Console.WriteLine("Hashing Completed");
            }
        }

        static void FileHashed(string currentStatus) {
            Console.WriteLine(currentStatus);
        }
    }
}
