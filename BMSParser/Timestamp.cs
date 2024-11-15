using System.Collections.Generic;

namespace BMSParser
{
    public class Timestamp
    {
        public double Bpm { get; set; }
        public double StopDuration { get; set; }
        public double Time { get; set; }
        public double Scroll { get; set; }

        private readonly Dictionary<int, SortedDictionary<double, Note>> p1Lane = new Dictionary<int, SortedDictionary<double, Note>>();
        public Dictionary<int, SortedDictionary<double, Note>> P1Lane { get { return p1Lane; } }

        private readonly Dictionary<int, SortedDictionary<double, Note>> p2Lane = new Dictionary<int, SortedDictionary<double, Note>>();
        public Dictionary<int, SortedDictionary<double, Note>> P2Lane { get { return p2Lane; } }

        public Timestamp(double time)
        {
            Time = time;
        }
    }
}
