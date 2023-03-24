using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aula_2___Sockets___Server {
    public static class ChecksumUtil {
        public static string GetChecksum(HashingAlgoTypes hashingAlgoType, string filename) {
            using (var hasher = System.Security.Cryptography.HashAlgorithm.Create(hashingAlgoType.ToString())) {
                using (var stream = System.IO.File.OpenRead(filename)) {
                    var hash = hasher.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "");
                }
            }
        }

    }
    public enum HashingAlgoTypes {
        MD5,
        SHA1,
        SHA256,
        SHA384,
        SHA512
    }


    public static class CsvParser {

        public static List<List<string>> CsvToList(string path, char separator = ',') {
            if (!path.EndsWith(".csv")) throw new Exception("Error: Invalid file type. Please provide a .csv file");
            List<List<string>> rows = new List<List<string>>();
            var buff = File.ReadLines(path);

            foreach (var line in buff) {
                List<string> col = new List<string>();
                var strings = line.Split(separator);
                foreach (var s in strings) {
                    col.Add(s);
                }
                rows.Add(col);
            }
            return rows;
        }

    }


}
