using System;
namespace BMSParser
{
    public class Measure
    {
        public float MeasureBeat { get; set; } = 1.0f; // 4/4가 기본값이므로 1.0f로 지정
        private BPM[] bpmEvent = new BPM[0];
        public BPM[] BpmEvent { get { return bpmEvent; } }
        public Stop[] StopEvent { get; set; } = new Stop[0];
        public NormalNote[] BGM { get; set; } = new NormalNote[0];
        public Note[,] Lane { get; set; } = new Note[0, 0];

        public void AddBGM(int beat, int wav)
        {

        }

        /// <summary>
        /// 해당 마디의 비트를 지정합니다.
        /// </summary>
        /// <param name="measureBeat">지정하고 싶은 비트의 수 (4/4 박자는 1.0)</param>
        public void SetBeat(float measureBeat) => MeasureBeat = measureBeat;

        /// <summary>
        /// 해당 마디의 비트에 bpm을 갱신하는 이벤트를 추가합니다.
        /// </summary>
        /// <param name="previousTiming">지금가지 누산된 시간</param> // 추가해야됨!!
        /// <param name="currentBeat">현재비트</param>
        /// <param name="totalBeat">총 비트</param>
        /// <param name="prevBpm">해당 이벤트를 추가 하기 전 기존의 BPM</param>
        /// <param name="targetBpm">설정하고자 하는 BPM</param>
        public void AddBPMChange(double previousTiming, int currentBeat, int totalBeat, double prevBpm, double targetBpm)
        {
            Array.Resize(ref bpmEvent, bpmEvent.Length + 1);
            int targetIndex = bpmEvent.Length - 1;
            float beatPosition = currentBeat / (totalBeat * MeasureBeat);
            double timing = beatPosition * (60 / prevBpm) * 1000;
            bpmEvent[targetIndex] = new BPM(timing, targetBpm);
        }

        public void AddBGA(int beat, int bmp)
        {

        }

        public void AddStopEvent(int beat, double stopDuration)
        {

        }

        /// <summary>
        /// 노트를 추가하는 매서드입니다.
        /// </summary>
        /// <param name="line">키 위치</param>
        /// <param name="beat">비트</param>
        /// <param name="bpm">기준BPM</param>
        /// <param name="wav">키음</param>
        public void AddNotes(int line, int beat, double bpm, int wav)
        {
            // 해당 마디에 bpm 이벤트가 있는지 확인 후 있다면 파라미터로 넘어온 값을 무시하고 이 마디에 있는 마지막 bpm이벤트를 기준으로 BPM을 지정
            if (BpmEvent.Length > 0)
            {
                BPM previousBpmEvent = BpmEvent[BpmEvent.Length - 1];
            }
        }
    }
}
