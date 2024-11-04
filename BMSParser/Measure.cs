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

        public double GetLastBpmChangeTiming()
        {
            double lastTiming = 0;

            if (bpmEvent.Length > 1) 
            { 
                for (int i = 0; i < bpmEvent.Length; i++) 
                {
                    lastTiming += bpmEvent[i].Timing;
                }
            }
            else
            {
                lastTiming = 0;
            }

            return lastTiming;
        }
        
        /// <summary>
        /// 이게맞는결과물 BPM이벤트 추가하기
        /// </summary>
        /// <param name="measure"></param>
        /// <param name="currentBeat"></param>
        /// <param name="totalBeat"></param>
        /// <param name="prevBpm"></param>
        /// <param name="targetBpm"></param>
        public void AddBPMChange(int measure, int currentBeat, int totalBeat, double prevBpm, double targetBpm)
        {
            AddBPMChange(currentBeat, totalBeat, prevBpm, targetBpm);
            // DEBUG //
            float calculatedBeat = (float)currentBeat / totalBeat * 4.0f * MeasureBeat;
            float totalMeasureBeat = 4 * MeasureBeat;
            double maxTiming = 60000 / prevBpm;

            float position = calculatedBeat / totalMeasureBeat;
            double timing = maxTiming * (position);
            Console.WriteLine($"{measure}번의 비트는 {MeasureBeat} / {measure}번째에 추가할 이벤트의 비트는 {calculatedBeat} / {totalMeasureBeat} = {position}");
            Console.WriteLine($"{measure}번에 추가될 시간은 {timing} / 기존 bpm은 {prevBpm} / 변경될 bpm은 {targetBpm}");
            Console.WriteLine($"-----------------------------------------------------------------------------------------");
        }

        public void AddBPMChange(int currentBeat, int totalBeat, double prevBpm, double targetBpm)
        {
            Array.Resize(ref bpmEvent, bpmEvent.Length + 1);
            int targetIndex = bpmEvent.Length - 1;
            float calculatedBeat = (float)currentBeat / totalBeat * 4.0f * MeasureBeat;
            float totalMeasureBeat = 4 * MeasureBeat;
            double maxTiming = 60000 / prevBpm;

            float position = calculatedBeat / totalMeasureBeat;
            double timing = maxTiming * (position);

            bpmEvent[targetIndex] = new BPM(position, targetBpm);
        }

        public void AddBGA(int currentBeat, int totalBeat, int bmp)
        {
            float position = currentBeat / (totalBeat * MeasureBeat);
        }

        public void AddStopEvent(int beat, double stopDuration)
        {

        }

        public void AddNotes(int line, int currentBeat, int totalBeat, double bpm, int wav)
        {
            
        }
    }
}
