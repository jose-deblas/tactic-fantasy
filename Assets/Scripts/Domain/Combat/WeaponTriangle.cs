using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Domain.Combat
{
    public static class WeaponTriangle
    {
        public const int ADVANTAGE_DAMAGE = 1;
        public const int ADVANTAGE_HIT = 10;
        public const int DISADVANTAGE_DAMAGE = -1;
        public const int DISADVANTAGE_HIT = -10;

        public static (int damageBonus, int hitBonus) GetTriangleModifiers(IWeapon attacker, IWeapon defender)
        {
            if (HasAdvantage(attacker.Type, defender.Type))
            {
                return (ADVANTAGE_DAMAGE, ADVANTAGE_HIT);
            }
            else if (HasDisadvantage(attacker.Type, defender.Type))
            {
                return (DISADVANTAGE_DAMAGE, DISADVANTAGE_HIT);
            }
            return (0, 0);
        }

        private static bool HasAdvantage(WeaponType attacker, WeaponType defender)
        {
            // Physical triangle: Sword > Axe > Lance > Sword
            // Magic triangle: Fire > Wind > Thunder > Fire
            return (attacker == WeaponType.SWORD && defender == WeaponType.AXE) ||
                   (attacker == WeaponType.AXE && defender == WeaponType.LANCE) ||
                   (attacker == WeaponType.LANCE && defender == WeaponType.SWORD) ||
                   (attacker == WeaponType.FIRE && defender == WeaponType.WIND) ||
                   (attacker == WeaponType.WIND && defender == WeaponType.THUNDER) ||
                   (attacker == WeaponType.THUNDER && defender == WeaponType.FIRE);
        }

        private static bool HasDisadvantage(WeaponType attacker, WeaponType defender)
        {
            return (attacker == WeaponType.AXE && defender == WeaponType.SWORD) ||
                   (attacker == WeaponType.LANCE && defender == WeaponType.AXE) ||
                   (attacker == WeaponType.SWORD && defender == WeaponType.LANCE) ||
                   (attacker == WeaponType.WIND && defender == WeaponType.FIRE) ||
                   (attacker == WeaponType.THUNDER && defender == WeaponType.WIND) ||
                   (attacker == WeaponType.FIRE && defender == WeaponType.THUNDER);
        }
    }
}
