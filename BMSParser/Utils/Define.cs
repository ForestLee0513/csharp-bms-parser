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
                BMS,    // standard
                BME,    // extend
                BML,    // long note
                PMS     // popn
            }

            public enum Base
            {
                BASE36,
                BASE62
            }

            public enum LNType
            {
                NONE,
                LNTYPE1,
                LNTYPE2,
                LNOBJ
            }
        }

        public class PatternProcessor
        {
            /// <summary>
            /// BMS 파싱 과정에서 정의되는 채널
            /// 실제 노트는 36진수 기준 11에서 시작되며 10진수로 변환 시 37임
            /// </summary>
            public enum Channel
            {
                BGM = 1,
                MEASURE_BEAT,
                BPM_CHANGE,
                BGA_BASE,
                BGA_POOR = 6,
                BGA_LAYER = 7,
                EXBPM,
                STOP,

                P1_NORMAL_START_POS = 1 * 36 + 1,
                P2_NORMAL_START_POS = 2 * 36 + 1,
                P1_INVISIBLE_START_POS = 3 * 36 + 1,
                P2_INVISIBLE_START_POS = 4 * 36 + 1,
                P1_LONGNOTE_START_POS = 5 * 36 + 1,
                P2_LONGNOTE_START_POS = 6 * 36 + 1,
                P1_LANDMINE_START_POS = 13 * 36 + 1,
                P2_LANDMINE_START_POS = 14 * 36 + 1,

                SCROLL = 1020
            }

            public class KeyMap
            {
                public int[] Keyboard { get; }
                public int[] Scratch { get; }
                public int[] FootPedal { get; }

                /// <summary>
                /// BMS에서 노트를 다 파싱하고 나서 몇 키인지 정의하는 클래스<br />
                /// index를 기준으로 0부터 시작하며 7키의 경우 아래와 같이 정의됨<br />
                /// 스크래치: 5 / 일반 노트: 0, 1, 2, 3, 4, 7, 8<br />
                /// 할당하고 싶지 않은 영역이 있다면 빈 배열로 초기화 처리
                /// </summary>
                /// <param name="keyboard">노트 키에 해당되는 채널</param>
                /// <param name="scratch">스크래치 키에 해당되는 채널</param>
                /// <param name="footPedal">페달 키에 해당되는 채널</param>
                public KeyMap(int[] keyboard, int[] scratch, int[] footPedal)
                {
                    Keyboard = keyboard;
                    Scratch = scratch;
                    FootPedal = footPedal;
                }
            }

            public enum BMSKey
            {
                BEAT_4K,    // Extend command required: '#4K'
                BEAT_5K,
                BEAT_6K,    // Extend command required: '#6K'
                BEAT_7K,
                BEAT_10K,
                BEAT_14K,
                POPN
            }

            public static Dictionary<BMSKey, KeyMap> SinglePlayKeyMap = new Dictionary<BMSKey, KeyMap>()
            {
                {
                    BMSKey.BEAT_5K, new KeyMap(new int[] { 0, 1, 2, 3, 4 }, new int[] { 5 }, new int[0])
                },
                {
                    BMSKey.BEAT_7K, new KeyMap(new int[] { 0, 1, 2, 3, 4, 7, 8 }, new int[] { 5 }, new int[0])
                },
                {
                    BMSKey.POPN, new KeyMap(new int[] { 0, 1, 2, 3, 4, 11, 12, 13, 14 }, new int[0], new int[0])
                }
            };
        }

        public class BMSObject
        {
            public enum BGA
            {
                BASE = 4,
                POOR = 6,
                LAYER = 7
            }

            public enum PlayerSide
            {
                P1,
                P2
            }

            public enum NoteType
            {
                NORMAL_NOTE,
                LN_START,
                LN_END,
                LANDMINE
            }
        }
    }
}
