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
                // 노트 시작 기준숫자 채널
                P1_NORMAL_START_POS = 37,
                P2_NORMAL_START_POS = 73,
                P1_INVISIBLE_START_POS = 109,
                P2_INVISIBLE_START_POS = 145,
                P1_LONGNOTE_START_POS = 181,
                P2_LONGNOTE_START_POS = 217,
                // 지뢰노트 기준숫자 채널 / (정수화 된 데이터 값 / 2) = 데미지, ZZ는 바로 게임오버 처리
                P1_LANDMINE_START_POS = 469,
                P2_LANDMINE_START_POS = 505,
            }
        }
    }
}
