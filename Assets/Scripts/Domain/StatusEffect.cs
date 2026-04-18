namespace TacticFantasy.Domain
{
    public interface IStatusEffect
    {
        string Name { get; }
        float Duration { get; }
        void Tick(float deltaTime, IUnit target);
        bool IsExpired { get; }
    }

    public interface IUnit
    {
        float Health { get; set; }
        void TakeDamage(float amount);
    }

    public class PoisonEffect : IStatusEffect
    {
        public string Name => "Poison";
        public float Duration { get; private set; }
        public float DamagePerSecond { get; }
        private float elapsed = 0f;

        public PoisonEffect(float duration, float dps)
        {
            Duration = duration;
            DamagePerSecond = dps;
        }

        public void Tick(float deltaTime, IUnit target)
        {
            if (IsExpired) return;
            float effective = deltaTime;
            float damage = DamagePerSecond * effective;
            target.TakeDamage(damage);
            elapsed += deltaTime;
        }

        public bool IsExpired => elapsed >= Duration;
    }
}