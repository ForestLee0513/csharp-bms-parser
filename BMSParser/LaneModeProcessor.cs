using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static BMSParser.Define.PatternProcessor;

namespace BMSParser
{
    public class LaneModeProcessor
    {
        /// <summary>
        /// 해당 BMS 모델의 라인 모드를 변경합니다. <br />
        /// 변경 가능한 모드는 각 플레이어 레인 별 랜덤, R-랜덤, S-랜덤, 미러 임.
        /// </summary>
        /// <param name="model">라인 모드를 변경하고 싶은 모델</param>
        /// <param name="randomSeed">랜덤 시드</param>
        /// <param name="p1RandomMode">1P 랜덤 모드</param>
        /// <param name="p2RandomMode">2P 랜덤 모드</param>
        public static void ProcessLaneMode(ref BMSModel model, int randomSeed, Define.RandomMode p1RandomMode, Define.RandomMode p2RandomMode)
        {
            BMSKey lineCountPerLane;
            if (model.Mode == BMSKey.BEAT_10K)
            {
                lineCountPerLane = BMSKey.BEAT_5K;
            }
            else if (model.Mode == BMSKey.BEAT_14K)
            {
                lineCountPerLane = BMSKey.BEAT_7K;
            }
            else
            {
                lineCountPerLane = model.Mode;
            }

            Random noteRandom = new Random(randomSeed);
            int[] lineRandomMap = new int[SinglePlayKeyMap[lineCountPerLane].Keyboard.Length];
            for (int i = 0; i < lineRandomMap.Length; i++)
            {
                lineRandomMap[i] = SinglePlayKeyMap[lineCountPerLane].Keyboard[i];
            }

            // NORMAL RANDOM //
            if (p1RandomMode == Define.RandomMode.RANDOM)
            {
                lineRandomMap = lineRandomMap.OrderBy(_ => noteRandom.Next()).ToArray();

                foreach (KeyValuePair<double, Timestamp> timestamp in model.Timestamp)
                {
                    Dictionary<int, List<NormalNote>> originalNormalLane = timestamp.Value.P1Lane;
                    Dictionary<int, List<NormalNote>> randomizedNormalLane = new Dictionary<int, List<NormalNote>>();

                    Dictionary<int, List<LongNote>> originalLongLane = timestamp.Value.P1LongLane;
                    Dictionary<int, List<LongNote>> randomizedLongLane = new Dictionary<int, List<LongNote>>();

                    Dictionary<int, List<MineNote>> originalMineLane = timestamp.Value.P1MineLane; 
                    Dictionary<int, List<MineNote>> randomizedMineLane = new Dictionary<int, List<MineNote>>();

                    Dictionary<int, List<HiddenNote>> originalHiddenLane = timestamp.Value.P1HiddenLane;
                    Dictionary<int, List<HiddenNote>> randomizedHiddenLane = new Dictionary<int, List<HiddenNote>>();

                    for (int i = 0; i < lineRandomMap.Length; i++)
                    {
                        if (originalNormalLane.ContainsKey(lineRandomMap[i]))
                        {
                            randomizedNormalLane.Add(SinglePlayKeyMap[lineCountPerLane].Keyboard[i], originalNormalLane[lineRandomMap[i]]);                            
                        }

                        if (originalLongLane.ContainsKey(lineRandomMap[i]))
                        {
                            randomizedLongLane.Add(SinglePlayKeyMap[lineCountPerLane].Keyboard[i], originalLongLane[lineRandomMap[i]]);
                        }

                        if (originalMineLane.ContainsKey(lineRandomMap[i]))
                        {
                            randomizedMineLane.Add(SinglePlayKeyMap[lineCountPerLane].Keyboard[i], originalMineLane[lineRandomMap[i]]);
                        }

                        if (originalHiddenLane.ContainsKey(lineRandomMap[i]))
                        {
                            randomizedHiddenLane.Add(SinglePlayKeyMap[lineCountPerLane].Keyboard[i], originalHiddenLane[lineRandomMap[i]]);
                        }
                    }

                    // 스크래치 키를 새로운 랜덤화 패턴으로 복사
                    for (int i = 0; i < SinglePlayKeyMap[lineCountPerLane].Scratch.Length; i++)
                    {
                        if (originalNormalLane.ContainsKey(SinglePlayKeyMap[lineCountPerLane].Scratch[i]))
                        {
                            randomizedNormalLane[SinglePlayKeyMap[lineCountPerLane].Scratch[i]] = originalNormalLane[SinglePlayKeyMap[lineCountPerLane].Scratch[i]];
                        }
                        if (originalLongLane.ContainsKey(SinglePlayKeyMap[lineCountPerLane].Scratch[i]))
                        {
                            randomizedLongLane[SinglePlayKeyMap[lineCountPerLane].Scratch[i]] = originalLongLane[SinglePlayKeyMap[lineCountPerLane].Scratch[i]];
                        }
                        if (originalMineLane.ContainsKey(SinglePlayKeyMap[lineCountPerLane].Scratch[i]))
                        {
                            randomizedMineLane[SinglePlayKeyMap[lineCountPerLane].Scratch[i]] = originalMineLane[SinglePlayKeyMap[lineCountPerLane].Scratch[i]];
                        }
                        if (originalHiddenLane.ContainsKey(SinglePlayKeyMap[lineCountPerLane].Scratch[i]))
                        {
                            randomizedHiddenLane[SinglePlayKeyMap[lineCountPerLane].Scratch[i]] = originalHiddenLane[SinglePlayKeyMap[lineCountPerLane].Scratch[i]];
                        }
                    }

                    // 페달 키를 새로운 랜덤화 패턴으로 복사
                    for (int i = 0; i < SinglePlayKeyMap[lineCountPerLane].FootPedal.Length; i++)
                    {
                        if (originalNormalLane.ContainsKey(SinglePlayKeyMap[lineCountPerLane].FootPedal[i]))
                        {
                            randomizedNormalLane[SinglePlayKeyMap[lineCountPerLane].FootPedal[i]] = originalNormalLane[SinglePlayKeyMap[lineCountPerLane].FootPedal[i]];
                        }
                        if (originalLongLane.ContainsKey(SinglePlayKeyMap[lineCountPerLane].FootPedal[i]))
                        {
                            randomizedLongLane[SinglePlayKeyMap[lineCountPerLane].FootPedal[i]] = originalLongLane[SinglePlayKeyMap[lineCountPerLane].FootPedal[i]];
                        }
                        if (originalMineLane.ContainsKey(SinglePlayKeyMap[lineCountPerLane].FootPedal[i]))
                        {
                            randomizedMineLane[SinglePlayKeyMap[lineCountPerLane].FootPedal[i]] = originalMineLane[SinglePlayKeyMap[lineCountPerLane].FootPedal[i]];
                        }
                        if (originalHiddenLane.ContainsKey(SinglePlayKeyMap[lineCountPerLane].FootPedal[i]))
                        {
                            randomizedHiddenLane[SinglePlayKeyMap[lineCountPerLane].FootPedal[i]] = originalHiddenLane[SinglePlayKeyMap[lineCountPerLane].FootPedal[i]];
                        }
                    }

                    model.Timestamp[timestamp.Key].P1Lane = randomizedNormalLane;
                    model.Timestamp[timestamp.Key].P1LongLane = randomizedLongLane;
                    model.Timestamp[timestamp.Key].P1MineLane = randomizedMineLane;
                    model.Timestamp[timestamp.Key].P1HiddenLane = randomizedHiddenLane;
                }
            }

            if (p2RandomMode == Define.RandomMode.RANDOM)
            {
                lineRandomMap = lineRandomMap.OrderBy(_ => noteRandom.Next()).ToArray();

                foreach (KeyValuePair<double, Timestamp> timestamp in model.Timestamp)
                {
                    Dictionary<int, List<NormalNote>> originalNormalLane = timestamp.Value.P2Lane;
                    Dictionary<int, List<NormalNote>> randomizedNormalLane = new Dictionary<int, List<NormalNote>>();

                    Dictionary<int, List<LongNote>> originalLongLane = timestamp.Value.P2LongLane;
                    Dictionary<int, List<LongNote>> randomizedLongLane = new Dictionary<int, List<LongNote>>();

                    Dictionary<int, List<MineNote>> originalMineLane = timestamp.Value.P2MineLane;
                    Dictionary<int, List<MineNote>> randomizedMineLane = new Dictionary<int, List<MineNote>>();

                    Dictionary<int, List<HiddenNote>> originalHiddenLane = timestamp.Value.P2HiddenLane;
                    Dictionary<int, List<HiddenNote>> randomizedHiddenLane = new Dictionary<int, List<HiddenNote>>();

                    for (int i = 0; i < lineRandomMap.Length; i++)
                    {
                        if (originalNormalLane.ContainsKey(lineRandomMap[i]))
                        {
                            randomizedNormalLane.Add(SinglePlayKeyMap[lineCountPerLane].Keyboard[i], originalNormalLane[lineRandomMap[i]]);
                        }

                        if (originalLongLane.ContainsKey(lineRandomMap[i]))
                        {
                            randomizedLongLane.Add(SinglePlayKeyMap[lineCountPerLane].Keyboard[i], originalLongLane[lineRandomMap[i]]);
                        }

                        if (originalMineLane.ContainsKey(lineRandomMap[i]))
                        {
                            randomizedMineLane.Add(SinglePlayKeyMap[lineCountPerLane].Keyboard[i], originalMineLane[lineRandomMap[i]]);
                        }

                        if (originalHiddenLane.ContainsKey(lineRandomMap[i]))
                        {
                            randomizedHiddenLane.Add(SinglePlayKeyMap[lineCountPerLane].Keyboard[i], originalHiddenLane[lineRandomMap[i]]);
                        }
                    }

                    // 스크래치 키를 새로운 랜덤화 패턴으로 복사
                    for (int i = 0; i < SinglePlayKeyMap[lineCountPerLane].Scratch.Length; i++)
                    {
                        if (originalNormalLane.ContainsKey(SinglePlayKeyMap[lineCountPerLane].Scratch[i]))
                        {
                            randomizedNormalLane[SinglePlayKeyMap[lineCountPerLane].Scratch[i]] = originalNormalLane[SinglePlayKeyMap[lineCountPerLane].Scratch[i]];
                        }
                        if (originalLongLane.ContainsKey(SinglePlayKeyMap[lineCountPerLane].Scratch[i]))
                        {
                            randomizedLongLane[SinglePlayKeyMap[lineCountPerLane].Scratch[i]] = originalLongLane[SinglePlayKeyMap[lineCountPerLane].Scratch[i]];
                        }
                        if (originalMineLane.ContainsKey(SinglePlayKeyMap[lineCountPerLane].Scratch[i]))
                        {
                            randomizedMineLane[SinglePlayKeyMap[lineCountPerLane].Scratch[i]] = originalMineLane[SinglePlayKeyMap[lineCountPerLane].Scratch[i]];
                        }
                        if (originalHiddenLane.ContainsKey(SinglePlayKeyMap[lineCountPerLane].Scratch[i]))
                        {
                            randomizedHiddenLane[SinglePlayKeyMap[lineCountPerLane].Scratch[i]] = originalHiddenLane[SinglePlayKeyMap[lineCountPerLane].Scratch[i]];
                        }
                    }

                    // 페달 키를 새로운 랜덤화 패턴으로 복사
                    for (int i = 0; i < SinglePlayKeyMap[lineCountPerLane].FootPedal.Length; i++)
                    {
                        if (originalNormalLane.ContainsKey(SinglePlayKeyMap[lineCountPerLane].FootPedal[i]))
                        {
                            randomizedNormalLane[SinglePlayKeyMap[lineCountPerLane].FootPedal[i]] = originalNormalLane[SinglePlayKeyMap[lineCountPerLane].FootPedal[i]];
                        }
                        if (originalLongLane.ContainsKey(SinglePlayKeyMap[lineCountPerLane].FootPedal[i]))
                        {
                            randomizedLongLane[SinglePlayKeyMap[lineCountPerLane].FootPedal[i]] = originalLongLane[SinglePlayKeyMap[lineCountPerLane].FootPedal[i]];
                        }
                        if (originalMineLane.ContainsKey(SinglePlayKeyMap[lineCountPerLane].FootPedal[i]))
                        {
                            randomizedMineLane[SinglePlayKeyMap[lineCountPerLane].FootPedal[i]] = originalMineLane[SinglePlayKeyMap[lineCountPerLane].FootPedal[i]];
                        }
                        if (originalHiddenLane.ContainsKey(SinglePlayKeyMap[lineCountPerLane].FootPedal[i]))
                        {
                            randomizedHiddenLane[SinglePlayKeyMap[lineCountPerLane].FootPedal[i]] = originalHiddenLane[SinglePlayKeyMap[lineCountPerLane].FootPedal[i]];
                        }
                    }

                    model.Timestamp[timestamp.Key].P2Lane = randomizedNormalLane;
                    model.Timestamp[timestamp.Key].P2LongLane = randomizedLongLane;
                    model.Timestamp[timestamp.Key].P2MineLane = randomizedMineLane;
                    model.Timestamp[timestamp.Key].P2HiddenLane = randomizedHiddenLane;
                }
            }
            // R-RANDOM //
            if (p1RandomMode == Define.RandomMode.R_RANDOM)
            {

            }

            if (p2RandomMode == Define.RandomMode.R_RANDOM)
            {

            }

            // S-RANDOM //
            if (p1RandomMode == Define.RandomMode.S_RANDOM)
            {

            }

            if (p2RandomMode == Define.RandomMode.S_RANDOM)
            {

            }

            // MIRROR //
            if (p1RandomMode == Define.RandomMode.MIRROR)
            {

            }

            if (p2RandomMode == Define.RandomMode.MIRROR)
            {

            }
        }
    }
}