using System.Security.Cryptography;
using System.Text;

namespace BMS
{
    public class BMSDecoder : ChartDecoder
    {
        private string[] wavList = new string[62 * 62];
        private int[] wm = new int[62 * 62];

        private string[] bgaList = new string[62 * 62];
        private int[] bm = new int[62 * 62];

        public BMSDecoder()
        {
            lnType = BMSModelDefine.LNTYPE.LONGNOTE;
        }

        public BMSDecoder(BMSModelDefine.LNTYPE lnType)
        {
            base.lnType = lnType;
        }

        // BMSModel 반환
        public new BMSModel? Decode(string path)
        {
            try 
            {

                BMSModel? model = Decode(path, File.ReadAllBytes(path), path.ToLower().EndsWith(".pms"), []);
                if (model == null)
                    return null;

                return model;
            }
            catch (Exception e) 
            {

            }

            return null;
        }

        public override BMSModel? Decode(ChartInformation info)
        {
            try 
            {
                lnType = info.LnType;
                return Decode(info.Path, File.ReadAllBytes(info.Path), info.Path.ToLower().EndsWith(".pms"), info.SelectedRandoms);
            }
            catch (Exception e) 
            {

            }

            return null;
        }

        public BMSModel? Decode(byte[] data, bool isPms, int[] random)
        {
            return Decode(null, data, isPms, random);
        }

        private Stack<int> randomStack = new Stack<int>();
        private Stack<bool> skipStack = new Stack<bool>();
        private Stack<int> selectedRandom = new Stack<int>();

        private BMSModel? Decode(string? path, byte[] data, bool isPms, int[] selectedRandom)
        {
            long time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            BMSModel model = new();

            using MemoryStream stream = new(data);

            #region Hashing
            string md5Hash;
            using (var md5Stream = new MemoryStream(data))
            {
                md5Hash = BitConverter.ToString(MD5.Create().ComputeHash(md5Stream)).Replace("-", "").ToLower();
            }

            // Compute SHA256 hash
            string sha256Hash;
            using (var sha256Stream = new MemoryStream(data))
            {
                sha256Hash = BitConverter.ToString(SHA256.Create().ComputeHash(sha256Stream)).Replace("-", "").ToLower();
            }
            #endregion

            #region Decode
            model.Mode = isPms ? Mode.POPN_9K : Mode.BEAT_5K;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using var reader = new StreamReader(stream, Encoding.GetEncoding(932));
            do
            {
                string? line = reader.ReadLine();

                Console.WriteLine(line);
            } while (!reader.EndOfStream);

            #endregion

            return null;
        }
    }
}
