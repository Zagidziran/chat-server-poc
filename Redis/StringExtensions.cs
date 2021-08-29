namespace Redis
{
    using System;
    using System.Linq;
    using System.Text;

    internal static class StringExtensions
    {
        public static double CalculateScore(this string value)
        {
            var firstChars = value.Substring(0, Math.Min(8, value.Length));
            var firstBytes = Encoding.Default.GetBytes(firstChars);
            var toConvert = new byte[8];
            Array.Copy(firstBytes, toConvert, Math.Min(toConvert.Length, firstBytes.Length));
            return BitConverter.ToUInt64(toConvert.Reverse().ToArray());
        }
    }
}
