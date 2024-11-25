namespace BMSParser
{
    public class LongNote : Note
    {
        public double PairBeat { get; set; }

        public LongNote(int wav)
        {
            KeySound = wav;
        }
    }
}
