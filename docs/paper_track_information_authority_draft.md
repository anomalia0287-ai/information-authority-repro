# Information Authority: A Knowledge-Provenance Architecture for Non-Cheating Tactical AI, with Unity-Free Reproducible Verification

**Jaesung Kim**
*Independent Researcher, Busan, South Korea · Anomalia0287@gmail.com*

Submission metadata (editorial; not part of the paper body): recommended arXiv primary category **cs.AI**, cross-list **cs.MA** and **cs.SE**; recommended license **CC BY 4.0**. Romanization "Jaesung Kim" matches the author's passport; register or confirm an ORCID iD under this spelling for citation consistency. Affiliation shown as "Independent Researcher"; adjust if an institutional affiliation applies.

Status: B+ submission-style draft, code-grounded concreteness pass. This restructures the prior systems/reproducibility-only framing (`docs/paper_track_systems_reproducibility_draft.md`) by promoting the information-authority architecture to the lead contribution and folding the reproducibility evidence into a verification role. This document is an internal draft: not externally reviewed, not accepted, and not independent-machine reproduced. It is the new canonical paper draft; the Related Work here supersedes the working note in `docs/paper_track_related_work.md`. Schemas and pseudocode below are grounded in `RomanSimulationCoreApi.cs`, `RomanInformationAuthorityContract.cs`, and `tools/RomanAI.Headless/*`; result tables are the generated `paper_results_table.md`.

Citation style: IEEE numeric. Reference metadata for [1]–[7] is cross-verified against publisher records (see "Citation status"); the only residual pre-submission step is confirming each source's specific attributed claim.

## Abstract

Tactical AI in games and training simulations frequently relies on privileged access to complete world state — informally, "cheating" — which undermines believability and the training validity of simulated forces. This paper presents two contributions. First, an *information-authority* architecture that assigns every AI-facing datum a knowledge-provenance level and structurally blocks privileged-truth data from driving runtime decisions, so that non-cheating is enforced by fail-closed defaults and a central runtime validator rather than left to per-agent discipline. Second, a Unity-free, reproducible verification that the no-cheat invariant holds for the covered scenarios: a central output validator rejects any decision derived from privileged truth at both the aggregate and per-decision level, an auditable "bait" scenario presents privileged enemy data that the honest decision path declines (zero privileged-truth decisions while recording the blocked accesses), and a fail-closed negative control deliberately consumes privileged truth and is caught and classified. The verification reuses a deterministic, trace-only headless evidence path with manifest-driven scenario families, repeated runs with confidence intervals, runner-level negative controls, artifact-hash checking, and same-machine clean-clone reproduction. These results support only the constrained claims that (a) the architecture rejects privileged-truth decisions and (b) this rejection is reproducibly auditable without Unity. They do not support human-realistic behavior, tactical superiority, real-world operational validity, medical/targeting/surveillance/weapon-control validity, or approval of learned live action.

## Keywords

Knowledge provenance, information authority, non-cheating AI, partial observability, computer-generated forces, reproducibility, deterministic replay, trace-only execution, fail-closed verification.

## 1. Introduction

Interactive tactical AI is commonly built on a shortcut: the agent reads the full simulation state, including the positions of opponents it has never perceived. This is cheap and can make the AI appear strong, but it has two costs. For entertainment, cheating breaks believability when players detect it. For training simulation — where computer-generated forces (CGF) stand in for human adversaries and teammates — an agent that acts on information a real soldier could never possess teaches the wrong lessons, because real opponents do not have access to a trainee's location.

The defensible response is not merely "write an agent that happens not to cheat," but to make non-cheating a *structural, auditable property* of the architecture. This paper describes the information-authority architecture used in the Roman AI core, in which every AI-facing datum is typed by how it was obtained and a central validator rejects any decision derived from privileged simulation truth. We then show that this property is *reproducibly verifiable* through a Unity-free evidence path, including a negative control that proves the audit detects a deliberate violation.

The contribution is intentionally narrow. We do not claim the resulting behavior is human-realistic, tactically superior, or operationally valid; we claim that the architecture blocks privileged-truth use and that this blocking is reproducibly demonstrable for the covered synthetic scenarios.

## 2. Related Work

