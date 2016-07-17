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
            var bd = new MacDictionary.BodyData("bin/Body.data");
            var dc = MacDictionary.CompressedEntry.ByteArrayToText( bd.Entries[0].Decompress(),System.Text.Encoding.UTF8);

            var kt = new MacDictionary.KeyTextData("bin/KeyText.data");
            var kc = kt.Entries[0].Decompress();
            var kte = MacDictionary.KeyText.Parse(kc);
        }
    }
}
