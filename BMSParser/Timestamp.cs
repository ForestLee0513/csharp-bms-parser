using System.Collections.Generic;

namespace BMSParser
{
    public class Timestamp
    {
        public double Bpm { get; set; }
        public double StopDuration { get; set; }
        public double Time { get; set; }
        public double Scroll { get; set; }
        // 일반노트 //
        // P1
        private readonly Dictionary<int, List<NormalNote>> p1Lane = new Dictionary<int, List<NormalNote>>();
        public Dictionary<int, List<NormalNote>> P1Lane { get { return p1Lane; } }
        // P2
        private readonly Dictionary<int, List<NormalNote>> p2Lane = new Dictionary<int, List<NormalNote>>();
        public Dictionary<int, List<NormalNote>> P2Lane { get { return p2Lane; } }

        // 롱노트 //
        // P1
        private readonly Dictionary<int, List<LongNote>> p1LongLane = new Dictionary<int, List<LongNote>>();
        public Dictionary<int, List<LongNote>> P1LongLane { get { return p1LongLane; } }
        // P2
        private readonly Dictionary<int, List<LongNote>> p2LongLane = new Dictionary<int, List<LongNote>>();
        public Dictionary<int, List<LongNote>> P2LongLane { get { return p2LongLane; } }

        // 지뢰노트 //
        // P1
        private readonly Dictionary<int, List<MineNote>> p1MineLane = new Dictionary<int, List<MineNote>>();
        public Dictionary<int, List<MineNote>> P1MineLane { get { return p1MineLane; } }
        // P2
        private readonly Dictionary<int, List<MineNote>> p2MineLane = new Dictionary<int, List<MineNote>>();
        public Dictionary<int, List<MineNote>> P2MineLane { get { return p2MineLane; } }

        // 숨겨진 노트 //
        // P1
        private readonly Dictionary<int, List<HiddenNote>> p1HiddenLane = new Dictionary<int, List<HiddenNote>>();
        public Dictionary<int, List<HiddenNote>> P1HiddenLane { get { return p1HiddenLane; } }
        // P2
        private readonly Dictionary<int, List<HiddenNote>> p2HiddenLane = new Dictionary<int, List<HiddenNote>>();
        public Dictionary<int, List<HiddenNote>> P2HiddenLane { get { return p2HiddenLane; } }

        public Timestamp(double time)
        {
            Time = time;
        }
    }
}
