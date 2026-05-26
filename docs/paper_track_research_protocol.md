# Paper Track Research Protocol

This protocol defines the first Unity-free paper track for Roman AI. It is intentionally scoped to the evidence the repository can reproduce today without Unity.

It does not claim human-realistic tactical behavior, real-world validity, combat superiority, medical prediction, targeting validity, or production approval for learned movement/combat policies.

## Candidate Paper Type

```text
Systems / reproducibility paper
```

The current defensible topic is not "better tactical AI." The defensible topic is a constrained simulation-core architecture with manifest-driven reproducibility gates, deterministic headless traces, fail-closed validation, and explicit safety boundaries.

## Research Question

```text
Can a Unity-free synthetic tactical human-behavior simulation core provide deterministic, auditable, non-mutating behavior traces with manifest-driven validation across baseline, contact uncertainty, behavior-state modifier, and squad-cohesion scenarios?
```

## Narrow Claims To Test

The first paper track can test these claims:

```text
The core simulation contract builds and runs without Unity.
Tracked validation manifests drive scenario identity, order, tick counts, and invariant expectations.
Headless runs are deterministic for the default smoke window and long replay profile.
Core outputs remain trace-only and non-mutating in covered scenarios.
Information-authority gating blocks PrivilegedTruth enemy-position records from driving decisions in the Unity-free runner.
Behavior-state modifiers produce expected synthetic state-direction changes in the covered performance trace.
Multi-agent squad traces preserve decision cardinality and behavior-state stability when no modifier is applied.
Corrupted baseline evidence fails closed through negative fixture tests.
```

The first paper track must not claim:

```text
general tactical superiority
human-realistic behavioral validity
real-world operational validity
medical, impairment, dosing, or treatment prediction
target selection, weapon control, or live-fire suitability
learned movement/combat policy approval
```

## Existing Unity-Free Scenario Set

The current manifest-backed scenario set is:

| Scenario | Role In Protocol | Current Purpose |
| --- | --- | --- |
| `baseline_trace_hold` | Control | Stable trace-only baseline with no observations and no behavior drift. |
| `contact_uncertainty_trace` | Perception condition | One contact observation per tick with no world mutation. |
| `performance_state_trace` | Behavior-state modifier condition | Synthetic modifier trace that reduces fatigue and raises confidence. |
| `squad_cohesion_trace` | Multi-agent condition | Four-agent deterministic trace with one trace-only decision per live agent per tick. |

The current replay profile is:

| Profile | Role In Protocol | Current Purpose |
| --- | --- | --- |
| `long_replay` | Determinism stress window | Expands the default validation scenarios beyond the smoke tick count. |

The information-authority audit adds a separate Unity-free paper-bundle condition without changing the default validation manifest:

| Scenario | Role In Protocol | Current Purpose |
| --- | --- | --- |
| `authority_privileged_bait` | No-cheat audit condition | Presents a PrivilegedTruth-only enemy-position record and verifies the decision path blocks it while counting the blocked access attempt. |

## Existing Metrics

The current gate already produces these paper-relevant metrics:

```text
scenario_count
scenario_ids_match_manifest
all_tick_validations_ok
no_world_mutation
trace_only_decisions_cover_outputs
deterministic_replay
long_replay_profile_deterministic
privileged_truth_decisions
blocked_privileged_access_attempts
no_cheat_invariant_evidence
baseline_behavior_stable
contact_observations_each_tick
contact_no_mutation
performance_fatigue_decreases
performance_confidence_increases
squad_agent_count
squad_decisions_each_tick
squad_behavior_stable
```

The current baseline values are:

```text
setId = roman_core_validation_scenarios
setVersion = 0.3.2
scenarios = 4
invariants = 21/21 passed
coverage rows = 14/14 passed
trace-only decisions = 18
mutations = 0
invalid tick inputs = 0
invalid tick outputs = 0
authority no-cheat paper condition = separate bundle experiment
authority no-cheat expectation = privileged_truth_decisions 0 and blocked_privileged_access_attempts > 0
```

## Baseline And Ablation Plan

The paper is not ready until these comparisons exist as repeatable Unity-free runs:

