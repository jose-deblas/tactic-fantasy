using System;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Domain.Skills
{
    public interface ISkill
    {
        string Name { get; }
        SkillActivationPhase ActivationPhase { get; }
        bool CanActivate(IUnit owner, IUnit opponent, Random rng);
        void Apply(CombatContext ctx);
    }
}
