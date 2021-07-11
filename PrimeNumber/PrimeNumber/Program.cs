using System;
using System.Collections.Generic;
using System.Text;
using Alchemi.Core;
using Alchemi.Core.Owner;


namespace PrimeNumber
{
    class PrimeNumber : GApplication
    {
        public static GApplication App = new GApplication();
        private static int[] matrix;//Mang chua so can kiem tra
        private static int NumPerThread;//So luong so trong 1 thread
        private static DateTime start;
     
        [STAThread]
        static void Main(string[] args)
        {
            int n;
            string host;
            Random random = new Random();
            Console.Write("Host[localhost]:");
            host = Console.ReadLine();
            if (host.Length < 1)
            {
                host = "localhost";
            }
            Console.Write("Dua vao so luong so can kiem tra:");
            n = Int32.Parse(Console.ReadLine());
            Console.Write("Dua vao so luong so cho 1 thread:");
            NumPerThread = Int32.Parse(Console.ReadLine());
            matrix = new int[n];
            for (int i = 0; i < n; i++)
            {
                matrix[i] = random.Next();
            }
            int NumRemain = n;
            int NumCur = 0;
            while (NumRemain > 0)
            {
                int NumberOfThread;
                if (NumRemain > NumPerThread)
                {
                    NumberOfThread = NumPerThread;
                }
                else
                {
                    NumberOfThread = NumRemain;
                }
                int[] Nums = new int[NumberOfThread];
                for (int i = 0; i < NumberOfThread; i++)
                {
                    Nums[i] = matrix[NumCur];
                    NumCur++;
                }
                App.Threads.Add(new PrimeNumberCheck(NumCur-NumPerThread,Nums));
                NumRemain -= NumberOfThread;
            }
            App.Connection = new GConnection("localhost",9000,"user","user");
            App.Manifest.Add(new ModuleDependency(typeof(PrimeNumberCheck).Module));
            App.ThreadFinish += new GThreadFinish(App_ThreadFinish);
            App.ApplicationFinish += new GApplicationFinish(App_ApplicationFinish);
            start = DateTime.Now;
            Console.WriteLine("Thread started!");
            App.Start();
            Console.ReadLine();

        }

        private static void App_ThreadFinish(GThread thread)
        {
            PrimeNumberCheck pnc = (PrimeNumberCheck)thread;
            Console.WriteLine("So{0}-{1} hoan thanh",pnc.StartNums,pnc.StartNums+pnc.Primes.GetLength(0)-1);
        }

        private static void App_ApplicationFinish()
        {
            Console.WriteLine("Hoan thanh sau {0} seconds.",DateTime.Now-start);
        }
    }
    [Serializable]
    class PrimeNumberCheck : GThread
    {
        public int StartNums;
        public int[] Primes;
        public int Factors;
        public PrimeNumberCheck(int StartNums, int[] Primes)
        {
            this.StartNums = StartNums;
            this.Primes = Primes;
        }
        public override void Start()
        {
            for (int i = 0; i < Primes.GetLength(0); i++)
            { 
                Factors=0;
                for (int j=1;j<=Primes[i];j++)
                {
                    if (Primes[i]%j==0)
                    {
                        Factors++;
                    }
                }
            }
        }
    }
}
