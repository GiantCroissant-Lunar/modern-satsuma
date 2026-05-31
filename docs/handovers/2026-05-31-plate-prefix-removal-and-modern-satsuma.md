# Handover — 2026-05-31: `Plate.` prefix removal (sweep done) → next: adjust modern-satsuma

**Status: active, next-session-ready.** This session removed the `Plate.` prefix across the plate-projects
(it collided conceptually with fantasim's `Geosphere.Plate.*` tectonic-plate naming). **modern-satsuma is the
last family left.** Everything below is committed and green; nothing is half-done. Cross-repo state also lives
in memory: `project-drop-plate-prefix-timedete` and `reference-rfc0014-shared-contract-alc`.

---

## 0. TL;DR + the ONE next move

> **Next move:** rename **modern-satsuma** off the `Plate.` prefix (`Plate.ModernSatsuma.*` → `ModernSatsuma.*`),
> following the proven recipe in §3. It's the smallest remaining family: only **2 in-scope consumers**
> (fantasim-app-godot, unify-storage), **no out-of-scope (muni-dungeon) blocker**, and it's a **leaf dep** for
> both (no multi-layer re-pack cascade). The main work is resolving modern-satsuma's *internal naming
> inconsistency* (see §3) + a **manual** feed sync (its `build.config.json` has `syncLocalNugetFeed:false`).

**The naming RULE established this session (user, 2026-05-31):** drop the `Plate.` prefix from plate-projects, but:
- **Standalone-identity libs** → bare name: `time-dete`→`TimeDete.*` (done), `modern-satsuma`→`ModernSatsuma.*` (next).
- **The shared-foundation repo `plate-shared`** → keep identity via `PlateShared.*` (bare `SCG.`/`General.` would be too generic).
- **PackageId suite grouping** `GiantCroissant.Plate.<Lib>` → `GiantCroissant.<Lib>` (this is csproj-only; never in code).

---

## 1. What shipped this session (all committed, all build/test-verified)

### A. PLUG009 analyzer (the RFC-0014 boundary guardrail) — earlier in the session
| Repo | Commit | What |
|---|---|---|
| **plugin-archi** | `cbcde60` (tagged `v0.1.4`) | New `SharedContractApiLeakAnalyzer` (**PLUG009**): complement of PLUG008 — flags a `[PluginSharedContract]` assembly whose public API exposes a type from a non-shared assembly. + `PluginArchiSharedAssemblyPrefixes` config props, 8 tests, docs. **144 tests green.** |

PLUG009 was then used to derive the produce-boundary leak inventory in fantasim-world (recorded in
`reference-rfc0014-shared-contract-alc` memory). **OPEN BUG:** the `PluginArchiSharedAssemblyPrefixes` whitelist
isn't honored at real build time (works in unit tests; the build_property reaches the editorconfig but the
analyzer's `GlobalOptions` doesn't surface it) — needs a focused fix + a version bump to re-test.