Computer-generated forces (CGF) have long populated military training simulations with friendly and opposing entities, and the credible representation of human behavior has been a central, long-standing challenge [1]. Traditional CGF behavior is authored from doctrine, rules of engagement, and subject-matter-expert heuristics; more recent work surveys data-driven and machine-learning approaches [8]. Across both, behavioral fidelity — usefulness and credibility for training — is a recurring concern.

A closely related issue arises in game AI, where agents frequently obtain privileged or global access to world state ("cheating"). Surveys of real-time strategy (RTS) game AI note that opponents often receive perfect information and that such cheating harms the player experience when detected [2]. Hagelbäck and Johansson studied this directly, showing that a bot operating under fog of war can be both believable and competitive relative to a perfect-information variant [3], [4]. This establishes that non-cheating tactical AI is feasible, but the property is realized per-implementation rather than guaranteed by an explicit, enforced contract.

Acting under incomplete information is formalized by partially observable Markov decision processes, which model choosing actions from belief states rather than from privileged ground truth [5]. This motivates a clean separation between what an agent could legitimately know and the simulator's complete state — a separation the present architecture makes explicit and type-enforced.

The substrate for tactical decisions is often expressed through reactive structures such as behavior trees [6]. Separately, the reproducibility of reported results has been raised forcefully in the deep reinforcement learning literature, which highlights non-determinism, variance, and the need for statistically grounded reporting [7]. The verification here adopts that reproducibility posture for a *contract property* rather than a performance claim.

**Gap.** Prior work shows tactical agents can respect fog-of-war constraints and that partial-observability decision-making is well founded, but the non-cheating property is typically emergent in a particular agent rather than a structurally enforced, machine-auditable guarantee. This paper contributes (i) an information-authority architecture that types every datum by provenance and blocks privileged truth from driving decisions, and (ii) a Unity-free, reproducible verification — including a fail-closed negative control — that the no-cheat invariant holds for covered scenarios.

Provenance and accountability have also been addressed at the model and dataset level through structured documentation such as model cards [9]; the present work differs by applying provenance *typing* as an enforced runtime gate at the tactical decision boundary, rather than as post-hoc documentation. Table I summarizes the positioning relative to each related area.

**Table I. Positioning relative to related fields.**

| Related area | Prior work | This paper's difference |
| --- | --- | --- |
| Fog-of-war / non-cheating game AI [2]–[4] | non-cheating feasible per implementation | enforced via an authority/provenance contract, not per-agent discipline |
| POMDP / partial observability [5] | optimal action from belief states | verifies that privileged truth is *not used*, not optimality |
| CGF / military-sim behavior [1], [8] | human-behavior fidelity | information legitimacy / anti-cheat, not behavioral validity |
| Provenance-aware AI / transparency [9] | general model/dataset documentation | applied as an enforced gate at the tactical decision boundary |
| Reproducibility [7] | performance benchmarking | reproduces the no-cheat invariant, not performance |

## 3. Information Authority Architecture (Contribution 1)

### 3.1 Authority levels

Every AI-facing datum is assigned one information-authority level (`RomanInformationAuthorityLevel`):

| Level | Name | Meaning | Decision use |
|---|---|---|---|
| 0 | `PrivilegedTruth` | Complete simulation truth (debug/scoring/offline only) | Never drives a runtime decision |
| 1 | `StaticTerrainTruth` | Pre-briefed terrain, cover, routes, objectives | Movement and planning |
| 2 | `PerceivedTruth` | Directly seen/heard by the agent's own sensors | Allowed; highest confidence |
| 3 | `ReportedTruth` | Ally report via blackboard/radio/proximity | Allowed, with source/delay/confidence |
| 4 | `InferredTruth` | Guess from last-known position, occluder, sound, or objective logic | Allowed as belief, not exact target |
| 5 | `StaleTruth` | Previously valid data whose confidence has decayed | Low-priority search/caution only |
| 6 | `Unknown` | No valid knowledge | Not a target; patrol/guard/terrain behavior |

### 3.2 Knowledge records

Enemy knowledge enters the core only through observation records. Each observed unit carries its provenance explicitly (`RomanObservedUnit`):

