using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TacticFantasy.Domain.Save;
using TacticFantasy.Domain.Turn;
using TacticFantasy.Domain.Units;

namespace TacticFantasy.Adapters.Persistence
{
    /// <summary>
    /// Hexagonal adapter: persists a <see cref="GameSnapshot"/> to a JSON file on disk.
    /// Pure C# — no Unity dependencies. Implements the <see cref="IGameRepository"/> port.
    /// </summary>
    public class JsonFileGameRepository : IGameRepository
    {
        private readonly string _filePath;

        public JsonFileGameRepository(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        public bool HasSave => File.Exists(_filePath);

        // -----------------------------------------------------------------------

        public void Save(GameSnapshot snapshot)
        {
            var dto = GameSnapshotDto.FromDomain(snapshot);

            var json = JsonConvert.SerializeObject(dto, Formatting.Indented);

            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(_filePath, json);
        }

        public GameSnapshot Load()
        {
            if (!HasSave) return null;

            var json = File.ReadAllText(_filePath);
            var dto  = JsonConvert.DeserializeObject<GameSnapshotDto>(json);
            return dto.ToDomain();
        }

        // -----------------------------------------------------------------------
        // Private DTOs — serialisation detail, not part of the domain
        // -----------------------------------------------------------------------

        private class GameSnapshotDto
        {
            public string          CurrentPhase { get; set; }
            public int             TurnCount    { get; set; }
            public List<UnitSnapshotDto> Units  { get; set; }

            public static GameSnapshotDto FromDomain(GameSnapshot s)
            {
                var units = new List<UnitSnapshotDto>(s.Units.Count);
                foreach (var u in s.Units)
                    units.Add(UnitSnapshotDto.FromDomain(u));

                return new GameSnapshotDto
                {
                    CurrentPhase = s.CurrentPhase.ToString(),
                    TurnCount    = s.TurnCount,
                    Units        = units
                };
            }

            public GameSnapshot ToDomain()
            {
                var phase = Enum.Parse<Phase>(CurrentPhase);
                var units = new List<UnitSnapshot>(Units.Count);
                foreach (var u in Units)
                    units.Add(u.ToDomain());

                return GameSnapshot.Rebuild(phase, TurnCount, units);
            }

            // GameSnapshot.Rebuild is a static factory — no reflection needed.
        }

        private class UnitSnapshotDto
        {
            public int    Id                   { get; set; }
            public string Name                 { get; set; }
            public string Team                 { get; set; }
            public string ClassName            { get; set; }
            public string WeaponName           { get; set; }
            public int    CurrentHP            { get; set; }
            public int    PositionX            { get; set; }
            public int    PositionY            { get; set; }
            public int    Level                { get; set; }
            public int    Experience           { get; set; }
            public string StatusType           { get; set; }
            public int    StatusRemainingTurns { get; set; }

            public static UnitSnapshotDto FromDomain(UnitSnapshot s)
            {
                return new UnitSnapshotDto
                {
                    Id                   = s.Id,
                    Name                 = s.Name,
                    Team                 = s.Team.ToString(),
                    ClassName            = s.ClassName,
                    WeaponName           = s.WeaponName,
                    CurrentHP            = s.CurrentHP,
                    PositionX            = s.PositionX,
                    PositionY            = s.PositionY,
                    Level                = s.Level,
                    Experience           = s.Experience,
                    StatusType           = s.StatusType.ToString(),
                    StatusRemainingTurns = s.StatusRemainingTurns
                };
            }

            public UnitSnapshot ToDomain()
            {
                var team       = Enum.Parse<Team>(Team);
                var statusType = Enum.Parse<StatusEffectType>(StatusType);
                return UnitSnapshot.Rebuild(
                    Id, Name, team, ClassName, WeaponName,
                    CurrentHP, PositionX, PositionY,
                    statusType, StatusRemainingTurns,
                    Level, Experience);
            }
        }
    }
}
