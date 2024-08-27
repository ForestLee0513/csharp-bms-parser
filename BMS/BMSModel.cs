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
        public int Player { get; private set; }
        public Mode Mode { get; private set; } // Mode는 class임.
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Genre { get; private set; }
        public string Artist { get; private set; }
        public string Subartist { get; private set; }
        public string Banner { get; private set; }
        public string StageFile { get; private set; }
        public string BackBmp { get; private set; }
        public string Preview { get; private set; }
        public double Bpm { get; private set; }

        public string PlayLevel { get; private set; }
        public int Difficulty { get; private set; }
        public int JudgeRank { get; private set; }
        public int JudgeRankType { get; private set; } // enum추가해야됨
        public int Total { get; private set; }
        public int TotalType { get; private set; } // enum  추가해야됨
        public int VolWav { get; private set; }

        public string MD5 { get; private set; }
        public string SHA256 { get; private set; }

        public int BaseType { get; private set; }

        public int LnType { get; private set; }
        public int Lnobj { get; private set; }

        // 타임라인 관련

        // set methods
        public void SetPlayer(int player) => Player = player;
        public void SetMode(Mode mode) => Mode = mode;
        public void SetTitle(string title) => Title = title;
        public void SetSubtitle(string subtitle) => Subtitle = subtitle;
        public void SetGenre(string genre) => Genre = genre;
        public void SetArtist(string artist) => Artist = artist;
        public void SetSubartist(string subartist) => Subartist = subartist;
        public void SetBanner(string banner) => Banner = banner;
        public void SetStageFile(string stageFile) => StageFile = stageFile;
        public void SetBackBmp(string backBmp) => BackBmp = backBmp;
        public void SetPreview(string preview) => Preview = preview;
        public void SetBpm(double bpm) => Bpm = bpm;

        public void SetPlayLevel(string playLevel) => PlayLevel = playLevel;
        public void SetDifficulty(int difficulty) => Difficulty = difficulty;
        public void SetJudgeRank(int judgeRank) => JudgeRank = judgeRank;
        public void SetJudgeRankType(int judgeRankType) => JudgeRankType = judgeRankType;
        public void SetTotal(int total) => Total = total;
        public void SetTotalType(int totalType) => TotalType = totalType;
        public void SetVolWav(int volWav) => VolWav = volWav;
        
        public void SetMD5(string md5) => MD5 = md5;
        public void SetSHA256(string sha256) => SHA256 = sha256;
        
        public void SetBaseType(int baseType) => BaseType = baseType;
        public void SetLnType(int lnType) => LnType = lnType;
        public void SetLnobj(int lnobj) => Lnobj = lnobj;
    }
}
