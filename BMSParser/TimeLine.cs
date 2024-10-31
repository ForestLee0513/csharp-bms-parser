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

        public void AddBMSObject(int measure, int channel, string value)
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

            for (int i = 0; i <  value.Length - 1; i += 2) 
            {
                int beat = i / 2;
                int DecodedValue = (int)Util.Decode.DecodeBase36(value.Substring(i, 2));

                // Event Channel //
                if ((Channel)channel == Channel.BGM)
                {

                }

                if ((Channel)channel == Channel.BEAT)
                {

                }

                if ((Channel)channel == Channel.BPM_CHANGE)
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