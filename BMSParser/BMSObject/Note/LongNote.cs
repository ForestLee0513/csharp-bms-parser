namespace BMSParser
{
    public class LongNote : Note
    {
        public double Pair { get; set; }

        public LongNote(int wav, double pair)
        {
            KeySound = wav;
            Pair = pair;
        }

        public LongNote(int wav)
        {
            KeySound = wav;
        }
    }
}
