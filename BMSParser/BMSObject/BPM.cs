namespace BMSParser
{
    public class BPM : BMSObject
    {
        public double Bpm { get; }

        public BPM(double timing, double bpm)
        { 
            Timing = timing;
            Bpm = bpm;
        }
    }
}
