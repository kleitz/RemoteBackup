using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeDup
{
    class FileSystemHaher<T> where T : HashAlgorithm
    {
        public static async Task<Dictionary<byte[], List<String>>> HashDirectory(DirectoryInfo directory) {
            return await HashDirectory(directory, new Progress<string>(), new CancellationToken());
        }

        public static async Task<Dictionary<byte[], List<String>>> HashDirectory(DirectoryInfo directory, CancellationToken cancellation) {
            return await HashDirectory(directory, new Progress<string>(), cancellation);
        }

        public static async Task<Dictionary<byte[], List<String>>> HashDirectory(DirectoryInfo directory, IProgress<string> progress) {
            return await HashDirectory(directory, progress, new CancellationToken());
        }

        public static async Task<Dictionary<byte[], List<string>>> HashDirectory(DirectoryInfo directory, IProgress<string> progress, CancellationToken cancellation) {
            if (!directory.Exists) {
                throw new ArgumentException("Directory does not exist");
            }
            var files = directory.GetFiles();
            var result = new Dictionary<byte[], List<string>>();
            foreach(var file in files) {
                using (var fs = file.OpenRead()) {
                    var hashResult = await HashStreamAsync(fs);
                    progress.Report(String.Format("Hashed file {0} with hash: {1}", file.FullName, HashToString(hashResult)));
                    List<string> list = result.ContainsKey(hashResult) ? result[hashResult] : new List<string>();
                    list.Add(file.FullName);
                    result[hashResult] = list;
                }
            }
            return result;
        }

        private static async Task<byte[]> HashStreamAsync(Stream input) {
            const int READ_SIZE = 8192;
            byte[] buffer = new byte[READ_SIZE];
            T hash = (T)Activator.CreateInstance(typeof(T));
            int bytesRead = await input.ReadAsync(buffer, 0, READ_SIZE);
            //As soon as we read less than a full block, we have reached the end of the file.
            while (bytesRead == READ_SIZE) {
                //Transform the current block, updating the hash internal state, dumping output in buffer as 
                //it will be replaced immediately when the next block is read.
                hash.TransformBlock(buffer, 0, READ_SIZE, buffer, 0);
                bytesRead = await input.ReadAsync(buffer, 0, READ_SIZE);
            }
            hash.TransformFinalBlock(buffer, 0, buffer.Length);
            return hash.Hash;
        }

        private static string HashToString(byte[] hash) {
            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }
    }

    class FileSystemHasher : FileSystemHaher<SHA256Cng> { }
}
