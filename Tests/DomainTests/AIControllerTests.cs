using NUnit.Framework;
using System.Collections.Generic;
using TacticFantasy.Domain.AI;
using TacticFantasy.Domain.Map;
using TacticFantasy.Domain.Units;
using TacticFantasy.Domain.Weapons;

namespace DomainTests
{
    public class AIControllerTests
    {
        // Minimal fake implementations to exercise selection logic
        private class DummyMap : IGameMap
        {
            public int Width => 10;
            public int Height => 10;
            public Weather CurrentWeather => Weather.Clear;
            public void SetWeather(Weather weather) { }
            public ITile GetTile(int x, int y) => new Tile(x, y, TerrainType.Plain);
            public bool IsValidPosition(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
            public int GetDistance(int x1, int y1, int x2, int y2) => System.Math.Abs(x1 - x2) + System.Math.Abs(y1 - y2);
        }

        private class DummyUnit : IUnit
        {
            public int Id { get; set; }
            public Team Team { get; set; }
            public (int x, int y) Position { get; set; }
            public bool IsAlive => CurrentHP > 0;
            public bool HasBrokenWeapon => false;
            public bool IsLaguz => false;
            public bool IsTransformed => true;
            public IStats CurrentStats { get; set; }
            public IStats CurrentStatsForTests => CurrentStats;
            public IStats CurrentStatsLegacy => CurrentStats;
            public IStats CurrentStatsProp => CurrentStats;
            public IStats CurrentStats2 => CurrentStats;

            public int CurrentHP { get; set; }
            public int MaxHP { get; set; }
            public IWeapon EquippedWeapon { get; set; }
            public IStats CurrentStatsExplicit => CurrentStats;
        }

        // Minimal weapon and stats implementations
        private class SimpleWeapon : IWeapon
        {
            public WeaponType Type { get; set; }
            public int MinRange { get; set; }
            public int MaxRange { get; set; }
            public int Might { get; set; }
            public StatusEffectType? OnHitStatus => null;
        }

        private class SimpleStats : IStats
        {
            public int MOV { get; set; }
            public int STR { get; set; }
        }

        [Test]
        public void GetBestAttackOptionForReachable_PrefersLowerHpOnTie()
        {
            var ai = new AIController(null);

            var attacker = new DummyUnit
            {
                Id = 1,
                Team = Team.EnemyTeam,
                Position = (0,0),
                CurrentHP = 10,
                MaxHP = 10,
                EquippedWeapon = new SimpleWeapon { MinRange = 1, MaxRange = 1, Might = 5, Type = WeaponType.SWORD },
                CurrentStats = new SimpleStats { MOV = 3, STR = 5 }
            };

            var targetA = new DummyUnit
            {
                Id = 2,
                Team = Team.PlayerTeam,
                Position = (1,0),
                CurrentHP = 8,
                MaxHP = 10,
                EquippedWeapon = new SimpleWeapon { MinRange = 1, MaxRange = 1, Might = 5, Type = WeaponType.SWORD },
                CurrentStats = new SimpleStats { MOV = 3, STR = 3 }
            };

            var targetB = new DummyUnit
            {
                Id = 3,
                Team = Team.PlayerTeam,
                Position = (1,0),
                CurrentHP = 4,
                MaxHP = 10,
                EquippedWeapon = new SimpleWeapon { MinRange = 1, MaxRange = 1, Might = 5, Type = WeaponType.SWORD },
                CurrentStats = new SimpleStats { MOV = 3, STR = 3 }
            };

            var enemies = new List<IUnit> { targetA, targetB };
            var reachable = new HashSet<(int, int)> { (1,0) };
            var map = new DummyMap();

            var best = ai.GetBestAttackOptionForReachable(attacker, enemies, reachable, map);

            Assert.IsNotNull(best);
            Assert.AreEqual(3, best.Value.target.Id, "AI should prefer the lower-HP target when scores tie.");
        }

        [Test]
        public void GetBestAttackOptionForReachable_PrefersHigherDefenseTileOnTie()
        {
            var ai = new AIController(null);

            var attacker = new DummyUnit
            {
                Id = 1,
                Team = Team.EnemyTeam,
                Position = (0,0),
                CurrentHP = 20,
                MaxHP = 20,
                EquippedWeapon = new SimpleWeapon { MinRange = 1, MaxRange = 1, Might = 5, Type = WeaponType.SWORD },
                CurrentStats = new SimpleStats { MOV = 3, STR = 5 }
            };

            var target = new DummyUnit
            {
                Id = 2,
                Team = Team.PlayerTeam,
                Position = (2,0),
                CurrentHP = 12,
                MaxHP = 20,
                EquippedWeapon = new SimpleWeapon { MinRange = 1, MaxRange = 1, Might = 5, Type = WeaponType.SWORD },
                CurrentStats = new SimpleStats { MOV = 3, STR = 3 }
            };

            // Map with two candidate attack positions: (1,0) plain, (1,1) fort
            var map = new DummyMapWithTerrain();

            var reachable = new HashSet<(int, int)> { (1,0), (1,1) };
            var enemies = new List<IUnit> { target };

            var best = ai.GetBestAttackOptionForReachable(attacker, enemies, reachable, map);

            Assert.IsNotNull(best);
            // Expect the AI to pick the (1,1) tile because it's a Fort (higher defense bonus)
            Assert.AreEqual((1,1), best.Value.position, "AI should prefer the tile with higher terrain defense when scores tie.");
        }

        // Additional DummyMap that returns different terrain per coordinate
        private class DummyMapWithTerrain : IGameMap
        {
            public ITile GetTile(int x, int y)
            {
                if (x == 1 && y == 1)
                    return new Tile(1,1, TerrainType.Fort);
                return new Tile(x,y, TerrainType.Plain);
            }
            public int GetDistance(int x1, int y1, int x2, int y2) => System.Math.Abs(x1 - x2) + System.Math.Abs(y1 - y2);
            public int Width => 10;
            public int Height => 10;
            public Weather CurrentWeather => Weather.Clear;
            public void SetWeather(Weather weather) { }
            public bool IsValidPosition(int x, int y) => x >= 0 && x < Width && y >=0 && y < Height;
        }
    }
}