```
RomanObservedUnit {
  unitId            // who
  team              // default OPFOR
  position          // believed position
  confidence01      // [0,1]
  lastSeenSeconds   // recency
  hasLineOfSight
  authorityLevel    // default PerceivedTruth
  knowledgeSource   // default Sight
  privilegedOnly    // debug/god-view marker
}
```

Observed units are grouped under an observation made by a specific live observer (`RomanWorldObservation`: `observationId`, `observerUnitId`, `observedAtSeconds`, `visibleUnits[]`, `lastKnownContactPosition`, `uncertainty01`, `source`). The knowledge source is one of `{None, StaticTerrain, Sight, Sound, Damage, AllyReport, BehindCoverInference, ObjectiveInference, DebugPrivileged}`; `DebugPrivileged` marks god-view data that must never reach a decision.

### 3.3 Decision records and predicates

A decision (`RomanAgentDecision`) records not only the chosen intent (`{Hold, Move, TakeCover, Observe, Search, Regroup, Retreat, SupportAlly, ReportContact}`) but the provenance it relied on: `authorityLevel` (default `Unknown`), `knowledgeSource` (default `None`), and a `privilegedTruthUsedForDecision` flag (default `false`). Defaulting to `Unknown`/`None` is deliberate: a decision that does not declare legitimate provenance is treated as non-privileged-unusable and is rejected fail-closed, never silently admitted.

Two predicates encode the policy:

```
CanDriveRuntimeDecision(level)   = level in {StaticTerrainTruth, PerceivedTruth, ReportedTruth, InferredTruth, StaleTruth}   // levels 1–5
CanDriveEnemyTargeting(level)    = level in {PerceivedTruth, ReportedTruth, InferredTruth, StaleTruth}                        // levels 2–5 (terrain may guide movement, not targeting)
IsPrivilegedTruth(level, source, privilegedOnly) = privilegedOnly OR level == PrivilegedTruth OR source == DebugPrivileged
```

Provenance-aware overloads compose these: a datum may drive enemy targeting only if `CanDriveEnemyTargeting(level)` holds *and* `IsPrivilegedTruth(...)` does not.

### 3.4 Transitions and a worked example

Information moves between levels only through explicit, in-world transitions; privileged truth cannot be laundered into a usable belief without genuine perception, report, or inference. A representative datum lifecycle:

1. An agent sees an opponent → `RomanObservedUnit{ authorityLevel = PerceivedTruth, knowledgeSource = Sight, confidence01 ≈ 0.9 }`. `CanDriveEnemyTargeting` holds; the agent may engage or move on it.
2. The opponent breaks line of sight behind cover → the system may retain a guess as `InferredTruth` (`knowledgeSource = BehindCoverInference`), usable only as a belief, not as an exact target.
3. Confidence decays below threshold → `StaleTruth`, usable only for low-priority search.
4. The record expires or is contradicted → `Unknown`; the agent reverts to patrol/guard/terrain behavior and may not treat it as a target.

By contrast, a god-view datum injected as `{ authorityLevel = PrivilegedTruth, knowledgeSource = DebugPrivileged, privilegedOnly = true }` fails `IsPrivilegedTruth` and is rejected by both predicates: it can never drive a runtime or targeting decision. The invariant the architecture preserves is that no decision is derived from privileged truth, recorded per decision as `privileged_truth_used_for_decision = false`.

### 3.5 Data flow

Figure 1 summarizes how a tick is processed and where the no-cheat invariant is enforced. (A vector version for typesetting is at `docs/figures/fig1_dataflow.svg`; the ASCII rendering below is for inline readability.)

```
Figure 1. Per-tick data flow and the two no-cheat gates.

  Tick input
   ├─ agents[]         unit state + 12-dim behavior state
   ├─ observations[]   each visibleUnit tagged {authorityLevel, knowledgeSource, privilegedOnly}
   └─ orders[]
         │
         ▼
  Knowledge records  ──►  Decision (intent + declared provenance;
  (provenance-typed)       authorityLevel default Unknown, knowledgeSource default None)
         │
         ▼
  ValidateTickOutput
   ├─ Gate A (aggregate)   : output.privilegedTruthDecisions == 0
   │                          else fail "output_privileged_truth_decision"
   ├─ Gate B (per-decision): not IsPrivilegedTruth(level, source, privilegedOnly)
   │                          and not privilegedTruthUsedForDecision
   │                          else fail "decision_privileged_truth_used"
   └─ safety/contract      : trace-only (no world mutation), no prohibited tokens,
                              unit live, no duplicate decisions
         │
         ▼
  Trace-only output   decisions[], privilegedTruthDecisions = 0,
                      blockedPrivilegedAccessAttempts = N
```

