namespace BMSParser
{
    public class BPM : BMSObject
    {
        public double Bpm { get; }

        public BPM(float position, double bpm)
        {
            Position = position;
            Bpm = bpm;
        }
    }
}
