using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

#region IronPython related imports 
using IronPython.Hosting;
using IronPython.Compiler;
#endregion 

namespace MemeTracker {

    class Program {

        /// <summary>
        /// Cache folder for RSS Bandit which will be the input to the memetracker script
        /// </summary>
        private static string RssBanditCacheFolder = @"C:\Documents and Settings\dareo\Local Settings\Application Data\RssBandit\Cache";

        /// <summary>
        /// Instance of the IronPython scripting engine 
        /// </summary>
        private PythonEngine engine;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Program() {
            engine = new PythonEngine();
        }


        /// <summary>
        /// Runs the meme tracker script
        /// </summary>
        public void Run() { 
            string script = File.ReadAllText("memetracker.py");
            System.IO.FileStream fs = new System.IO.FileStream("scripting-log.txt",
              System.IO.FileMode.Create);
            engine.SetStandardOutput(fs);
            engine.SetStandardError(fs);
            engine.Execute(script);
            engine.Shutdown();
        }


        static void Main(string[] args) {

            Program memetracker = new Program();
            memetracker.Run(); 

            Console.ReadLine(); 
        }
    }
}
