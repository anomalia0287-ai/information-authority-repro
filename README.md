# Information Authority — Paper Reproduction Artifact

[![DOI](https://zenodo.org/badge/1249789748.svg)](https://doi.org/10.5281/zenodo.20405018)

Unity-free, deterministic reproduction bundle for the paper:
**"Information Authority: A Knowledge-Provenance Architecture for Non-Cheating Tactical AI, with Unity-Free Reproducible Verification."**

This repository contains the code subset, tracked scenario manifests, Dockerfile,
GitHub Actions workflow, and reference result tables needed to reproduce the paper's
no-cheat and reproducibility evidence **without Unity** (no Editor, Hub, licensing,
scenes, prefabs, or player builds).

## What this verifies

- A central output validator (`ValidateTickOutput`) rejects any decision derived from
  privileged ("god-view") simulation truth, at both the aggregate and per-decision level.
- An `authority_privileged_bait` scenario presents privileged enemy data that the honest
  decision path refuses (zero privileged-truth decisions; positive blocked-access count).
- A fail-closed negative control (`privileged_truth_violation`) deliberately consumes
  privileged truth and is caught and classified as `authority_failure`.
- Deterministic, trace-only execution across default / held-out / broader scenario
  families, each at >= 30 repetitions.

## Scope (what this does NOT claim)

This is a software-architecture / verification / reproducibility result only. It does
**not** claim human-realistic behavior, tactical superiority, real-world operational
validity, or medical / targeting / surveillance / weapon-control / learned-action
suitability.

## Reproduce

### Docker (recommended)

```
docker build -f paper/repro/Dockerfile -t info-authority-repro .
docker run --rm -v "${PWD}/paper/repro/out:/workspace/out" info-authority-repro
```

The container runs `Romana/Tools/Test-PaperReadyBundle.ps1` (30 repetitions) and writes
the regenerated bundle to `paper/repro/out/paper-ready-bundle`.

### Local (.NET SDK 10 + PowerShell)

```
pwsh -NoProfile -File ./Romana/Tools/Test-PaperReadyBundle.ps1 -ProjectPath ./Romana
```

### Continuous integration

`.github/workflows/paper-repro.yml` builds the image in a clean GitHub Actions Linux
container, runs the bundle, requires `passed=true`, and asserts the exact reported
numeric rows (the bait row, four modifier deltas, and the negative control) bit-for-bit.

## Layout

- `src/RomanAI.Core/`, `Romana/Assets/Scripts/RomanAI/Core/` — Unity-free simulation core (linked sources).
- `tools/RomanAI.Headless/` — headless experiment runner and tracked manifests.
- `tests/RomanAI.Core.Tests/` — core boundary tests.
- `paper/repro/Dockerfile`, `paper/repro/commands.md` — reproduction entrypoint and exact commands.
- `paper/repro/results/paper-ready-bundle/` — reference result tables and hashes from a passing run.
- `docs/` — research protocol and reproducibility method notes.

## Reference results

`paper/repro/results/paper-ready-bundle/paper_results_table.md` holds the reference
numbers that the CI run asserts against.

---

Developed with large-language-model assistance (Anthropic Claude, OpenAI Codex) under
the author's direction; the author is responsible for the architecture, implementation,
experiments, and all claims.
