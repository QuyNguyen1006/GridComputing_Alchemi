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
        public static List<List<int>> matrix = new List<List<int>>();

        [STAThread]
        static void Main(string[] args)
        {
            Random rd = new Random();
            Console.Write("Host[localhost]: ");
            string host = Console.ReadLine();
            if (host.Length < 1)
            {
                host = "localhost";
            }

            Console.Write("Nhập số hàng :");
            int m = int.Parse(Console.ReadLine());
            Console.Write("Nhập số cột :");
            int n = int.Parse(Console.ReadLine());
            Console.Write("Nhập số hàng của mỗi luồng :");
            int k = int.Parse(Console.ReadLine());

            for (int i = 0; i < m; i++)
            {
                List<int> pseudo = new List<int>();
                for (int j = 0; j < n; j++)
                {
                    pseudo.Add(rd.Next(1, 10));
                }
                matrix.Add(pseudo);
            }
            /*for (int i = 0; i < m; i++)
            {
                Console.Write("\n");
                for (int j = 0; j < n; j++)
                {
                    Console.Write(matrix[i][j]);
                }
            }*/

            for (int i = 0; i <= m / k + 1; i++)
            {
                if ((i + 1) * k <= m && i * k < m)
                {
                    App.Threads.Add(new CaculatorLCM(matrix, i * k, i * k + k - 1));
                }
                else if (i * k <= m)
                {
                    App.Threads.Add(new CaculatorLCM(matrix, i * k, m - 1));
                    Console.Write(i * k);
                }
            }
            App.Connection = new GConnection("localhost", 9000, "user", "user");
            App.Manifest.Add(new ModuleDependency(typeof(CaculatorLCM).Module));
            App.ThreadFinish += App_ThreadFinish1;
            App.ApplicationName = "BCNN";
            App.ApplicationFinish += new GApplicationFinish(App_ApplicationFinish);
            startTime = DateTime.Now;
            Console.WriteLine("Thread Started");
            App.Start();
            Console.ReadLine();
        }

        private static void App_ThreadFinish1(GThread thread)
        {
            CaculatorLCM s = thread as CaculatorLCM;
            Console.Write("Luong {0} ({1}:{2}): ", thread.Id, s.start, s.end);
            for (int i = 0; i < s.ans.Count; i++)
            {
                Console.Write(s.ans[i] + " ");
            }
            Console.Write("\n");
        }

        private static void App_ApplicationFinish()
        {
            Console.WriteLine("Calculation finished after {0} seconds", DateTime.Now - startTime);
        }
    }
    [Serializable]
    class CaculatorLCM : GThread
    {
        public int start, end;
        public List<List<int>> matrix;
        public List<int> ans = new List<int>();
        public CaculatorLCM(List<List<int>> matrix, int start, int end)
        {
            this.matrix = matrix;
            this.start = start;
            this.end = end;
        }
        public static int LCM(int a, int b)
        {
            int result = a * b;
            while (a != 0 && b != 0)
            {
                if (a > b)
                {
                    a %= b;
                }
                else
                {
                    b %= a;
                }
            }
            return a > 0 ? result / a : result / b;
        }
        public override void Start()
        {
            for (int i = start; i <= end; i++)
            {
                int bcnn = 1;
                for (int j = 0; j < matrix[0].Count; j++)
                {
                    bcnn = LCM(bcnn, matrix[i][j]);
                }
                ans.Add(bcnn);
            }
        }
    }
}
