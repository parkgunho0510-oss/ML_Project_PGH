namespace Wooduduk.Core
{
    public interface IBaseStats
    {
        float MoveSpeed { get; }
        float SwingRange { get; }
        float SwingDamage { get; }
        float SwingSpeed { get; }
        float Defense { get; }
        int MaxHealth { get; }

    }
}
