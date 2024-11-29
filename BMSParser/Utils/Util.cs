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

            public static int DecodeBase16(string s)
            {
                const string chars = "0123456789ABCDEF";
                int result = 0;

                foreach (char c in s)
                {
                    result = result * 16 + chars.IndexOf(c);
                }

                return result;
            }

            public static int DecodeBase36(string s)
            {
                const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                int result = 0;

                foreach (char c in s)
                {
                    result = result * 36 + chars.IndexOf(c);
                }

                return result;
            }

            public static int DecodeBase62(string s)
            {
                const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
                int result = 0;

                foreach (char c in s)
                {
                    result = result * 62 + chars.IndexOf(c);
                }

                return result;
            }
        }
    }
}
