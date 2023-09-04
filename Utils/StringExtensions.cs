using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace Build1.UnityConfig.Utils
{
    public static class StringExtensions
    {
        public static string Compress(this string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str);
            var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                gZipStream.Write(buffer, 0, buffer.Length);

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
            return Convert.ToBase64String(gZipBuffer);
        }
        
        public static string Decompress(this string compressedString)
        {
            var gZipBuffer = Convert.FromBase64String(compressedString);
            using var memoryStream = new MemoryStream();
            var dataLength = BitConverter.ToInt32(gZipBuffer, 0);
            memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

            var buffer = new byte[dataLength];
            memoryStream.Position = 0;
            
            using var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
            gZipStream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }

        public static string FormatCamelCaseToSpacedCamelCase(this string input)
        {
            return Regex.Replace(input, "(\\B[A-Z])", " $1");
        }
    }
}