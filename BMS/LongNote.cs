namespace BMS
{
    public class LongNote : Note
    {
        public bool End { get; private set; }
        public LongNote Pair { get; private set; }
        public BMSModelDefine.LNTYPE Type { get; private set; }

        public LongNote(int wav) => SetWav(wav);
        public LongNote(int wav, long startTime, long duration)
        {
            SetWav(wav);
            SetStartTime(startTime);
            SetDuration(duration);
        }

        public void SetPair(LongNote pair)
        {
            pair.Pair = this;
            Pair = pair;

            pair.End = pair.Section > Section;
            End = !pair.End;
            Type = pair.Type = (Type != BMSModelDefine.LNTYPE.UNDEFINED ? Type : pair.Type);
        }

        public void SetType(BMSModelDefine.LNTYPE lnType)
        {
            Type = lnType;
        }

        public new object Clone()
        {
            return Clone(true);
        }

        private object Clone(bool copyPair) 
        {
            LongNote ln = (LongNote)base.Clone();
            if (copyPair)
            {
                ln.SetPair((LongNote)Pair.Clone(false));
            }

            return ln;
        }
    }
}