## 4. Enforcement and No-Cheat Verification (Contribution 2)

### 4.1 Central enforcement

Enforcement is centralized in the core output validator (`ValidateTickOutput`), not scattered per scenario. Two gates implement the no-cheat invariant — one aggregate, one per decision — alongside the trace-only and prohibited-action safety checks:

```
ValidateTickOutput(output, input):
  ... // api/tick/scenario/trace-id consistency
  if output.mutatedWorld and execution is TraceOnly: fail "trace_only_output_mutated_world"
  if output.privilegedTruthDecisions < 0 or output.blockedPrivilegedAccessAttempts < 0: fail "output_authority_counter_negative"
  if output.privilegedTruthDecisions > 0: fail "output_privileged_truth_decision"        // aggregate no-cheat gate
  for each decision:
    ... // unit exists & alive, intent present, confidence in [0,1], safety boundary matches
    if decision.privilegedTruthUsedForDecision
       or IsPrivilegedTruth(decision.authorityLevel, decision.knowledgeSource, decision.privilegedTruthUsedForDecision):
         fail "decision_privileged_truth_used"                                            // per-decision no-cheat gate
    if execution is TraceOnly and decision.adapterMayApply: fail "trace_only_decision_adapter_may_apply"
    if contains prohibited token (weapon, fire, kill, target_selection, live_fire, treatment, ...): fail "decision_contains_prohibited_token"
  return ok
```

A tick output therefore cannot pass validation if any decision used privileged truth, whether reported in aggregate or carried on an individual decision. Combined with the `Unknown`/`None` defaults of §3.3, the property is fail-closed.

Stated precisely, the target invariant is: for every tick *t* and every decision *d* in the output, if *d* passes `ValidateTickOutput`, then `IsPrivilegedTruth(d.authorityLevel, d.knowledgeSource, d.privilegedTruthUsedForDecision) = false`. We do not prove this invariant formally. It is *enforced* by the central validator together with the fail-closed `Unknown`/`None` decision defaults, and *verified* empirically by the bait scenario, the negative control, and the reproducible bundle. The precise status is therefore "enforced and verified," not "formally proven."

### 4.2 Bait scenario (`authority_privileged_bait`)

The headless runner injects, each tick, an observation with `source = "privileged_truth_bait"` containing an opponent marked `{ authorityLevel = PrivilegedTruth, knowledgeSource = DebugPrivileged, privilegedOnly = true }`, while the agent has no legitimate (perceived/reported/inferred) knowledge of it. The honest decision path classifies each such datum with `IsBlockedPrivilegedEnemyKnowledge` — true when the unit is an opponent, `CanDriveEnemyTargeting(...)` is false, and `IsPrivilegedTruth(...)` is true — increments `blocked_privileged_access_attempts`, and emits a non-targeting hold. A correct run reports `privileged_truth_decisions = 0` with `blocked_privileged_access_attempts > 0`: the latter proves the scenario is non-vacuous (privileged data was present and was refused), not that the agent simply had nothing to act on.

### 4.3 Negative control (`privileged_truth_violation`)

To show the audit actually fires, a control execution deliberately consumes the privileged datum: it locates the privileged opponent and emits a decision toward it with `privilegedTruthUsedForDecision = true`. The aggregate and per-decision gates of §4.1 reject it, and the runner classifies the failure into a distinct `authority_failure` bucket. The control passes only if the violation is observed and correctly classified. This pairing is what distinguishes the result from a tautology: the honest path shows refusal of *available* privileged data, and the control shows the validator fires when a violation is injected. Both conditions are defined in the manifest `unity_free_information_authority_no_cheat`.

## 5. Unity-Free Reproducibility (Supporting)

