using System;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Skills
{
    public static class SkillDatabase
    {
        public static ISkill CreateAdept() => new AdeptSkill();
        public static ISkill CreateVantage() => new VantageSkill();
        public static ISkill CreateWrath() => new WrathSkill();
        public static ISkill CreateResolve() => new ResolveSkill();
        public static ISkill CreateNihil() => new NihilSkill();
        public static ISkill CreateParagon() => new ParagonSkill();

        private class AdeptSkill : ISkill
        {
            public string Name => "Adept";
            public SkillActivationPhase ActivationPhase => SkillActivationPhase.OnAttack;

            public bool CanActivate(IUnit owner, IUnit opponent, Random rng)
            {
                return rng.Next(100) < owner.CurrentStats.SKL;
            }

            public void Apply(CombatContext ctx)
            {
                ctx.ExtraAttacks += 1;
                ctx.ActivatedSkills.Add(Name);
            }
        }

        private class VantageSkill : ISkill
        {
            public string Name => "Vantage";
            public SkillActivationPhase ActivationPhase => SkillActivationPhase.PreCombat;

            public bool CanActivate(IUnit owner, IUnit opponent, Random rng)
            {
                return owner.CurrentHP <= owner.MaxHP / 2;
            }

            public void Apply(CombatContext ctx)
            {
                ctx.VantageActivated = true;
                ctx.ActivatedSkills.Add(Name);
            }
        }

        private class WrathSkill : ISkill
        {
            public string Name => "Wrath";
            public SkillActivationPhase ActivationPhase => SkillActivationPhase.OnAttack;

            public bool CanActivate(IUnit owner, IUnit opponent, Random rng)
            {
                return owner.CurrentHP <= owner.MaxHP / 2;
            }

            public void Apply(CombatContext ctx)
            {
                ctx.ForceCritical = true;
                ctx.ActivatedSkills.Add(Name);
            }
        }

        private class ResolveSkill : ISkill
        {
            public const int StatBoost = 7;

            public string Name => "Resolve";
            public SkillActivationPhase ActivationPhase => SkillActivationPhase.Passive;

            public bool CanActivate(IUnit owner, IUnit opponent, Random rng)
            {
                return owner.CurrentHP <= owner.MaxHP / 2;
            }

            public void Apply(CombatContext ctx)
            {
                var stats = ctx.AttackerEffectiveStats;
                ctx.AttackerEffectiveStats = new CharacterStats(
                    stats.HP,
                    stats.STR,
                    stats.MAG,
                    stats.SKL + StatBoost,
                    stats.SPD + StatBoost,
                    stats.LCK,
                    stats.DEF + StatBoost,
                    stats.RES,
                    stats.MOV
                );
                ctx.ActivatedSkills.Add(Name);
            }
        }

        private class NihilSkill : ISkill
        {
            public string Name => "Nihil";
            public SkillActivationPhase ActivationPhase => SkillActivationPhase.PreCombat;

            public bool CanActivate(IUnit owner, IUnit opponent, Random rng)
            {
                return true;
            }

            public void Apply(CombatContext ctx)
            {
                ctx.DefenderSkillsNegated = true;
                ctx.ActivatedSkills.Add(Name);
            }
        }

        public static ISkill CreateSol() => new SolSkill();
        public static ISkill CreateLuna() => new LunaSkill();

        private class SolSkill : ISkill
        {
            public string Name => "Sol";
            public SkillActivationPhase ActivationPhase => SkillActivationPhase.OnDamageDealt;

            /// <summary>Activation chance = SKL / 2 percent.</summary>
            public bool CanActivate(IUnit owner, IUnit opponent, Random rng)
            {
                int chance = owner.CurrentStats.SKL / 2;
                return chance > 0 && rng.Next(100) < chance;
            }

            /// <summary>Heals the attacker for the amount of damage dealt this strike.</summary>
            public void Apply(CombatContext ctx)
            {
                ctx.SolHealAmount += ctx.LastStrikeDamage;
                ctx.ActivatedSkills.Add(Name);
            }
        }

        private class LunaSkill : ISkill
        {
            public string Name => "Luna";
            public SkillActivationPhase ActivationPhase => SkillActivationPhase.OnAttack;

            /// <summary>Activation chance = SKL / 2 percent.</summary>
            public bool CanActivate(IUnit owner, IUnit opponent, Random rng)
            {
                int chance = owner.CurrentStats.SKL / 2;
                return chance > 0 && rng.Next(100) < chance;
            }

            /// <summary>Flags that defender DEF/RES should be halved for the current strike.</summary>
            public void Apply(CombatContext ctx)
            {
                ctx.LunaActive = true;
                ctx.ActivatedSkills.Add(Name);
            }
        }

        private class ParagonSkill : ISkill
        {
            public string Name => "Paragon";
            public SkillActivationPhase ActivationPhase => SkillActivationPhase.Passive;

            public bool CanActivate(IUnit owner, IUnit opponent, Random rng)
            {
                return true;
            }

            public void Apply(CombatContext ctx)
            {
                ctx.HasParagon = true;
                ctx.ActivatedSkills.Add(Name);
            }
        }
    }
}
