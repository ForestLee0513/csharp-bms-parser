using System;
using static BMSParser.Define.TimeLine;

namespace BMSParser
{
    public class TimeLine
    {
        // Measure를 1P / 2P를 구분해야 할 듯?
        // BMS 스펙 상에서 보면 11 ~ 1Z까지 확장되는 경우가 있는데, 모든 이벤트로 분리하지 않더라고 1P와 2P는 분리해야지 확장성 챙길 수 있을 듯 함.
        private Measure[] measures = new Measure[0];
        public Measure[] Measures { get { return measures; } }
        public bool[] assignedKeys = new bool[0];

        /// <summary>
        /// 해당 마디가 있는지 확인하고 나서 없으면 해당 마디 + 1 만큼 확장
        /// </summary>
        /// <param name="measure">확인하고 싶은 마디</param>
        public void ExtendMeasure(int measure)
        {
            // 마디가 생성되지 않았으면 생성
            if (measures.Length < measure + 1)
            {
                Array.Resize(ref measures, measure + 1);
            }

            if (measures[measure] == null)
            {
                measures[measure] = new Measure();
            }
        }

        // BPM 이벤트 추가 하기 전 마지막 BPM 확인
        private double GetLastChangedBpm(int measure)
        {
            double bpm = 0;
            for (int i = 0; i < measure + 1; i++)
            {
                Measure previousMeasure = measures[i];

                if (previousMeasure != null)
                {
                    if (previousMeasure.BpmEvent.Length > 0)
                    {
                        bpm = previousMeasure.BpmEvent[previousMeasure.BpmEvent.Length - 1] != null ? previousMeasure.BpmEvent[previousMeasure.BpmEvent.Length - 1].Bpm : 0;
                    }
                }
            }

            return bpm;
        }
        
        // Measure와 BPM을 순회하면서 원하는 타이밍 이전의 BPM을 확인
        //private double GetLastChangedBpm(double timing)
        //{

        //}