### B. `Plate.` prefix removal sweep
| Repo | Commit(s) | What |
|---|---|---|
| **time-dete** | `d21e933` | namespaces + PackageId `Plate.TimeDete.*` → `TimeDete.*`; packed `TimeDete.* 0.1.1`; **dual-publish** (old `Plate.TimeDete.* 0.1.0` kept for out-of-scope muni-dungeon). 44 tests green. |
| **orc-bot** | `c95e9bd` | 3 TimeDete refs → `TimeDete.* 0.1.1`; builds. |
| **fantasim-world** | `f0b9aba` | 96 files `using Plate.TimeDete.*`→`TimeDete.*` + 2 pkg refs; re-packed **0.2.93** (now declares `TimeDete.Time.Primitives 0.1.1`). |
| **fantasim-app-godot** | `6cabdc5` | bumped 94 `GiantCroissant.FantaSim.* 0.2.90`→`0.2.93` + 9 `using Plate.TimeDete.*`→`TimeDete.*`. 4 plugins build green. |
| **crosscut-foundation** | `18c47e3` | unused `Plate.TimeDete.Time.Primitives` ref → `TimeDete.Time.Primitives 0.1.1`. (Its own projects were already `CrosscutFoundation.*` from the 0.2.0 unify pass.) |
| **plugin-archi** | `a5fb032` | PLUG009 test example → real `TimeDete.Time.Primitives`. |
| **service-archi** | `dc72d27` | synthetic tick test-stub namespace → `TimeDete.Time.Primitives`. |
| **plate-shared** | `80568aa` | `Plate.SCG.*`/`Plate.General.*`/`Plate.Game.*` → `PlateShared.*` across namespaces + AssemblyNames + PackageIds (60 files incl. Verify snapshots). All generator tests green. Fully contained (project-path-consumed, ships no packages). |
| **service-archi** | `ad80ec2` | PackageIds `GiantCroissant.Plate.ServiceArchi.*` → `GiantCroissant.ServiceArchi.* 0.1.1`; re-packed; dual-publish. (PackageId-only — code already bare `ServiceArchi.*`.) |
| **plugin-archi** | `1307456` | PackageIds `GiantCroissant.Plate.PluginArchi.*` → `GiantCroissant.PluginArchi.* 0.1.5` (+ renamed the analyzer's packed `build/$(PackageId).props`); re-packed; dual-publish. |

**Nothing is broken:** un-migrated consumers (incl. out-of-scope **muni-dungeon**, 481 refs to
`GiantCroissant.Plate.*` packages) keep resolving the preserved old packages.

---

## 2. The mechanics you MUST carry forward (hard-won)

1. **Dual-publish** for any package family with **out-of-scope consumers** (muni-dungeon is do-not-edit): pack
   the new-named packages, **leave the old-named packages in the feed** (`C:\lunar-horse\packages\nuget`). Never
   delete the old ones. muni-dungeon stays on old IDs; in-scope repos migrate to new.
2. **Same-DLL-name collision:** if the **AssemblyName is unchanged** by the rename (e.g. time-dete's DLLs were
   always bare `TimeDete.*.dll`; a PackageId-only rename keeps the DLL name), then old + new packages ship the
   **same DLL** → a single build graph **cannot reference both**. Consequence: each consumer graph must migrate
   **fully** (all refs to new IDs), and any in-scope *foundation* that transitively ships an old dep must be
   re-packed before its consumers migrate. If the **AssemblyName also changes**, old/new can coexist (no collision).
3. **Version resolution when packing (the trap that cost time):** `dotnet-gitversion` fails `.git` detection
   from Git Bash, and a `GITVERSION_MAJORMINORPATCH=...` env prefix was **ignored** by unify-build. The reliable
   lever is **`build/build.config.json` `artifactsVersion`** — unify-build falls back to it. To pack a specific
   version: temporarily set `artifactsVersion` to it, pack, then revert. (`task pack` worked directly for
   service-archi/plugin-archi because their GitVersion resolved; fantasim-world needed the artifactsVersion lever.)
4. **Feed sync:** most repos auto-sync via `build.config.json` `syncLocalNugetFeed:true` →
   `localNugetFeedRoot: C:\lunar-horse\packages\nuget`. **time-dete and modern-satsuma do NOT auto-sync** —
   copy `build/_artifacts/<ver>/nuget/<new>.*.nupkg` to the feed manually (only the NEW-named ones).
5. **Multi-agent hygiene:** path-scoped commits only (`git add <files>` + `git commit -- <files>`); never
   reset/stash/checkout; leave foreign WIP alone (fantasim-world `vault/truth-stream-path-convention.md`,
   fantasim-app-godot `docs/architecture/activity-log-ui.md` were left untouched).

---

## 3. NEXT TASK — rename modern-satsuma (`Plate.ModernSatsuma.*` → `ModernSatsuma.*`)

### Current naming (INCONSISTENT — resolve before renaming)
- **Namespaces:** `Plate.ModernSatsuma.*` (e.g. `Plate.ModernSatsuma`, `.Abstractions`, `.Drawing.SkiaSharp`,
  `.Drawing.SystemDrawing`, `.Generators`). → target `ModernSatsuma.*`.
- **AssemblyName:** the upstream ref-project (`ref-projects/satsumagraph-code/Satsuma.csproj`) is `Satsuma`; the
  shipped `dotnet/framework/src/Plate.ModernSatsuma/` lib's AssemblyName must be **verified** (likely defaults to
  the project name `Plate.ModernSatsuma`). **Decision:** set shipped AssemblyName → `ModernSatsuma` to match the
  namespace (and decide whether the `Satsuma` ref-project is shipped or reference-only).
