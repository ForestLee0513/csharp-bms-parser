namespace BMS
{
    public static class BMSModelDefine
    {
        public enum LNTYPE
        {
            LONGNOTE,
            CHARGENOTE,
            HELL_CHARGENOTE
        }
    }

    public class BMSModel
    {
        public int Player { get; set; }
        public Mode Mode { get; set; } // Mode는 class임.
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Genre { get; set; }
        public string Artist { get; set; }
        public string Subartist { get; set; }
        public string Banner { get; set; }
        public string StageFile { get; set; }
        public string BackBmp { get; set; }
        public string Preview { get; set; }
        public double Bpm { get; set; }

        public string PlayLevel { get; set; }
        public int Difficulty { get; set; }
        public int JudgeRank { get; set; }
        public int JudgeRankType { get; set; } // enum추가해야됨
        public int Total { get; set; }
        public int TotalType { get; set; } // enum  추가해야됨
        public int VolWav { get; set; }

        public string MD5 { get; set; }
        public string SHA256 { get; set; }

        public int BaseType { get; set; }

        public int LnType { get; set; }
        public int Lnobj { get; set; }

        // 타임라인 관련
    }
}
