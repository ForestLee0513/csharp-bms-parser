﻿using System;
using System.Collections.Generic;
using System.Linq;
using static BMSParser.Define.PatternProcessor;

namespace BMSParser
{
    public class PatternProcessor
    {
        private Measure[] measures = new Measure[0];
        public Measure[] Measures { get { return measures; } }
        public bool[] assigned1PKeys = new bool[0];
        public bool[] assigned2PKeys = new bool[0];

        // 시간 흐름을 비트로 구분
        public SortedDictionary<double, Timestamp> Timestamp { get; } = new SortedDictionary<double, Timestamp>();

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
        public float GetPreviousSection(int measure)
        {
            float sum = 0;
            for (int i = 0; i < measure; i++)
            {
                //sum += measures[i].Scale * 4.0f;
                sum += measures[i].Scale;
            }

            return sum;
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

            for (int i = 0; i < value.Length - 1; i += 2)
            {
                string splittedValue = value.Substring(i, 2);

                if ((Channel)channel == Channel.MEASURE_BEAT)
                {
                    float measureBeat = float.Parse(value);
                    measures[measure].SetScale(measureBeat);
                }

                double pos = (1.0 * i / 2) / (value.Length / 2);

                if ((Channel)channel == Channel.BGM)
                {
                    if (splittedValue == "00")
                        continue;

                    if (model.Base == Define.BMSModel.Base.BASE62)
                    {
                        measures[measure].AddBGM(pos, Util.Decode.DecodeBase62(splittedValue));
                    }
                    else
                    {
                        measures[measure].AddBGM(pos, Util.Decode.DecodeBase36(splittedValue));
                    }
                }

                if ((Channel)channel == Channel.BPM_CHANGE)
                {
                    if (splittedValue == "00")
                        continue;

                    if (Util.Decode.DecodeBase16(splittedValue) < 256)
                    {
                        measures[measure].AddBPMEvent(pos, Util.Decode.DecodeBase16(splittedValue));
                    }
                }

                if ((Channel)channel == Channel.SCROLL)
                {
                    if (model.Base == Define.BMSModel.Base.BASE62)
                    {
                        measures[measure].AddScroll(pos, model.ScrollList[Util.Decode.DecodeBase62(splittedValue)]);
                    }
                    else
                    {
                        measures[measure].AddScroll(pos, model.ScrollList[Util.Decode.DecodeBase36(splittedValue)]);
                    }
                }

                if ((Channel)channel == Channel.BGA_BASE)
                {
                    if (model.Base == Define.BMSModel.Base.BASE62)
                    {
                        measures[measure].AddBGA(pos, Util.Decode.DecodeBase62(splittedValue), Define.BMSObject.BGA.BASE);
                    }
                    else
                    {
                        measures[measure].AddBGA(pos, Util.Decode.DecodeBase36(splittedValue), Define.BMSObject.BGA.BASE);
                    }
                }

                if ((Channel)channel == Channel.BGA_POOR)
                {
                    if (model.Base == Define.BMSModel.Base.BASE62)
                    {
                        measures[measure].AddBGA(pos, Util.Decode.DecodeBase62(splittedValue), Define.BMSObject.BGA.POOR);
                    }
                    else
                    {
                        measures[measure].AddBGA(pos, Util.Decode.DecodeBase36(splittedValue), Define.BMSObject.BGA.POOR);
                    }
                }

                if ((Channel)channel == Channel.BGA_LAYER)
                {
                    if (model.Base == Define.BMSModel.Base.BASE62)
                    {
                        measures[measure].AddBGA(pos, Util.Decode.DecodeBase62(splittedValue), Define.BMSObject.BGA.LAYER);
                    }
                    else
                    {
                        measures[measure].AddBGA(pos, Util.Decode.DecodeBase36(splittedValue), Define.BMSObject.BGA.LAYER);
                    }
                }

                if ((Channel)channel == Channel.EXBPM)
                {
                    if (splittedValue == "00")
                        continue;

                    if (model.Base == Define.BMSModel.Base.BASE62)
                    {
                        measures[measure].AddBPMEvent(pos, model.BpmList[Util.Decode.DecodeBase62(splittedValue)]);
                    }
                    else
                    {
                        measures[measure].AddBPMEvent(pos, model.BpmList[Util.Decode.DecodeBase36(splittedValue)]);
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
                        measures[measure].AddStopEvent(pos, model.StopList[Util.Decode.DecodeBase62(splittedValue)]);
                    }
                    else
                    {
                        measures[measure].AddStopEvent(pos, model.StopList[Util.Decode.DecodeBase36(splittedValue)]);
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
    
        /// <summary>
        /// 타이밍 계산
        /// </summary>
        public void CalculateTiming()
        {
            for (int i = 0; i < measures.Length; i++)
            {
                IEnumerator<KeyValuePair<double, Stop>> stops = measures[i].StopEvents.GetEnumerator();
                KeyValuePair<double, Stop>? ste = stops.MoveNext() ? stops.Current : (KeyValuePair<double, Stop>?)null;

                IEnumerator<KeyValuePair<double, BPM>> bpms = measures[i].BpmEvents.GetEnumerator();
                KeyValuePair<double, BPM>? bce = bpms.MoveNext() ? bpms.Current : (KeyValuePair<double, BPM>?)null;

                IEnumerator<KeyValuePair<double, Scroll>> scrolls = measures[i].ScrollEvents.GetEnumerator();
                KeyValuePair<double, Scroll>? sce = scrolls.MoveNext() ? scrolls.Current : (KeyValuePair<double, Scroll>?)null;

                double previousSection = GetPreviousSection(i);

                while (ste.HasValue || bce.HasValue || sce.HasValue)
                {
                    double bc = bce.HasValue ? bce.Value.Key : 2;
                    double st = ste.HasValue ? ste.Value.Key : 2;
                    double sc = sce.HasValue ? sce.Value.Key : 2;

                    if (sc <= st && sc <= bc)
                    {
                        double section = previousSection + bc * measures[i].Scale;
                        Timestamp timestamp = GetTimestamp(section);
                        timestamp.Scroll = sce.Value.Value.ScrollValue;
                        sce = scrolls.MoveNext() ? scrolls.Current : (KeyValuePair<double, Scroll>?)null;
                    }
                    else if (bc <= st)
                    {
                        double section = previousSection + bc * measures[i].Scale;
                        Timestamp timestamp = GetTimestamp(section);
                        timestamp.Bpm = bce.Value.Value.Bpm;
                        bce = bpms.MoveNext() ? bpms.Current : (KeyValuePair<double, BPM>?)null;
                    }
                    else if (st <= 1)
                    {
                        double section = previousSection + ste.Value.Key * measures[i].Scale;
                        Timestamp timestamp = GetTimestamp(section);
                        timestamp.StopDuration = 1000.0 * 60 * 4 * ste.Value.Value.Duration / timestamp.Bpm;
                        ste = stops.MoveNext() ? stops.Current : (KeyValuePair<double, Stop>?)null;
                    }
                }
            }
        }

        public Timestamp GetTimestamp(double section)
        {
            if (Timestamp.ContainsKey(section))
            {
                return Timestamp[section];
            }

            var le = Timestamp.Where(x => x.Key < section).OrderByDescending(x => x.Key).FirstOrDefault();

            double scroll = le.Value.Scroll;
            double bpm = le.Value.Bpm;
            double time = le.Value.Time + le.Value.StopDuration + (240000.0 * (section - le.Key)) / bpm;

            Timestamp newTimestamp = new Timestamp(time)
            {
                Scroll = scroll,
                Bpm = bpm
            };

            Timestamp[section] = newTimestamp;

            return newTimestamp;
        }
    }
}