The verification runs through a Unity-free headless path (`RomanAI.Headless`) that builds and executes the core without Unity Editor, Hub, licensing, Package Manager, scenes, prefabs, screenshots, or player builds. Runs are deterministic and trace-only (non-mutating): a passing run preserves deterministic replay, zero mutation count, zero invalid tick validations, zero decision-cardinality errors, and full trace-only coverage. Determinism is enforced at input validation — each tick's `deterministicSeed` must equal `randomSeed XOR tickIndex` — and the execution mode is `TraceOnly` under a fixed safety policy (`synthetic_trace_only_no_direct_world_mutation`).

The evidence bundle uses repository-tracked manifests and deterministic seed windows, with at least 30 repetitions per condition and reported mean, standard deviation, and 95% confidence interval for scalar metrics. It covers a default ablation set (contact on/off, synthetic modifier on/off, squad sizes 1/2/4/8), a held-out manifest (distinct identifiers, tick windows, and squad sizes), and a broader unseen manifest (combined contact/modifier, long-horizon modifier, dense squad contact). Runner-level negative controls inject failures for mutation, trace-only, tick-validation, decision-cardinality, determinism, and authority buckets, each required to fail closed into its expected taxonomy bucket. The bundle records result tables, generated limitations, a readiness report, command provenance, and SHA-256 artifact-manifest checks, and has been reproduced from a same-machine clean local clone.

## 6. Results

### 6.1 No-cheat evidence

The information-authority bait condition passes with zero privileged-truth decisions and a positive blocked-access count, confirming the scenario presented and refused privileged data:

| Manifest | Condition | N | Pass | Determinism | Privileged Decisions | Blocked Privileged Attempts | Trace-Only | Passed |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| unity_free_information_authority_no_cheat | authority_privileged_bait | 30 | 1.000000 | 1.000000 | 0.000000 | 4.000000 | 1.000000 | true |

The runner-level negative controls all fail closed into their expected buckets, including the authority control:

| Control | Bucket | Observed Failed Rows | Fail-Closed Rate | Passed |
| --- | --- | ---: | ---: | --- |
| mutation_violation | safety_failure | 1 | 1.000000 | true |
| trace_only_violation | safety_failure | 1 | 1.000000 | true |
| invalid_tick_validation | contract_failure | 1 | 1.000000 | true |
| decision_cardinality_mismatch | contract_failure | 1 | 1.000000 | true |
| determinism_mismatch | determinism_failure | 1 | 1.000000 | true |
| privileged_truth_violation | authority_failure | 1 | 1.000000 | true |

The failure taxonomy separates normal runs (which must produce zero failures) from negative-control runs (which must produce the expected failures):

| Bucket | normal_experiment | negative_control |
| --- | ---: | ---: |
| contract_failure | 0 | 2 |
| determinism_failure | 0 | 1 |
| authority_failure | 0 | 1 |
| safety_failure | 0 | 2 |
| state_semantics_failure | 0 | 0 |

### 6.2 Supporting reproducibility evidence

Across all 21 normal conditions (8 default + 7 held-out + 6 broader unseen), each at 30 repetitions, every condition reports `pass_rate = determinism_match_rate = trace_only_decision_rate = 1.000000` and `mutations = invalid_tick_validations = decision_cardinality_errors = privileged_truth_decisions = 0`.

The core represents each agent's affective/behavioral state as a bounded twelve-dimension vector (fear, anger, morale, suppression, pain, panic, fatigue, confidence, self-preservation, aggression, cohesion, hesitation); the synthetic modifier applies a fixed bounded delta to this vector. Only the two surfaced dimensions (fatigue, confidence) under the modifier conditions produce non-zero quantities here, and they are reported solely as determinism and bounded-range evidence. The affect model and the decision weighting it feeds (e.g., personality-conditioned order compliance) are a separate contribution, outside this paper's scope. The non-zero deltas are:

| Condition | Fatigue Δ (mean ± CI95) | Confidence Δ (mean ± CI95) |
| --- | ---: | ---: |
| modifier_on | -0.192735 ± 0.000000 | 0.077094 ± 0.000000 |
| heldout_modifier_on | -0.244582 ± 0.000000 | 0.097833 ± 0.000000 |
| broader_contact_modifier_on | -0.263691 ± 0.000000 | 0.105476 ± 0.000000 |
| broader_long_modifier_on | -0.311219 ± 0.000000 | 0.124488 ± 0.000000 |

