namespace BMS
{
    public class NormalNote : Note
    {
        public NormalNote(int wav)
        {
            SetWav(wav);
        }

        public NormalNote(int wav, long start, long duration)
        {
            SetWav(wav);
            SetStartTime(start);
            SetDuration(duration);
        }
    }
}
