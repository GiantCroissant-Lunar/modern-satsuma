# Handover — 2026-05-31: modern-satsuma `Plate.` prefix removal **COMPLETE**

**Status: DONE & verified.** This session executed the rename described in the recipe handover
`2026-05-31-plate-prefix-removal-and-modern-satsuma.md` (`04d1696`). modern-satsuma was the **last
family** in the workspace-wide `Plate.`-prefix removal; that arc is now complete across all in-scope repos.
Cross-repo state lives in memory: `project-drop-plate-prefix-timedete` (updated this session),
`project-modern-satsuma-version-truth` (updated), `reference-rfc0014-shared-contract-alc`.

---

## 0. What the user decided at session start

PackageId scheme = **`GiantCroissant.ModernSatsuma.*`** (the company prefix, matching service-archi/plugin-archi),
**not** the bare `ModernSatsuma.*` the recipe had predicted. So the final mapping is:

| Surface | Before | After |
|---|---|---|
| Namespace / `using` (code) | `Plate.ModernSatsuma` | `ModernSatsuma` |
| AssemblyName / DLL | `Plate.ModernSatsuma.dll` | `ModernSatsuma.dll` |
| Folders / `.csproj` / `.sln` | `Plate.ModernSatsuma*` | `ModernSatsuma*` |
| PackageId (NuGet) | `Plate.ModernSatsuma.*` (no GiantCroissant) | `GiantCroissant.ModernSatsuma.*` |