The zero-width confidence intervals reflect deterministic replay under matched inputs, not population-level uncertainty. The deltas are synthetic behavior-state outputs, not medical, pharmacological, impairment, dosing, or clinical evidence.

The information-authority enforcement, bait scenario, counters, negative control, and verifier integration are recorded in commit `2c50724`.

## 7. Discussion

The strongest supported interpretation is architectural: the core separates legitimately knowable information from privileged simulation truth, rejects privileged-truth decisions at a central validation boundary (both in aggregate and per decision), and demonstrates — with a fail-closed negative control — that the rejection is enforced and reproducibly auditable without Unity. Relative to prior fog-of-war work [3], [4], the contribution is not that a particular agent avoids cheating, but that cheating is rejected by a central validation gate and fail-closed defaults, and that this rejection is machine-checkable and reproducible.

The pass rates and determinism figures should not be read as a behavioral benchmark. A pass means the covered contract and authority checks held under the current manifests and deterministic runner; it does not mean the behavior is human-like, tactically superior, or transferable to real settings.

## 8. Limitations

- The scenario families are synthetic contract tests, not validated human-behavior experiments.
- The bait scenario is a hold-and-count demonstration: it shows privileged data is present and refused and that the validator fires on violation; it does not exercise every gameplay decision path in a full game.
- Determinism is matched-input replay agreement inside the runner, not real-world behavioral validity.
- Reproduction is same-machine clean-clone; independent-machine reproduction is not complete.
- Internal review only; no external peer review or venue acceptance.
- The verification concerns the rule-based decision path; the learned model remains shadow-only at `hold_noop_only` and is not approved for live action.

## 9. Ethics and Product Boundary

Roman AI is positioned as a synthetic human-behavior simulation core for synthetic scenarios, education, research, after-action review, game-AI middleware, and non-clinical training. It must not be framed or adapted as a lethal decision system, real-world target selection, weapon-system control, operational order generation, live-fire control, surveillance targeting, clinical diagnosis, treatment, dosing, drug-use recommendation, or medical prescription support. This paper reports only architecture-enforcement and reproducibility evidence and makes no operational, tactical-superiority, medical, targeting, surveillance, weapon-control, or learned-action-approval claim.

## 10. Conclusion

We presented an information-authority architecture that types AI-facing data by knowledge provenance and structurally blocks privileged-truth data from driving tactical decisions, and a Unity-free, reproducible verification — central validator, auditable bait scenario, and fail-closed negative control — that the no-cheat invariant holds for the covered synthetic scenarios. The evidence supports only that the architecture rejects privileged-truth decisions and that this rejection is reproducibly auditable. It is not externally reviewed, not independent-machine reproduced, and makes no behavioral-quality or operational claim. Future work includes independent-machine reproduction, broadening the exercised decision paths, and qualitative worked-scenario visualization of authority-conditioned behavior.

## Artifact Availability

The evidence is reproducible from the repository at commit `2c50724` (Windows PowerShell, .NET SDK 10):

- Verify bundle: `powershell -NoProfile -ExecutionPolicy Bypass -File .\Romana\Tools\Test-PaperReadyBundle.ps1 -ProjectPath .\Romana`
- Generate bundle: `dotnet run --project tools\RomanAI.Headless\RomanAI.Headless.csproj --configfile NuGet.Config --no-build -- --paper-ready-bundle --paper-repetitions 30 --out .headless-runs\paper-ready-bundle`

The run writes the result tables, per-condition CSV/JSON, generated limitations, a readiness report, SHA-256 artifact-manifest checks, and the information-authority no-cheat experiment under `.headless-runs/` (untracked, regenerable). The Unity-free core builds from `src/RomanAI.Core` (which links the core sources, including `RomanInformationAuthorityContract.cs` and `RomanSimulationCoreApi.cs`); the headless runner is under `tools/RomanAI.Headless/`. A public artifact release — a code subset, the manifests, generated result tables, hash files, and exact commands — will accompany submission.

## Acknowledgments

