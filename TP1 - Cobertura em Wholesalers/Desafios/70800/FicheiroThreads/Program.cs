using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FicheiroThreads {
    internal class Program {
        // Create a new Mutex. The creating thread does not own the mutex.
        private static Mutex mut = new Mutex();
        private const int numIterations = 1;
        private const int numThreads = 3;
        private static string[] data = new string[3];

        static void Main() {
            // Create the threads that will use the protected resource.
            data[0] = Console.ReadLine();
            data[1] = Console.ReadLine();
            data[2] = Console.ReadLine();
            for (int i = 0; i < numThreads; i++) {
                Thread newThread = new Thread(ThreadProc);
                newThread.Name = String.Format("Thread{0}", i + 1);
                newThread.Start(data[i]);
            }

            // The main thread exits, but the application continues to
            // run until all foreground threads have exited.
            Console.ReadKey();
        }

        private static void ThreadProc(object data) {
            for (int i = 0; i < numIterations; i++) {
                UseResource(data);
            }
        }

        // This method represents a resource that must be synchronized
        // so that only one thread at a time can enter.
        private static void UseResource(object data) {
            // Wait until it is safe to enter.
            Console.WriteLine("{0} is requesting the mutex",
                Thread.CurrentThread.Name);
            mut.WaitOne();

            Console.WriteLine("{0} has entered the protected area",
                Thread.CurrentThread.Name);
            // Place code to access non-reentrant resources here.

            // Simulate some work.
            EscreveFicheiro((string)data);

            Console.WriteLine("{0} is leaving the protected area",
                Thread.CurrentThread.Name);

            // Release the Mutex.
            mut.ReleaseMutex();
            Console.WriteLine("{0} has released the mutex",
                Thread.CurrentThread.Name);
        }
        public static void EscreveFicheiro( string data) {
            
            var file = File.Open("teste.txt", FileMode.Append);
            byte[] buff = Encoding.UTF8.GetBytes(Thread.CurrentThread.Name + " -> " + data + "\n");
            file.Write(buff, 0, buff.Length);
            file.Close();
            Console.WriteLine("{0} has written to the file",
                Thread.CurrentThread.Name);


        }
    }
}
