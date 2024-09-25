using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Schema;
using static BMS.Layer;

namespace BMS
{
    public static class SectionDefine
    {
        public enum Channels
        {
            ILLEGAL = -1,
            LANE_AUTOPLAY = 1,
            SECTION_RATE = 2,
            BPM_CHANGE = 3,
            BGA_PLAY = 4,
            POOR_PLAY = 6,
            LAYER_PLAY = 7,
            BPM_CHANGE_EXTEND = 8,
            STOP = 9,

            P1_KEY_BASE = 1 * 36 + 1,
            P2_KEY_BASE = 2 * 36 + 1,
            P1_INVISIBLE_KEY_BASE = 3 * 36 + 1,
            P2_INVISIBLE_KEY_BASE = 4 * 36 + 1,
            P1_LONG_KEY_BASE = 5 * 36 + 1,
            P2_LONG_KEY_BASE = 6 * 36 + 1,
            P1_MINE_KEY_BASE = 13 * 36 + 1,
            P2_MINE_KEY_BASE = 14 * 36 + 1,

            SCROLL = 1020,
        }

        public static readonly Channels[] noteChannels =
        {
            Channels.P1_KEY_BASE,
            Channels.P2_KEY_BASE,
            Channels.P1_INVISIBLE_KEY_BASE,
            Channels.P2_INVISIBLE_KEY_BASE,
            Channels.P1_LONG_KEY_BASE,
            Channels.P2_LONG_KEY_BASE,
            Channels.P1_MINE_KEY_BASE,
            Channels.P2_MINE_KEY_BASE
        };
    }

    public class Section
    {
        public double rate { get; private set; } = 1.0;
        public int[] poor { get; private set; } = [];
        private readonly BMSModel model;
        private readonly double sectionNum;
        // Decode Log에 관련된 리스트도 추가해야하지만 아직 구현하지 않아 이후 구현예정
        // I have to apply List of Debug Log. but I didn't make class of Log yet. So It'll apply soon.
        private List<string> channelLines;
        public Section(BMSModel model, Section prev, List<string> lines, Dictionary<int, double> bpmTable, Dictionary<int, double> stopTable, Dictionary<int, double> scrollTable)
        {
            this.model = model;
            int baseType = model.BaseType;

            channelLines = new List<string>(new string[lines.Count]);
            if (prev != null)
                sectionNum = prev.sectionNum + prev.rate;
            else
                sectionNum = 0;

            foreach (string line in lines)
            {
                int channel = ChartDecoder.ParseInt36(line[4], line[5]);
                switch (channel)
                {
                    case (int)SectionDefine.Channels.ILLEGAL:
                        Console.WriteLine("ILLEGAL LINE.");
                        break;
                    case (int)SectionDefine.Channels.LANE_AUTOPLAY:
                    case (int)SectionDefine.Channels.BGA_PLAY:
                    case (int)SectionDefine.Channels.LAYER_PLAY:
                        channelLines.Add(line);
                        break;
                    case (int)SectionDefine.Channels.SECTION_RATE:
                        int colonIndex = line.IndexOf(":");
                        try
                        {
                            rate = Convert.ToDouble(line.Substring(colonIndex + 1));
                        }
                        catch (FormatException e)
                        {
                            Console.WriteLine("PARSE ERROR at SECTION RATE");
                        }
                        break;
                    case (int)SectionDefine.Channels.BPM_CHANGE:
                        break;
                    case (int)SectionDefine.Channels.POOR_PLAY:
                        poor = SplitData(line);
                        int singleId = 0;
                        foreach (int id in poor)
                        {
                            if (id != 0)
                            {
                                if (singleId != 0 && singleId != id)
                                {
                                    singleId = -1;
                                    break;
                                }
                                else
                                {
                                    singleId = id;
                                }
                            }
                        }
                        if (singleId != -1)
                            poor = [singleId];
                        break;
                    case (int)SectionDefine.Channels.BPM_CHANGE_EXTEND:
                        ProcessData(line, (pos, data) =>
                        {
                            double bpm = bpmTable[data];
                            if (bpm != null)
                                bpmChange[pos] = bpm;
                            else
                                Console.WriteLine("Undefined BPM Reference exception");
                        });
                        break;
                    case (int)SectionDefine.Channels.STOP:
                        ProcessData(line, (pos, data) => 
                        {
                            double stopValue = stopTable[data];
                            if (stopValue != null)
                                stop[pos] = stopValue;
                            else
                                Console.WriteLine("Undefined STOP Reference exception");
                        });
                        break;
                    case (int)SectionDefine.Channels.SCROLL:
                        ProcessData(line, (pos, data) => 
                        {
                            double scrollValue = scrollTable[data];
                            if (scrollValue != null)
                                scroll[pos] = scrollValue;
                            else
                                Console.WriteLine("Undefined SCROLL Reference exception ");
                        });
                        break;
                }

                int basech = 0;
                int ch2 = -1;
                foreach (int ch in SectionDefine.noteChannels)
                {
                    if (ch <= channel && channel <= ch + 8)
                    {
                        basech = ch;
                        ch2 = channel - ch;
                        channelLines.Add(line);
                        break;
                    }
                }

                // 5/10KEY -> 7/14KEY
                if (ch2 == 7 || ch2 == 8)
                {
                    Mode mode = (model.Mode == Mode.BEAT_5K) ? Mode.BEAT_7K : (model.Mode == Mode.BEAT_10K ? Mode.BEAT_14K : null);
                    if (mode != null)
                    {
                        ProcessData(line, (pos, data) =>
                        {
                            model.SetMode(mode);
                        });
                    }
                }

                // 5/10KEY -> 7/14KEY
                if (basech == (int)SectionDefine.Channels.P2_KEY_BASE || 
                    basech == (int)SectionDefine.Channels.P2_INVISIBLE_KEY_BASE || 
                    basech == (int)SectionDefine.Channels.P2_LONG_KEY_BASE || 
                    basech == (int)SectionDefine.Channels.P2_MINE_KEY_BASE)
                {
                    Mode mode = (model.Mode == Mode.BEAT_5K) ? Mode.BEAT_7K : (model.Mode == Mode.BEAT_10K ? Mode.BEAT_14K : null);
                    if (mode != null)
                    {
                        ProcessData(line, (pos, data) =>
                        {
                            model.SetMode(mode);
                        });
                    }
                }
            }
        }

