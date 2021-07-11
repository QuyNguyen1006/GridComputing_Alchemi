using System;
using System.Reflection;
using System.Text;
using Alchemi.Core;
using Alchemi.Core.Owner;
using Alchemi.Core.Utility;
using log4net;

// Configure log4net using the .config file
[assembly: log4net.Config.XmlConfigurator(Watch=true)]

namespace Alchemi.Examples.PiCalculator
{
    class PiCalculatorMain
    {
		// Create a logger for use in this class
		private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static int NumThreads = 10;
        static int DigitsPerThread = 10;

		static int NumberOfDigits = NumThreads * DigitsPerThread;
        
        static DateTime StartTime;
        static GApplication App;

		static int th = 0;

        [STAThread]
        static void Main()
        {
            Console.WriteLine("[Pi Calculator Grid Application]\n--------------------------------\n");

            Console.WriteLine("Press <enter> to start ...");
            Console.ReadLine();

			Logger.LogHandler += new LogEventHandler(LogHandler);

            try
            {
				// get the number of digits from the user
				bool numberOfDigitsEntered = false;

            	while (!numberOfDigitsEntered)
            	{
            		try
            		{
            			NumberOfDigits = Int32.Parse(Utils.ValueFromConsole("Digits to calculate", "100"));

						if (NumberOfDigits > 0)
						{
							numberOfDigitsEntered = true;
						}
            		}
            		catch (Exception)
            		{
						Console.WriteLine("Invalid numeric value.");
            			numberOfDigitsEntered = false;
            		}
            	}

            	// get settings from user
                GConnection gc = GConnection.FromConsole("localhost", "9000", "user", "user");

                StartTiming();
                
                // create a new grid application
                App = new GApplication(gc);
                App.ApplicationName = "PI Calculator - Alchemi sample";

                // add the module containing PiCalcGridThread to the application manifest        
                App.Manifest.Add(new ModuleDependency(typeof(PiCalculator.PiCalcGridThread).Module));

				NumThreads = (Int32)Math.Floor((double)NumberOfDigits / DigitsPerThread);
				if (DigitsPerThread * NumThreads < NumberOfDigits)
				{
					NumThreads++;
				}

				// create and add the required number of grid threads
				for (int i = 0; i < NumThreads; i++)
				{
					int StartDigitNum = 1 + (i*DigitsPerThread);

					/// the number of digits for each thread
					/// Each thread will get DigitsPerThread digits except the last one
					/// which might get less
					int DigitsForThisThread = Math.Min(DigitsPerThread, NumberOfDigits - i * DigitsPerThread);
          
					Console.WriteLine(
						"starting a thread to calculate the digits of pi from {0} to {1}",
						StartDigitNum,
						StartDigitNum + DigitsForThisThread - 1);
          
					PiCalcGridThread thread = new PiCalcGridThread(
						StartDigitNum,
						DigitsForThisThread
						);

					App.Threads.Add(thread);
				}

                // subcribe to events
                App.ThreadFinish += new GThreadFinish(ThreadFinished);
                App.ApplicationFinish += new GApplicationFinish(ApplicationFinished); 
        
                // start the grid application
                App.Start();
				
				logger.Debug("PiCalc started.");
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: {0}", e.StackTrace);
            }

            Console.ReadLine();
        }

    	private static void LogHandler(object sender, LogEventArgs e)
    	{
			switch (e.Level)
			{
				case LogLevel.Debug:
					string message = e.Source  + ":" + e.Member + " - " + e.Message;
					logger.Debug(message,e.Exception);
					break;
				case LogLevel.Info:
					logger.Info(e.Message);
					break;
				case LogLevel.Error:
					logger.Error(e.Message,e.Exception);
					break;
				case LogLevel.Warn:
					logger.Warn(e.Message);
					break;
			}
    	}

    	static void StartTiming()
        {
            StartTime = DateTime.Now;
        }

        static void ThreadFinished(GThread thread)
        {
			th++;
            Console.WriteLine("grid thread # {0} finished executing", thread.Id);

//			if (th > 1)
//			{
//				Console.WriteLine("For testing aborting threads beyond th=5");
//				try
//				{
//					Console.WriteLine("Aborting thread th=" + th);
//					thread.Abort();
//					Console.WriteLine("DONE Aborting thread th=" + th);
//				}
//				catch (Exception e)
//				{
//					Console.WriteLine(e.ToString());
//				}
//			}
        }

        static void ApplicationFinished()
        {
            StringBuilder result = new StringBuilder();
            for (int i=0; i<App.Threads.Count; i++)
            {
                PiCalcGridThread pcgt = (PiCalcGridThread) App.Threads[i];
                result.Append(pcgt.Result);
            }
            
            Console.WriteLine(
                "===\nThe value of Pi to {0} digits is:\n3.{1}\n===\nTotal time taken = {2}\n===",
                NumberOfDigits,
                result,
                DateTime.Now - StartTime);

			//Console.WriteLine("Thread finished fired: " + th + " times");
			Console.WriteLine("Application Finished");
        }
    }
}
