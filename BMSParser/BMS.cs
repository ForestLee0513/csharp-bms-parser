using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static BMSParser.Define.BMSModel;
using static BMSParser.Define.PatternProcessor;

namespace BMSParser
{
    public class BMS
    {
        readonly Stack<int> randomStack = new Stack<int>();
        readonly Stack<bool> skipStack = new Stack<bool>();

        public BMSModel Decode(string path)
        {
            return Decode(path, Environment.TickCount);
        }
        public BMSModel Decode(string path, int randomSeed)
        {
            BMSModel model = new BMSModel();

            #region Extension validation
            string extension = Path.GetExtension(path);
            if (extensionValidations.ContainsKey(extension))
            {
                extensionValidations[extension](model);
            }
            else
            {
                throw new FileLoadException(".bms, .bme, .bml, .pms 이외의 파일은 불러올 수 없습니다.");
            }
            #endregion

            model.Mode = model.Extension == Extension.PMS ? BMSKey.POPN : BMSKey.BMS_5K_ONLY;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var reader = new StreamReader(path, Encoding.GetEncoding(932)))
            {
                do
                {
                    string line = reader.ReadLine();

                    if (line.Length < 2)
                    {
                        continue;
                    }

                    if (line[0] == '#')
                    {
                        string key = line.IndexOf(' ') > -1 ? line.Substring(1, line.IndexOf(' ') - 1) : line.Substring(1);
                        string value = line.IndexOf(' ') > -1 ? line.Substring(line.IndexOf(' ') + 1) : "";

                        #region Random
                        if (key == "RANDOM")
                        {
                            bool isValidValue = int.TryParse(value, out int n);
                            if (!isValidValue)
                            {
                                Console.WriteLine("This RANDOM command isn't correct");
                                continue;
                            }
                            int randomResult = new Random(randomSeed).Next(1, n + 1);
                            randomStack.Push(randomResult);
                            continue;
                        }
                        else if (key == "IF")
                        {
                            if (randomStack.Count == 0)
                                continue;

                            int currentRandom = randomStack.Peek();
                            bool isValidValue = int.TryParse(value, out int n);
                            if (!isValidValue)
                            {
                                Console.WriteLine("This IF command isn't correct");
                                continue;
                            }
                            skipStack.Push(currentRandom != n);
                            continue;
                        }
                        else if (key == "ELSE")
                        {
                            if (skipStack.Count == 0)
                                continue;
                            bool currentSkip = skipStack.Pop();
                            skipStack.Push(!currentSkip);
                            continue;
                        }
                        else if (key == "ELSEIF")
                        {
                            if (skipStack.Count == 0)
                                continue;
                            bool currentSkip = skipStack.Pop();
                            int currentRandom = randomStack.Peek();
                            bool isValidValue = int.TryParse(value, out int n);
                            if (!isValidValue)
                            {
                                Console.WriteLine("This ELSEIF command isn't correct");
                                continue;
                            }
                            skipStack.Push(currentSkip && currentRandom != n);

                            continue;
                        }
                        else if (key == "ENDIF")
                        {
                            if (skipStack.Count == 0)
                                continue;
                            skipStack.Pop();
                            continue;
                        }
                        else if (key == "ENDRANDOM")
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
                        #endregion Random

                        #region Parse headers
                        if (key.StartsWith("WAV"))
                        {
                            string wavKey = key.Substring(3);
                            long decodedWavKey;
                            if (model.Base == Base.BASE62)
                            {
                                decodedWavKey = Util.Decode.DecodeBase62(wavKey);
                            }
                            else
                            {
                                decodedWavKey = Util.Decode.DecodeBase36(wavKey);
                            }

                            model.Wav[decodedWavKey] = value;
                        }

                        if (key.StartsWith("BMP"))
                        {
                            string bmpKey = key.Substring(3);
                            long decodedBmpKey;
                            if (model.Base == Base.BASE62)
                            {
                                decodedBmpKey = Util.Decode.DecodeBase62(bmpKey);
                            }
                            else
                            {
                                decodedBmpKey = Util.Decode.DecodeBase36(bmpKey);
                            }

                            model.Bmp[decodedBmpKey] = value;
                        }

                        if (key.StartsWith("STOP"))
                        {
                            double.TryParse(value, out double stop);
                            string stopKey = key.Substring(4);
                            long decodedStopKey;
                            if (model.Base == Base.BASE62)
                            {
                                decodedStopKey = Util.Decode.DecodeBase62(stopKey);
                            }
                            else
                            {
                                decodedStopKey = Util.Decode.DecodeBase36(stopKey);
                            }

                            model.StopList[decodedStopKey] = stop / 192;
                        }

                        if (key.StartsWith("BPM"))
                        {
                            if (line[4] == ' ')
                            {
                                double.TryParse(value, out double bpm);
                                model.Bpm = bpm;
                                model.PatternProcessor.ExtendMeasure(0);
                                model.PatternProcessor.Measures[0].AddBPMEvent(0, bpm);
                            }
                            else
                            {
                                double.TryParse(value, out double bpm);
                                string bpmKey = key.Substring(3);
                                long decodedBpmKey;
                                if (model.Base == Base.BASE62)
                                {
                                    decodedBpmKey = Util.Decode.DecodeBase62(bpmKey);
                                }
                                else
                                {
                                    decodedBpmKey = Util.Decode.DecodeBase36(bpmKey);
                                }

                                model.BpmList[decodedBpmKey] = bpm;
                            }
                        }

                        if (key.StartsWith("SCROLL"))
                        {
                            double.TryParse(value, out double scroll);
                            string scrollKey = key.Substring(6);
                            long decodedScrollKey;
                            if (model.Base == Base.BASE62)
                            {
                                decodedScrollKey = Util.Decode.DecodeBase62(scrollKey);
                            }
                            else
                            {
                                decodedScrollKey = Util.Decode.DecodeBase36(scrollKey);
                            }

                            model.ScrollList[decodedScrollKey] = scroll;
                        }

                        if (key.StartsWith("4K"))
                        {
                            model.Mode = BMSKey.BMS_4K;
                        }

                        if (key.StartsWith("6K"))
                        {
                            model.Mode = BMSKey.BMS_6K;
                        }

                        if (headerCommands.ContainsKey(key))
                        {
                            headerCommands[key](model, value);
                        }
                        #endregion

                        #region Parse main data
                        bool isMainData = line.IndexOf(':') > -1 && !headerCommands.ContainsKey(key);

                        if (isMainData)
                        {
                            string[] mainDataKeyValuePair = line.Substring(1).Split(':');
                            string mainDataHeader = mainDataKeyValuePair[0];
                            string mainDataValue = mainDataKeyValuePair[1];

                            int measure = int.Parse(mainDataHeader.Substring(0, 3));
                            int channel = Util.Decode.DecodeBase36(mainDataHeader.Substring(3));

                            model.PatternProcessor.AssignObjectsBeat(measure, channel, mainDataValue, model);
                        }
                        #endregion
                    }
                } while (!reader.EndOfStream);
            }

