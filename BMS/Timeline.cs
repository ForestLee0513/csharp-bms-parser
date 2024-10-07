namespace BMS
{
    public class Timeline
    {
        public double Time { get; private set; }
        public double Section { get; private set; }
        private Note[] notes = [];
        public Note[] Notes
        {
            get
            {
                return notes;
            }
        }
        private Note[] hiddenNotes = [];
        public Note[] HiddenNotes
        {
            get
            {
                return hiddenNotes;
            }
        }
        private Note[] bgNotes = [];
        public Note[] BgNotes
        {
            get
            {
                return bgNotes;
            }
        }
        public bool SectionLine { get; private set; } = false;
        public double Bpm { get; private set; }
        public double Stop { get; private set; }
        public double Scroll { get; private set; }
        public int Bga { get; private set; } = -1;
        public int Layer { get; private set; } = -1;
        public Layer[] EventLayer { get; private set; } = [];

        public Timeline(double section, double time, int noteSize)
        {
            Section = section;
            Time = time;
            notes = new Note[noteSize];
            hiddenNotes = new Note[noteSize];
        }

        public void SetTime(double time)
        {
            Time = time;
            foreach (Note n in notes)
            {
                n?.SetTime(time);
            }

            foreach (Note n in hiddenNotes)
            {
                n?.SetTime(time);
            }

            foreach (Note n in bgNotes)
            {
                n?.SetTime(time);
            }
        }

        public int GetTotalNotes()
        {
            return GetTotalNotes(BMSModelDefine.LNTYPE.LONGNOTE);
        }

        public int GetTotalNotes(BMSModelDefine.LNTYPE lntype)
        {
            int count = 0;
            foreach (Note note in notes)
            {
                if (note != null)
                {
                    if (note is LongNote)
                    {
                        LongNote ln = (LongNote)note;
                        if (
                            ln.Type == BMSModelDefine.LNTYPE.CHARGENOTE ||
                            ln.Type == BMSModelDefine.LNTYPE.HELL_CHARGENOTE ||
                            (ln.Type == BMSModelDefine.LNTYPE.UNDEFINED && lntype != BMSModelDefine.LNTYPE.LONGNOTE) ||
                            !ln.End)
                        {
                            count++;
                        }
                    } else if (note is NormalNote)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public bool ExistNote()
        {
            foreach (Note n in notes)
            {
                if (n != null)
                    return true;
            }

            return false;
        }

        public bool ExistNote(int lane)
        {
            return notes[lane] != null;
        }

        public Note GetNote(int lane)
        {
            return notes[lane];
        }

        public void SetNote(int lane, Note note)
        {
            notes[lane] = note;
            if (note == null)
                return;
            note.SetSection(Section);
            note.SetTime(Time);
        }

        public bool ExistHiddenNote()
        {
            foreach (Note note in hiddenNotes)
            {
                if (note != null)
                    return true;
            }

            return false;
        }

        public void SetHiddenNote(int lane, Note note)
        {
            hiddenNotes[lane] = note;
            if (note == null)
                return;
            note.SetSection(Section);
            note.SetTime(Time);
        }


        public void AddBackgroundNote(Note note)
        {
            if (note == null)
                return;
            note.SetSection(Section);
            note.SetTime(Time);
            Array.Resize(ref bgNotes, bgNotes.Length + 1);
            bgNotes[bgNotes.Length - 1] = note;
        }

        public void RemoveBackgroundNote(Note note)
        {
            for (int i = 0; i < bgNotes.Length; i++)
            {
                if (bgNotes[i] == note)
                {
                    Note[] newBg = new Note[bgNotes.Length - 1];
                    for (int j = 0, index = 0; j < bgNotes.Length; j++)
                    {
                        newBg[index] = bgNotes[j];
                        index++;
                    }
                    bgNotes = newBg;
                    break;
                }
            }
        }

        public void SetBga(int bga)
        {
            Bga = bga;
        }

        public void SetBpm(double bpm)
        {
            Bpm = bpm;
        }

        public void SetSectionLine(bool section)
        {
            SectionLine = section;
        }

        public void SetLayer(int layer)
        {
            Layer = layer;
        }

        public void SetEventLayer(Layer[] eventLayer)
        {
            EventLayer = eventLayer;
        }

        public void SetSection(double section)
        {
            foreach (Note n in notes)
            {
                n?.SetSection(section);
            }

            foreach (Note n in hiddenNotes)
            {
                n?.SetSection(section);
            }

            foreach (Note n in bgNotes)
            {
                n?.SetSection(section);
            }

            Section = section;
        }

        public void SetStop(double stop)
        {
            Stop = stop;
        }

        public void SetScroll(double scroll)
        {
            Scroll = scroll;
        }
    }
}
