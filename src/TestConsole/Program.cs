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
            var dc = MacDictionary.CompressedEntry.ByteArrayToText(bd.Entries[1].Decompress(), System.Text.Encoding.UTF8);
            var d = bd.GetEntryByAddress(0).GetEntryByAddress(0, System.Text.Encoding.UTF8);

            var kt = new MacDictionary.KeyTextData("bin/KeyText.data");
            var kc = kt.Entries[0].Decompress();
            var kte = MacDictionary.KeyText.Parse(kc);
            //foreach (var item1 in kte)
            //{
            //    foreach (var item2 in item1)
            //    {
            //        Console.Write(string.Join(",", item2.Head) + ",");
            //        Console.WriteLine(string.Join(",", item2.Texts));
            //    }
            //}
            //foreach (var item1 in dc)
            //{
            //    Console.WriteLine(item1);
            //}
        }
    }
}