        private int[] SplitData(string line)
        {
            int baseType = model.BaseType;
            int fIndex = line.IndexOf(':') + 1;
            int lIndex = line.Length;
            int split = (lIndex - fIndex) / 2;

            int[] result = new int[split];
            for (int i = 0; i < split; i++)
            {
                if (baseType == 62)
                    result[i] = ChartDecoder.ParseInt62(line[fIndex + i * 2], line[fIndex + i * 2 + 1]);
                else
                    result[i] = ChartDecoder.ParseInt36(line[fIndex + i * 2], line[fIndex + i * 2 + 1]);

                if (result[i] == -1)
                {
                    Console.WriteLine("WRONG CHANNEL");
                    result[i] = 0;
                }
            }

            return result;
        }

        private void ProcessData(string line, Action<double, int> processor)
        {
            int baseType = model.BaseType;
            int fIndex = line.IndexOf(':') + 1;
            int lIndex = line.Length;
            int split = (lIndex - fIndex) / 2;

            int result;
            for (int i = 0; i < split; i++)
            {
                if (baseType == 62)
                {
                    result = ChartDecoder.ParseInt62(line[fIndex + i * 2], line[fIndex + i * 2 + 1]);
                }
                else
                {
                    result = ChartDecoder.ParseInt36(line[fIndex + i * 2], line[fIndex + i * 2 + 1]);
                }
                if (result > 0)
                {
                    processor((double)i / split, result);
                }
                else if (result == -1)
                {
                    Console.WriteLine("WRONG CHANNEL");
                }
            }
        }

        private readonly Dictionary<double, double> bpmChange = new();
        private readonly Dictionary<double, double> stop = new();
        private readonly Dictionary<double, double> scroll = new();

        private static readonly int[] CHANNELASSIGN_BEAT5 = [0, 1, 2, 3, 4, 5, -1, -1, -1, 6, 7, 8, 9, 10, 11, -1, -1, -1];
        private static readonly int[] CHANNELASSIGN_BEAT7 = [0, 1, 2, 3, 4, 7, -1, 5, 6, 8, 9, 10, 11, 12, 15, -1, 13, 14];
        private static readonly int[] CHANNELASSIGN_POPN = [0, 1, 2, 3, 4, -1, -1, -1, -1, -1, 5, 6, 7, 8, -1, -1, -1, -1];

