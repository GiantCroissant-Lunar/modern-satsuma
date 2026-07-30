# Agent adapters

Adapter configuration is grouped by responsibility:

- `targets/` describes generated agent-resource surfaces. The shared `agents`
  target writes `.agents/skills/` and `AGENTS.md`, and Kiro writes its native
  `.kiro/` structure.
- `runners/` records CLI invocation and context metadata for tools that consume
  an existing target without requiring their own generated resource surface.

Run `python tools/sync-agent-resources.py` to synchronize every adapter. Pass a
leaf name such as `--provider agents` or a qualified path such as
`--provider targets/agents` to synchronize one adapter.
