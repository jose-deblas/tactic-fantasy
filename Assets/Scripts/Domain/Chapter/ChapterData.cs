using System;
using System.Collections.Generic;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Turn;

namespace TacticFantasy.Domain.Chapter
{
    public class ChapterData
    {
        public string Name { get; }
        public int MapSeed { get; }
        public int BaseBexpReward { get; }
        public int ParTurns { get; }
        public IReadOnlyList<string> ShopItems { get; }
        public IVictoryCondition VictoryCondition { get; }
        public MapDefinition MapDefinition { get; }

        public ChapterData(
            string name,
            int mapSeed,
            int baseBexpReward,
            int parTurns,
            IReadOnlyList<string> shopItems = null,
            IVictoryCondition victoryCondition = null,
            MapDefinition mapDefinition = null)
        {
            Name = name;
            MapSeed = mapSeed;
            BaseBexpReward = baseBexpReward;
            ParTurns = parTurns;
            ShopItems = shopItems ?? Array.Empty<string>();
            VictoryCondition = victoryCondition ?? VictoryConditionFactory.Rout();
            MapDefinition = mapDefinition;
        }

        /// <summary>
        /// Calculates total BEXP earned for a chapter completion.
        /// </summary>
        public int CalculateBexpReward(int turnsTaken, int alliesAlive, int totalAllies)
        {
            int completion = BaseBexpReward;
            int turnBonus = Math.Max(0, (ParTurns - turnsTaken) * 5);
            int survivalBonus = totalAllies > 0
                ? (int)(alliesAlive * 50.0 / totalAllies)
                : 0;

            return completion + turnBonus + survivalBonus;
        }
    }
}
