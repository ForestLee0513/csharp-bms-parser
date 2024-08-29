using System.Reflection;
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
        public new BMSModel Decode(string path)
        {
            BMSModel model = Decode(path, File.ReadAllBytes(path), path.ToLower().EndsWith(".pms"), []);

            return model;
        }

        public override BMSModel Decode(ChartInformation info)
        {
            lnType = info.LnType;
            return Decode(info.Path, File.ReadAllBytes(info.Path), info.Path.ToLower().EndsWith(".pms"), info.SelectedRandoms); 
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
                    string arg = line.IndexOf(' ') > -1 ? line.Substring(line.IndexOf(' ') + 1) : "";

                    #region Random
                    if (MatchesReserveWord(line, "RANDOM"))
                    {
                        if (selectedRandom.Length == 0)
                        {
                            bool isValidValue = int.TryParse(arg, out int n);
                            if (!isValidValue)
                            {
                                Console.WriteLine("This RANDOM command isn't correct");
                                continue;
                            }

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
                        bool isValidValue = int.TryParse(arg, out int n);
                        if (!isValidValue)
                        {
                            Console.WriteLine("This IF command isn't correct");
                            continue;
                        }
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
                        bool isValidValue = int.TryParse(arg, out int n);
                        if (!isValidValue)
                        {
                            Console.WriteLine("This ELSEIF command isn't correct");
                            continue;
                        }
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
                        try
                        {
                            if (line[4] == ' ')
                            {
                                // Some track bpm is using decimal point like 'xi - FREEDOM DiVE↓' (BPM: 222.22)
                                double.TryParse(arg, out double bpm);
                                if (bpm > 0)
                                    model.SetBpm(bpm);
                                else
                                    Console.WriteLine($"You can't set bpm to 0 {line}");
                            }
                            else
                            {
                                double.TryParse(arg, out double bpm);
                                if (bpm > 0)
                                {
                                    if (model.BaseType == 62)
                                        bpmTable.Add(ParseInt62(line, 4), bpm);
                                    else
                                        bpmTable.Add(ParseInt36(line, 4), bpm);
                                }
                            }
                        }
                        catch (FormatException e)
                        {
                            Console.WriteLine($"This BPM command isn't correct: {line}");
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
                                Console.WriteLine($"This BMP command isn't correct: {line}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"This WAV command isn't correct: {line}");
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
                                Console.WriteLine($"This BMP command isn't correct: {line}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"This BMP command isn't correct. {line}");
                        }
                    }
                    else if (MatchesReserveWord(line, "STOP"))
                    {
                        if (line.Length >= 9)
                        {
                            try
                            {
                                double.TryParse(line.Substring(8).Trim(), out double stop);
                                stop /= 192;
                                if (stop < 0)
                                {
                                    stop = Math.Abs(stop);
                                    Console.WriteLine($"You can't use negative stop value. {line}");
                                }
                                if (baseType == 62)
                                    stopTable.Add(ParseInt62(line, 5), stop);
                                else
                                    stopTable.Add(ParseInt36(line, 5), stop);
                            }
                            catch (FormatException e)
                            {
                                Console.WriteLine($"This STOP command isn't correct. {line}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"This STOP command isn't correct. {line}");
                        }
                    }
                    else if (MatchesReserveWord(line, "SCROLL"))
                    {
                        if (line.Length >= 11)
                        {
                            try
                            {
                                double.TryParse(line.Substring(10).Trim(), out double scroll);
                                if (baseType == 62)
                                    scrollTable.Add(ParseInt62(line, 7), scroll);
                                else
                                    scrollTable.Add(ParseInt36(line, 7), scroll);
                            }
                            catch (FormatException e)
                            {
                                Console.WriteLine($"This SCROLL command is not correct. {line}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"This SCROLL command is not correct. {line}");
                        }
                    }
                    else
                    {
                        foreach (FieldInfo command in typeof(Command).GetFields(BindingFlags.Public | BindingFlags.Static))
                        {
                            string commandName = command.Name;
                            if (line.Length > commandName.Length + 2 && MatchesReserveWord(line, commandName))
                            {
                                if (command.GetValue(null) is Command.CommandAction action)
                                {
                                    action(model, arg);
                                }
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

        private class Command
        {
            public delegate void CommandAction(BMSModel model, string arg);

            public static readonly CommandAction PLAYER = (model, arg) =>
            {
                try
                {
                    int.TryParse(arg, out int player);

                    if (player >= 1 && player < 3)
                        model.SetPlayer(player);
                    else
                        Console.WriteLine("This PLAYER command is incorrect.");
                }
                catch (FormatException e)
                {
                    Console.WriteLine("This PLAYER command is incorrect.");
                }
            };

            public static readonly CommandAction GENRE = (model, arg) =>
            {
                model.SetGenre(arg);
            };

            public static readonly CommandAction TITLE = (model, arg) =>
            {
                model.SetTitle(arg);
            };

            public static readonly CommandAction SUBTITLE = (model, arg) =>
            {
                model.SetSubtitle(arg);
            };

            public static readonly CommandAction ARTIST = (model, arg) =>
            {
                model.SetArtist(arg);
            };

            public static readonly CommandAction SUBARTIST = (model, arg) =>
            {
                model.SetSubartist(arg);
            };

            public static readonly CommandAction PLAYLEVEL = (model, arg) =>
            {
                model.SetPlayLevel(arg);
            };

            public static readonly CommandAction RANK = (model, arg) =>
            {
                try
                {
                    int.TryParse(arg, out int rank);
                    if (rank >= 0 && rank < 5)
                    {
                        model.SetJudgeRank(rank);
                        model.SetJudgeRankType(BMSModelDefine.JudgeRankType.BMS_RANK);
                    }
                    else
                    {
                        Console.WriteLine("This RANK command isn't correct.");
                    }
                }
                catch (FormatException e)
                {
                    Console.WriteLine("This RANK command isn't correct.");
                }
            };

            public static readonly CommandAction DEFEXRANK = (model, arg) =>
            {
                try
                {
                    int.TryParse(arg, out int rank);
                    if (rank >= 1)
                    {
                        model.SetJudgeRank(rank);
                        model.SetJudgeRankType(BMSModelDefine.JudgeRankType.BMS_DEFEXRANK);
                    }
                    else
                    {
                        Console.WriteLine("This DEFEXRANK command isn't correct.");
                    }
                }
                catch (FormatException e)
                {
                    Console.WriteLine("This DEFEXRANK command isn't correct.");
                }
            };

            public static readonly CommandAction TOTAL = (model, arg) =>
            {
                try
                {
                    double.TryParse(arg, out double total);
                    if (total > 0)
                    {
                        model.SetTotal(total);
                        model.SetTotalType(BMSModelDefine.TotalType.BMS);
                    }
                    else
                    {
                        Console.WriteLine("This TOTAL command isn't correct.");
                    }
                }
                catch (FormatException e)
                {
                    Console.WriteLine("This TOTAL command isn't correct.");
                }
            };

            public static readonly CommandAction VOLWAV = (model, arg) =>
            {
                try
                {
                    int.TryParse(arg, out int volwav);
                    model.SetVolWav(volwav);
                }
                catch (FormatException e)

                {
                    Console.WriteLine("this VOLWAV command isn't correct.");
                }
            };

            public static readonly CommandAction STAGEFILE = (model, arg) =>
            {
                model.SetStageFile(arg.Replace('\\', '/'));
            };

            public static readonly CommandAction BACKBMP = (model, arg) =>
            {
                model.SetBackBmp(arg.Replace('\\', '/'));
            };

            public static readonly CommandAction PREVIEW = (model, arg) =>
            {
                model.SetPreview(arg.Replace('\\', '/'));
            };

            public static readonly CommandAction LNOBJ = (model, arg) =>
            {
                try
                {
                    if (model.BaseType == 62)
                    {
                        model.SetLnobj(ParseInt62(arg, 0));
                    }
                    else
                    {
                        model.SetLnobj(ParseInt36(arg, 0));
                    }
                }
                catch (FormatException e)
                {
                    Console.WriteLine("This LNOBJ command isn't correct.");
                }
            };

            public static readonly CommandAction LNMODE = (model, arg) =>
            {
                try
                {
                    int.TryParse(arg, out int lnMode);
                    if (lnMode < 0 || lnMode > 3)
                    {
                        Console.WriteLine("This LNMODE command isn't correct.");
                        return;
                    }
                    model.SetLnMode(lnMode);
                }
                catch (FormatException e)
                {
                    Console.WriteLine("This LNMODE command isn't correct.");
                }
            };

            public static readonly CommandAction DIFFICULTY = (model, arg) =>
            {
                try
                {
                    int.TryParse(arg, out int difficulty);
                    model.SetDifficulty(difficulty);
                }
                catch (FormatException e)
                {
                    Console.WriteLine("This DIFFICULTY command isn't correct");
                }
            };

            public static readonly CommandAction BANNER = (model, arg) =>
            {
                model.SetBanner(arg.Replace('\\', '/'));
            };

            public static readonly CommandAction BASE = (model, arg) =>
            {
                try
                {
                    int.TryParse(arg, out int baseType);
                    if (baseType != 62)
                    {
                        Console.WriteLine("This BASE command isn't correct.");
                        return;
                    }
                    model.SetBaseType(baseType);
                }
                catch (FormatException e)
                {
                    Console.WriteLine("This BASE command isn't correct.");
                }
            };
        }
    }
}
