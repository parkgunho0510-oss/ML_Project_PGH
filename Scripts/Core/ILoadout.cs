using Wooduduk.Combat;

namespace Wooduduk.Core
{
    // 아웃 게임에서 고른 로드아웃
    public interface ILoadout
    {
        IWeapon Weapon { get; }
        ITrait Trait { get; }
    }
}
