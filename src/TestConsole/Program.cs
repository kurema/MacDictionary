using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            MacDictionary.Dictionary dic = new MacDictionary.Dictionary("bin/Body.data", "bin/KeyText.data");
            sw.Start();
            var sr = dic.FindEntry("ApPle");
            sw.Stop();
            var ms= sw.ElapsedMilliseconds;
        }
    }
}