        private SortedDictionary<double, TimelineCache> tlCache;

        public void MakeTimelines(int[] wavMap, int[] bgaMap, SortedDictionary<double, TimelineCache> tlCache, List<LongNote>[] lnList, LongNote[] startLn)
        {
            int lnobj = model.Lnobj;
            BMSModelDefine.LNTYPE lnmode = model.LnMode;
            this.tlCache = tlCache;
            int[] cassign = model.Mode == Mode.POPN_9K ? CHANNELASSIGN_POPN :
                (model.Mode == Mode.BEAT_7K || model.Mode == Mode.BEAT_14K ? CHANNELASSIGN_BEAT7 : CHANNELASSIGN_BEAT5);
            int baseType = model.BaseType;

            Timeline baseTl = GetTimeline(sectionNum);
            baseTl.SetSectionLine(true);

            // Poor //
            if (poor.Length > 0)
            {
                Layer.Sequence[] poors = new Layer.Sequence[poor.Length + 1];
                int poorTime = 500;

                for (int i = 0; i < poor.Length; i++)
                {
                    if (bgaMap[poor[i]] != -2)
                        poors[i] = new Layer.Sequence((long)(i * poorTime / poor.Length), bgaMap[poor[i]]);
                    else
                        poors[i] = new Layer.Sequence((long)(i * poorTime / poor.Length), -1);
                }

                poors[poors.Length - 1] = new Layer.Sequence(poorTime);
                baseTl.SetEventLayer(new Layer[] { new Layer(new Layer.Event(EventType.MISS, 1), new Layer.Sequence[][] { poors }) });
            }

            // BGA //
            IEnumerator<KeyValuePair<double, double>> stops = stop.GetEnumerator();
            KeyValuePair<double, double>? ste = stops.MoveNext() ? stops.Current : null;
            IEnumerator<KeyValuePair<double, double>> bpms = bpmChange.GetEnumerator();
            KeyValuePair<double, double>? bce = bpms.MoveNext() ? bpms.Current : null;
            IEnumerator<KeyValuePair<double, double>> scrolls = scroll.GetEnumerator();
            KeyValuePair<double, double>? sce = scrolls.MoveNext() ? scrolls.Current : null;

            while (ste != null || bce != null || sce != null)
            {
                double bc = bce != null ? bce.Value.Key : 2;
                double st = ste != null ? ste.Value.Key : 2;
                double sc = sce != null ? sce.Value.Key : 2;

                if (sc <= st && sc <= bc)
                {
                    GetTimeline(sectionNum + sc * rate).SetScroll(sce.Value.Value);
                    sce = scrolls.MoveNext() ? scrolls.Current : null;
                }
                else if (bc <= st)
                {
                    GetTimeline(sectionNum + bc * rate).SetScroll(bce.Value.Value);
                    bce = bpms.MoveNext() ? bpms.Current : null;
                }
                else if (st <= 1)
                {
                    Timeline tl = GetTimeline(sectionNum + ste.Value.Value * rate);
                    tl.SetStop((long)(1000.0 * 60 * 4 * ste.Value.Value / (tl.Bpm)));
                    ste = stops.MoveNext() ? stops.Current : null;
                }
            }

            // NOTES //
            foreach (string line in channelLines)
            {
                int channel = ChartDecoder.ParseInt36(line[4], line[5]);
                int tmpkey = 0;
                if (channel >= (int)SectionDefine.Channels.P1_KEY_BASE && channel < (int)SectionDefine.Channels.P1_KEY_BASE + 9)
                {
                    tmpkey = cassign[channel - (int)SectionDefine.Channels.P1_KEY_BASE];
                    channel = (int)SectionDefine.Channels.P1_KEY_BASE;
                }
                else if (channel >= (int)SectionDefine.Channels.P2_KEY_BASE && channel < (int)SectionDefine.Channels.P2_KEY_BASE + 9)
                {
                    tmpkey = cassign[channel - (int)SectionDefine.Channels.P2_KEY_BASE + 9];
                    channel = (int)SectionDefine.Channels.P1_KEY_BASE;
                }
                else if (channel >= (int)SectionDefine.Channels.P1_INVISIBLE_KEY_BASE && channel < (int)SectionDefine.Channels.P1_INVISIBLE_KEY_BASE + 9)
                {
                    tmpkey = cassign[channel - (int)SectionDefine.Channels.P1_INVISIBLE_KEY_BASE + 9];
                    channel = (int)SectionDefine.Channels.P1_INVISIBLE_KEY_BASE;
                }
                else if (channel >= (int)SectionDefine.Channels.P2_INVISIBLE_KEY_BASE && channel < (int)SectionDefine.Channels.P2_INVISIBLE_KEY_BASE + 9)
                {
                    tmpkey = cassign[channel - (int)SectionDefine.Channels.P2_INVISIBLE_KEY_BASE + 9];
                    channel = (int)SectionDefine.Channels.P2_INVISIBLE_KEY_BASE;
                }
                else if (channel >= (int)SectionDefine.Channels.P1_LONG_KEY_BASE && channel < (int)SectionDefine.Channels.P1_LONG_KEY_BASE + 9)
                {
                    tmpkey = cassign[channel - (int)SectionDefine.Channels.P1_LONG_KEY_BASE + 9];
                    channel = (int)SectionDefine.Channels.P1_LONG_KEY_BASE;
                }
                else if (channel >= (int)SectionDefine.Channels.P2_LONG_KEY_BASE && channel < (int)SectionDefine.Channels.P2_LONG_KEY_BASE + 9)
                {
                    tmpkey = cassign[channel - (int)SectionDefine.Channels.P2_LONG_KEY_BASE + 9];
                    channel = (int)SectionDefine.Channels.P2_LONG_KEY_BASE;
                }
                else if (channel >= (int)SectionDefine.Channels.P1_MINE_KEY_BASE && channel < (int)SectionDefine.Channels.P1_MINE_KEY_BASE + 9)
                {
                    tmpkey = cassign[channel - (int)SectionDefine.Channels.P1_MINE_KEY_BASE + 9];
                    channel = (int)SectionDefine.Channels.P1_MINE_KEY_BASE;
                }
                else if (channel >= (int)SectionDefine.Channels.P2_MINE_KEY_BASE && channel < (int)SectionDefine.Channels.P2_MINE_KEY_BASE + 9)
                {
                    tmpkey = cassign[channel - (int)SectionDefine.Channels.P2_MINE_KEY_BASE + 9];
                    channel = (int)SectionDefine.Channels.P2_MINE_KEY_BASE;
                }

                int key = tmpkey;
                if (key == -1)
                    continue;
                switch (channel)
                {
                    case (int)SectionDefine.Channels.P1_KEY_BASE:
                        ProcessData(line, (pos, data) =>
                        {
                            Timeline tl = GetTimeline(sectionNum + rate * pos);
                            if (tl.ExistNote(key))
                            {
                                Console.WriteLine($"Conflict to adding notes at {key + 1} : {tl.Time}");
                            }

                            if (data == lnobj)
                            {
                                foreach (KeyValuePair<double, TimelineCache> e in tlCache.OrderByDescending(kvp => kvp.Key))
                                {
                                    if (e.Key >= tl.Section)
                                        continue;
                                    Timeline tl2 = e.Value.timeline;
                                    if (tl2.ExistNote(key))
                                    {
                                        Note note = tl2.GetNote(key);
                                        if (note is NormalNote)
                                        {
                                            LongNote ln = new LongNote(note.Wav);
                                            ln.SetType(lnmode);
                                            tl2.SetNote(key, ln);
                                            LongNote lnEnd = new LongNote(-2);
                                            tl.SetNote(key, lnEnd);
                                            ln.SetPair(lnEnd);

                                            if (lnList[key] == null)
                                                lnList[key] = [];
                                            lnList[key].Add(ln);
                                            break;
                                        } else if (note is LongNote && ((LongNote)note).Pair == null)
                                        {
                                            Console.WriteLine($"Start defined in the LN lane and terminated in the LN object. Lane: {key + 1} - Section : {tl2.Section} - {tl.Section}");
                                            LongNote lnEnd = new LongNote(-2);
                                            tl.SetNote(key, lnEnd);
                                            ((LongNote)note).SetPair(lnEnd);

                                            if (lnList[key] == null)
                                                lnList[key] = [];

                                            lnList[key].Add((LongNote)note);
                                            startLn[key] = null;
                                            break;
                                        }
                                        else
                                        {
                                            Console.WriteLine($"The LN object is not corresponding. Lane: {key} - {tl2.Time}");
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                tl.SetNote(key, new NormalNote(wavMap[data]));
                            }
                        });
                        break;
                    case (int)SectionDefine.Channels.P1_INVISIBLE_KEY_BASE:
                        ProcessData(line, (pos, data) => 
                        {
                            GetTimeline(sectionNum + rate * pos).SetHiddenNote(key, new NormalNote(wavMap[data]));
                        });
                        break;
                    case (int)SectionDefine.Channels.P1_LONG_KEY_BASE:
                        ProcessData(line, (pos, data) => 
                        {
                            Timeline tl = GetTimeline(sectionNum + rate * pos);
                            bool insideLn = false;
                            if (!insideLn && lnList[key] != null)
                            {
                                double section = tl.Section;
                                foreach (LongNote ln in lnList[key])
                                {
                                    if (ln.Section <=  section && section <= ln.Pair.Section)
                                    {
                                        insideLn = true;
                                        break;
                                    }
                                }
                            }

                            if (!insideLn)
                            {
                                if (startLn[key] == null)
                                {
                                    if (tl.ExistNote(key))
                                    {
                                        Note note = tl.GetNote(key);
                                        Console.WriteLine($"A normal note is exists at {key + 1} / {tl.Time}ms");
                                        if (note is NormalNote && note.Wav != wavMap[data])
                                            tl.AddBackgroundNote(note);
                                    }
                                    LongNote ln = new LongNote(wavMap[data]);
                                    tl.SetNote(key, ln);
                                    startLn[key] = ln;
                                }
                                else if (startLn[key].Section == double.MinValue)
                                {
                                    startLn[key] = null;
                                }
                                else
                                {
                                    foreach (KeyValuePair<double, TimelineCache> e in tlCache.OrderByDescending(kvp => kvp.Key))
                                    {
                                        if (e.Key >= tl.Section)
                                            continue;
                                        Timeline tl2 = e.Value.timeline;
                                        if (tl2.Section == startLn[key].Section)
                                        {
                                            Note note = startLn[key];
                                            ((LongNote)note).SetType(lnmode);
                                            LongNote noteEnd = new LongNote(startLn[key].Wav != wavMap[data] ? wavMap[data] : -2);
                                            tl.SetNote(key, noteEnd);
                                            if (lnList[key] == null)
                                                lnList[key] = new();
                                            lnList[key].Add((LongNote)note);

                                            startLn[key] = null;
                                            break;
                                        }
                                        else if(tl2.ExistNote(key))
                                        {
                                            Note note = tl2.GetNote(key);
                                            Console.WriteLine($"A normal note is exists at {key + 1} / {tl2.Time}ms");
                                            tl2.SetNote(key, null);
                                            if (note is NormalNote)
                                                tl2.AddBackgroundNote(note);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (startLn[key] == null)
                                {
                                    LongNote ln = new LongNote(wavMap[data]);
                                    ln.SetSection(double.MinValue);
                                    startLn[key] = ln;
                                    Console.WriteLine($"A normal note is exists at {key + 1} / {tl.Time}ms");

                                }
                                else
                                {
                                    if (startLn[key].Section != double.MinValue)
                                    {
                                        if (tlCache.TryGetValue(startLn[key].Section, out var tlc))
                                        {
                                            tlc.timeline.SetNote(key, null);
                                            startLn[key] = null;
                                            Console.WriteLine($"A normal note is exists at {key + 1} / {tl.Time}ms");

                                        }
                                    }
                                }
                            }
                        });
                        break;
                }
            }
        }

        private Timeline GetTimeline(double section)
        {
            if (tlCache.TryGetValue(section, out TimelineCache tlc))
            {
                return tlc.timeline;
            }

            var le = tlCache.FirstOrDefault(x => x.Key < section);
            if (le.Equals(default(KeyValuePair<double, TimelineCache>)))
            {
                throw new Exception("No entry found lower than the section.");
            }

            double scroll = le.Value.timeline.Scroll;
            double bpm = le.Value.timeline.Bpm;
            double time = le.Value.timeline.Stop + (240000.0 * (section - le.Key)) / bpm;

            Timeline tl = new Timeline(section, (long)time, model.Mode.Key);
            tl.SetBpm(bpm);
            tl.SetScroll(scroll);
            tlCache[section] = new TimelineCache(time, tl);

            return tl;
        }
    }
}