            Timestamp baseTimestamp = new Timestamp(0)
            {
                Bpm = model.Bpm
            };

            model.PatternProcessor.Timestamp.Add(0, baseTimestamp);
            model.PatternProcessor.CalculateTiming(model.Lnobj);
            model.Timestamp = model.PatternProcessor.Timestamp;

            return model;
        }

        private delegate void CommandRunner(BMSModel model, string value);
        private readonly Dictionary<string, CommandRunner> headerCommands = new Dictionary<string, CommandRunner>()
        {
            {
                "PLAYER", (BMSModel model, string value) =>
                {
                    try
                    {
                        int.TryParse(value, out int player);

                        if (player >= 1 && player <= 3)
                            model.Player = player;
                        else
                            Console.WriteLine("#PLAYER 명령어는 1에서 3사이로 지정돼야 합니다.");
                    }
                    catch
                    {
                        throw new FormatException("#PLAYER 파싱에 실패했습니다. 숫자가 제대로 들어오는지 확인 해주세요.");
                    }
                }
            },
            {
                "RANK", (BMSModel model, string value) =>
                {
                    try
                    {
                        int.TryParse(value, out int rank);

                        if (rank >= 0 && rank <= 3)
                            model.Rank = rank;
                        else
                            Console.WriteLine("#RANK 명령어는 0에서 3사이로 지정돼야 합니다.");
                    }
                    catch
                    {
                        throw new FormatException("#RANK 파싱에 실패했습니다. 숫자가 제대로 들어오는지 확인 해주세요.");
                    }
                }
            },
            {
                "DEFEXRANK", (BMSModel model, string value) =>
                {
                    try
                    {
                        int.TryParse(value, out int rank);
                        if (rank > 0)
                        {
                            model.Rank = rank;
                            model.RankType = RankType.DEFEXRANK;
                        }
                        else
                        {
                            Console.WriteLine("#DEFEXRANK는 0보다 높게 지정돼야 합니다.");
                        }
                    }
                    catch
                    {
                        throw new FormatException("#DEFEXRANK 파싱에 실패했습니다. 숫자가 제대로 들어오는지 확인 해주세요.");
                    }
                }
            },
            {
                "TOTAL", (BMSModel model, string value) =>
                {
                    try
                    {
                        int.TryParse(value, out int total);
                        if (total > 0)
                        {
                            model.Total = total;
                        }
                        else
                        {
                            Console.WriteLine("#TOTAL은 0보다 높게 지정돼야 합니다.");
                        }
                    }
                    catch
                    {
                        throw new FormatException("#TOTAL 파싱에 실패했습니다. 숫자가 제대로 들어오는지 확인 해주세요.");
                    }
                }
            },
            {
                "VOLWAV", (BMSModel model, string value) =>
                {
                    try
                    {
                        int.TryParse(value, out int volWav);
                        model.VolWav = volWav;
                    }
                    catch
                    {
                        throw new FormatException("#VOLWAV 파싱에 실패했습니다. 숫자가 제대로 들어오는지 확인 해주세요.");
                    }
                }
            },
            {
                "STAGEFILE", (BMSModel model, string value) => model.StageFile = value
            },
            {
                "BANNER", (BMSModel model, string value) => model.Banner = value
            },
            {
                "BACKBMP", (BMSModel model, string value) => model.BackBmp = value
            },
            {
                "PLAYLEVEL", (BMSModel model, string value) => model.PlayLevel = value
            },
            {
                "DIFFICULTY", (BMSModel model, string value) =>
                {
                    try
                    {
                        int.TryParse(value, out int difficulty);
                        model.Difficulty = difficulty;
                    }
                    catch
                    {
                        throw new FormatException("#DIFFICULTY 파싱에 실패했습니다. 숫자가 제대로 들어오는지 확인 해주세요.");
                    }
                }
            },
            {
                "TITLE", (BMSModel model, string value) => model.Title = value
            },
            {
                "SUBTITLE", (BMSModel model, string value) => model.SubTitle = value
            },
            {
                "ARTIST", (BMSModel model, string value) => model.Artist = value
            },
            {
                "SUBARTIST", (BMSModel model, string value) => model.SubArtist = value
            },
            {
                "GENRE", (BMSModel model, string value) => model.Genre = value
            },
            {
                "LNOBJ", (BMSModel model, string value) =>
                {
                    try
                    {
                        int decodedLNOBJKey;
                        if (model.Base == Base.BASE62)
                        {
                            decodedLNOBJKey = Util.Decode.DecodeBase62(value);
                        }
                        else
                        {
                            decodedLNOBJKey = Util.Decode.DecodeBase36(value);
                        }

                        model.Lnobj = decodedLNOBJKey;
                        model.LnType = LNType.LNOBJ;
                    }
                    catch
                    {
                        throw new FormatException("#LNOBJ 파싱에 실패했습니다. 숫자가 제대로 들어오는지 확인 해주세요.");
                    }
                }
            },
            {
                "LNTYPE", (BMSModel model, string value) =>
                {
                    try
                    {
                        int.TryParse(value, out int lnType);

                        model.LnType = (LNType)lnType;
                    }
                    catch
                    {
                        throw new FormatException("#LNTYPE 파싱에 실패했습니다. 숫자가 제대로 들어오는지 확인 해주세요.");
                    }
                }
            },
            {
                "MIDIFILE", (BMSModel model, string value) => model.MidiFile = value
            },
            {
                "BASE", (BMSModel model, string value) =>
                {
                    try
                    {
                        if (int.Parse(value) != 62)
                            throw new FormatException("BASE62로 지정되지 않았습니다.");

                        model.Base = Base.BASE62;
                    }
                    catch
                    {
                            throw new FormatException("#BASE 파싱에 실패했습니다. 숫자가 제대로 들어오는지 확인 해주세요.");
                    }
                }
            }
        };

        private delegate void ExtensionValidationRunner(BMSModel model);
        private readonly Dictionary<string, ExtensionValidationRunner> extensionValidations = new Dictionary<string, ExtensionValidationRunner>()
        {
            {
                ".bms", (model) => model.Extension = Extension.BMS
            },
            {
                ".bme", (model) => model.Extension = Extension.BME
            },
            {
                ".bml", (model) => model.Extension = Extension.BML
            },
            {
                ".pms", (model) => model.Extension = Extension.PMS
            }
        };
    }
}
