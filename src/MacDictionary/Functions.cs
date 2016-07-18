using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MacDictionary
{
    public static class Functions
    {
        public static int UnpackInt(byte[] bytes)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToInt32(bytes, 0);
        }

        public static uint UnpackUInt(byte[] bytes)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToUInt32(bytes, 0);
        }

        public static short UnpackShort(byte[] bytes)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToInt16(bytes, 0);
        }

        public static short UnpackShortLE(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToInt16(bytes, 0);
        }

        public static void SeekZeros(System.IO.Stream sr,int count=1)
        {
            int i = 0;
            while (sr.ReadByte() == 0)
            {
                i++;
            }
            sr.Seek(-1 - (i % count), System.IO.SeekOrigin.Current);
        }

        public static string LoadStringShort(System.IO.Stream sr,System.Text.Encoding enc)
        {
            var str = LoadBytesShort(sr);
            if (str == null) return null;
            return (enc.GetString(str));
        }

        public static byte[] LoadBytesShort(System.IO.Stream sr)
        {
            var bytes = new byte[2];
            sr.Read(bytes, 0, 2);
            int strLen1 = Functions.UnpackShort(bytes);
            if (strLen1 <= 0) { return null; }
            var str = new byte[strLen1];
            sr.Read(str, 0, strLen1);
            return str;
        }

        public static byte[][] LoadBytesShortArray(System.IO.Stream sr)
        {
            List<byte[]> result = new List<byte[]>();
            while (true)
            {
                result.Add(LoadBytesShort(sr));
                if (sr.Position >= sr.Length) break;
            }
            return result.ToArray();
        }
    }
}
