namespace BMS
{
    [Serializable]
    public class Note : ICloneable
    {
        public double Section { get; private set; }
        public long Time { get; private set; }
        public int Wav { get; private set; }
        public long Start { get; private set; }
        public long Duration { get; private set; }
        public int State { get; private set; }
        public long PlayTime { get; private set; }
        public List<Note> LayeredNotes { get; private set; } = [];

        public void SetSection(double section) => Section = section;
        public void SetWav(int wav) => Wav = wav;
        public void SetState(int state) => State = state;
        public void SetStartTime(long time) => Start = time;
        public void SetDuration(long duration) => Duration = duration;
        public void SetPlayTime(int playtime) => PlayTime = playtime;
        public void SetTime(long time) => Time = time;

        public void AddLayeredNote(Note n)
        {
            if (n == null)
                return;
            n.SetSection(Section);
            n.SetTime(Time);
            LayeredNotes.Add(n);
        }

        public object Clone()
        {
            return (Note)MemberwiseClone();
        }
    }
}
