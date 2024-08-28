using System.Security.Cryptography;
using System.Text;

namespace BMS
{
    public class BMSDecoder : ChartDecoder
    {
        private string[] wavList = new string[62 * 62];
        private int[] wm = new int[62 * 62];

        private string[] bgaList = new string[62 * 62];
        private int[] bm = new int[62 * 62];

        public BMSDecoder()
        {
            lnType = BMSModelDefine.LNTYPE.LONGNOTE;
        }

        public BMSDecoder(BMSModelDefine.LNTYPE lnType)
        {
            base.lnType = lnType;
        }

        // BMSModel 반환
        public new BMSModel? Decode(string path)
        {
            try
            {
                BMSModel? model = Decode(path, File.ReadAllBytes(path), path.ToLower().EndsWith(".pms"), []);
                if (model == null)
                    return null;

                return model;
            }
            catch (Exception e) 
            {

            }

            return null;
        }

        public override BMSModel? Decode(ChartInformation info)
        {
            try 
            {
                lnType = info.LnType;
                return Decode(info.Path, File.ReadAllBytes(info.Path), info.Path.ToLower().EndsWith(".pms"), info.SelectedRandoms);
            }
            catch (Exception e) 
            {

            }

            return null;
        }

        public BMSModel? Decode(byte[] data, bool isPms, int[] random)
        {
            return Decode(null, data, isPms, random);
        }

        private Stack<int> randomStack = new Stack<int>();
        private Stack<bool> skipStack = new Stack<bool>();

        private BMSModel Decode(string? path, byte[] data, bool isPms, int[] selectedRandom)
        {
            long time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            BMSModel model = new();

            using MemoryStream stream = new(data);

            #region Hashing
            string md5Hash;
            using (var md5Stream = new MemoryStream(data))
            {
                md5Hash = BitConverter.ToString(MD5.Create().ComputeHash(md5Stream)).Replace("-", "").ToLower();
            }

            // Compute SHA256 hash
            string sha256Hash;
            using (var sha256Stream = new MemoryStream(data))
            {
                sha256Hash = BitConverter.ToString(SHA256.Create().ComputeHash(sha256Stream)).Replace("-", "").ToLower();
            }

            model.SetMD5(md5Hash);
            model.SetSHA256(sha256Hash);
            #endregion

            #region Decode
            model.SetMode(isPms ? Mode.POPN_9K : Mode.BEAT_5K);

            randomStack.Clear();
            skipStack.Clear();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using var reader = new StreamReader(stream, Encoding.GetEncoding(932));
            do
            {
                string? line = reader.ReadLine();

                if (line == null || line.Length < 2)
                    continue;

                if (line[0] == '#')
                {
                    string value = line.IndexOf(' ') > -1 ? line.Substring(line.IndexOf(' ') + 1) : "";

                    #region Random
                    if (MatchesReserveWord(line, "RANDOM"))
                    {
                        if (selectedRandom.Length == 0)
                        {
                            int.TryParse(value, out int n);
                            int randomResult = new Random().Next(1, n + 1);
                            randomStack.Push(randomResult);
                            continue;
                        }
                        else
                        {
                            for (int i = selectedRandom.Length - 1; i > -1; --i)
                            {
                                int result = selectedRandom[i];
                                randomStack.Push(result);
                                continue;
                            }
                        }
                    }
                    else if (MatchesReserveWord(line, "IF"))
                    {
                        if (randomStack.Count == 0)
                            continue;

                        int currentRandom = randomStack.Peek();
                        Int32.TryParse(value, out int n);
                        skipStack.Push(currentRandom != n);
                        continue;
                    }
                    else if (MatchesReserveWord(line, "ELSE"))
                    {
                        if (skipStack.Count == 0)
                            continue;
                        bool currentSkip = skipStack.Pop();
                        skipStack.Push(!currentSkip);
                        continue;
                    }
                    else if (MatchesReserveWord(line, "ELSEIF"))
                    {
                        if (skipStack.Count == 0)
                            continue;
                        bool currentSkip = skipStack.Pop();
                        int currentRandom = randomStack.Peek();
                        Int32.TryParse(value, out int n);
                        skipStack.Push(currentSkip && currentRandom != n);
                        continue;
                    }
                    else if (MatchesReserveWord(line, "ENDIF"))
                    {
                        if (skipStack.Count == 0)
                            continue;
                        skipStack.Pop();
                        continue;
                    }
                    else if (MatchesReserveWord(line, "ENDRANDOM"))
                    {
                        if (randomStack.Count == 0)
                            continue;

                        randomStack.Pop();
                        continue;
                    }
                    if (skipStack.Count > 0 && skipStack.Peek() == true)
                    {
                        continue;
                    }
                    #endregion
                    #region Commands
                    Console.WriteLine(line);
                    #endregion
                }
            } while (!reader.EndOfStream);
            #endregion

            return model;
        }
        
        private bool MatchesReserveWord(string line, string s)
        {
            if (line.Length <= s.Length)
                return false;

            for (int i = 0; i < s.Length; i++) {
                char c = line[i + 1];
                char c2 = s[i];
                if (c != c2 && c != c2 + 32) {
                    return false;
                }
            }

            return true;
        }
    }
}
