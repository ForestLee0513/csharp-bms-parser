namespace BMS
{
    public class ChartInformation
    {
        public string Path { get; private set; }
        public BMSModelDefine.LNTYPE LnType { get; private set; }
        public int[] SelectedRandoms { get; private set; }

        public ChartInformation(string path, BMSModelDefine.LNTYPE lnType, int[] selectedRandoms)
        {
            Path = path;
            LnType = lnType;
            SelectedRandoms = selectedRandoms;
        }
    }
}
