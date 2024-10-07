using System.Linq;

namespace BMS
{
    public class Mode
    {
        public int Id { get; }
        public string Hint { get; }
        public int Player { get; }
        public int Key { get; }
        public int[] ScratchKey { get; }

        private Mode(int id, string hint, int player, int key, int[] scratchKey)
        {
            Id = id;
            Hint = hint;
            Player = player;
            Key = key;
            ScratchKey = scratchKey;
        }

        public static readonly Mode BEAT_5K = new Mode(5, "beat-5k", 1, 6, new int[] { 5 });
        public static readonly Mode BEAT_7K = new Mode(7, "beat-7k", 1, 8, new int[] { 7 });
        public static readonly Mode BEAT_10K = new Mode(10, "beat-10k", 2, 12, new int[] { 5, 11 });
        public static readonly Mode BEAT_14K = new Mode(14, "beat-14k", 2, 16, new int[] { 7, 15 });
        public static readonly Mode POPN_5K = new Mode(5, "popn-5k", 1, 5, new int[] { });
        public static readonly Mode POPN_9K = new Mode(5, "popn-9k", 1, 9, new int[] { });
        public static readonly Mode KEYBOARD_24K = new Mode(25, "keyboard-24k", 1, 26, new int[] { 24, 25 });
        public static readonly Mode KEYBOARD_24K_DOUBLE = new Mode(50, "keyboard-24k", 2, 52, new int[] { 24, 25, 50, 51 });

        public static readonly Mode[] Values = 
        {
            BEAT_5K, BEAT_7K, BEAT_10K, BEAT_14K,
            POPN_5K, POPN_9K, KEYBOARD_24K, KEYBOARD_24K_DOUBLE
        };

        public bool IsScratchKey(int key)
        {
            return ScratchKey.Contains(key);
        }

        public static Mode? GetMode(string hint)
        {
            return Values.FirstOrDefault(m => m.Hint.Equals(hint, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
