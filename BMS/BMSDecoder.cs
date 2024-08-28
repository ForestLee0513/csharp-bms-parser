using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.InteropServices.Marshalling;
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

        private Stack<int> randomStack = new();
        private Stack<bool> skipStack = new();

        private List<string>[] lines = new List<string>[1000];

        private Dictionary<int, double> scrollTable = new();
        private Dictionary<int, double> stopTable = new();
        private Dictionary<int, double> bpmTable = new();

        private BMSModel Decode(string? path, byte[] data, bool isPms, int[] selectedRandom)
        {
            long time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            BMSModel model = new();
            scrollTable.Clear();
            stopTable.Clear();
            bpmTable.Clear();

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

            int maxsec = 0;

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
                    char c = line[1];
                    int baseType = model.BaseType;
                    if ('0' <= c && c <= '9' && line.Length > 6)
                    {
                        char c2 = line[2];
                        char c3 = line[3];
                        if ('0' <= c2 && c2 <= '9' && '0' <= c3 && c3 <= '9')
                        {
                            int barIndex = (c - '0') * 100 + (c2 - '0') * 10 + (c3 - '0');
                            List<string> l = lines[barIndex];
                            if (l == null)
                            {
                                l = new List<string>();
                                lines[barIndex] = l;
                            }
                            l.Add(line);
                            maxsec = (maxsec > barIndex) ? maxsec : barIndex;
                        }
                    }
                    else if (MatchesReserveWord(line, "BPM"))
                    {
                        if (line[4] == ' ')
                        {
                            // Some track bpm is using decimal point like 'xi - FREEDOM DiVE↓' (BPM: 222.22)
                            double.TryParse(value, out double bpm);
                            if (bpm > 0)
                                model.SetBpm(bpm);
                            else
                                Console.WriteLine("You can't set bpm to 0");
                        }
                        else
                        {
                            double.TryParse(value, out double bpm);
                            if (bpm > 0)
                            {
                                if (model.BaseType == 62)
                                    bpmTable.Add(ParseInt62(line, 4), bpm);
                                else
                                    bpmTable.Add(ParseInt36(line, 4), bpm);
                            }
                        }
                    }
                    else if (MatchesReserveWord(line, "WAV"))
                    {
                        if (line.Length >= 8)
                        {
                            try 
                            {
                                string fileName = line.Substring(7).Trim().Replace("\\", "/");
                                if (baseType == 62)
                                {
                                    wm[ParseInt62(line, 4)] = wavList.Count(s => s != null);
                                }
                                else
                                {
                                    wm[ParseInt36(line, 4)] = wavList.Count(s => s != null);
                                }
                                wavList[wavList.Count(s => s != null)] = fileName;
                            }
                            catch (FormatException e)
                            {

                            }
                        }
                    }
                    else if (MatchesReserveWord(line, "BMP"))
                    {
                        if (line.Length >= 8)
                        {
                            try
                            {
                                string fileName = line.Substring(7).Trim().Replace("\\", "/");
                                if (baseType == 62)
                                {
                                    bm[ParseInt62(line, 4)] = bgaList.Count(s => s != null);
                                }
                                else
                                {
                                    bm[ParseInt36(line, 4)] = bgaList.Count(s => s != null);
                                }
                                bgaList[bgaList.Count(s => s != null)] = fileName;
                            }
                            catch (FormatException e)
                            {

                            }
                        }
                    }
                    #endregion
                }
            } while (!reader.EndOfStream);

            model.SetWavMap(wavList);
            model.SetBgaMap(bgaList);
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
