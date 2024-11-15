using System.Collections.Generic;
using static BMSParser.Define.BMSModel;
using static BMSParser.Define.PatternProcessor;

namespace BMSParser
{
    public class BMSModel
    {
        #region Custom Definition
        public Extension Extension { get; set; }
        public RankType RankType { get; set; } = RankType.RANK;
        public string SHA256 { get; set; }
        public string MD5 { get; set; }
        public Base Base { get; set; }
        public BMSKey Mode { get; set; } = BMSKey.UNKNOWN;
        #endregion

        #region HEADER
        public int Player { get; set; } = 1;
        public int Rank { get; set; }
        public int DefExRank { get; set; }
        public double Total { get; set; }
        public int VolWav { get; set; }
        public string StageFile { get; set; }
        public string Banner { get; set; }
        public string BackBmp { get; set; }
        public string PlayLevel { get; set; }
        public int Difficulty { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Artist { get; set; }
        public string SubArtist { get; set; }
        public string Genre { get; set; }
        public double Bpm { get; set; }
        public double[] BpmList { get; set; } = new double[62 * 62];
        public double[] StopList { get; set; } = new double[62 * 62];
        public double[] ScrollList { get; set; } = new double[62 * 62];
        public int Lnobj { get; set; } = -1;
        public string[] Wav { get; set; } = new string[62 * 62];
        public string MidiFile { get; set; }
        public string[] Bmp { get; set; } = new string[62 * 62];
        #endregion

        #region Main Data
        public PatternProcessor PatternProcessor { get; } = new PatternProcessor();
        public SortedDictionary<double, Timestamp> Timestamp;
        #endregion
    }
}
