using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKposter
{
    class Logger
    {
        public static void ToFile(string message)
        {
            string logFile = "logs.txt";

            if (!File.Exists(logFile))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(logFile))
                {
                    sw.WriteLine(message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(logFile))
                {
                    sw.WriteLine(message);
                }
            }
        }
    }
}