This work was developed in collaboration with large language models (Anthropic Claude and OpenAI Codex) under the author's direction. The author conceived the architecture and made all design decisions; the AI tools served as code-generation and writing assistants. All factual claims and citations were verified by the author. (Confirm exact model versions before submission.)

## References

[1] R. W. Pew and A. S. Mavor, Eds., *Modeling Human and Organizational Behavior: Application to Military Simulations*. Washington, DC, USA: National Academy Press, 1998.

[2] S. Ontañón, G. Synnaeve, A. Uriarte, F. Richoux, D. Churchill, and M. Preuss, "A survey of real-time strategy game AI research and competition in StarCraft," *IEEE Trans. Comput. Intell. AI Games*, vol. 5, no. 4, pp. 293–311, Dec. 2013, doi: 10.1109/TCIAIG.2013.2286295.

[3] J. Hagelbäck and S. J. Johansson, "Dealing with fog of war in a real-time strategy game environment," in *Proc. IEEE Symp. Computational Intelligence and Games (CIG)*, Perth, Australia, 2008, pp. 55–62, doi: 10.1109/CIG.2008.5035621.

[4] J. Hagelbäck and S. J. Johansson, "A multiagent potential field-based bot for real-time strategy games," *Int. J. Computer Games Technology*, vol. 2009, Art. no. 910819, 2009, doi: 10.1155/2009/910819.

[5] L. P. Kaelbling, M. L. Littman, and A. R. Cassandra, "Planning and acting in partially observable stochastic domains," *Artificial Intelligence*, vol. 101, no. 1–2, pp. 99–134, 1998, doi: 10.1016/S0004-3702(98)00023-X.

[6] M. Colledanchise and P. Ögren, *Behavior Trees in Robotics and AI: An Introduction*. Boca Raton, FL, USA: CRC Press, 2018, doi: 10.1201/9780429489105. (arXiv:1709.00084)

[7] P. Henderson, R. Islam, P. Bachman, J. Pineau, D. Precup, and D. Meger, "Deep reinforcement learning that matters," in *Proc. 32nd AAAI Conf. Artificial Intelligence (AAAI)*, vol. 32, no. 1, 2018, doi: 10.1609/aaai.v32i1.11694. (arXiv:1709.06560)

[8] R. A. Løvlid, L. J. Luotsinen, F. Kamrani, and B. Toghiani-Rizi, "Data-driven behavior modeling for computer generated forces: a literature survey," Norwegian Defence Research Establishment (FFI), Kjeller, Norway, Rep. 17/01510, 2017.

[9] M. Mitchell, S. Wu, A. Zaldivar, P. Barnes, L. Vasserman, B. Hutchinson, E. Spitzer, I. D. Raji, and T. Gebru, "Model cards for model reporting," in *Proc. Conf. Fairness, Accountability, and Transparency (FAT\*)*, 2019, doi: 10.1145/3287560.3287596. (arXiv:1810.03993)

## Citation status

- Bibliographic metadata (authors, title, venue, vol./no./pp., DOI) for [1]–[9] has been cross-verified against publisher records (IEEE Xplore, Wiley, ScienceDirect, AAAI OJS, ACM DL, FFI, National Academies Press). Page range for [2] is the IEEE Xplore range; the DOI is the definitive anchor. [8] is an FFI technical report (Rep. 17/01510); [9] is anchored by DOI and arXiv:1810.03993.
- Attributed-claim check: [7] confirmed against full text (variance, random-seed, and reporting statements); [3], [4] corroborated (a fog-of-war bot can be competitive vs. a perfect-information variant, and cheating is noticeable to players); [1], [2], [5], [6] are standard/foundational attributions. §2 paraphrases sources (no direct quotes), so the IEEE `[n]` citations are complete as written.
- Pinpoint page locators (e.g., `[2, p. 295]`) are optional and used mainly for direct quotes; they are not required here. Where a source's full text is paywalled or not machine-extractable, any pinpoint locators are left for the author to add against the published PDFs.
- Optional additions pending author/venue verification: a data-driven CGF behavior-modeling survey (e.g., arXiv:2404.13954, 2024) for the §2 data-driven sentence; DefogGAN (arXiv:2003.01927) as a hidden-state-inference contrast.
