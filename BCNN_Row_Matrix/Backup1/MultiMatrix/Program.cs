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
        private static double[,] matrix1;
        private static double[,] matrix2;
        private static double[,] result;
        private static DateTime startTime;
        private static int rperth;//so dong cho moi thread
        [STAThread]
        static void Main(string[] args)
        {
            Random random = new Random();
            int n;
            Console.Write("Host[localhost]: ");
            string host = Console.ReadLine();
            if (host.Length < 1)
            {
                host = "localhost";
            }
            Console.Write("Size of matrix: ");
            n = Int32.Parse(Console.ReadLine());
            Console.Write("Rows per thread: ");
            rperth = Int32.Parse(Console.ReadLine());
            matrix1 = new double [n,n];
            matrix2 = new double [n,n];
            result = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrix1[i, j] = random.NextDouble();
                    matrix2[i, j] = random.NextDouble();
                }
            }
            int RowRemain = n;
            int RowCur = 0;
            while (RowRemain > 0)
            {
                int RowPerThread;
                if (RowRemain > rperth)
                {
                    RowPerThread = rperth;
                }
                else
                {
                    RowPerThread = RowRemain;
                }
                double[,] rows = new double[RowPerThread, n];
                for (int i = 0; i < RowPerThread; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        rows[i, j] = matrix1[RowCur, j];
                    }
                    RowCur++;
                }
                App.Threads.Add(new MultiRowsMatrix(RowCur - RowPerThread, rows, matrix2));
                RowRemain -= RowPerThread;
            }
            App.Connection = new GConnection(host,9000,"user","user");
            App.Manifest.Add(new ModuleDependency(typeof(MultiRowsMatrix).Module));
            App.ThreadFinish += new GThreadFinish(App_ThreadFinish);
            App.ApplicationFinish += new GApplicationFinish(App_ApplicationFinish);
            startTime = DateTime.Now;
            Console.WriteLine("Thread Started");
            App.Start();
            Console.ReadLine();
            
        }
        private static void App_ThreadFinish(GThread thread)
        {
            MultiRowsMatrix th = (MultiRowsMatrix)thread;
            Console.WriteLine("Dong {0}-{1} hoan thanh!",th.StartRowsid,th.rows.GetLength(0)+th.StartRowsid-1);
            for (int i = 0; i < th.rows.GetLength(0); i++)
            {
                for (int j = 0; j < th.rows.GetLength(1); j++)
                {
                    result[th.StartRowsid + i, j] = th.result[i, j];
                }
            }
        }
        private static void App_ApplicationFinish()
        {
            Console.WriteLine("Calculation finished after {0} seconds", DateTime.Now - startTime);
        }
    }
    [Serializable]
    class MultiRowsMatrix : GThread
    {
        public int StartRowsid;
        public double[,] rows;
        public double[,] matrix2;
        public double[,] result;
        public MultiRowsMatrix(int StartRowsid, double[,] rows, double[,] matrix2)
        {
            this.StartRowsid = StartRowsid;
            this.rows = rows;
            this.matrix2 = matrix2;
            result = new double[rows.GetLength(0), matrix2.GetLength(1)];
        }
        public override void Start()
        {
            for (int i = 0; i < rows.GetLength(0); i++)
            {
                for (int j = 0; j < rows.GetLength(1); j++)
                {
                    double sum = 0;
                    for (int k = 0; k < rows.GetLength(1); k++)
                    {
                        sum += rows[i, k] * matrix2[k, j];
                    }
                    result[i, j] = sum;
                }
            }
        }
    }
}