- **PackageId:** `Plate.ModernSatsuma.*` — **note: NO `GiantCroissant.` prefix** (unlike service-archi/plugin-archi).
  Plus a stray `TestSatellite` PackageId. **Decision:** → `ModernSatsuma.*` (bare, matching time-dete's outcome),
  *or* `GiantCroissant.ModernSatsuma.*` if you want to also adopt the company prefix (the user kept `GiantCroissant.`
  for service-archi/plugin-archi but time-dete ended bare — confirm which they want here).

### Consumers (both IN-SCOPE; no out-of-scope blocker)
- **fantasim-app-godot** (7): `<PackageReference Include="Plate.ModernSatsuma" Version="0.0.1" />` in
  `project/hosts/complete-app/complete-app.csproj` + `project/plugins/App.Composition/App.Composition.csproj`,
  plus `using Plate.ModernSatsuma` in code (e.g. `App.Composition/Diagnostics/ModernSatsumaSmoke`). Leaf dep —
  **no fantasim-world re-pack needed.**
- **unify-storage** (4): `using Plate.ModernSatsuma;` in `UnifyStorage.Runtime.RocksDb/{RocksDbGraphAdapter,
  SatsumaGraphTraversal}.cs` (+ a PackageReference). Both .cs usings **and** the package ref must update.

Because the **namespace changes**, consumer `using Plate.ModernSatsuma;` → `using ModernSatsuma;` (unlike the
service-archi/plugin-archi PackageId-only renames, this DOES touch consumer code).

### Feed + version
- Feed currently has only `Plate.ModernSatsuma.0.0.1.nupkg` (+ `.snupkg`). Versioning is murky — memory
  `project-modern-satsuma-version-truth` notes canonical is **0.0.1** per GitVersion; a `1.0.0` pack was ad-hoc
  and missing `TopologicalSort`. **Pack the renamed `ModernSatsuma.* 0.0.1`** and manually copy to the feed;
  keep `Plate.ModernSatsuma.0.0.1` for dual-publish (only if an out-of-scope consumer needs it — none found, so
  dual-publish may be optional here; verify).

### Proven recipe (mirror time-dete `d21e933` + the consumer commits)
1. **In modern-satsuma:** scripted replace `Plate.ModernSatsuma` → `ModernSatsuma` across `.cs/.csproj/.props`
   (namespaces + PackageIds), set the shipped lib's `AssemblyName`/`RootNamespace` to `ModernSatsuma`, decide the
   `Satsuma` ref-project + `TestSatellite` fate. `task build` + `task test` (it has Abstractions/Drawing tests +
   benchmarks). Watch for any Verify-style snapshot/golden files that also need the rename (plate-shared had 15).
2. **Pack** `ModernSatsuma.* 0.0.1` (set `artifactsVersion` if needed per §2.3) and **manually copy** the new
   `ModernSatsuma.*.nupkg` to `C:\lunar-horse\packages\nuget` (no auto-sync). Keep old `Plate.ModernSatsuma.0.0.1`.
3. **Migrate consumers** (path-scoped commits, build-verify each):
   - unify-storage: `using Plate.ModernSatsuma;`→`using ModernSatsuma;` + PackageReference `Plate.ModernSatsuma`→`ModernSatsuma` (+ version). Build.
   - fantasim-app-godot: 2 PackageReferences + code usings. Build the affected plugins (`dotnet build`, not full Godot export). NOTE the transient `CS2012` output-DLL lock on `complete-app` (Godot/Defender) is environmental, not a code error.
4. Commit each repo path-scoped; conventional `refactor(naming)!:`.

---

## 4. Other open threads (recorded in memory; not blocking modern-satsuma)

1. **service-archi + plugin-archi consumer migration** to the new `GiantCroissant.{ServiceArchi,PluginArchi}.*`
   PackageIds (the libs are renamed + dual-published; consumers still on old IDs, safe). Multi-layer cascade:
   crosscut-foundation (20 service-archi refs → re-pack crosscut), fantasim-world (7 `PluginArchi.Extensibility.Abstractions`
   refs → re-pack fantasim-world AGAIN → app-godot bump AGAIN), app-godot (28), orc-bot (2). PackageId-only (no
   consumer code changes). Stage as a focused follow-up.
2. **PLUG009 whitelist read bug** (plugin-archi) — the `PluginArchiSharedAssemblyPrefixes` MSBuild prop reaches
   the compiler's editorconfig but isn't surfaced to the DiagnosticAnalyzer's `GlobalOptions` at real build time
   (unit tests pass). Leading fix: read options in the symbol action vs capturing from `CompilationStartAnalysisContext`.
   Re-testing needs a `v0.1.6` bump (NuGet pins the extracted version in the global cache).
3. **fantasim-world contract-only produce-boundary cleanup** — PLUG009 derived the leak inventory
   (`reference-rfc0014-shared-contract-alc` memory); slices not yet re-spec'd/executed. Decisive findings:
   producer/builder public ctors take concrete impl collaborators (validates interface/impl-split); sibling-sphere
   contracts (Biosphere/Noosphere/Mythosphere) need `[PluginSharedContract]` marking.

## 5. Feed-pollution caveat (low-harm, cleanup later)
The FIRST fantasim-world re-pack ran at the wrong version `0.1.0` (the env-override trap, §2.3) and synced ~71
inert `GiantCroissant.FantaSim.*.0.1.0` packages + stale 0.1.0 artifacts to the feed before I corrected to
0.2.93. They're superseded for all `0.2.*`/`0.1.*`-max consumers (harmless); not deleted (never-delete rule). If
a feed cleanup is wanted, list exact paths first and confirm.

## 6. Authoritative pointers
- Memory: `project-drop-plate-prefix-timedete` (full cross-repo state + commits + the naming rule),
  `reference-rfc0014-shared-contract-alc` (PLUG009 + boundary inventory + whitelist bug),
  `project-modern-satsuma-version-truth` (the 0.0.1-vs-1.0.0 version history).
- plugin-archi: `docs/rfcs/RFC-0014-...md`, `docs/unload-safety-analyzers.md` (PLUG009 row).
