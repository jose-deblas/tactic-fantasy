#!/usr/bin/env python3
import time, datetime, subprocess, os

# Calculate seconds until next 02:30 local time
now = datetime.datetime.now()
target = now.replace(hour=2, minute=30, second=0, microsecond=0)
if target <= now:
    target += datetime.timedelta(days=1)
secs = int((target - now).total_seconds())
print('Sleeping', secs, 'seconds until', target.isoformat())

# Sleep until target
time.sleep(secs)

# Claude prompt
prompt = '''Implement Phase 7: Support/Affinity System + Biorhythm per the project's Plan of Improvement. Work TDD inside this repository at /Users/r2d2/Projects/fire-emblem/tactic-fantasy.

Requirements:
- Create and checkout a new local branch named: phase-7-affinity-system. Do NOT push to remote.
- Implement a Support/Affinity system that: tracks support relationships between units (support levels: None, C, B, A), grants stat bonuses when units are adjacent or in support range, persists learnable supports to Unit, and integrates with CombatResolver so bonuses apply to CombatForecast/CombatResolver calculations.
- Implement a Biorhythm system that: provides periodic temporary stat modifiers (e.g., +X ATK or -Y SKL) per unit based on a deterministic cycle. Provide a simple deterministic implementation suitable for tests and gameplay.
- Add EditMode NUnit tests under the existing test project(s): SupportBonusTests.cs, SupportTrackerTests.cs, BiorhythmTests.cs. Follow existing test patterns (arrange/act/assert), ensure tests cover typical cases (bonus application, persistent support leveling, biorhythm phase effects) and run dotnet test.
- Run 'dotnet test' and fix failing tests until all Domain tests pass or until you encounter unrelated failures that block progress (in which case stop and print details for manual intervention).
- Commit changes locally with message: "feat(phase-7): support/affinity system + biorhythm (TDD)". Do NOT push.

When finished, print a concise summary including: git status, last commit message, and a list of modified/created files. Then run: openclaw system event --text "Done: Phase 7 implemented (phase-7-affinity-system)" --mode now

If you need to ask a design question, stop and print the question so a human can answer.
'''

# Run Claude Code CLI with the prompt
print('Launching Claude Code...')
ret = subprocess.run(['claude', '--permission-mode', 'bypassPermissions', '--print', prompt], cwd=os.getcwd())
print('Claude exit code:', ret.returncode)

# End of launcher
