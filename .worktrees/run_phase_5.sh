#!/usr/bin/env bash
set -euo pipefail
REPO_DIR="/Users/r2d2/Projects/fire-emblem/tactic-fantasy/.worktrees/phase-5"
cd "$REPO_DIR"
secs=$(python3 - <<'PY'
import datetime
now=datetime.datetime.now()
target=datetime.datetime.combine(now.date(), datetime.time(15,0))
secs=int((target-now).total_seconds())
if secs<0: secs=0
print(secs)
PY
)
sleep "$secs"
claude --permission-mode bypassPermissions --print <<'PROMPT'
You are an expert game developer. Implement the Phase 5 tasks described in PLAN-OF-IMPROVEMENT.md located at /Users/r2d2/Projects/fire-emblem/docs/PLAN-OF-IMPROVEMENT.md for 'Base / Shops + Bonus Experience (BEXP)'.

Objectives (in order):
1) Add base/shop systems and item management UI hooks; 2) Implement Bonus Experience (BEXP) rules and spending mechanic; 3) Ensure persistence of purchased items and BEXP between battles; 4) Run tests if available, commit small changes, push branch and open PR titled: "phase-5: Base / Shops + BEXP".

Workflow same as before: small commits (feat(phase-5) prefix), run tests if present, push and create PR using 'gh' when possible.

When completely finished, run: openclaw system event --text "Done: phase-5-base-shops-bexp" --mode now
PROMPT

exit 0
