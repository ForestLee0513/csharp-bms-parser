namespace BMSParser
{
    public class Stop : BMSObject
    {
        public double Duration { get; }

        public Stop(double duration) 
        {
            Duration = duration;
        }
    }
}
