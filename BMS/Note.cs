using System;

namespace BMS
{
    [Serializable]
    public class Note : ICloneable
    {
        public double Section { get; private set; }
        public double Time { get; private set; }
        public int Wav { get; private set; }
        public double Start { get; private set; }
        public double Duration { get; private set; }
        public int State { get; private set; }
        public double PlayTime { get; private set; }
        private Note[] layeredNotes = new Note[0];
        public Note[] LayeredNotes {
            get 
            {
                return layeredNotes;
            }
        }

        public void SetSection(double section) => Section = section;
        public void SetWav(int wav) => Wav = wav;
        public void SetState(int state) => State = state;
        public void SetStartTime(double time) => Start = time;
        public void SetDuration(double duration) => Duration = duration;
        public void SetPlayTime(int playtime) => PlayTime = playtime;
        public void SetTime(double time) => Time = time;

        public void AddLayeredNote(Note n)
        {
            if (n == null)
                return;
            n.SetSection(Section);
            n.SetTime(Time);
            Array.Resize(ref layeredNotes, layeredNotes.Length + 1);
            layeredNotes[layeredNotes.Length - 1] = n;
        }

        public object Clone()
        {
            return (Note)MemberwiseClone();
        }
    }
}
