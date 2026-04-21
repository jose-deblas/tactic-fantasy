using TacticFantasy.Domain.Weapons;

namespace TacticFantasy.Domain.Map
{
    public static class WeatherEffects
    {
        public const int RAIN_BOW_HIT_PENALTY = -15;
        public const int RAIN_FIRE_DAMAGE_PENALTY = -2;
        public const int RAIN_MOVE_COST_INCREASE = 1;

        public const int SNOW_AVOID_PENALTY = -10;
        public const int SNOW_MOVE_COST_INCREASE = 1;

        public const int SANDSTORM_HIT_PENALTY = -20;
        public const int SANDSTORM_VISION_PENALTY = -2;

        public static int GetHitModifier(Weather weather, WeaponType weaponType)
        {
            return weather switch
            {
                Weather.Rain when weaponType == WeaponType.BOW => RAIN_BOW_HIT_PENALTY,
                Weather.Sandstorm => SANDSTORM_HIT_PENALTY,
                _ => 0
            };
        }

        public static int GetDamageModifier(Weather weather, WeaponType weaponType)
        {
            return weather switch
            {
                Weather.Rain when weaponType == WeaponType.FIRE => RAIN_FIRE_DAMAGE_PENALTY,
                _ => 0
            };
        }

        public static int GetAvoidModifier(Weather weather)
        {
            return weather switch
            {
                Weather.Snow => SNOW_AVOID_PENALTY,
                _ => 0
            };
        }

        public static int GetMoveCostIncrease(Weather weather)
        {
            return weather switch
            {
                Weather.Rain => RAIN_MOVE_COST_INCREASE,
                Weather.Snow => SNOW_MOVE_COST_INCREASE,
                _ => 0
            };
        }

        public static int GetVisionPenalty(Weather weather)
        {
            return weather switch
            {
                Weather.Sandstorm => SANDSTORM_VISION_PENALTY,
                _ => 0
            };
        }
    }
}