The `Satsuma` ref-project (`ref-projects/satsumagraph-code/`) and the Nuke `TestSatellite` fixture were
correctly left alone (reference-only / build-harness, not shipped, not `Plate.`-prefixed in TestSatellite's case).

## 1. Commits (all build/test-verified)

| Repo | Commit | What |
|---|---|---|
| **modern-satsuma** | `e0613cb` | Rename: 128 files. Namespaces/usings/folders/projects/sln/snapshots `Plate.ModernSatsuma.*`→`ModernSatsuma.*`; 5 shipped PackageIds → `GiantCroissant.ModernSatsuma.*` (SourceGenerators gained an explicit PackageId — it had `IsPackable` but no id). `Build.cs` solution path updated. **Also** fixed a pre-existing CPM gap (see §3). |
| **modern-satsuma** | `15b9b66` | Docs: `CLAUDE.md` (namespace/package rule, paths, sln, status/date), top + 6 per-project `README.md`, `CONTRIBUTING.md`. Install commands now use `GiantCroissant.ModernSatsuma.*`. Historical `docs/` (RFCs, analysis, implementation, status, prior handovers) left as point-in-time records. |
| **unify-storage** | `ba39f18` | Consumer. It uses a **cross-repo `ProjectReference`** into modern-satsuma/src (NOT the NuGet package) — fixed the path (`...\src\ModernSatsuma\ModernSatsuma.csproj`) + 3 `using`s. RocksDb runtime builds green. (`native/build/` foreign WIP left untouched.) |
| **fantasim-app-godot** | `4b3b04a` | Consumer. 3 PackageReferences `Plate.ModernSatsuma`→`GiantCroissant.ModernSatsuma` (Version `0.0.1` unchanged) + code usings (App.NodeGraph.Executor incl. `SatsumaNode` alias, App.Composition, a doc comment). App.NodeGraph.Executor + App.Composition build green; complete-app restores. |

**Note:** the consumer surface was larger than the recipe's "7 + 4" — app-godot now also has an
`App.NodeGraph.Executor` plugin + `App.NodeGraph.Contracts` contract using it (added since the recipe was written).

## 2. Packages + feed

- Packed **`GiantCroissant.ModernSatsuma.* 0.0.1`** (5 nupkg + 4 snupkg) via
  `dotnet pack dotnet/framework/ModernSatsuma.sln -c Release -p:Version=0.0.1`.
  - GitVersion reports prerelease `0.0.1-43`; packed **stable `0.0.1`** explicitly (matches the prior package +
    what consumers pin; stable > prerelease in NuGet ordering). Core nuspec verified: id `GiantCroissant.ModernSatsuma`,
    version `0.0.1`, DLL `lib/netstandard2.1/ModernSatsuma.dll`.
- `build.config.json syncLocalNugetFeed:false` → **manually** copied the 5 new packages to
  `C:\lunar-horse\packages\nuget`. Old `Plate.ModernSatsuma.0.0.1` **kept** (never-delete); no out-of-scope
  consumer exists (no muni-dungeon refs) → dual-publish was unnecessary, old pkg is now inert.

## 3. Pre-existing issues surfaced (NOT caused by the rename)

1. **7 `SystemDrawing.Tests` failures** (GDI+: `Bitmap(0,0)` "Parameter is not valid", exception-throw assertions).
   **Verified pre-existing** by building+testing HEAD code in a detached worktree → **identical 7 fail / 121 pass**.
   A namespace rename cannot change GDI+ runtime behavior; stack traces originate in `System.Drawing` internals.
   Full suite otherwise: 415 pass (core 135, Abstractions 59, SkiaSharp 100).
2. **CPM gap** — `dotnet/framework/tests/Directory.Packages.props` (a standalone CPM file, doesn't import the root)
   lacked `System.Drawing.Common` + `SkiaSharp` `PackageVersion`, so a *clean* restore of the renderer test
   projects failed `NU1010`. Surfaced only because clearing `obj/` forced a fresh restore. Added the two entries
   (matching the framework root props versions) in `e0613cb`.

## 4. Gotchas worth carrying forward

- **`git mv` of project folders hit "Permission denied"** because a running **Roslyn build server (VBCSCompiler) /
  design-time build** held handles on `obj/`. Fix that worked: `dotnet build-server shutdown`, then **per-file**
  `git mv` (moves tracked `.cs`/`.csproj` into a fresh dir, leaving the locked `obj/` behind), then remove the
  artifact-only husk dirs. `obj/` regenerated mid-operation — a build server was active.
- **A concurrent agent** staged a whole-file CRLF/LF churn of `tests/ModernSatsuma.Tests/DeterministicDijkstraTests.cs`
  *after* my rename commit. Left untouched; all modern-satsuma commits were **path-scoped** so it was never swept in.
- A stray untracked `nul` file sits at the repo root (not mine; Windows reserved-name artifact) and pre-existing
  junk `tests/.../SystemDrawingGraphicsContextTests.cs.bak` + `temp_fix.txt` remain — left alone.

## 5. NEXT — remaining `Plate.`-suite threads (NOT modern-satsuma)

Authority: memory **`project-drop-plate-prefix-timedete`** (STILL TODO section) + the recipe handover §4.

1. ~~Consumer migration to the new `GiantCroissant.{ServiceArchi,PluginArchi}.*` PackageIds~~ **DONE 2026-05-31**
   (crosscut → fantasim-world → app-godot). crosscut-foundation `e7b8b68` (+tag `v0.2.1`, re-packed 0.2.1),
   fantasim-world `4251c45` (re-packed 0.2.94), fantasim-app-godot `c2d24a0` (32 csproj; crosscut floats →0.2.1,
   FantaSim 0.2.93→0.2.94). **orc-bot SKIPPED per user** — its migration `080de17` was reverted (`859fcea`); it stays
   on old IDs (standalone leaf, dual-published, nothing consumes it). Zero in-scope old-ID refs remain *outside
   orc-bot*; muni-dungeon stays old via dual-publish. Full detail in the memory. Carry-forward gotchas: new
   plugin-archi has **no `.Contracts`** package; crosscut `task pack:scg` is broken (empty-version subtask env) so the
   SCG decorator packages stayed at 0.2.0 (harmless).
2. **PLUG009 whitelist read bug** (plugin-archi) — `PluginArchiSharedAssemblyPrefixes` reaches the editorconfig but
   not the analyzer's `GlobalOptions` at real build time; needs a `v0.1.6` bump to re-test. See
   `reference-rfc0014-shared-contract-alc`. **← now the top open thread.**
3. (Lower priority) fantasim-world contract-only produce-boundary cleanup (PLUG009 leak inventory in that memory).
