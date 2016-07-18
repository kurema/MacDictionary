using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.IO;

namespace MacDictionary
{
    public class Dictionary
    {
        public BodyData BodyData;
        public KeyTextData KeyTextData;

        public Dictionary(string pathBodyData,string pathKeyTextData)
        {
            BodyData = new BodyData(pathBodyData);
            KeyTextData = new KeyTextData(pathKeyTextData);
        }

        public string[] FindEntry(string word,SearchType st=SearchType.StartWith)
        {
            var searchResult = KeyTextData.FindEntry(word, st);
            var result = new List<string>();

            var CheckedAddress = new List<int[]>();

            foreach (var entry in searchResult)
            {
                if (CheckedAddress.Exists((a)=> { return a[0] == entry.CompressedAddress && a[1] == entry.TextAddress; })) continue;
                result.Add(BodyData.GetEntryByAddress(entry.CompressedAddress + BodyData.AddressOffset )?.GetEntryByAddress(entry.TextAddress, System.Text.Encoding.UTF8));
                CheckedAddress.Add(new int[] { entry.CompressedAddress, entry.TextAddress });
            }
            return result.ToArray();
        }
    }

    public class BodyData
    {
        public CompressedEntry[] Entries { get; private set; }
        public long[] Address { get; private set; }

        public BodyData(string path)
        {
            using (var Stream = new FileStream(path, FileMode.Open))
            {
                byte[] bytes = new byte[4];

                Stream.Seek(0x40, SeekOrigin.Begin);
                Stream.Read(bytes, 0, 4);
                int length = Functions.UnpackInt(bytes);
                var entries = new List<CompressedEntry>();
                var address = new List<long>();

                Stream.Seek(0x60, SeekOrigin.Begin);
                int i = 0;
                while (true)
                {
                    int compdLen;
                    address.Add(Stream.Position);
                    entries.Add(GetEntry(Stream,out compdLen));

                    i += 12 + compdLen;
                    if (i > length) { break; }
                }
                Address = address.ToArray();
                Entries = entries.ToArray();
            }
        }

        public CompressedEntry GetEntryByAddress(long address)
        {
            //int i = 0;
            //foreach (var addr in Address)
            //{
            //    if (addr>=address)
            //    {
            //        return Entries[i];
            //    }
            //    i++;
            //}
            //return null;
            if (Address.Contains(address))
            {
                return Entries[Array.IndexOf(Address, address)];
            }
            return null;
        }

        public static CompressedEntry GetEntry(string path, long Address)
        {
            using (var Stream = new FileStream(path, FileMode.Open))
            {
                Stream.Seek(Address, SeekOrigin.Begin);
                int dummy;
                return GetEntry(Stream, out dummy);
            }
        }

        public static long AddressOffset { get { return 0x40; } }

        private static CompressedEntry GetEntry(Stream Stream,out int compdLen)
        {
            byte[] bytes = new byte[4];

            Stream.Read(bytes, 0, 4);
            int dataLen = Functions.UnpackInt(bytes);
            Stream.Read(bytes, 0, 4);
            compdLen = Functions.UnpackInt(bytes);
            Stream.Read(bytes, 0, 4);
            int rawLen = Functions.UnpackInt(bytes);

            Stream.Seek(2, SeekOrigin.Current);
            var data = new byte[dataLen - 8 - 2];
            Stream.Read(data, 0, dataLen - 8 - 2);
            return (new CompressedEntry(data, dataLen, rawLen));
        }
    }

    public enum SearchType
    {
        StartWith,EndWith,Contains,Equal
    }

    public class KeyTextData
    {
        public CompressedEntry[] Entries { get; private set; }


        public KeyText[] FindEntry(string word,SearchType st=SearchType.StartWith)
        {
            var result = new List<KeyText>();
            foreach(var entry in Entries)
            {
                var str1 = KeyText.Parse(entry.Decompress());
                foreach(var str2 in str1)
                {
                    foreach(var str3 in str2)
                    {
                        bool check = false;
                        foreach (var text in str3.Texts) {
                            switch (st)
                            {
                                case SearchType.Contains:check = check || text.Contains(word);break;
                                case SearchType.StartWith: check = check || text.StartsWith(word); break;
                                case SearchType.EndWith: check = check || text.EndsWith(word); break;
                                case SearchType.Equal: check = check || text==word; break;
                            }
                        }
                        if (check) result.Add(str3);
                    }
                }
            }
            return result.ToArray();
        }

        public KeyTextData(string path)
        {
            using (var Stream = new FileStream(path, FileMode.Open))
            {
                Stream.Seek(0x40, SeekOrigin.Begin);
                byte[] bytes = new byte[4];
                Stream.Read(bytes, 0, 4);
                int length = Functions.UnpackInt(bytes);
                Stream.Read(bytes, 0, 4);
                int count = Functions.UnpackInt(bytes);

                var entries = new List<CompressedEntry>();
                int i = 0;
                while (true)
                {
                    Stream.Read(bytes, 0, 4);
                    int dataLen = Functions.UnpackInt(bytes);
                    Stream.Read(bytes, 0, 4);
                    int rawLen = Functions.UnpackInt(bytes);

                    Stream.Seek(2, SeekOrigin.Current);
                    var data = new byte[dataLen - 4 - 2];
                    Stream.Read(data, 0, dataLen - 4 - 2);
                    entries.Add(new CompressedEntry(data, dataLen, rawLen));

                    Stream.Seek(8 - (Stream.Position + 7) % 8 - 1, SeekOrigin.Current);
                    while (Stream.ReadByte() == 0)
                    {
                        Stream.Seek(7, SeekOrigin.Current);
                        if (Stream.Position + 8 >= Stream.Length)
                        {
                            break;
                        }
                    }
                    if (Stream.Position + 8 >= Stream.Length)
                    {
                        break;
                    }
                    Stream.Seek(-1, SeekOrigin.Current);

                    i += 4 + dataLen;
                    if (i >= length)
                    {
                        break;
                    }
                }
                Entries = entries.ToArray();
            }
        }
    }

