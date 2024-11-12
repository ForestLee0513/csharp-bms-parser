namespace BMSParser
{
    public class NormalNote : Note
    {
        public NormalNote(int wav)
        {
            KeySound = wav;
        }

        public NormalNote(float beat, int wav)
        {
            Beat = beat;
            KeySound = wav;
        }
    }
}
