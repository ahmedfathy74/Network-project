using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTTPServer
{
    class Logger
    {
        //lw feh error yro7 yktbo f el file dh bdl myktbo f console 
        public static void LogException(Exception ex)
        {
            StreamWriter writer = new StreamWriter("log.txt", true);
            writer.WriteLine("DateTime: " + DateTime.Now);
            writer.WriteLine("Message: " + ex.Message);
            writer.WriteLine("-----------------------------");
            writer.Close();
            //error file 

        }
    }
}
