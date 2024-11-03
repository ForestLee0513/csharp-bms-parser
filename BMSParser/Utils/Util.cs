using System.Linq;

namespace BMSParser
{
    public static class Util
    {
        public static class Decode
        {
            public static bool IsBase16(string s)
            {
                return s.All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F'));
            }

            public static bool IsBase36(string s)
            {
                return s.All(c => char.IsDigit(c) || (c >= 'A' && c <= 'Z'));
            }

            public static bool IsBase62(string s)
            {
                return s.All(c => char.IsDigit(c) || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'));
            }

            public static long DecodeBase16(string s)
            {
                const string chars = "0123456789ABCDEF";
                long result = 0;

                foreach (char c in s)
                {
                    result = result * 16 + chars.IndexOf(c);
                }

                return result;
            }

            public static long DecodeBase36(string s)
            {
                const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                long result = 0;

                foreach (char c in s)
                {
                    result = result * 36 + chars.IndexOf(c);
                }

                return result;
            }

            public static long DecodeBase62(string s)
            {
                const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
                long result = 0;

                foreach (char c in s)
                {
                    result = result * 62 + chars.IndexOf(c);
                }

                return result;
            }
        }
    }
}
