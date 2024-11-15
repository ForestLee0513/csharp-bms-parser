namespace BMSParser
{
    public class Scroll : BMSObject
    {
        public double ScrollValue { get; set; }

        public Scroll(double scrollValue)
        {
            ScrollValue = scrollValue;
        }
    }
}
