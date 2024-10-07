namespace BMS
{
    public class NormalNote : Note
    {
        public NormalNote(int wav)
        {
            SetWav(wav);
        }

        public NormalNote(int wav, double start, double duration)
        {
            SetWav(wav);
            SetStartTime(start);
            SetDuration(duration);
        }
    }
}
