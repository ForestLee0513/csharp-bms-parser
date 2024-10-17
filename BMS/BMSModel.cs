using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BMS
{
    public static class BMSModelDefine
    {
        public enum LNTYPE
        {
            UNDEFINED,
            LONGNOTE,
            CHARGENOTE,
            HELL_CHARGENOTE
        }

        public enum JudgeRankType
        {
            BMS_RANK, 
            BMS_DEFEXRANK, 
            BMSON_JUDGERANK
        }

        public enum TotalType
        {
            BMS, 
            BMSON
        }
    }

    public class BMSModel
    {
        public int Player { get; private set; }
        public Mode Mode { get; private set; }
        public string Title { get; private set; } = "";
        public string Subtitle { get; private set; } = "";
        public string Genre { get; private set; } = "";
        public string Artist { get; private set; } = "";
        public string Subartist { get; private set; } = "";
        public string Banner { get; private set; } = "";
        public string StageFile { get; private set; } = "";
        public string BackBmp { get; private set; } = "";
        public string Preview { get; private set; } = "";
        public double Bpm { get; private set; }

        public string PlayLevel { get; private set; } = "";
        public int Difficulty { get; private set; }
        public int JudgeRank { get; private set; }
        public BMSModelDefine.JudgeRankType JudgeRankType { get; private set; }
        public double Total { get; private set; } = 100;
        public BMSModelDefine.TotalType TotalType { get; private set; }
        public int VolWav { get; private set; }

        public string MD5 { get; private set; } = "";
        public string SHA256 { get; private set; } = "";
        public string[] WavMap { get; private set; }
        public string[] BgaMap { get; private set; }

        public int BaseType { get; private set; } = 36;

        public BMSModelDefine.LNTYPE LnMode { get; private set; }
        public int Lnobj { get; private set; } = -1;


        // 타임라인 관련
        public Timeline[] Timelines { get; private set; }
        public ChartInformation Info { get; private set; }
        public Dictionary<string, string> Values { get; private set; }

        public void SetAllTimelines(Timeline[] timelines) => Timelines = timelines;
        public void SetChartInformation(ChartInformation info) => Info = info;
        public BMSModelDefine.LNTYPE GetLnType()
        {
            return Info != null ? Info.LnType : BMSModelDefine.LNTYPE.LONGNOTE;
        }

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
        public void SetJudgeRankType(BMSModelDefine.JudgeRankType judgeRankType) => JudgeRankType = judgeRankType;
        public void SetTotal(double total) => Total = total;
        public void SetTotalType(BMSModelDefine.TotalType totalType) => TotalType = totalType;
        public void SetVolWav(int volWav) => VolWav = volWav;
        
        public void SetMD5(string md5) => MD5 = md5;
        public void SetSHA256(string sha256) => SHA256 = sha256;
        public void SetWavMap(string[] wavMap) => WavMap = wavMap;
        public void SetBgaMap(string[] bgaMap) => BgaMap = bgaMap;
        
        public void SetBaseType(int baseType) => BaseType = baseType;
        public void SetLnMode(BMSModelDefine.LNTYPE lnMode) => LnMode = lnMode;
        public void SetLnobj(int lnobj) => Lnobj = lnobj;
    }
}
