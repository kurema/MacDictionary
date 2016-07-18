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
            MacDictionary.Dictionary dic = new MacDictionary.Dictionary("bin/Body.data", "bin/KeyText.data");
            var sr = dic.FindEntry("america");
        }
    }
}