| Comparison | Minimum Implementation | Metric |
| --- | --- | --- |
| Rule trace baseline | Current `baseline_trace_hold` plus explicit baseline table | Determinism, zero mutation, stable state |
| Contact uncertainty ablation | Contact observations on/off across matched seeds | Observation coverage, mutation count, trace determinism |
| Synthetic modifier ablation | Modifier on/off across matched seeds and ticks | Fatigue delta, confidence delta, invariant pass rate |
| Squad-size ablation | 1, 2, 4, and larger squad traces | Decision cardinality, validation pass rate, replay determinism |
| Information-authority bait | PrivilegedTruth-only enemy-position record | privileged_truth_decisions = 0; blocked_privileged_access_attempts > 0 |
| Manifest drift negative control | Corrupted scenario order/version/count fixtures | Expected failure rate for invalid evidence |

Current repository status:

```text
The rule trace baseline, contact uncertainty condition, synthetic modifier condition, four-agent squad condition, long replay determinism, and negative baseline fixture tests exist.
The first Unity-free paper experiment runner exists under RomanAI.Headless.
It runs contact on/off, synthetic modifier on/off, and squad-size 1/2/4/8 matched-seed ablations.
It defaults to at least 30 repetitions and reports pass rates, determinism match rates, safety counters, means, standard deviations, and 95% confidence intervals.
The first held-out paper manifest exists at tools/RomanAI.Headless/paper_heldout_manifest.json.
The first broader unseen paper manifest exists at tools/RomanAI.Headless/paper_broader_manifest.json.
The runner can execute held-out paper conditions with runner-level fail-closed negative controls and failure taxonomy output.
The runner can generate a paper-ready evidence bundle with a result table, artifact bundle manifest, limitations section, and readiness report.
The paper-ready bundle includes a separate `authority_no_cheat_experiment` whose honest bait condition reports zero privileged-truth decisions and positive blocked privileged-access attempts.
The runner-level negative controls include `privileged_truth_violation`, which fails closed into `authority_failure`.
`Romana/Tools/Test-PaperReadyBundle.ps1` can regenerate and verify the Unity-free paper bundle locally.
The first systems/reproducibility paper draft exists at `docs/paper_track_systems_reproducibility_draft.md`.
Independent clean-machine reproduction and external narrative review are not complete yet.
```

## Train/Validation/Test Split Policy

The first paper track should avoid machine-learning performance claims until a proper split exists.

For Unity-free simulation-core claims, use this split policy:

```text
train = not applicable for contract-only validation
validation = current manifest scenarios and negative fixtures
test = newly added unseen scenario manifests that are not used to tune thresholds
```

If a learned model or imitation artifact is included later, the split must become:

```text
train = seeds/scenarios/maps used to fit model parameters
validation = seeds/scenarios/maps used to select thresholds or policies
test = held-out seeds/scenarios/maps reported once for paper results
```

Do not reuse the current validation manifest as the only paper test set after tuning thresholds against it.

## Statistical Repetition Plan

Paper completion requires more than one passing gate run. The first Unity-free experiment runner can write ignored batch outputs for:

```text
N >= 30 seed repetitions per scenario family
matched seed comparisons for each ablation
mean, standard deviation, and 95% confidence interval for scalar metrics
pass/fail counts for contract and safety invariants
determinism replay hash match rate
```

Implemented by the first runner:

```text
runner-level negative-control fail-closed rate
held-out scenario manifests
failure taxonomy populated from actual failed and passing runs
privileged-truth decision and blocked-access authority counters
```

Still required before paper completion:

```text
independent clean-machine reproduction log
final paper narrative interpretation
external review against the claim boundary
```

Treat the current runner and generated bundle as systems/reproducibility evidence infrastructure, not as complete paper evidence.

## Failure Taxonomy

Classify failures into these buckets:

