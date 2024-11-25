namespace BMSParser
{
    public class MineNote : Note
    {
        public double Damage { get; }

        public MineNote(double damage)
        {
            Damage = damage;
        }
    }
}
