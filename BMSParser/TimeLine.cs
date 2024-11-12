using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using static BMSParser.Define.TimeLine;

namespace BMSParser
{
    public class TimeLine
    {
        // Measure를 1P / 2P를 구분해야 할 듯?
        // BMS 스펙 상에서 보면 11 ~ 1Z까지 확장되는 경우가 있는데, 모든 이벤트로 분리하지 않더라고 1P와 2P는 분리해야지 확장성 챙길 수 있을 듯 함.
        private Measure[] measures = new Measure[0];
        public Measure[] Measures { get { return measures; } }
        public bool[] assigned1PKeys = new bool[0];
        public bool[] assigned2PKeys = new bool[0];

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

            for (int i = 0; i < measure + 1; i++)
            {
                if (measures[i] == null)
                {
                    measures[measure] = new Measure();
                }
            }
        }

        /// <summary>
        /// 지금까지 추가된 총 비트의 수를 값으로 반환
        /// </summary>
        /// <param name="measure">확인하고 싶은 마디의 마지막 범위</param>
        /// <returns></returns>
        public float GetPreviousTotalScales(int measure)
        {
            float sum = 0;
            for (int i = 0; i < measure; i++)
            {
                sum += measures[i].Scale;
            }

            return sum;
        }

        public double GetBPM(int measureToLookUp, double beatToLookUp)
        {
            double bpm = 0;

            for (int i = 0; i <= measureToLookUp; i++)
            {
                foreach (KeyValuePair<double, BPM> bpmEvent in measures[i].BpmEvents)
                {
                    if (measureToLookUp == i && beatToLookUp.CompareTo(bpmEvent.Key) < 0)
                        break;

                    bpm = bpmEvent.Value.Bpm;
                }
            }

            return bpm;
        }