```text
environment_failure = SDK, PowerShell, path, or local permission issue
build_failure = RomanAI.slnx does not build
contract_failure = tick input/output validation fails
determinism_failure = replay output differs for matched inputs
safety_failure = mutation count is nonzero or trace-only boundary is violated
authority_failure = privileged-truth data drives a decision or the authority no-cheat control fails
coverage_failure = scenario, invariant, or coverage row is missing
state_semantics_failure = expected behavior-state direction or stability fails
manifest_failure = manifest schema, order, version, or unsupported id fails
negative_control_failure = corrupted evidence does not fail closed
overclaim_failure = paper wording exceeds the evidence matrix
```

## Reproduction Commands

Run the current Unity-free gate:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Romana\Tools\Test-CoreSimulationGate.ps1 -ProjectPath .\Romana
```

Run the Unity-free paper bundle verification:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Romana\Tools\Test-PaperReadyBundle.ps1 -ProjectPath .\Romana
```

Run the headless batch manually:

```powershell
dotnet build RomanAI.slnx --configfile NuGet.Config -m:1 -v:minimal
dotnet run --project tests\RomanAI.Core.Tests\RomanAI.Core.Tests.csproj --configfile NuGet.Config --no-build
dotnet run --project tools\RomanAI.Headless\RomanAI.Headless.csproj --configfile NuGet.Config --no-build -- --ticks 2 --validation-manifest tools\RomanAI.Headless\validation_scenarios_manifest.json --out .headless-runs\core-simulation-gate
```

Run the first paper experiment:

```powershell
dotnet run --project tools\RomanAI.Headless\RomanAI.Headless.csproj --configfile NuGet.Config --no-build -- --paper-experiment --paper-repetitions 30 --out .headless-runs\paper-experiment-smoke
```

Run the held-out paper experiment with runner-level fail-closed negative controls:

```powershell
dotnet run --project tools\RomanAI.Headless\RomanAI.Headless.csproj --configfile NuGet.Config --no-build -- --paper-experiment --paper-repetitions 30 --paper-heldout-manifest tools\RomanAI.Headless\paper_heldout_manifest.json --paper-negative-controls --out .headless-runs\paper-heldout-negative-smoke
```

Generate the paper result table, artifact bundle manifest, limitations section, and readiness report:

```powershell
dotnet run --project tools\RomanAI.Headless\RomanAI.Headless.csproj --configfile NuGet.Config --no-build -- --paper-ready-bundle --paper-repetitions 30 --out .headless-runs\paper-ready-bundle
```

Expected paper experiment outputs:

```text
paper_experiment_summary.csv
paper_experiment_runs.csv
paper_experiment_negative_controls.csv
paper_experiment_failure_taxonomy.csv
paper_experiment_result.json
```

Expected paper-ready bundle outputs:

```text
paper_results_table.md
paper_limitations.md
paper_readiness_report.csv
paper_readiness_report.json
paper_artifact_bundle_manifest.json
paper_bundle_verification_report.json
default_experiment/*
heldout_negative_experiment/*
broader_unseen_experiment/*
authority_no_cheat_experiment/*
```

Generated `.headless-runs` output remains local and untracked.

## Paper Completion Gates

Treat the paper track as incomplete until all of these are true:

```text
research question is fixed
baseline and ablation matrix is implemented
held-out scenario/seed set exists
statistical repetition runner exists
confidence intervals are reported
failure taxonomy is populated from actual failed and passing runs
information-authority no-cheat evidence exists with a caught privileged-truth violation control
artifact bundle runs without Unity
ethics and limitations section matches the product contract
all claims stay inside the evidence matrix
broader unseen scenario families are added
independent clean-machine reproduction log exists
paper narrative draft exists
final paper narrative is externally reviewed against the evidence matrix
```

Current status against those gates:

```text
research question = present
baseline and ablation matrix = initial implementation present
held-out scenario/seed set = initial manifest present
statistical repetition runner = present
confidence intervals = reported by runner
failure taxonomy = initial automatic output present
information-authority no-cheat evidence = implemented in Unity-free bundle
artifact bundle = initial generated bundle present
ethics and limitations section = initial generated limitations present
paper-ready results table = initial generated table present
broader unseen scenario families = initial implementation present
clean-machine reproduction script = present
independent clean-machine reproduction = not complete
paper-ready narrative draft = present
external narrative review = not complete
```
