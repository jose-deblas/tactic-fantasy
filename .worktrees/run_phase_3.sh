#!/usr/bin/env bash
set -euo pipefail
REPO_DIR="/Users/r2d2/Projects/fire-emblem/tactic-fantasy/.worktrees/phase-3"
cd "$REPO_DIR"
# compute seconds until 13:00 local
secs=$(python3 - <<'PY'
import datetime
now=datetime.datetime.now()
target=datetime.datetime.combine(now.date(), datetime.time(13,0))
secs=int((target-now).total_seconds())
if secs<0: secs=0
print(secs)
PY
)
# wait
sleep "$secs"
# run Claude with heredoc prompt
claude --permission-mode bypassPermissions --print <<'PROMPT'
You are an expert game developer. Implement the Phase 3 tasks described in PLAN-OF-IMPROVEMENT.md located at /Users/r2d2/Projects/fire-emblem/docs/PLAN-OF-IMPROVEMENT.md for 'Third-Tier Classes + Mastery Skills'.

Objectives (in order):
1) Add third-tier class definitions and their balance/data entries in the appropriate data files; 2) Implement Mastery Skills system: data, checks, and basic hooks so units can gain mastery-level effects; 3) Make minimal code changes that are scoped, well-tested, and documented in code comments; 4) Run project tests (if any), fix failing tests if within the scope, and commit changes.

When making changes, follow this workflow:
- Make small focused commits with clear messages (prefix: feat(phase-3))
- If tests exist, run them and only push if tests pass; if tests fail, explain failures and stop for user input
- Push the branch to origin and create a PR titled: "phase-3: Third-Tier Classes + Mastery Skills" with a short body listing changes
- If 'gh' is installed and authenticated, use it to create the PR. If not available, push the branch and leave a note in the commit message with the git push command and suggested PR body.

When completely finished, run: openclaw system event --text "Done: phase-3-third-tier-classes-mastery-skills" --mode now
PROMPT

# Save claude output to log is handled by the caller

exit 0