    public class KeyText
    {
        public string[] Texts;
        public int TextAddress;
        public int CompressedAddress;

        public KeyText(string[] strs, byte[] head)
        {
            this.Texts = strs;
        }

        public KeyText() { }

        public static KeyText[][] Parse(byte[][] datas)
        {
            var result = new KeyText[datas.GetLength(0)][];
            for (int i = 0; i < datas.GetLength(0); i++)
            {
                result[i] = Parse(datas[i]);
            }
            return result;
        }

        public static KeyText[] Parse(byte[] data)
        {
            var ms = new MemoryStream(data);
            ms.Seek(4, SeekOrigin.Begin);
            var strs = new List<KeyText>();
            var bytes = Functions.LoadBytesShortArray(ms);

            foreach(var item in bytes)
            {
                var nms = new MemoryStream(item);

                int head1, head2;
                {
                    var bytesInt = new byte[4];
                    nms.Read(bytesInt, 0, 4);
                    head1 = Functions.UnpackInt(bytesInt);
                }
                {
                    var bytesInt = new byte[4];
                    nms.Read(bytesInt, 0, 4);
                    head2 = Functions.UnpackInt(bytesInt);
                }
                nms.Seek(-8, SeekOrigin.Current);

                var head = new byte[10];
                nms.Read(head, 0, 10);
                KeyText str = new KeyText() { TextAddress=head1,CompressedAddress=head2};
                var texts = new List<string>();
                while (true)
                {
                    string temp;
                    if ((temp = Functions.LoadStringShort(nms, System.Text.Encoding.Unicode)) == null) break;
                    texts.Add(temp);
                    Functions.SeekZeros(nms, 2);
                    if (nms.Position >= nms.Length) { break; }
                }
                {
                    str.Texts = texts.ToArray();
                    strs.Add(str);
                }
            }

            return strs.ToArray();
        }

        public static byte[] SeekNonTextArea(Stream sr,int count=10)
        {
            byte[] result = new byte[0];

            var tempByte = new byte[2];
            sr.Read(tempByte, 0, 2);

            if (tempByte[0] != 0 || tempByte[1] != 0)
            {
                sr.Seek(-2, SeekOrigin.Current);
                result = new byte[count];
                sr.Read(result, 0, count);
            }
            else
            {
                sr.Seek(-2, SeekOrigin.Current);
            }
            Functions.SeekZeros(sr, 2);
            return result;
        }
    }


    public class CompressedEntry
    {
        private byte[] Content;
        private int RawLength;
        private int CompressedLength;

        public CompressedEntry(byte[] content, int compLength, int rawLength)
        {
            this.Content = content;
            this.CompressedLength = compLength;
            this.RawLength = rawLength;
        }

        public static string[] ByteArrayToText(byte[][] data, System.Text.Encoding enc)
        {
            var result = new string[data.GetLength(0)];
            for (int i = 0; i < data.GetLength(0); i++)
            {
                result[i] = enc.GetString(data[i]);
            }
            return result;
        }

        public byte[] GetEntryByAddress(long address)
        {
            var stream = new MemoryStream(Content);
            using (var ds = new System.IO.Compression.DeflateStream(stream, System.IO.Compression.CompressionMode.Decompress))
            {
                for(long i = 0; i < address; i++)
                {
                    ds.ReadByte();
                }

                var bytes = new byte[4];
                ds.Read(bytes, 0, 4);
                int strLen = Functions.UnpackInt(bytes);

                var str = new byte[strLen];
                ds.Read(str, 0, strLen);
                return str;
            }
        }
        public string GetEntryByAddress(long address,System.Text.Encoding enc)
        {
            return enc.GetString(GetEntryByAddress(address));
        }

        public byte[][] Decompress()
        {
            var stream = new MemoryStream(Content);
            var result = new List<byte[]>();
            stream.Seek(0, SeekOrigin.Begin);
            using (var ds = new System.IO.Compression.DeflateStream(stream, System.IO.Compression.CompressionMode.Decompress))
            {
                int i = 0;
                while (true)
                {
                    var bytes = new byte[4];
                    ds.Read(bytes, 0, 4);
                    int strLen = Functions.UnpackInt(bytes);

                    if (strLen == 0) { break; }

                    var str = new byte[strLen];
                    ds.Read(str, 0, strLen);
                    result.Add(str);

                    i += strLen;
                    if (i >= RawLength)
                    {
                        break;
                    }
                }
            }
            return result.ToArray();
        }

    }
}