        // Model 가져오기.
        public void AddBMSObject(int measure, int channel, string value, BMSModel model)
        {
            ExtendMeasure(measure);
            int totalBeat = value.Length / 2;

            for (int i = 0; i <  value.Length - 1; i += 2) 
            {
                int beat = i / 2;
                string encodedValue = value.Substring(i, 2);
                int decodedValue = (int)Util.Decode.DecodeBase36(encodedValue);

                // Event Channel //
                if ((Channel)channel == Channel.BGM)
                {

                }

                if ((Channel)channel == Channel.MEASURE_BEAT)
                {
                    float measureBeat = float.Parse(value);
                    measures[measure].SetBeat(measureBeat);
                }

                if ((Channel)channel == Channel.BPM_CHANGE)
                {

                    // Cases //
                    // 1. 16진수로 파싱한 값이 255이하이면서 bpm 테이블에 없는 경우 - 16진수 처리
                    // 2. 16진수로 파싱한 값이 255이하이지만 bpm 테이블에 있는 경우 - 36진수 처리
                    // 3. 16진수로 파싱이 안되는 경우 - 36진수 처리
                    
                    // IMPORTANT //
                    // 기존에 가산됐던 시간도 같이 넣어줘야됨!!!!!
                    if (decodedValue > 0)
                    {
                        if (Util.Decode.IsBase16(encodedValue))
                        {
                            measures[measure].AddBPMChange(measure, beat, totalBeat, GetLastChangedBpm(measure), Util.Decode.DecodeBase16(encodedValue));
                            continue;
                        }
                        else
                        {
                            measures[measure].AddBPMChange(measure, beat, totalBeat, GetLastChangedBpm(measure), decodedValue);
                            continue;
                        }
                    }
                }

                if ((Channel)channel == Channel.BGA_BASE)
                {

                }

                if ((Channel)channel == Channel.BGA_POOR)
                {

                }

                if ((Channel)channel == Channel.BGA_LAYER)
                {

                }

                if ((Channel)channel == Channel.EXBPM)
                {

                }

                if ((Channel)channel == Channel.STOP)
                {

                }

                // Note Channel //
                // 1P Normal
                if (channel >= (int)Channel.P1_NORMAL_START_POS && channel < (int)Channel.P2_NORMAL_START_POS)
                {
                    int lane = channel - (int)Channel.P1_NORMAL_START_POS;
                    //double lastBpm = GetLastChangedBpm(measure);

                    //Console.WriteLine($"{beat} / {totalBeat}에 추가 하기 전 {measure}의 bpm은 {lastBpm}");

                    // 모드 구분을 위한 할당된 키 갱신
                    if (assignedKeys.Length < lane + 1)
                    {
                        Array.Resize(ref assignedKeys, lane + 1);
                    }
                    assignedKeys[lane] = true;

                    // 노트 추가
                }

                // 2P Normal
                if (channel >= (int)Channel.P2_NORMAL_START_POS && channel < (int)Channel.P1_INVISIBLE_START_POS)
                {
                    int lane = (channel - (int)Channel.P2_NORMAL_START_POS) + 10;

                    //모드 구분을 위한 할당된 키 갱신
                    if (assignedKeys.Length < lane + 1)
                    {
                        Array.Resize(ref assignedKeys, lane + 1);
                    }
                    assignedKeys[lane] = true;

                    // 노트 추가
                }

                // 1P Invisible
                if (channel >= (int)Channel.P1_INVISIBLE_START_POS && channel < (int)Channel.P2_INVISIBLE_START_POS)
                {
                    int lane = channel - (int)Channel.P1_INVISIBLE_START_POS;

                    // 모드 구분을 위한 할당된 키 갱신
                    if (assignedKeys.Length < lane + 1)
                    {
                        Array.Resize(ref assignedKeys, lane + 1);
                    }
                    assignedKeys[lane] = true;

                    // 노트 추가
                }

                // 2P Invisible
                if (channel >= (int)Channel.P2_INVISIBLE_START_POS && channel < (int)Channel.P1_LONGNOTE_START_POS)
                {
                    int lane = (channel - (int)Channel.P2_INVISIBLE_START_POS) + 10;

                    // 모드 구분을 위한 할당된 키 갱신
                    if (assignedKeys.Length < lane + 1)
                    {
                        Array.Resize(ref assignedKeys, lane + 1);
                    }
                    assignedKeys[lane] = true;

                    // 노트 추가
                }

                // 1P Longnote
                if (channel >= (int)Channel.P1_LONGNOTE_START_POS && channel < (int)Channel.P2_LONGNOTE_START_POS)
                {
                    int lane = channel - (int)Channel.P1_LONGNOTE_START_POS;

                    // 모드 구분을 위한 할당된 키 갱신
                    if (assignedKeys.Length < lane + 1)
                    {
                        Array.Resize(ref assignedKeys, lane + 1);
                    }
                    assignedKeys[lane] = true;

                    // 노트 추가
                }

                // 2P Longnote
                if (channel >= (int)Channel.P2_LONGNOTE_START_POS && channel < (int)Channel.P1_LANDMINE_START_POS)
                {
                    int lane = channel - (int)Channel.P2_LONGNOTE_START_POS + 10;

                    // 모드 구분을 위한 할당된 키 갱신
                    if (assignedKeys.Length < lane + 1)
                    {
                        Array.Resize(ref assignedKeys, lane + 1);
                    }
                    assignedKeys[lane] = true;

                    // 노트 추가
                }

                // 1P Land Mine
                if (channel >= (int)Channel.P1_LANDMINE_START_POS && channel < (int)Channel.P2_LANDMINE_START_POS)
                {
                    int lane = channel - (int)Channel.P1_LANDMINE_START_POS;

                    // 모드 구분을 위한 할당된 키 갱신
                    if (assignedKeys.Length < lane + 1)
                    {
                        Array.Resize(ref assignedKeys, lane + 1);
                    }
                    assignedKeys[lane] = true;

                    // 노트 추가
                }

                // 2P Land Mine
                if (channel >= (int)Channel.P2_LANDMINE_START_POS)
                {
                    int lane = channel - (int)Channel.P2_LANDMINE_START_POS + 10;

                    // 모드 구분을 위한 할당된 키 갱신
                    if (assignedKeys.Length < lane + 1)
                    {
                        Array.Resize(ref assignedKeys, lane + 1);
                    }
                    assignedKeys[lane] = true;

                    // 노트 추가
                }
            }
        }
    }
}