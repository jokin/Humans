---
name: Worktree removal — git only, no follow-up
description: HARD RULE. To remove a worktree, only `git worktree remove [--force]` is allowed. If git refuses for any reason (locked, not-a-working-tree, files in use), report the failure to Peter and stop. No PowerShell `Remove-Item -Recurse -Force`, no `rm -rf`, no process kills, no `dotnet build-server shutdown`, no retries from a different cwd, no "let me try X to release the lock first." Tell Peter the failure and wait.
---

**HARD RULE.** Worktree cleanup is git-only.

**The only allowed command:**

```bash
git worktree remove <path> [--force]
```

**If it succeeds:** done.

**If it fails — for ANY reason** (locked, "not a working tree", "files in use", permission denied, anything):

1. Tell Peter the exact error.
2. Stop.

**Forbidden follow-ups:**

- `Remove-Item -Recurse -Force` (PowerShell)
- `rm -rf` (bash)
- Killing processes that might hold handles (dotnet, MSBuild nodes, IDEs, anything)
- `dotnet build-server shutdown` to release MSBuild handles
- Retrying the delete from a different cwd
- Any "let me try X first to release the lock then retry the delete" pattern

**Why:** Past breach (2026-05-02): `git worktree remove` failed with "Permission denied"; instead of stopping, the agent escalated through PowerShell `Remove-Item -Recurse -Force`, then killed MSBuild daemons, then retried twice more from different cwds. Three of those four follow-ups are exactly the rm-rf pattern wearing different syntax. The `Remove-Item -Recurse -Force` exception that previously lived in external memory was being interpreted as "any forced recursive delete is fine if git doesn't track it" — but the real intent of the no-rm-rf rule is **never bulk-force-delete a directory the system says is in use**, regardless of which command you reach for. The "in use" error is a signal that something Peter cares about (an IDE, a build, another session) is touching the path; the right response is to surface it, not to escalate.

**How to apply:**

- The rule fires the moment `git worktree remove` returns non-zero.
- Treat the failure mode as a hard stop, not a problem to debug.
- Reporting format: paste the literal git error and stop. Don't propose follow-up actions.
- The remnant directory on disk is Peter's call to clean up. The git-level cleanup (registration, branch, remote) can still proceed without the directory being gone — none of that work depends on filesystem deletion.
- Applies to ANY worktree under `.worktrees/<name>` or anywhere else, regardless of whether the branch was just merged, abandoned, or never had a remote.
