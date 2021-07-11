using System;
using System.Collections.Generic;
using System.Text;
using Alchemi.Core;
using Alchemi.Core.Owner;

namespace MultiMatrix
{
    class MultiMatrix : GApplication
    {
        public static GApplication App = new GApplication();
        private static DateTime startTime;
        [STAThread]
        static void Main(string[] args)
        {
            Console.Write("Host[localhost]: ");
            string host = Console.ReadLine();
            if (host.Length < 1)
            {
                host = "localhost";
            }
            Console.Write("Nhap n : ");
            int n = int.Parse(Console.ReadLine());
            Console.Write("Khoang chia cho cho moi luong : ");
            int size = int.Parse(Console.ReadLine());
            for (int i = 0; i < n / size + 1; i++)
            {
                if ((i + 1) * size <= n && i * size < n)
                {
                    App.Threads.Add(new SquareNumber(i * size + 1, i * size + size));
                }
                else if (i * size < n)
                {
                    App.Threads.Add(new SquareNumber(i * size + 1, n));
                }
            }
            App.Connection = new GConnection(host, 9000, "user", "user");
            App.Manifest.Add(new ModuleDependency(typeof(SquareNumber).Module));
            App.ThreadFinish += App_ThreadFinish1;
            App.ApplicationName = "Số chính phương";
            App.ApplicationFinish += new GApplicationFinish(App_ApplicationFinish);
            startTime = DateTime.Now;
            Console.WriteLine("Thread Started");
            App.Start();
            Console.ReadLine();

        }

        private static void App_ThreadFinish1(GThread thread)
        {
            SquareNumber s = thread as SquareNumber;
            Console.Write("Luong {0} ({1}:{2}): ", thread.Id, s.start, s.end);
            for (int i = 0; i < s.L.Count; i++)
            {
                Console.Write(s.L[i] + " ");
            }
            Console.WriteLine("\n");
        }

        private static void App_ApplicationFinish()
        {
            Console.WriteLine("Calculation finished after {0} seconds", DateTime.Now - startTime);
        }
    }
    [Serializable]
    class SquareNumber : GThread
    {
        public int start, end;
        public List<int> L = new List<int>();
        public SquareNumber(int start, int end)
        {
            this.start = start;
            this.end = end;
        }
        public override void Start()
        {
            for (int i = (int)Math.Ceiling(Math.Sqrt(start)); i <= (int)Math.Floor(Math.Sqrt(end)); i++)
            {
                L.Add(i * i);
            }
        }
    }
}
