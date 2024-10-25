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
    }
}
