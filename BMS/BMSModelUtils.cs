using System.ComponentModel;

namespace BMS
{
    public static class BMSModelUtilDefine
    {
        public enum TotalNotes
        {
            ALL,
            KEY,
            LONG_KEY,
            SCRATCH,
            LONG_SCRATCH,
            MINE
        }
    }

    public static class BMSModelUtils
    {
        public static int GetTotalNotes(BMSModel model)
        {
            return GetTotalNotes(model, 0, int.MaxValue);
        }

        public static int GetTotalNotes(BMSModel model, BMSModelUtilDefine.TotalNotes type)
        {
            return GetTotalNotes(model, 0, int.MaxValue, type);
        }

        public static int GetTotalNotes(BMSModel model, int start, int end)
        {
            return GetTotalNotes(model, start, end, BMSModelUtilDefine.TotalNotes.ALL);
        }

        public static int GetTotalNotes(BMSModel model, int start, int end, BMSModelUtilDefine.TotalNotes type)
        {
            return GetTotalNotes(model, start, end, type, 0);
        }

        public static int GetTotalNotes(BMSModel model, int start, int end, BMSModelUtilDefine.TotalNotes type, int side)
        {
            Mode mode = model.Mode;
            if (mode.Player == 1 && side == 2)
                return 0;

            int[] sLane = new int[mode.ScratchKey.Length / (side == 0 ? 1 : mode.Player)];
            for (int i = (side == 2 ? sLane.Length : 0), index = 0; index < sLane.Length; i++)
            {
                sLane[index] = mode.ScratchKey[i];
                index++;
            }
            int[] nLane = new int[mode.Key - mode.ScratchKey.Length / (side == 0 ? 1 : mode.Player)];
            for (int i = 0, index = 0; index < nLane.Length; i++)
            {
                if (!mode.IsScratchKey(i))
                {
                    nLane[index] = i;
                    index++;
                }
            }

            int count = 0;
            foreach (Timeline tl in model.Timelines)
            {
                if (tl.Time >= start && tl.Time < end)
                {
                    switch (type)
                    {
                        case BMSModelUtilDefine.TotalNotes.ALL:
                            count += tl.GetTotalNotes(model.GetLnType());
                            break;
                        case BMSModelUtilDefine.TotalNotes.KEY:
                            foreach (int lane in nLane)
                            {
                                if (tl.ExistNote(lane) && (tl.GetNote(lane) is NormalNote))
                                    count++;
                            }
                            break;
                        case BMSModelUtilDefine.TotalNotes.LONG_KEY:
                            foreach (int lane in nLane)
                            {
                                if (tl.ExistNote(lane) && (tl.GetNote(lane) is LongNote))
                                {
                                    LongNote ln = (LongNote)tl.GetNote(lane);
                                    if (ln.Type == BMSModelDefine.LNTYPE.CHARGENOTE 
                                        || ln.Type == BMSModelDefine.LNTYPE.HELL_CHARGENOTE
                                        || (ln.Type == BMSModelDefine.LNTYPE.UNDEFINED && model.GetLnType() != BMSModelDefine.LNTYPE.LONGNOTE)
                                        || !ln.End)
                                    {
                                        count++;
                                    }
                                }
                            }
                            break;
                        case BMSModelUtilDefine.TotalNotes.SCRATCH:
                            foreach (int lane in sLane)
                            {
                                if (tl.ExistNote(lane) && (tl.GetNote(lane) is NormalNote))
                                    count++;
                            }
                            break;
                        case BMSModelUtilDefine.TotalNotes.LONG_SCRATCH:
                            foreach (int lane in sLane)
                            {
                                Note n = tl.GetNote(lane);
                                if (n is LongNote)
                                {
                                    LongNote ln = (LongNote)n;
                                    if (ln.Type == BMSModelDefine.LNTYPE.CHARGENOTE
                                        || ln.Type == BMSModelDefine.LNTYPE.HELL_CHARGENOTE
                                        || (ln.Type == BMSModelDefine.LNTYPE.UNDEFINED && model.GetLnType() != BMSModelDefine.LNTYPE.LONGNOTE)
                                        || !ln.End)
                                    {
                                        count++;
                                    }
                                }
                            }
                            break;
                        case BMSModelUtilDefine.TotalNotes.MINE:
                            foreach (int lane in nLane)
                            {
                                if (tl.ExistNote(lane) && (tl.GetNote(lane) is MineNote))
                                {
                                    count++;
                                }
                            }
                            foreach (int lane in sLane)
                            {
                                if (tl.ExistNote(lane) && (tl.GetNote(lane) is MineNote))
                                {
                                    count++;
                                }
                            }
                            break;
                    }
                }
            }

            return count;
        }
        public static double GetAverageNotesPerTime(BMSModel model, int start, int end)
        {
            return (double)GetTotalNotes(model, start, end) * 1000 / (end - start);
        }

        public static double GetMaxNotesPerTime(BMSModel model, int range)
        {
            int maxNotes = 0;
            Timeline[] tl = model.Timelines;
            for (int i = 0; i < tl.Length; i++)
            {
                int notes = 0;
                for (int j = 0; j < tl.Length && tl[j].Time < tl[i].Time + range; j++)
                {
                    notes += tl[j].GetTotalNotes(model.GetLnType());
                }
                maxNotes = (maxNotes < notes) ? notes : maxNotes;
            }

            return maxNotes;
        }

        // 쓰긴하나..? 검증 해봐야할듯.
        //public static long SetStartTime
    }
}
