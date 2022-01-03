using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGgerServer
{
    public class Utilities
    {
        public static string ByteToHexa(byte[] bytes)
        {
            string response = SplitBytes(bytes, 16);

            return response;
        }

        private static string SplitBytes(byte[] inputBytes, int bytesToSplit)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte[] copySlice in inputBytes.Slices(bytesToSplit))
            {
                StringBuilder sbtemp = new StringBuilder();
                for (int i = 0; i < copySlice.Length; i++)
                {
                    sbtemp.Append(copySlice[i].ToString("X2") + " ");
                }
                string rawdata = ConvertHex(sbtemp.ToString().Replace(" ", "")).Replace("\u000e", ".").Replace("\0\t", "").Replace("\u001c", ".").Replace("\n", ".").Replace("", "").Replace("\u001b", ".").Replace("\f", ".").Replace("\u000f", ".");//.Replace("\u0004", "").Replace("\u0001", "").Replace("\u0005", "");
                //LogEvents($"{DateTime.Now:MM-dd-yyyy HH:mm:ss}:      {sbtemp,-48} |{rawdata,-16}|"); // Gives empty white spaces to the right for specified length
                sb.Append(rawdata);

            }

            return sb.ToString();

        }

        private static string ConvertHex(string hexString)
        {
            try
            {
                string ascii = string.Empty;

                for (int i = 0; i < hexString.Length; i += 2)
                {
                    string hs = string.Empty;

                    hs = hexString.Substring(i, 2);
                    ulong decval = Convert.ToUInt64(hs, 16);
                    long deccc = Convert.ToInt64(hs, 16);
                    char character = Convert.ToChar(deccc);
                    ascii += character;

                }

                return ascii;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return string.Empty;
        }

        public static byte[] GetSendBytes(string input)
        {
            try
            {
                byte[] ba = Encoding.Default.GetBytes(input);
                var hexString = BitConverter.ToString(ba);
                hexString += "0D0A";
                hexString = hexString.Replace("-", "");
                var dataarray = StringToByteArray(hexString, true);

                //string response = SplitBytes(dataarray, 16);

                //string values = dataarray.Length.ToString("x");

                //while (values.Length < 4)
                //{
                //    values = "0" + values;

                //}
                //byte[] headerBytes = StringToByteArray(values, true);

                return dataarray;//headerBytes.Concat(dataarray).ToArray();
            }
            catch (Exception ex)
            {

                throw;
            }


        }
        private static byte[] StringToByteArray(String hex, bool checkOdd)
        {
            int NumberChars = hex.Length;
            if (checkOdd && NumberChars % 2 != 0)
                NumberChars++;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        private static void LogEvents(string input)
        {
            Logger.LogWithNoLock($"{DateTime.Now:MM-dd-yyyy HH:mm:ss}:{input}");
        }

    }

    public static class SplitBytes
    {
        public static T[] CopySlice<T>(this T[] source, int index, int length, bool padToLength = false)
        {
            int n = length;
            T[] slice = null;

            if (source.Length < index + length)
            {
                n = source.Length - index;
                if (padToLength)
                {
                    slice = new T[length];
                }
            }

            if (slice == null) slice = new T[n];
            Array.Copy(source, index, slice, 0, n);
            return slice;
        }

        public static IEnumerable<T[]> Slices<T>(this T[] source, int count, bool padToLength = false)
        {
            for (var i = 0; i < source.Length; i += count)
                yield return source.CopySlice(i, count, padToLength);
        }
    }
}
