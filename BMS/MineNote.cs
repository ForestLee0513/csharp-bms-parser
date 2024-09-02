namespace BMS
{
    public class MineNote : Note
    {
        public double Damage { get; private set; }

        public MineNote(int wav, double damage)
        {
            SetWav(wav);
            Damage = damage;
        }

        public void SetDamage(double damage) => Damage = damage;
    }
}
