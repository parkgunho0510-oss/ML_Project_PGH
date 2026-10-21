namespace Wooduduk.Session
{
    public interface IMatchResult
    {
        int FireWood { get; }
        int Kills { get; }
        string WeaponId { get; }
        bool IsAlive { get; } // 흠,, 이건 뺄 수도 있을거같음
    }
}
