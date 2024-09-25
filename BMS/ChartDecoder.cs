using System.Runtime.ConstrainedExecution;
using System.Text;

namespace BMS
{
    public abstract class ChartDecoder
    {
        protected BMSModelDefine.LNTYPE lnType;

        public BMSModel Decode(string path)
        {
            return Decode(new ChartInformation(path, lnType, []));
        }

        public abstract BMSModel Decode(ChartInformation info);

        public static ChartDecoder GetDecoder(string path)
        {
            if (path.EndsWith(".bms") || path.EndsWith(".bml") || path.EndsWith(".bme") || path.EndsWith(".pms"))
            {
                // Decode regular bms, bml, bme, pms
                return new BMSDecoder(BMSModelDefine.LNTYPE.LONGNOTE);
            }
            else if (path.EndsWith(".bmson"))
            {
                // Decode json based bms
            }

            return null;
        }

        public static int ParseInt36(string s, int index)
        {
            int result = ParseInt36(s[index], s[index + 1]);

            if (result == -1)
            {
                throw new FormatException();
            }

            return result;
        }

        public static int ParseInt36(char c1, char c2)
        {
            int result;
            if (c1 >= '0' && c1 <= '9')
            {
                result = (c1 - '0') * 36;
            }
            else if (c1 >= 'a' && c1 <= 'z')
            {
                result = ((c1 - 'a') + 10) * 36;
            }
            else if (c1 >= 'A' && c1 <= 'Z')
            {
                result = ((c1 - 'A') + 10) * 36;
            }
            else
            {
                return -1;
            }

            if (c2 >= '0' && c2 <= '9')
            {
                result += (c2 - '0');
            }
            else if (c2 >= 'a' && c2 <= 'z')
            {
                result += (c2 - 'a') + 10;
            }
            else if (c2 >= 'A' && c2 <= 'Z')
            {
                result += (c2 - 'A') + 10;
            }
            else
            {
                return -1;
            }

            return result;
        }

        public static int ParseInt62(string s, int index)
        {
            int result = ParseInt62(s[index], s[index + 1]);

            if (result == -1)
            {
                throw new FormatException();
            }

            return result;
        }

        public static int ParseInt62(char c1, char c2)
        {
            int result;
            if (c1 >= '0' && c1 <= '9')
            {
                result = (c1 - '0') * 62;
            }
            else if (c1 >= 'A' && c1 <= 'Z')
            {
                result = ((c1 - 'A') + 10) * 62;
            }
            else if (c1 >= 'a' && c1 <= 'z')
            {
                result = ((c1 - 'a') + 36) * 62;
            }
            else
            {
                return -1;
            }

            if (c2 >= '0' && c2 <= '9')
            {
                result += (c2 - '0');
            }
            else if (c2 >= 'A' && c2 <= 'Z')
            {
                result += (c2 - 'A') + 10;
            }
            else if (c2 >= 'a' && c2 <= 'z')
            {
                result += (c2 - 'a') + 36;
            }
            else
            {
                return -1;
            }

            return result;
        }

        public static string ToBase62(int num)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 2; i++)
            {
                int mod = num % 62;
                if (mod < 10)
                {
                    sb.Insert(0, mod);
                }
                else if (mod < 36)
                {
                    mod = mod - 10 + 'A';
                    sb.Insert(0, (char)mod);
                }
                else if (mod < 62)
                {
                    mod = mod - 36 + 'a';
                    sb.Insert(0, (char)mod);
                }
                else 
                {
                    sb.Insert(0, '0');
                }
                num /= 62;
            }

            return sb.ToString();
        }
    }

    public class TimelineCache
    {
        public double time;
        public Timeline timeline;

        public TimelineCache(double time, Timeline timeline)
        {
            this.time = time;
            this.timeline = timeline;
        }
    }
}
