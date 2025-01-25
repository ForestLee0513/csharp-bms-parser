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
        public Dictionary<int, List<NormalNote>> P1Lane { get; set; } = new Dictionary<int, List<NormalNote>>();
        // P2
        public Dictionary<int, List<NormalNote>> P2Lane { get; set; } = new Dictionary<int, List<NormalNote>>();

        // 롱노트 //
        // P1
        public Dictionary<int, List<LongNote>> P1LongLane { get; set; } = new Dictionary<int, List<LongNote>>();
        // P2
        public Dictionary<int, List<LongNote>> P2LongLane { get; set; } = new Dictionary<int, List<LongNote>>();

        // 지뢰노트 //
        // P1
        public Dictionary<int, List<MineNote>> P1MineLane { get; set; } = new Dictionary<int, List<MineNote>>();
        // P2
        public Dictionary<int, List<MineNote>> P2MineLane { get; set; } = new Dictionary<int, List<MineNote>>();

        // 숨겨진 노트 //
        // P1
        public Dictionary<int, List<HiddenNote>> P1HiddenLane { get; set; } = new Dictionary<int, List<HiddenNote>>();
        // P2
        public Dictionary<int, List<HiddenNote>> P2HiddenLane { get; set; } = new Dictionary<int, List<HiddenNote>>();

        public List<NormalNote> BGM { get; set; } = new List<NormalNote>();

        public BGA BaseBGA { get; set; }
        public BGA LayerBGA { get; set; }
        public BGA PoorBGA { get; set; }

        public Timestamp(double time)
        {
            Time = time;
        }
    }
}
