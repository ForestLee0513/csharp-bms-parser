using System;
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
                double pos = (1.0 * i / 2) / (value.Length / 2);

                // 이벤트 채널 //
                switch ((Channel)channel)
                {
                    case Channel.MEASURE_BEAT:
                        float measureBeat = float.Parse(value);
                        measures[measure].SetScale(measureBeat);
                        break;
                    case Channel.BGM:
                        if (splittedValue == "00")
                            break;

                        if (model.Base == Define.BMSModel.Base.BASE62)
                        {
                            measures[measure].AddBGM(pos, Util.Decode.DecodeBase62(splittedValue));
                        }
                        else
                        {
                            measures[measure].AddBGM(pos, Util.Decode.DecodeBase36(splittedValue));
                        }
                        break;
                    case Channel.BPM_CHANGE:
                        if (splittedValue == "00")
                            break;

                        if (Util.Decode.DecodeBase16(splittedValue) < 256)
                        {
                            measures[measure].AddBPMEvent(pos, Util.Decode.DecodeBase16(splittedValue));
                        }
                        break;
                    case Channel.SCROLL:
                        if (model.Base == Define.BMSModel.Base.BASE62)
                        {
                            measures[measure].AddScroll(pos, model.ScrollList[Util.Decode.DecodeBase62(splittedValue)]);
                        }
                        else
                        {
                            measures[measure].AddScroll(pos, model.ScrollList[Util.Decode.DecodeBase36(splittedValue)]);
                        }
                        break;
                    case Channel.BGA_BASE:
                    case Channel.BGA_LAYER:
                    case Channel.BGA_POOR:
                        if (model.Base == Define.BMSModel.Base.BASE62)
                        {
                            measures[measure].AddBGA(pos, Util.Decode.DecodeBase62(splittedValue), (Define.BMSObject.BGA)channel);
                        }
                        else
                        {
                            measures[measure].AddBGA(pos, Util.Decode.DecodeBase36(splittedValue), (Define.BMSObject.BGA)channel);
                        }
                        break;
                    case Channel.EXBPM:
                        if (splittedValue == "00")
                            break;

                        if (model.Base == Define.BMSModel.Base.BASE62)
                        {
                            measures[measure].AddBPMEvent(pos, model.BpmList[Util.Decode.DecodeBase62(splittedValue)]);
                        }
                        else
                        {
                            measures[measure].AddBPMEvent(pos, model.BpmList[Util.Decode.DecodeBase36(splittedValue)]);
                        }
                        break;
                    case Channel.STOP:
                        if (splittedValue == "00")
                        {
                            break;
                        }

                        if (model.Base == Define.BMSModel.Base.BASE62)
                        {
                            measures[measure].AddStopEvent(pos, model.StopList[Util.Decode.DecodeBase62(splittedValue)]);
                        }
                        else
                        {
                            measures[measure].AddStopEvent(pos, model.StopList[Util.Decode.DecodeBase36(splittedValue)]);
                        }
                        break;
                }

                // 1P Normal
                if (channel >= (int)Channel.P1_NORMAL_START_POS && channel < (int)Channel.P2_NORMAL_START_POS)
                {
                    if (splittedValue == "00")
                        continue;

                    int line = channel - (int)Channel.P1_NORMAL_START_POS;

                    // 모드 구분을 위한 할당된 키 갱신
                    if (assigned1PKeys.Length < line + 1)
                    {
                        Array.Resize(ref assigned1PKeys, line + 1);
                    }
                    assigned1PKeys[line] = true;

                    if ((line == 7 || line == 8) && model.Mode == BMSKey.BMS_5K_ONLY)
                    {
                        model.Mode = BMSKey.BMS_7K_ONLY;
                    }

                    if (line == 5 && model.Mode == BMSKey.BMS_5K_ONLY || model.Mode == BMSKey.BMS_7K_ONLY)
                    {
                        model.Mode = model.Mode == BMSKey.BMS_5K_ONLY ? BMSKey.BMS_5K : BMSKey.BMS_7K;
                    }

                    // 노트 추가
                    if (model.Base == Define.BMSModel.Base.BASE62)
                    {
                        measures[measure].AddNotes(line, pos, Util.Decode.DecodeBase62(splittedValue), Define.BMSObject.PlayerSide.P1);
                    }
                    else
                    {
                        measures[measure].AddNotes(line, pos, Util.Decode.DecodeBase36(splittedValue), Define.BMSObject.PlayerSide.P1);
                    }
                }

                // 2P Normal
                if (channel >= (int)Channel.P2_NORMAL_START_POS && channel < (int)Channel.P1_INVISIBLE_START_POS)
                {
                    if (splittedValue == "00")
                        continue;

                    int line = channel - (int)Channel.P2_NORMAL_START_POS;

                    //모드 구분을 위한 할당된 키 갱신
                    if (assigned2PKeys.Length < line + 1)
                    {
                        Array.Resize(ref assigned2PKeys, line + 1);
                    }
                    assigned2PKeys[line] = true;

                    if (model.Mode == BMSKey.BMS_5K_ONLY || model.Mode == BMSKey.BMS_5K)
                    {
                        model.Mode = BMSKey.BMS_10K;
                    }


                    if (line == 7 || line == 8)
                    {
                        model.Mode = BMSKey.BMS_14K;
                    }

                    // 노트 추가
                    if (model.Base == Define.BMSModel.Base.BASE62)
                    {
                        measures[measure].AddLongNotes(line, pos, Util.Decode.DecodeBase62(splittedValue), Define.BMSObject.PlayerSide.P2);
                    }
                    else
                    {
                        measures[measure].AddLongNotes(line, pos, Util.Decode.DecodeBase36(splittedValue), Define.BMSObject.PlayerSide.P2);
                    }
                }

                // 1P Invisible
                if (channel >= (int)Channel.P1_INVISIBLE_START_POS && channel < (int)Channel.P2_INVISIBLE_START_POS)
                {
                    if (splittedValue == "00")
                        continue;

                    int line = channel - (int)Channel.P1_INVISIBLE_START_POS;

                    // 모드 구분을 위한 할당된 키 갱신
                    if (assigned1PKeys.Length < line + 1)
                    {
                        Array.Resize(ref assigned1PKeys, line + 1);
                    }
                    assigned1PKeys[line] = true;

                    // 노트 추가
                    if (model.Base == Define.BMSModel.Base.BASE62)
                    {
                        measures[measure].AddHiddenNotes(line, pos, Util.Decode.DecodeBase62(splittedValue), Define.BMSObject.PlayerSide.P1);
                    }
                    else
                    {
                        measures[measure].AddHiddenNotes(line, pos, Util.Decode.DecodeBase36(splittedValue), Define.BMSObject.PlayerSide.P1);
                    }
                }

                // 2P Invisible
                if (channel >= (int)Channel.P2_INVISIBLE_START_POS && channel < (int)Channel.P1_LONGNOTE_START_POS)
                {
                    if (splittedValue == "00")
                        continue;

                    int line = (channel - (int)Channel.P2_INVISIBLE_START_POS);

                    // 모드 구분을 위한 할당된 키 갱신
                    if (assigned2PKeys.Length < line + 1)
                    {
                        Array.Resize(ref assigned2PKeys, line + 1);
                    }
                    assigned2PKeys[line] = true;

                    // 노트 추가
                    if (model.Base == Define.BMSModel.Base.BASE62)
                    {
                        measures[measure].AddHiddenNotes(line, pos, Util.Decode.DecodeBase62(splittedValue), Define.BMSObject.PlayerSide.P2);
                    }
                    else
                    {
                        measures[measure].AddHiddenNotes(line, pos, Util.Decode.DecodeBase36(splittedValue), Define.BMSObject.PlayerSide.P2);
                    }
                }

                // 1P Longnote
                if (channel >= (int)Channel.P1_LONGNOTE_START_POS && channel < (int)Channel.P2_LONGNOTE_START_POS)
                {
                    if (splittedValue == "00")
                        continue;

                    if (model.LnType == 2)
                    {
                        Console.WriteLine($"LNTYPE 2는 지원하지 않습니다. LNTYPE 1 혹은 LNOBJ를 할당하여 롱노트 형식을 변경 해주세요.");
                        continue;
                    }

                    int line = channel - (int)Channel.P1_LONGNOTE_START_POS;

                    // 모드 구분을 위한 할당된 키 갱신
                    if (assigned1PKeys.Length < line + 1)
                    {
                        Array.Resize(ref assigned1PKeys, line + 1);
                    }
                    assigned1PKeys[line] = true;

                    if ((line == 7 || line == 8) && model.Mode == BMSKey.BMS_5K_ONLY)
                    {
                        model.Mode = BMSKey.BMS_7K_ONLY;
                    }

                    if (line == 5 && model.Mode == BMSKey.BMS_5K_ONLY || model.Mode == BMSKey.BMS_7K_ONLY)
                    {
                        model.Mode = model.Mode == BMSKey.BMS_5K_ONLY ? BMSKey.BMS_5K : BMSKey.BMS_7K;
                    }

                    // 노트 추가
                    if (model.Base == Define.BMSModel.Base.BASE62)
                    {
                        measures[measure].AddLongNotes(line, pos, Util.Decode.DecodeBase62(splittedValue), Define.BMSObject.PlayerSide.P1);
                    }
                    else
                    {
                        measures[measure].AddLongNotes(line, pos, Util.Decode.DecodeBase36(splittedValue), Define.BMSObject.PlayerSide.P1);
                    }
                }

                // 2P Longnote
                if (channel >= (int)Channel.P2_LONGNOTE_START_POS && channel < (int)Channel.P1_LANDMINE_START_POS)
                {
                    if (splittedValue == "00")
                        continue;

                    if (model.LnType == 2)
                    {
                        Console.WriteLine($"LNTYPE 2는 지원하지 않습니다. LNTYPE 1 혹은 LNOBJ를 할당하여 롱노트 형식을 변경 해주세요.");
                        continue;
                    }

                    int line = channel - (int)Channel.P2_LONGNOTE_START_POS;

                    // 모드 구분을 위한 할당된 키 갱신
                    if (assigned2PKeys.Length < line + 1)
                    {
                        Array.Resize(ref assigned2PKeys, line + 1);
                    }
                    assigned2PKeys[line] = true;

                    if (model.Mode == BMSKey.BMS_5K_ONLY || model.Mode == BMSKey.BMS_5K)
                    {
                        model.Mode = BMSKey.BMS_10K;
                    }

                    if (line == 7 || line == 8)
                    {
                        model.Mode = BMSKey.BMS_14K;
                    }

                    // 노트 추가
                    if (model.Base == Define.BMSModel.Base.BASE62)
                    {
                        measures[measure].AddLongNotes(line, pos, Util.Decode.DecodeBase62(splittedValue), Define.BMSObject.PlayerSide.P2);
                    }
                    else
                    {
                        measures[measure].AddLongNotes(line, pos, Util.Decode.DecodeBase36(splittedValue), Define.BMSObject.PlayerSide.P2);
                    }
                }

                // 1P Land Mine
                if (channel >= (int)Channel.P1_LANDMINE_START_POS && channel < (int)Channel.P2_LANDMINE_START_POS)
                {
                    if (splittedValue == "00")
                        continue;

                    int line = channel - (int)Channel.P1_LANDMINE_START_POS;

                    // 모드 구분을 위한 할당된 키 갱신
                    if (assigned1PKeys.Length < line + 1)
                    {
                        Array.Resize(ref assigned1PKeys, line + 1);
                    }
                    assigned1PKeys[line] = true;

                    if ((line == 7 || line == 8) && model.Mode == BMSKey.BMS_5K_ONLY)
                    {
                        model.Mode = BMSKey.BMS_7K_ONLY;
                    }

                    if (line == 5 && model.Mode == BMSKey.BMS_5K_ONLY || model.Mode == BMSKey.BMS_7K_ONLY)
                    {
                        model.Mode = model.Mode == BMSKey.BMS_5K_ONLY ? BMSKey.BMS_5K : BMSKey.BMS_7K;
                    }

                    // 노트 추가
                    if (model.Base == Define.BMSModel.Base.BASE62)
                    {
                        measures[measure].AddMineNotes(line, pos, Util.Decode.DecodeBase62(splittedValue) / 2, Define.BMSObject.PlayerSide.P1);
                    }
                    else
                    {
                        measures[measure].AddMineNotes(line, pos, Util.Decode.DecodeBase36(splittedValue) / 2, Define.BMSObject.PlayerSide.P1);
                    }
                }

                // 2P Land Mine
                if (channel >= (int)Channel.P2_LANDMINE_START_POS)
                {
                    if (splittedValue == "00")
                        continue;

                    int line = channel - (int)Channel.P2_LANDMINE_START_POS;

                    // 모드 구분을 위한 할당된 키 갱신
                    if (assigned2PKeys.Length < line + 1)
                    {
                        Array.Resize(ref assigned2PKeys, line + 1);
                    }
                    assigned2PKeys[line] = true;

                    if (model.Mode == BMSKey.BMS_5K_ONLY || model.Mode == BMSKey.BMS_5K)
                    {
                        model.Mode = BMSKey.BMS_10K;
                    }

                    if (line == 7 || line == 8)
                    {
                        model.Mode = BMSKey.BMS_14K;
                    }

                    // 노트 추가
                    if (model.Base == Define.BMSModel.Base.BASE62)
                    {
                        measures[measure].AddMineNotes(line, pos, Util.Decode.DecodeBase62(splittedValue), Define.BMSObject.PlayerSide.P2);
                    }
                    else
                    {
                        measures[measure].AddMineNotes(line, pos, Util.Decode.DecodeBase36(splittedValue), Define.BMSObject.PlayerSide.P2);
                    }
                }
            }
        }
    
        /// <summary>
        /// 타이밍 계산
        /// </summary>
        /// <param name="lnobj">일반 노트 처리방식에서 롱노트의 끝을 지정하는 키음 키</param>
        public void CalculateTiming(int lnobj)
        {
            // 이전 일반노트 (LNOBJ 롱노트 변환용)
            Dictionary<int, double> lastP1Section = new Dictionary<int, double>();
            Dictionary<int, double> lastP2Section = new Dictionary<int, double>();
            for (int i = 0; i < assigned1PKeys.Length; i++)
            {
                if (assigned1PKeys[i] == true)
                    lastP1Section.Add(i, -1.0);
            }
            for (int i = 0; i < assigned2PKeys.Length; i++)
            {
                if (assigned2PKeys[i] == true)
                    lastP2Section.Add(i, -1.0);
            }

            // 이전 롱노트 (롱노트 짝 구분용)
            Dictionary<int, double> lastP1LongNoteSection = new Dictionary<int, double>();
            Dictionary<int, double> lastP2LongNoteSection = new Dictionary<int, double>();
            for (int i = 0; i < assigned1PKeys.Length; i++)
            {
                if (assigned1PKeys[i] == true)
                    lastP1LongNoteSection.Add(i, -1.0);
            }
            for (int i = 0; i < assigned2PKeys.Length; i++)
            {
                if (assigned2PKeys[i] == true)
                    lastP2LongNoteSection.Add(i, -1.0);
            }

            // 아무런 이벤트가 없는 마디 초기화
            for (int i = 0; i < measures.Length; i++) 
            {
                if (measures[i] == null)
                    measures[i] = new Measure();
            }

            for (int i = 0; i < measures.Length; i++)
            {
                double previousSectionLength = GetPreviousSection(i);

                // 기믹 처리
                IEnumerator<KeyValuePair<double, Stop>> stops = measures[i].StopEvents.GetEnumerator();
                KeyValuePair<double, Stop>? ste = stops.MoveNext() ? stops.Current : (KeyValuePair<double, Stop>?)null;

                IEnumerator<KeyValuePair<double, BPM>> bpms = measures[i].BpmEvents.GetEnumerator();
                KeyValuePair<double, BPM>? bce = bpms.MoveNext() ? bpms.Current : (KeyValuePair<double, BPM>?)null;

                IEnumerator<KeyValuePair<double, Scroll>> scrolls = measures[i].ScrollEvents.GetEnumerator();
                KeyValuePair<double, Scroll>? sce = scrolls.MoveNext() ? scrolls.Current : (KeyValuePair<double, Scroll>?)null;

                while (ste.HasValue || bce.HasValue || sce.HasValue)
                {
                    double bc = bce.HasValue ? bce.Value.Key : 2;
                    double st = ste.HasValue ? ste.Value.Key : 2;
                    double sc = sce.HasValue ? sce.Value.Key : 2;

                    if (sc <= st && sc <= bc)
                    {
                        double section = previousSectionLength + bc * measures[i].Scale;
                        Timestamp timestamp = GetTimestamp(section);
                        timestamp.Scroll = sce.Value.Value.ScrollValue;
                        sce = scrolls.MoveNext() ? scrolls.Current : (KeyValuePair<double, Scroll>?)null;
                    }
                    else if (bc <= st)
                    {
                        double section = previousSectionLength + bc * measures[i].Scale;
                        Timestamp timestamp = GetTimestamp(section);
                        timestamp.Bpm = bce.Value.Value.Bpm;
                        bce = bpms.MoveNext() ? bpms.Current : (KeyValuePair<double, BPM>?)null;
                    }
                    else if (st <= 1)
                    {
                        double section = previousSectionLength + ste.Value.Key * measures[i].Scale;
                        Timestamp timestamp = GetTimestamp(section);
                        timestamp.StopDuration = 1000.0 * 60 * 4 * ste.Value.Value.Duration / timestamp.Bpm;
                        ste = stops.MoveNext() ? stops.Current : (KeyValuePair<double, Stop>?)null;
                    }
                }

                // 단노트 처리 //
                // LONBJ가 감지될 경우 현재 비트와 이전 비트를 롱노트로 변환 처리
                // 1P
                foreach (KeyValuePair<int, SortedDictionary<double, NormalNote>> lane in measures[i].P1Lane)
                {
                    foreach (KeyValuePair<double, NormalNote> note in lane.Value)
                    {
                        double section = previousSectionLength + note.Key * measures[i].Scale;
                        Timestamp timestamp = GetTimestamp(section);

                        if (note.Value.KeySound == lnobj)
                        {
                            Timestamp prevTimestamp = GetTimestamp(lastP1Section[lane.Key]);

                            // 롱노트의 시작
                            if (!prevTimestamp.P1LongLane.ContainsKey(lane.Key))
                            {
                                prevTimestamp.P1LongLane.Add(lane.Key, new List<LongNote>());
                            }
                            foreach (Note prevNote in prevTimestamp.P1Lane[lane.Key])
                            {
                                prevTimestamp.P1LongLane[lane.Key].Add(new LongNote(prevNote.KeySound, section));
                            }
                            prevTimestamp.P1Lane.Remove(lane.Key);

                            // 롱노트의 끝
                            if (!timestamp.P1LongLane.ContainsKey(lane.Key))
                            {
                                timestamp.P1LongLane.Add(lane.Key, new List<LongNote>());
                            }
                            timestamp.P1LongLane[lane.Key].Add(new LongNote(-1, lastP1Section[lane.Key]));
                        }
                        else
                        {
                            if (!timestamp.P1Lane.ContainsKey(lane.Key))
                            {
                                timestamp.P1Lane.Add(lane.Key, new List<NormalNote>());
                            }

                            timestamp.P1Lane[lane.Key].Add(new NormalNote(note.Value.KeySound));
                        }

                        lastP1Section[lane.Key] = section;
                    }
                }

                // 2P
                foreach (KeyValuePair<int, SortedDictionary<double, NormalNote>> lane in measures[i].P2Lane)
                {
                    foreach (KeyValuePair<double, NormalNote> note in lane.Value)
                    {
                        double section = previousSectionLength + note.Key * measures[i].Scale;
                        Timestamp timestamp = GetTimestamp(section);

                        if (note.Value.KeySound == lnobj)
                        {
                            Timestamp prevTimestamp = GetTimestamp(lastP2Section[lane.Key]);

                            // 롱노트의 시작
                            if (!prevTimestamp.P2LongLane.ContainsKey(lane.Key))
                            {
                                prevTimestamp.P2LongLane.Add(lane.Key, new List<LongNote>());
                            }
                            foreach (Note prevNote in prevTimestamp.P2Lane[lane.Key])
                            {
                                prevTimestamp.P2LongLane[lane.Key].Add(new LongNote(prevNote.KeySound, section));
                            }
                            prevTimestamp.P2Lane.Remove(lane.Key);

                            // 롱노트의 끝
                            if (!timestamp.P2LongLane.ContainsKey(lane.Key))
                            {
                                timestamp.P2LongLane.Add(lane.Key, new List<LongNote>());
                            }
                            timestamp.P2LongLane[lane.Key].Add(new LongNote(-1, lastP2Section[lane.Key]));
                        }
                        else
                        {
                            if (!timestamp.P2Lane.ContainsKey(lane.Key))
                            {
                                timestamp.P2Lane.Add(lane.Key, new List<NormalNote>());
                            }

                            timestamp.P2Lane[lane.Key].Add(new NormalNote(note.Value.KeySound));
                        }

                        lastP2Section[lane.Key] = section;
                    }
                }

                // 롱노트 처리 //
                // P1
                foreach (KeyValuePair<int, SortedDictionary<double, LongNote>> lane in measures[i].P1LongLane)
                {
                    foreach (KeyValuePair<double, LongNote> note in lane.Value)
                    {
                        double section = previousSectionLength + note.Key * measures[i].Scale;
                        Timestamp timestamp = GetTimestamp(section);

                        if (lastP1LongNoteSection[lane.Key] != -1)
                        {
                            Timestamp prevTimestamp = GetTimestamp(lastP1LongNoteSection[lane.Key]);

                            // 롱노트 시작
                            foreach (LongNote lnStart in prevTimestamp.P1LongLane[lane.Key])
                            {
                                lnStart.Pair = section;
                            }

                            // 롱노트 종료
                            if (!timestamp.P1LongLane.ContainsKey(lane.Key))
                                timestamp.P1LongLane.Add(lane.Key, new List<LongNote>());
                            timestamp.P1LongLane[lane.Key].Add(new LongNote(-1, lastP1LongNoteSection[lane.Key]));

                            lastP1LongNoteSection[lane.Key] = -1;
                        }
                        else
                        {
                            if (!timestamp.P1LongLane.ContainsKey(lane.Key))
                                timestamp.P1LongLane.Add(lane.Key, new List<LongNote>());

                            timestamp.P1LongLane[lane.Key].Add(new LongNote(note.Value.KeySound));

                            lastP1LongNoteSection[lane.Key] = section;
                        }
                    }
                }
                // P2
                foreach (KeyValuePair<int, SortedDictionary<double, LongNote>> lane in measures[i].P2LongLane)
                {
                    foreach (KeyValuePair<double, LongNote> note in lane.Value)
                    {
                        double section = previousSectionLength + note.Key * measures[i].Scale;
                        Timestamp timestamp = GetTimestamp(section);

                        if (lastP2LongNoteSection[lane.Key] != -1)
                        {
                            Timestamp prevTimestamp = GetTimestamp(lastP2LongNoteSection[lane.Key]);

                            // 롱노트 시작
                            foreach (LongNote lnStart in prevTimestamp.P2LongLane[lane.Key])
                            {
                                lnStart.Pair = section;
                            }

                            // 롱노트 종료
                            if (!timestamp.P2LongLane.ContainsKey(lane.Key))
                                timestamp.P2LongLane.Add(lane.Key, new List<LongNote>());
                            timestamp.P2LongLane[lane.Key].Add(new LongNote(-1, lastP2LongNoteSection[lane.Key]));

                            lastP2LongNoteSection[lane.Key] = -1;
                        }
                        else
                        {
                            if (!timestamp.P2LongLane.ContainsKey(lane.Key))
                                timestamp.P2LongLane.Add(lane.Key, new List<LongNote>());

                            timestamp.P2LongLane[lane.Key].Add(new LongNote(note.Value.KeySound));

                            lastP2LongNoteSection[lane.Key] = section;
                        }
                    }
                }

                // 지뢰노트 처리 //
                // P1
                foreach (KeyValuePair<int, SortedDictionary<double, MineNote>> lane in measures[i].P1MineLane)
                {
                    foreach (KeyValuePair<double, MineNote> note in lane.Value)
                    {
                        double section = previousSectionLength + note.Key * measures[i].Scale;
                        Timestamp timestamp = GetTimestamp(section);

                        if (!timestamp.P1MineLane.ContainsKey(lane.Key))
                        {
                            timestamp.P1MineLane.Add(lane.Key, new List<MineNote>());
                        }

                        timestamp.P1MineLane[lane.Key].Add(new MineNote(note.Value.Damage));
                    }
                }
                // P2
                foreach (KeyValuePair<int, SortedDictionary<double, MineNote>> lane in measures[i].P2MineLane)
                {
                    foreach (KeyValuePair<double, MineNote> note in lane.Value)
                    {
                        double section = previousSectionLength + note.Key * measures[i].Scale;
                        Timestamp timestamp = GetTimestamp(section);

                        if (!timestamp.P2MineLane.ContainsKey(lane.Key))
                        {
                            timestamp.P2MineLane.Add(lane.Key, new List<MineNote>());
                        }

                        timestamp.P2MineLane[lane.Key].Add(new MineNote(note.Value.Damage));
                    }
                }

                // 숨겨진 노트 처리 //
                // P1
                foreach (KeyValuePair<int, SortedDictionary<double, HiddenNote>> lane in measures[i].P1HiddenLane)
                {
                    foreach (KeyValuePair<double, HiddenNote> note in lane.Value)
                    {
                        double section = previousSectionLength + note.Key * measures[i].Scale;
                        Timestamp timestamp = GetTimestamp(section);

                        if (!timestamp.P1HiddenLane.ContainsKey(lane.Key))
                        {
                            timestamp.P1HiddenLane.Add(lane.Key, new List<HiddenNote>());
                        }

                        timestamp.P1HiddenLane[lane.Key].Add(new HiddenNote(note.Value.KeySound));
                    }
                }
                // P2
                foreach (KeyValuePair<int, SortedDictionary<double, HiddenNote>> lane in measures[i].P2HiddenLane)
                {
                    foreach (KeyValuePair<double, HiddenNote> note in lane.Value)
                    {
                        double section = previousSectionLength + note.Key * measures[i].Scale;
                        Timestamp timestamp = GetTimestamp(section);

                        if (!timestamp.P2HiddenLane.ContainsKey(lane.Key))
                        {
                            timestamp.P2HiddenLane.Add(lane.Key, new List<HiddenNote>());
                        }

                        timestamp.P2HiddenLane[lane.Key].Add(new HiddenNote(note.Value.KeySound));
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