using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcClient {
    public static class Utils {
        public static string ReadPassword() {
            string password = string.Empty;

            ConsoleKeyInfo keyInfo;
            do {
                keyInfo = Console.ReadKey(true);

                if (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Backspace) {
                    password += keyInfo.KeyChar;
                } else if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0) {
                    password = password.Substring(0, password.Length - 1);
                }
            } while (keyInfo.Key != ConsoleKey.Enter);

            Console.WriteLine();

            return password;
        }

    }
}
