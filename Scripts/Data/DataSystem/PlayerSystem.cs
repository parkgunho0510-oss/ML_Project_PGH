using Wooduduk.Data.Runtime;

namespace Wooduduk.Data.DataSystem
{
    public static class PlayerSystem
    {
        public static void DamagePlayer(PlayerRuntimeData player, float damage)
        {
            if (player._isDead) return;

            player._currentHp -= damage;

            if (player._currentHp <= 0)
            {
                player._currentHp = 0;
                player._isDead = true;
            }
        }

        public static void HealPlayer(PlayerRuntimeData player, float amount)
        {
            if (player._isDead) return;

            player._currentHp += amount;

            if (player._currentHp > player._currentMaxHp)
            {
                player._currentHp = player._currentMaxHp;
            }
        }

        public static void AddWood(PlayerRuntimeData player, int wood)
        {
            player._collectedWood += wood;
        }

        public static void AddKill(PlayerRuntimeData player)
        {
            player._killCount++;
        }

        public static void Extract(
            PlayerRuntimeData player)
        {
            player._isExtracted = true;
        }
    }
}