        /// <summary>
        /// BMS 이벤트의 비트 추가
        /// </summary>
        /// <param name="measure">마디</param>
        /// <param name="channel">채널</param>
        /// <param name="value">값</param>
        /// <param name="model">참조하고 싶은 BMS모델</param>
        public void AssignObjectsBeat(int measure, int channel, string value, BMSModel model)
        {
            ExtendMeasure(measure);
            int totalBeat = value.Length / 2;

            for (int i = 0; i < value.Length - 1; i += 2)
            {
                string splittedValue = value.Substring(i, 2);

                if ((Channel)channel == Channel.MEASURE_BEAT)
                {
                    float measureBeat = float.Parse(value);
                    measures[measure].SetScale(measureBeat);
                }

                float beat = (1.0f * i / 2 / totalBeat) * measures[measure].Scale;

                if ((Channel)channel == Channel.BGM)
                {
                    if (splittedValue == "00")
                        continue;

                    if (model.Base == Define.BMSModel.Base.BASE62)
                    {
                        measures[measure].AddBGM(beat, Util.Decode.DecodeBase62(splittedValue));
                    }
                    else
                    {
                        measures[measure].AddBGM(beat, Util.Decode.DecodeBase36(splittedValue));
                    }
                }

                if ((Channel)channel == Channel.BPM_CHANGE)
                {
                    if (splittedValue == "00")
                        continue;

                    if (Util.Decode.DecodeBase16(splittedValue) < 256)
                    {
                        measures[measure].AddBPMEvent(beat, Util.Decode.DecodeBase16(splittedValue));
                    }
                }

                if ((Channel)channel == Channel.BGA_BASE)
                {
                    if (model.Base == Define.BMSModel.Base.BASE62)
                    {
                        measures[measure].AddBGA(beat, Util.Decode.DecodeBase62(splittedValue), Define.BMSObject.BGA.BASE);
                    }
                    else
                    {
                        measures[measure].AddBGA(beat, Util.Decode.DecodeBase36(splittedValue), Define.BMSObject.BGA.BASE);
                    }
                }

                if ((Channel)channel == Channel.BGA_POOR)
                {
                    if (model.Base == Define.BMSModel.Base.BASE62)
                    {
                        measures[measure].AddBGA(beat, Util.Decode.DecodeBase62(splittedValue), Define.BMSObject.BGA.POOR);
                    }
                    else
                    {
                        measures[measure].AddBGA(beat, Util.Decode.DecodeBase36(splittedValue), Define.BMSObject.BGA.POOR);
                    }
                }

                if ((Channel)channel == Channel.BGA_LAYER)
                {
                    if (model.Base == Define.BMSModel.Base.BASE62)
                    {
                        measures[measure].AddBGA(beat, Util.Decode.DecodeBase62(splittedValue), Define.BMSObject.BGA.LAYER);
                    }
                    else
                    {
                        measures[measure].AddBGA(beat, Util.Decode.DecodeBase36(splittedValue), Define.BMSObject.BGA.LAYER);
                    }
                }

                if ((Channel)channel == Channel.EXBPM)
                {
                    if (splittedValue == "00")
                        continue;

                    if (model.Base == Define.BMSModel.Base.BASE62)
                    {
                        measures[measure].AddBPMEvent(beat, model.BpmList[Util.Decode.DecodeBase62(splittedValue)]);
                    }
                    else
                    {
                        measures[measure].AddBPMEvent(beat, model.BpmList[Util.Decode.DecodeBase36(splittedValue)]);
                    }
                }

                if ((Channel)channel == Channel.STOP)
                {
                    if (splittedValue == "00")
                    {
                        continue;
                    }

                    if (model.Base == Define.BMSModel.Base.BASE62)
                    {
                        measures[measure].AddStopEvent(beat, model.BpmList[Util.Decode.DecodeBase62(splittedValue)]);
                    }
                    else
                    {
                        measures[measure].AddStopEvent(beat, model.BpmList[Util.Decode.DecodeBase36(splittedValue)]);
                    }
                }

                // Note Channel //
                // 1P Normal
                if (channel >= (int)Channel.P1_NORMAL_START_POS && channel < (int)Channel.P2_NORMAL_START_POS)
                {
                    if (splittedValue == "00")
                        continue;

                    int lane = channel - (int)Channel.P1_NORMAL_START_POS;

                    // 모드 구분을 위한 할당된 키 갱신
                    if (assigned1PKeys.Length < lane + 1)
                    {
                        Array.Resize(ref assigned1PKeys, lane + 1);
                    }
                    assigned1PKeys[lane] = true;

                    // 노트 추가
                }

                // 2P Normal
                if (channel >= (int)Channel.P2_NORMAL_START_POS && channel < (int)Channel.P1_INVISIBLE_START_POS)
                {
                    if (splittedValue == "00")
                        continue;

                    int lane = channel - (int)Channel.P2_NORMAL_START_POS;

                    //모드 구분을 위한 할당된 키 갱신
                    if (assigned2PKeys.Length < lane + 1)
                    {
                        Array.Resize(ref assigned2PKeys, lane + 1);
                    }
                    assigned2PKeys[lane] = true;

                    // 노트 추가
                }

                // 1P Invisible
                if (channel >= (int)Channel.P1_INVISIBLE_START_POS && channel < (int)Channel.P2_INVISIBLE_START_POS)
                {
                    if (splittedValue == "00")
                        continue;

                    int lane = channel - (int)Channel.P1_INVISIBLE_START_POS;

                    // 모드 구분을 위한 할당된 키 갱신
                    if (assigned1PKeys.Length < lane + 1)
                    {
                        Array.Resize(ref assigned1PKeys, lane + 1);
                    }
                    assigned1PKeys[lane] = true;

                    // 노트 추가
                }

                // 2P Invisible
                if (channel >= (int)Channel.P2_INVISIBLE_START_POS && channel < (int)Channel.P1_LONGNOTE_START_POS)
                {
                    if (splittedValue == "00")
                        continue;

                    int lane = (channel - (int)Channel.P2_INVISIBLE_START_POS);

                    // 모드 구분을 위한 할당된 키 갱신
                    if (assigned2PKeys.Length < lane + 1)
                    {
                        Array.Resize(ref assigned2PKeys, lane + 1);
                    }
                    assigned2PKeys[lane] = true;

                    // 노트 추가
                }

                // 1P Longnote
                if (channel >= (int)Channel.P1_LONGNOTE_START_POS && channel < (int)Channel.P2_LONGNOTE_START_POS)
                {
                    if (splittedValue == "00")
                        continue;

                    int lane = channel - (int)Channel.P1_LONGNOTE_START_POS;

                    // 모드 구분을 위한 할당된 키 갱신
                    if (assigned1PKeys.Length < lane + 1)
                    {
                        Array.Resize(ref assigned1PKeys, lane + 1);
                    }
                    assigned1PKeys[lane] = true;

                    // 노트 추가
                }

                // 2P Longnote
                if (channel >= (int)Channel.P2_LONGNOTE_START_POS && channel < (int)Channel.P1_LANDMINE_START_POS)
                {
                    if (splittedValue == "00")
                        continue;

                    int lane = channel - (int)Channel.P2_LONGNOTE_START_POS;

                    // 모드 구분을 위한 할당된 키 갱신
                    if (assigned2PKeys.Length < lane + 1)
                    {
                        Array.Resize(ref assigned2PKeys, lane + 1);
                    }
                    assigned2PKeys[lane] = true;

                    // 노트 추가
                }

                // 1P Land Mine
                if (channel >= (int)Channel.P1_LANDMINE_START_POS && channel < (int)Channel.P2_LANDMINE_START_POS)
                {
                    if (splittedValue == "00")
                        continue;

                    int lane = channel - (int)Channel.P1_LANDMINE_START_POS;

                    // 모드 구분을 위한 할당된 키 갱신
                    if (assigned1PKeys.Length < lane + 1)
                    {
                        Array.Resize(ref assigned1PKeys, lane + 1);
                    }
                    assigned1PKeys[lane] = true;

                    // 노트 추가
                }

                // 2P Land Mine
                if (channel >= (int)Channel.P2_LANDMINE_START_POS)
                {
                    if (splittedValue == "00")
                        continue;

                    int lane = channel - (int)Channel.P2_LANDMINE_START_POS;

                    // 모드 구분을 위한 할당된 키 갱신
                    if (assigned2PKeys.Length < lane + 1)
                    {
                        Array.Resize(ref assigned2PKeys, lane + 1);
                    }
                    assigned2PKeys[lane] = true;

                    // 노트 추가
                }
            }
        }
    }
}