using System.Collections.Generic;

namespace BMSParser
{
    public static class Define
    {
        public class BMSModel
        {
            public enum RankType
            {
                RANK,
                DEFEXRANK,
                BMSON_JUDGERANK
            }

            public enum Extension
            {
                UNDEFINED,
                BMS,
                BME,
                BML,
                PMS
            }

            public enum Base
            {
                BASE36,
                BASE62
            }
        }

        public class TimeLine
        {
            /// <summary>
            /// BMS 파싱 과정에서 정의되는 채널
            /// 실제 노트는 36진수 기준 11에서 시작되며 10진수로 변환 시 37임
            /// </summary>
            public enum Channel
            {
                BGM,
                MEASURE,
                BPM_CHANGE,
                BGA_BASE,
                BGA_POOR = 6,
                BGA_LAYER = 7,
                EXBPM,
                STOP,
                P1_NORMAL_START_POS = 37,
                P2_NORMAL_START_POS = 73,
                P1_INVISIBLE_START_POS = 109,
                P2_INVISIBLE_START_POS = 145,
                P1_LONGNOTE_START_POS = 181,
                P2_LONGNOTE_START_POS = 217,
                P1_LANDMINE_START_POS = 469,
                P2_LANDMINE_START_POS = 505,
            }

            public class KeyMap
            {
                public int[] Keyboard { get; }
                public int[] Scratch { get; }
                public int[] FootPedal { get; }

                /// <summary>
                /// BMS에서 노트를 다 파싱하고 나서 몇 키인지 정의하는 클래스<br />
                /// index를 기준으로 0부터 시작하며 7키의 경우 아래와 같이 정의됨<br />
                /// 스크래치: 5 / 일반 노트: 0, 1, 2, 3, 4, 7, 8
                /// </summary>
                /// <param name="keyboard">노트 키에 해당되는 채널</param>
                /// <param name="scratch">스크래치 키에 해당되는 채널</param>
                /// <param name="footPedal">페달 키에 해당되는 채널.</param>
                public KeyMap(int[] keyboard, int[] scratch, int[] footPedal)
                {
                    Keyboard = keyboard;
                    Scratch = scratch;
                    FootPedal = footPedal;
                }
            }

            // 스크래치 없는 모드까지 추가해야하나?
            // PMS는 5키 전용으로 따로 내줘야하나?
            // 키보드매니아까지 추가할 경우 24키/48키에 대한 bms파일 채널 연구가 필요함.
            // Qwilight와 같은 일부 구동기에서만 사용되는 4키, 6키의 경우는 채널 연구와 동시에 저작권적으로 사용 허가가 필요할 듯
            public enum BMSKey
            {
                BMS_5K,
                BMS_7K,
                BMS_10K,
                BMS_14K,
                PMS
            }

            public Dictionary<BMSKey, KeyMap> KeyMapTable = new Dictionary<BMSKey, KeyMap>()
            {
                {
                    BMSKey.BMS_5K, new KeyMap(new int[] { 0, 1, 2, 3, 4 }, new int[] { 5 }, null)
                },
                {
                    BMSKey.BMS_7K, new KeyMap(new int[] { 0, 1, 2, 3, 4, 7, 8 }, new int[] { 5 }, null)
                },
                {
                    BMSKey.BMS_10K, new KeyMap(new int[] { 0, 1, 2, 3, 4, 10, 11, 12, 13, 14 }, new int[] { 5, 15 }, null)
                },
                {
                    BMSKey.BMS_14K, new KeyMap(new int[] { 0, 1, 2, 3, 4, 7, 8, 10, 11, 12, 13, 14, 17, 18 }, new int[] { 5, 15 }, null)
                },
                {
                    BMSKey.PMS, new KeyMap(new int[] { 0, 1, 2, 3, 4, 11, 12, 13, 14 }, null, null)
                }
            };
        }
    }
}
