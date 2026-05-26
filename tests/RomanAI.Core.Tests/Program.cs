using RomanAI;
using RomanAI.Headless;
using System.Text.Json;

int failures = 0;

void Check(bool condition, string message)
{
    if (condition)
    {
        return;
    }

    failures++;
    Console.Error.WriteLine("FAIL: " + message);
}

void CheckThrows<TException>(Action action, string message) where TException : Exception
{
    try
    {
        action();
        failures++;
        Console.Error.WriteLine("FAIL: " + message);
    }
    catch (TException)
    {
    }
}

RomanValidationScenarioManifestScenario ScenarioSpec(string scenarioId)
{
    return new RomanValidationScenarioManifestScenario
    {
        scenarioId = scenarioId,
        purpose = "test scenario",
        defaultTicks = 1
    };
}

RomanValidationInvariantSpec BatchPassInvariant()
{
    return new RomanValidationInvariantSpec
    {
        scenarioId = "batch",
        invariantId = "batch_passes",
        category = "safety",
        expected = "passed=true",
        reason = "test batch pass invariant",
        enabled = true
    };
}

string FindRepoFile(string relativePath)
{
    foreach (string startPath in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
    {
        DirectoryInfo directory = new DirectoryInfo(startPath);
        while (directory != null)
        {
            string candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }
    }

    return string.Empty;
}

RomanHeadlessScenarioSummary FindSummary(RomanHeadlessBatchResult result, string scenarioId)
{
    if (result == null || result.scenarioSummaries == null)
    {
        return null;
    }

    for (int i = 0; i < result.scenarioSummaries.Length; i++)
    {
        RomanHeadlessScenarioSummary summary = result.scenarioSummaries[i];
        if (summary != null && string.Equals(summary.scenarioId, scenarioId, StringComparison.Ordinal))
        {
            return summary;
        }
    }

    return null;
}

RomanPaperExperimentSummary FindPaperSummary(RomanPaperExperimentResult result, string conditionId)
{
    if (result == null || result.summaries == null)
    {
        return null;
    }

    for (int i = 0; i < result.summaries.Length; i++)
    {
        RomanPaperExperimentSummary summary = result.summaries[i];
        if (summary != null && string.Equals(summary.conditionId, conditionId, StringComparison.Ordinal))
        {
            return summary;
        }
    }

    return null;
}

RomanPaperExperimentRunRow FindPaperRunRow(RomanPaperExperimentRunRow[] rows, string conditionId)
{
    for (int i = 0; rows != null && i < rows.Length; i++)
    {
        if (rows[i] != null && string.Equals(rows[i].conditionId, conditionId, StringComparison.Ordinal))
        {
            return rows[i];
        }
    }

    return null;
}

RomanValidationInvariantResult FindInvariant(
    RomanValidationScenarioReport report,
    string scenarioId,
    string invariantId)
{
    if (report == null || report.invariants == null)
    {
        return null;
    }

    for (int i = 0; i < report.invariants.Length; i++)
    {
        RomanValidationInvariantResult invariant = report.invariants[i];
        if (invariant != null
            && string.Equals(invariant.scenarioId, scenarioId, StringComparison.Ordinal)
            && string.Equals(invariant.invariantId, invariantId, StringComparison.Ordinal))
        {
            return invariant;
        }
    }

    return null;
}

RomanAgentDecision ValidTraceDecision(string unitId)
{
    return new RomanAgentDecision
    {
        unitId = unitId,
        intent = RomanDecisionIntentKind.Hold,
        intentName = "hold",
        objective = "maintain_state",
        source = "roman_core_trace_only",
        reason = "unit_test",
        confidence01 = 1f,
        adapterMayApply = false,
        requiresHumanReview = false,
        safetyBoundary = RomanSimulationCoreApi.SafetyPolicy
    };
}

Check(RomanProductContract.DefaultTrack == RomanProductTrack.HumanBehaviorSimulationCore, "default product track");
Check(RomanProductContract.IsAllowedUseCase("synthetic_human_behavior_simulation"), "synthetic simulation allowed");
Check(RomanProductContract.IsAllowedUseCase("non_clinical_medical_training_simulation"), "non-clinical medical training allowed");
Check(RomanProductContract.RequiresReview("named_physiological_modifier_distribution"), "named physiological modifier requires review");
Check(RomanProductContract.IsProhibitedUseCase("drug_dosing_recommendation"), "drug dosing prohibited");

RomanHumanPerformanceModifierProfile profile = RomanHumanPerformanceStateModel.CreateSyntheticStimulantLikeProfile();
RomanBehaviorStateDelta early = RomanHumanPerformanceStateModel.Evaluate(profile, 0f);
RomanBehaviorStateDelta peak = RomanHumanPerformanceStateModel.Evaluate(profile, 120f);
Check(Math.Abs(early.fatigue) < 0.0001f, "zero elapsed has no fatigue effect");
Check(peak.fatigue < 0f, "profile reduces fatigue");
Check(peak.confidence > 0f, "profile raises confidence");
Check(peak.hesitation < 0f, "profile reduces hesitation");

RomanCombatStress stress = new RomanCombatStress
{
    fatigue = 0.75f,
    confidence = 0.45f
};
RomanCombatStress adjusted = RomanHumanPerformanceStateModel.ApplyToStress(stress, peak);
Check(adjusted.fatigue < stress.fatigue, "stress fatigue adjusted downward");
Check(adjusted.confidence > stress.confidence, "stress confidence adjusted upward");

RomanSimulationBehaviorState neutralBehavior = RomanBehaviorStateModel.CreateNeutral();
Check(RomanBehaviorStateModel.DimensionCount == 12, "behavior model dimension count");
Check(RomanBehaviorStateModel.Validate(neutralBehavior).isValid, "neutral behavior validates");
RomanSimulationBehaviorState unclampedBehavior = new RomanSimulationBehaviorState
{
    fatigue = 1.4f,
    confidence = -0.2f,
    morale = 0.5f,
    cohesion = 0.5f,
    selfPreservation = 0.5f
};
RomanBehaviorStateModel.Normalize(unclampedBehavior);
Check(Math.Abs(unclampedBehavior.fatigue - 1f) < 0.0001f, "behavior fatigue clamps high");
Check(Math.Abs(unclampedBehavior.confidence) < 0.0001f, "behavior confidence clamps low");
RomanSimulationBehaviorState behaviorBaseline = RomanBehaviorStateModel.CreateBaseline(0.7f, 0.45f);
RomanSimulationBehaviorState behaviorAdjusted = RomanHumanPerformanceStateModel.ApplyToBehavior(behaviorBaseline, peak);
Check(behaviorAdjusted.fatigue < behaviorBaseline.fatigue, "behavior fatigue adjusted downward");
Check(behaviorAdjusted.confidence > behaviorBaseline.confidence, "behavior confidence adjusted upward");
Check(RomanBehaviorStateModel.Validate(behaviorAdjusted).isValid, "adjusted behavior validates");
RomanCombatStress stressRoundTrip = RomanBehaviorStateModel.ToCombatStress(
    RomanBehaviorStateModel.FromCombatStress(stress, selfPreservation: 0.6f, aggression: 0.2f));
Check(Math.Abs(stressRoundTrip.fatigue - stress.fatigue) < 0.0001f, "behavior stress round trip preserves fatigue");
Check(Math.Abs(stressRoundTrip.confidence - stress.confidence) < 0.0001f, "behavior stress round trip preserves confidence");

RomanScenarioConfig scenario = RomanSimulationCoreApi.CreateDefaultScenarioConfig("api_contract_probe", 1337);
Check(scenario.apiVersion == RomanSimulationCoreApi.ApiVersion, "scenario api version");
Check(scenario.executionMode == RomanSimulationExecutionMode.TraceOnly, "scenario defaults to trace-only");
Check(!scenario.allowWorldMutation, "scenario disallows mutation by default");

RomanSimulationAgentState agent = new RomanSimulationAgentState
{
    unitId = "blue-1",
    team = RomanTeam.BLUFOR,
    soldierState = RomanSoldierState.Idle,
    position = new RomanCoreVector3(1f, 0f, 2f),
    forward = new RomanCoreVector3(0f, 0f, 1f),
    health01 = 1f,
    ammo = 30,
    isAlive = true,
    role = "rifle"
};
RomanSimulationTickInput tick = RomanSimulationCoreApi.CreateTickInput(
    scenario,
    4,
    0.4f,
    new[] { agent });
Check(RomanSimulationCoreApi.ValidateTickInput(tick).isValid, "valid tick input");
Check(tick.deterministicSeed == (1337 ^ 4), "deterministic seed derives from scenario and tick");
RomanWorldObservation validObservation = new RomanWorldObservation
{
    observationId = "obs-1",
    observerUnitId = "blue-1",
    observedAtSeconds = 0.4f,
    uncertainty01 = 0.25f,
    visibleUnits = new[]
    {
        new RomanObservedUnit
        {
            unitId = "opfor-1",
            team = RomanTeam.OPFOR,
            confidence01 = 0.8f,
            lastSeenSeconds = 0.4f,
            hasLineOfSight = true
        }
    }
};
RomanSimulationTickInput observationTick = RomanSimulationCoreApi.CreateTickInput(
    scenario,
    4,
    0.4f,
    new[] { agent },
    new[] { validObservation });
Check(RomanSimulationCoreApi.ValidateTickInput(observationTick).isValid, "valid synthetic observation input");

RomanSimulationTickOutput output = RomanSimulationCoreApi.BuildTraceOnlyHoldOutput(tick, "unit_test");
Check(output.executionMode == RomanSimulationExecutionMode.TraceOnly, "output remains trace-only");
Check(!output.mutatedWorld, "trace-only output does not mutate world");
Check(output.decisions.Length == 1, "one live agent produces one decision");
Check(output.decisions[0].intent == RomanDecisionIntentKind.Hold, "default decision is hold");
Check(!output.decisions[0].adapterMayApply, "trace-only decision cannot be applied");
Check(RomanSimulationCoreApi.ValidateTickOutput(output, tick).isValid, "valid tick output");

RomanSimulationTickInput duplicateTick = RomanSimulationCoreApi.CreateTickInput(
    scenario,
    5,
    0.5f,
    new[]
    {
        new RomanSimulationAgentState { unitId = "dup", health01 = 1f },
        new RomanSimulationAgentState { unitId = "dup", health01 = 1f }
    });
Check(!RomanSimulationCoreApi.ValidateTickInput(duplicateTick).isValid, "duplicate unit ids are rejected");
RomanScenarioConfig invalidSafetyScenario = scenario.Clone();
invalidSafetyScenario.safetyPolicy = "direct_world_control";
RomanSimulationTickInput invalidSafetyTick = RomanSimulationCoreApi.CreateTickInput(
    invalidSafetyScenario,
    4,
    0.4f,
    new[] { agent });
Check(!RomanSimulationCoreApi.ValidateTickInput(invalidSafetyTick).isValid, "scenario safety policy mismatch is rejected");
RomanSimulationTickInput invalidSeedTick = RomanSimulationCoreApi.CreateTickInput(
    scenario,
    4,
    0.4f,
    new[] { agent });
invalidSeedTick.deterministicSeed++;
Check(!RomanSimulationCoreApi.ValidateTickInput(invalidSeedTick).isValid, "deterministic seed mismatch is rejected");
RomanSimulationTickInput invalidObservationObserverTick = RomanSimulationCoreApi.CreateTickInput(
    scenario,
    4,
    0.4f,
    new[] { agent },
    new[]
    {
        new RomanWorldObservation
        {
            observationId = "obs-invalid-observer",
            observerUnitId = "ghost",
            observedAtSeconds = 0.4f,
            uncertainty01 = 0.2f
        }
    });
Check(!RomanSimulationCoreApi.ValidateTickInput(invalidObservationObserverTick).isValid, "observation observer must be a live synthetic agent");
RomanSimulationTickInput invalidObservationConfidenceTick = RomanSimulationCoreApi.CreateTickInput(
    scenario,
    4,
    0.4f,
    new[] { agent },
    new[]
    {
        new RomanWorldObservation
        {
            observationId = "obs-invalid-confidence",
            observerUnitId = "blue-1",
            observedAtSeconds = 0.4f,
            uncertainty01 = 0.2f,
            visibleUnits = new[]
            {
                new RomanObservedUnit
                {
                    unitId = "opfor-1",
                    confidence01 = 1.5f,
                    lastSeenSeconds = 0.4f
                }
            }
        }
    });
Check(!RomanSimulationCoreApi.ValidateTickInput(invalidObservationConfidenceTick).isValid, "observation confidence range is rejected");
RomanSimulationTickInput invalidBehaviorTick = RomanSimulationCoreApi.CreateTickInput(
    scenario,
    6,
    0.6f,
    new[]
    {
        new RomanSimulationAgentState
        {
            unitId = "invalid-behavior",
            health01 = 1f,
            behavior = new RomanSimulationBehaviorState { fatigue = 2f }
        }
    });
Check(!RomanSimulationCoreApi.ValidateTickInput(invalidBehaviorTick).isValid, "invalid behavior state is rejected");
RomanSimulationTickOutput traceApplyOutput = RomanSimulationCoreApi.BuildTraceOnlyHoldOutput(tick, "unit_test");
traceApplyOutput.decisions[0].adapterMayApply = true;
Check(!RomanSimulationCoreApi.ValidateTickOutput(traceApplyOutput, tick).isValid, "trace-only decisions cannot be adapter-applicable");
RomanSimulationTickOutput scenarioMismatchOutput = RomanSimulationCoreApi.BuildTraceOnlyHoldOutput(tick, "unit_test");
scenarioMismatchOutput.scenarioId = "other_scenario";
Check(!RomanSimulationCoreApi.ValidateTickOutput(scenarioMismatchOutput, tick).isValid, "output scenario id must match input scenario");
RomanSimulationTickOutput traceIdMismatchOutput = RomanSimulationCoreApi.BuildTraceOnlyHoldOutput(tick, "unit_test");
traceIdMismatchOutput.traceId = "wrong_trace";
Check(!RomanSimulationCoreApi.ValidateTickOutput(traceIdMismatchOutput, tick).isValid, "output trace id must match input tick identity");
RomanSimulationTickOutput duplicateDecisionOutput = RomanSimulationCoreApi.BuildTraceOnlyHoldOutput(tick, "unit_test");
duplicateDecisionOutput.decisions = new[] { ValidTraceDecision("blue-1"), ValidTraceDecision("blue-1") };
Check(!RomanSimulationCoreApi.ValidateTickOutput(duplicateDecisionOutput, tick).isValid, "duplicate decisions for one unit are rejected");
RomanSimulationTickOutput unknownDecisionOutput = RomanSimulationCoreApi.BuildTraceOnlyHoldOutput(tick, "unit_test");
unknownDecisionOutput.decisions[0].unitId = "ghost";
Check(!RomanSimulationCoreApi.ValidateTickOutput(unknownDecisionOutput, tick).isValid, "decision unit must be a live synthetic agent");
RomanSimulationTickInput deadAgentTick = RomanSimulationCoreApi.CreateTickInput(
    scenario,
    4,
    0.4f,
    new[]
    {
        new RomanSimulationAgentState
        {
            unitId = "dead-1",
            health01 = 0f,
            isAlive = false
        }
    });
RomanSimulationTickOutput deadAgentDecisionOutput = new RomanSimulationTickOutput
{
    scenarioId = scenario.scenarioId,
    tickIndex = 4,
    executionMode = RomanSimulationExecutionMode.TraceOnly,
    traceId = "api_contract_probe:4",
    decisions = new[] { ValidTraceDecision("dead-1") }
};
Check(!RomanSimulationCoreApi.ValidateTickOutput(deadAgentDecisionOutput, deadAgentTick).isValid, "decision unit must be alive");
RomanSimulationTickOutput mutationOutput = RomanSimulationCoreApi.BuildTraceOnlyHoldOutput(tick, "unit_test");
mutationOutput.mutatedWorld = true;
Check(!RomanSimulationCoreApi.ValidateTickOutput(mutationOutput, tick).isValid, "unauthorized output mutation is rejected");
RomanSimulationTickOutput safetyBoundaryOutput = RomanSimulationCoreApi.BuildTraceOnlyHoldOutput(tick, "unit_test");
safetyBoundaryOutput.decisions[0].safetyBoundary = "world_mutation_allowed";
Check(!RomanSimulationCoreApi.ValidateTickOutput(safetyBoundaryOutput, tick).isValid, "decision safety boundary must match core safety policy");

RomanSimulationTickOutput prohibitedOutput = new RomanSimulationTickOutput
{
    scenarioId = scenario.scenarioId,
    tickIndex = 4,
    executionMode = RomanSimulationExecutionMode.TraceOnly,
    traceId = "api_contract_probe:4",
    decisions = new[]
    {
        new RomanAgentDecision
        {
            unitId = "blue-1",
            intent = RomanDecisionIntentKind.ReportContact,
            intentName = "weapon_control",
            confidence01 = 1f
        }
    }
};
Check(!RomanSimulationCoreApi.ValidateTickOutput(prohibitedOutput, tick).isValid, "prohibited decision tokens are rejected");
Check(
    !RomanInformationAuthority.CanDriveEnemyTargeting(
        RomanInformationAuthorityLevel.PrivilegedTruth,
        RomanKnowledgeSource.DebugPrivileged,
        privilegedOnly: true),
    "privileged enemy knowledge cannot drive targeting");

RomanScenarioConfig authorityScenario = RomanSimulationCoreApi.CreateDefaultScenarioConfig("authority_privileged_bait", 20260524);
authorityScenario.maxTicks = 2;
RomanSimulationAgentState authorityAgent = new RomanSimulationAgentState
{
    unitId = "authority_privileged_bait-blue-1",
    team = RomanTeam.BLUFOR,
    soldierState = RomanSoldierState.Idle,
    position = new RomanCoreVector3(0f, 0f, 0f),
    forward = new RomanCoreVector3(0f, 0f, 1f),
    health01 = 1f,
    ammo = 30,
    isAlive = true,
    behavior = RomanBehaviorStateModel.CreateBaseline(0.35f, 0.60f)
};
RomanWorldObservation privilegedBaitObservation = new RomanWorldObservation
{
    observationId = "authority-bait-obs-0",
    observerUnitId = authorityAgent.unitId,
    observedAtSeconds = 0f,
    source = "privileged_truth_bait",
    lastKnownContactPosition = new RomanCoreVector3(8f, 0f, 4f),
    visibleUnits = new[]
    {
        new RomanObservedUnit
        {
            unitId = "authority-bait-opfor-1",
            team = RomanTeam.OPFOR,
            position = new RomanCoreVector3(8f, 0f, 4f),
            confidence01 = 1f,
            lastSeenSeconds = 0f,
            hasLineOfSight = false,
            authorityLevel = RomanInformationAuthorityLevel.PrivilegedTruth,
            knowledgeSource = RomanKnowledgeSource.DebugPrivileged,
            privilegedOnly = true
        }
    }
};
RomanSimulationTickInput authorityTick = RomanSimulationCoreApi.CreateTickInput(
    authorityScenario,
    0,
    0f,
    new[] { authorityAgent },
    new[] { privilegedBaitObservation });
RomanSimulationTickOutput honestAuthorityOutput = RomanSimulationCoreApi.BuildTraceOnlyHoldOutput(authorityTick, "authority_bait_test");
Check(RomanSimulationCoreApi.ValidateTickOutput(honestAuthorityOutput, authorityTick).isValid, "honest authority bait output validates");
Check(honestAuthorityOutput.privilegedTruthDecisions == 0, "honest authority bait has no privileged truth decisions");
Check(honestAuthorityOutput.blockedPrivilegedAccessAttempts == 1, "honest authority bait counts blocked privileged access");
Check(honestAuthorityOutput.decisions.Length == 1 && honestAuthorityOutput.decisions[0].intent == RomanDecisionIntentKind.Hold, "honest authority bait remains hold");
Check(honestAuthorityOutput.decisions[0].desiredPosition.x == authorityAgent.position.x, "honest authority bait does not move toward privileged position");
RomanSimulationTickOutput cheatingAuthorityOutput = RomanSimulationCoreApi.BuildTraceOnlyPrivilegedViolationOutput(authorityTick, "authority_negative_test");
Check(cheatingAuthorityOutput.privilegedTruthDecisions == 1, "cheating authority output records privileged truth decision");
Check(!RomanSimulationCoreApi.ValidateTickOutput(cheatingAuthorityOutput, authorityTick).isValid, "cheating authority output fails validation");

RomanValidationScenarioManifest defaultManifest = RomanValidationScenarioSet.LoadDefaultManifest();
RomanHeadlessBatchResult batch = RomanHeadlessScenarioRunner.RunBatch(
    RomanHeadlessScenarioRunner.CreateDefaultScenarios(defaultManifest));
Check(batch.passed, "default headless batch passes");
Check(batch.scenarioSummaries.Length == 4, "default headless batch has four scenarios");
Check(batch.tickRows.Length == 17, "default headless batch has expected tick rows");
RomanHeadlessScenarioSummary baselineSummary = FindSummary(batch, "baseline_trace_hold");
RomanHeadlessScenarioSummary contactSummary = FindSummary(batch, "contact_uncertainty_trace");
RomanHeadlessScenarioSummary performanceSummary = FindSummary(batch, "performance_state_trace");
Check(baselineSummary != null && baselineSummary.mutationCount == 0, "baseline summary has no mutations");
Check(contactSummary != null && contactSummary.totalObservations > 0, "contact scenario records observations");
Check(contactSummary != null && contactSummary.privilegedTruthDecisions == 0, "contact scenario has no privileged truth decisions");
Check(contactSummary != null && contactSummary.blockedPrivilegedAccessAttempts == 0, "contact scenario has no blocked privileged access");
Check(performanceSummary != null && performanceSummary.averageFatigue < 0.7f, "performance scenario changes fatigue");
RomanHeadlessScenarioSummary squadSummary = FindSummary(batch, "squad_cohesion_trace");
Check(squadSummary != null && squadSummary.agentCount == 4, "squad scenario has four agents");
Check(squadSummary != null && squadSummary.totalDecisions == squadSummary.ticks * squadSummary.agentCount, "squad scenario emits one decision per agent each tick");
string summaryCsv = RomanHeadlessScenarioRunner.ToSummaryCsv(batch);
string tickCsv = RomanHeadlessScenarioRunner.ToTickCsv(batch);
Check(summaryCsv.StartsWith("scenario_id,seed,ticks", StringComparison.Ordinal), "summary csv header");
Check(tickCsv.StartsWith("scenario_id,tick,time_s", StringComparison.Ordinal), "tick csv header");
Check(summaryCsv.Contains("privileged_truth_decisions", StringComparison.Ordinal), "summary csv includes authority counters");
Check(!summaryCsv.Contains("weapon", StringComparison.OrdinalIgnoreCase), "summary csv avoids prohibited weapon token");
RomanHeadlessBatchResult authorityBatch = RomanHeadlessScenarioRunner.RunBatch(new[]
{
    new RomanHeadlessScenarioSpec
    {
        scenarioId = "authority_privileged_bait",
        randomSeed = 505,
        ticks = 4,
        agentCount = 1,
        baselineFatigue = 0.35f,
        baselineConfidence = 0.60f,
        includePrivilegedBaitObservation = true
    }
});
RomanHeadlessScenarioSummary authoritySummary = FindSummary(authorityBatch, "authority_privileged_bait");
Check(authorityBatch.passed, "authority privileged bait headless batch passes honestly");
Check(authoritySummary != null && authoritySummary.privilegedTruthDecisions == 0, "authority bait has zero privileged truth decisions");
Check(authoritySummary != null && authoritySummary.blockedPrivilegedAccessAttempts == authoritySummary.ticks, "authority bait blocks privileged access each tick");
RomanHeadlessBatchResult repeatBatch = RomanHeadlessScenarioRunner.RunBatch(
    RomanHeadlessScenarioRunner.CreateDefaultScenarios(defaultManifest));
Check(summaryCsv == RomanHeadlessScenarioRunner.ToSummaryCsv(repeatBatch), "headless summary is deterministic");
Check(tickCsv == RomanHeadlessScenarioRunner.ToTickCsv(repeatBatch), "headless tick trace is deterministic");
Check(defaultManifest.setVersion == "0.3.2", "default validation manifest version");
Check(!string.IsNullOrWhiteSpace(defaultManifest.description), "default validation manifest description");
Check(defaultManifest.scenarios.Length == 4, "default validation manifest scenario coverage");
Check(defaultManifest.invariants.Length >= 21, "default validation manifest invariant coverage");
Check(defaultManifest.replayProfiles.Length == 1, "default validation manifest replay profile coverage");
Check(defaultManifest.replayProfiles[0].profileId == "long_replay", "default validation manifest includes long replay profile");
RomanHeadlessScenarioSpec[] longReplayScenarios = RomanHeadlessScenarioRunner.CreateReplayProfileScenarios(
    defaultManifest,
    defaultManifest.replayProfiles[0]);
RomanHeadlessBatchResult longReplayBatch = RomanHeadlessScenarioRunner.RunBatch(longReplayScenarios);
RomanHeadlessBatchResult longReplayRepeatBatch = RomanHeadlessScenarioRunner.RunBatch(
    RomanHeadlessScenarioRunner.CreateScenariosFromSummaries(longReplayBatch.scenarioSummaries));
Check(longReplayBatch.passed, "long replay profile batch passes");
Check(longReplayBatch.tickRows.Length == 102, "long replay profile expands tick window");
Check(
    RomanHeadlessScenarioRunner.ToSummaryCsv(longReplayBatch) == RomanHeadlessScenarioRunner.ToSummaryCsv(longReplayRepeatBatch),
    "long replay profile summary is deterministic");
Check(
    RomanHeadlessScenarioRunner.ToTickCsv(longReplayBatch) == RomanHeadlessScenarioRunner.ToTickCsv(longReplayRepeatBatch),
    "long replay profile tick trace is deterministic");
string manifestSchemaPath = FindRepoFile(Path.Combine("docs", "schemas", "roman_validation_scenario_manifest.schema.json"));
Check(!string.IsNullOrWhiteSpace(manifestSchemaPath), "validation manifest schema exists");
if (!string.IsNullOrWhiteSpace(manifestSchemaPath))
{
    using JsonDocument schemaDocument = JsonDocument.Parse(File.ReadAllText(manifestSchemaPath));
    Check(
        schemaDocument.RootElement.TryGetProperty("$id", out JsonElement schemaId)
            && string.Equals(schemaId.GetString(), "roman_validation_scenario_manifest.schema.json", StringComparison.Ordinal),
        "validation manifest schema id");
}
RomanValidationScenarioReport validation = RomanValidationScenarioSet.EvaluateDefault(batch);
Check(validation.passed, "default validation scenario set passes");
Check(validation.setVersion == defaultManifest.setVersion, "validation report uses manifest version");
Check(validation.manifestScenarioCount == defaultManifest.scenarios.Length, "validation report records manifest scenario count");
Check(validation.totalInvariants == defaultManifest.invariants.Length, "validation report is manifest-driven");
Check(validation.failedInvariants == 0, "validation scenario set has no failures");
string validationCsv = RomanValidationScenarioSet.ToCsv(validation);
Check(validationCsv.StartsWith("set_id,set_version,scenario_id", StringComparison.Ordinal), "validation csv header");
Check(validationCsv.Contains("deterministic_replay", StringComparison.Ordinal), "validation includes deterministic replay");
Check(validationCsv.Contains("long_replay_profile_deterministic", StringComparison.Ordinal), "validation includes long replay profile");
Check(validationCsv.Contains("scenario_ids_match_manifest", StringComparison.Ordinal), "validation includes manifest scenario id match");
Check(validationCsv.Contains("performance_fatigue_decreases", StringComparison.Ordinal), "validation includes performance state transition");
RomanValidationCoverageReport coverage = RomanValidationScenarioSet.BuildCoverageReport(validation);
Check(coverage.passed, "validation coverage matrix passes");
Check(coverage.totalRows >= defaultManifest.scenarios.Length, "validation coverage matrix has scenario/category rows");
string coverageCsv = RomanValidationScenarioSet.ToCoverageCsv(coverage);
Check(coverageCsv.StartsWith("set_id,set_version,scenario_id,category", StringComparison.Ordinal), "validation coverage csv header");
Check(coverageCsv.Contains("performance_state_trace,state", StringComparison.Ordinal), "validation coverage includes performance state row");
Check(coverageCsv.Contains("squad_cohesion_trace,contract", StringComparison.Ordinal), "validation coverage includes squad contract row");
Check(coverageCsv.Contains("squad_cohesion_trace,state", StringComparison.Ordinal), "validation coverage includes squad state row");
RomanPaperExperimentResult paperExperiment = RomanPaperExperimentRunner.Run(30, 7001);
Check(paperExperiment.passed, "paper experiment runner passes default repetitions");
Check(paperExperiment.manifestSetId == RomanPaperExperimentRunner.DefaultManifestSetId, "paper experiment records built-in manifest id");
Check(paperExperiment.actualRepetitions == 30, "paper experiment uses 30 repetitions");
Check(paperExperiment.summaries.Length == 8, "paper experiment has expected condition summaries");
Check(paperExperiment.rows.Length == 240, "paper experiment writes one row per condition per repetition");
RomanPaperExperimentSummary contactOffSummary = FindPaperSummary(paperExperiment, "contact_off");
RomanPaperExperimentSummary contactOnSummary = FindPaperSummary(paperExperiment, "contact_on");
RomanPaperExperimentSummary modifierOffSummary = FindPaperSummary(paperExperiment, "modifier_off");
RomanPaperExperimentSummary modifierOnSummary = FindPaperSummary(paperExperiment, "modifier_on");
RomanPaperExperimentSummary squadSizeEightSummary = FindPaperSummary(paperExperiment, "squad_size_8");
Check(contactOffSummary != null && contactOffSummary.meanObservationsPerTick == 0d, "paper contact-off condition has no observations");
Check(contactOnSummary != null && Math.Abs(contactOnSummary.meanObservationsPerTick - 1d) < 0.000001d, "paper contact-on condition has one observation per tick");
Check(modifierOffSummary != null && Math.Abs(modifierOffSummary.meanFatigueDelta) < 0.000001d, "paper modifier-off condition keeps fatigue stable");
Check(modifierOnSummary != null && modifierOnSummary.meanFatigueDelta < 0d, "paper modifier-on condition lowers fatigue");
Check(modifierOnSummary != null && modifierOnSummary.meanConfidenceDelta > 0d, "paper modifier-on condition raises confidence");
Check(squadSizeEightSummary != null && squadSizeEightSummary.agentCount == 8, "paper squad-size ablation includes eight agents");
string paperSummaryCsv = RomanPaperExperimentRunner.ToSummaryCsv(paperExperiment);
string paperRowsCsv = RomanPaperExperimentRunner.ToRowsCsv(paperExperiment);
Check(paperSummaryCsv.StartsWith("experiment_id,manifest_set_id,manifest_set_version", StringComparison.Ordinal), "paper experiment summary csv header");
Check(paperSummaryCsv.Contains("ci95_fatigue_delta", StringComparison.Ordinal), "paper experiment summary includes confidence interval column");
Check(paperSummaryCsv.Contains("ci95_blocked_privileged_access_attempts", StringComparison.Ordinal), "paper experiment summary includes authority confidence interval column");
Check(paperRowsCsv.StartsWith("experiment_id,manifest_set_id,manifest_set_version", StringComparison.Ordinal), "paper experiment rows csv header");
Check(paperRowsCsv.Contains("privileged_truth_decisions", StringComparison.Ordinal), "paper experiment rows include authority counters");
Check(paperRowsCsv.Contains("paper_squad_size_8", StringComparison.Ordinal), "paper experiment rows include squad-size ablation");
string heldOutManifestPath = FindRepoFile(Path.Combine("tools", "RomanAI.Headless", "paper_heldout_manifest.json"));
Check(!string.IsNullOrWhiteSpace(heldOutManifestPath), "paper held-out manifest exists");
string broaderManifestPath = FindRepoFile(Path.Combine("tools", "RomanAI.Headless", "paper_broader_manifest.json"));
Check(!string.IsNullOrWhiteSpace(broaderManifestPath), "paper broader manifest exists");
RomanPaperExperimentResult heldOutExperiment = RomanPaperExperimentRunner.Run(
    30,
    9001,
    heldOutManifestPath,
    includeNegativeControls: true);
Check(heldOutExperiment.passed, "paper held-out experiment passes with negative controls");
Check(heldOutExperiment.manifestSetId == "unity_free_paper_heldout_conditions", "paper held-out experiment records manifest id");
Check(heldOutExperiment.summaries.Length == 7, "paper held-out experiment has expected condition summaries");
Check(heldOutExperiment.rows.Length == 210, "paper held-out experiment writes held-out rows");
Check(heldOutExperiment.negativeControlRows.Length == 6, "paper negative controls include runner-level rows");
Check(heldOutExperiment.negativeControls.Length == 6, "paper negative controls are reported");
Check(heldOutExperiment.failureTaxonomy.Length == 10, "paper failure taxonomy has expected rows");
string negativeControlsCsv = RomanPaperExperimentRunner.ToNegativeControlsCsv(heldOutExperiment);
string failureTaxonomyCsv = RomanPaperExperimentRunner.ToFailureTaxonomyCsv(heldOutExperiment);
string heldOutRowsCsv = RomanPaperExperimentRunner.ToRowsCsv(heldOutExperiment);
RomanPaperExperimentRunRow mutationControlRow = FindPaperRunRow(heldOutExperiment.negativeControlRows, "mutation_violation");
RomanPaperExperimentRunRow determinismControlRow = FindPaperRunRow(heldOutExperiment.negativeControlRows, "determinism_mismatch");
RomanPaperExperimentRunRow authorityControlRow = FindPaperRunRow(heldOutExperiment.negativeControlRows, "privileged_truth_violation");
Check(mutationControlRow != null && !mutationControlRow.passed && mutationControlRow.mutationCount > 0, "paper mutation negative control is a failed runner row");
Check(determinismControlRow != null && !determinismControlRow.passed && !determinismControlRow.deterministicReplayMatched, "paper determinism negative control is a failed replay row");
Check(authorityControlRow != null && !authorityControlRow.passed && authorityControlRow.privilegedTruthDecisions > 0, "paper authority negative control is a failed privileged-use row");
Check(authorityControlRow != null && string.Equals(authorityControlRow.failureBucket, "authority_failure", StringComparison.Ordinal), "paper authority negative control uses authority bucket");
Check(negativeControlsCsv.Contains("mutation_violation,safety_failure,1,1,1.000000,true", StringComparison.Ordinal), "paper negative control mutation fails closed");
Check(negativeControlsCsv.Contains("determinism_mismatch,determinism_failure,1,1,1.000000,true", StringComparison.Ordinal), "paper negative control determinism fails closed");
Check(negativeControlsCsv.Contains("privileged_truth_violation,authority_failure,1,1,1.000000,true", StringComparison.Ordinal), "paper negative control authority fails closed");
Check(negativeControlsCsv.Contains("runner_level_failed_execution", StringComparison.Ordinal), "paper negative control summary records runner-level source");
Check(heldOutRowsCsv.Contains("negative_control,mutation_violation,paper_negative_control_runner", StringComparison.Ordinal), "paper rows csv includes runner-level negative-control rows");
Check(heldOutRowsCsv.Contains("negative_control,privileged_truth_violation,paper_negative_control_runner", StringComparison.Ordinal), "paper rows csv includes authority negative-control row");
Check(failureTaxonomyCsv.Contains("negative_control", StringComparison.Ordinal), "paper failure taxonomy includes negative-control source");
Check(failureTaxonomyCsv.Contains("authority_failure,negative_control,1,1,true", StringComparison.Ordinal), "paper failure taxonomy includes authority failure bucket");
Check(failureTaxonomyCsv.Contains("normal_experiment,0,0,true", StringComparison.Ordinal), "paper failure taxonomy records no normal failures");
RomanPaperReadyBundleResult paperBundle = RomanPaperExperimentRunner.CreatePaperReadyBundle(
    30,
    11001,
    heldOutManifestPath);
Check(paperBundle.passed, "paper-ready bundle evidence passes");
Check(!paperBundle.paperCompletionReady, "paper-ready bundle does not overclaim paper completion");
Check(paperBundle.defaultExperiment.summaries.Length == 8, "paper-ready bundle includes default ablation summaries");
Check(paperBundle.heldOutExperiment.summaries.Length == 7, "paper-ready bundle includes held-out summaries");
Check(paperBundle.broaderExperiment.summaries.Length == 6, "paper-ready bundle includes broader unseen summaries");
Check(paperBundle.authorityExperiment.summaries.Length == 1, "paper-ready bundle includes authority no-cheat summary");
RomanPaperExperimentSummary bundleAuthoritySummary = FindPaperSummary(paperBundle.authorityExperiment, "authority_privileged_bait");
Check(bundleAuthoritySummary != null && bundleAuthoritySummary.meanPrivilegedTruthDecisions == 0d, "paper-ready authority summary has zero privileged truth decisions");
Check(bundleAuthoritySummary != null && bundleAuthoritySummary.meanBlockedPrivilegedAccessAttempts > 0d, "paper-ready authority summary has blocked privileged attempts");
string paperResultsMarkdown = RomanPaperExperimentRunner.ToPaperResultsMarkdown(paperBundle);
string paperLimitationsMarkdown = RomanPaperExperimentRunner.ToPaperLimitationsMarkdown(paperBundle);
string paperReadinessCsv = RomanPaperExperimentRunner.ToReadinessCsv(paperBundle);
Check(paperResultsMarkdown.Contains("Paper-Ready Results Table", StringComparison.Ordinal), "paper-ready bundle writes results table markdown");
Check(paperResultsMarkdown.Contains("Held-Out Results", StringComparison.Ordinal), "paper-ready bundle results include held-out section");
Check(paperResultsMarkdown.Contains("Broader Unseen Results", StringComparison.Ordinal), "paper-ready bundle results include broader section");
Check(paperResultsMarkdown.Contains("Information Authority No-Cheat Evidence", StringComparison.Ordinal), "paper-ready bundle results include authority section");
Check(paperLimitationsMarkdown.Contains("Not Supported Claims", StringComparison.Ordinal), "paper-ready bundle writes limitations section");
Check(paperLimitationsMarkdown.Contains("architecture-enforcement evidence", StringComparison.Ordinal), "paper limitations bound no-cheat claim");
Check(paperReadinessCsv.Contains("no_cheat_invariant_evidence,true,implemented", StringComparison.Ordinal), "paper readiness report records no-cheat invariant evidence");
Check(paperReadinessCsv.Contains("broader_unseen_scenario_families,true,implemented", StringComparison.Ordinal), "paper readiness report records broader unseen scenario families");
Check(paperReadinessCsv.Contains("clean_machine_reproduction_script,true,implemented", StringComparison.Ordinal), "paper readiness report records reproduction script");
Check(paperReadinessCsv.Contains("paper_narrative_draft_present,true,implemented", StringComparison.Ordinal), "paper readiness report records narrative draft");
Check(paperReadinessCsv.Contains("independent_clean_machine_reproduction,false,blocked", StringComparison.Ordinal), "paper readiness report blocks missing clean-machine reproduction");
string paperBundleDirectory = Path.Combine(Path.GetTempPath(), "roman_paper_bundle_" + Guid.NewGuid().ToString("N"));
try
{
    RomanPaperExperimentRunner.WritePaperReadyBundle(paperBundle, paperBundleDirectory);
    Check(File.Exists(Path.Combine(paperBundleDirectory, "paper_results_table.md")), "paper-ready bundle writes results table file");
    Check(File.Exists(Path.Combine(paperBundleDirectory, "paper_limitations.md")), "paper-ready bundle writes limitations file");
    Check(File.Exists(Path.Combine(paperBundleDirectory, "paper_artifact_bundle_manifest.json")), "paper-ready bundle writes artifact manifest file");
    Check(File.Exists(Path.Combine(paperBundleDirectory, "paper_readiness_report.json")), "paper-ready bundle writes readiness report file");
    Check(File.Exists(Path.Combine(paperBundleDirectory, "heldout_negative_experiment", "paper_experiment_negative_controls.csv")), "paper-ready bundle writes held-out negative controls file");
    Check(File.Exists(Path.Combine(paperBundleDirectory, "broader_unseen_experiment", "paper_experiment_summary.csv")), "paper-ready bundle writes broader experiment file");
    Check(File.Exists(Path.Combine(paperBundleDirectory, "authority_no_cheat_experiment", "paper_experiment_summary.csv")), "paper-ready bundle writes authority experiment file");
    RomanPaperArtifactBundleManifest artifactManifest = JsonSerializer.Deserialize<RomanPaperArtifactBundleManifest>(
        File.ReadAllText(Path.Combine(paperBundleDirectory, "paper_artifact_bundle_manifest.json")),
        new JsonSerializerOptions { IncludeFields = true });
    Check(artifactManifest.commands.Length == 4, "paper artifact manifest records reproduction commands");
    Check(artifactManifest.commands[0].Contains("--paper-seed-start 11001", StringComparison.Ordinal), "paper artifact manifest records actual bundle seed");
    Check(artifactManifest.commands[0].Contains(paperBundleDirectory, StringComparison.Ordinal), "paper artifact manifest records actual output path");
    Check(artifactManifest.commands[2].Contains("--paper-negative-controls", StringComparison.Ordinal), "paper artifact manifest records held-out negative-control command");
    Check(artifactManifest.authorityManifestSetId == RomanPaperExperimentRunner.AuthorityManifestSetId, "paper artifact manifest records authority manifest id");
}
finally
{
    if (Directory.Exists(paperBundleDirectory))
    {
        Directory.Delete(paperBundleDirectory, recursive: true);
    }
}
RomanValidationScenarioManifest shortDefaultTicksManifest = new RomanValidationScenarioManifest
{
    setId = "short_default_ticks_manifest",
    setVersion = "test",
    description = "manifest defaultTicks drives runner probe",
    scenarios = new[]
    {
        new RomanValidationScenarioManifestScenario
        {
            scenarioId = "baseline_trace_hold",
            purpose = "short baseline",
            defaultTicks = 3
        }
    },
    invariants = new[] { BatchPassInvariant() }
};
RomanHeadlessBatchResult shortDefaultTicksBatch = RomanHeadlessScenarioRunner.RunBatch(
    RomanHeadlessScenarioRunner.CreateDefaultScenarios(shortDefaultTicksManifest));
Check(shortDefaultTicksBatch.tickRows.Length == 3, "manifest defaultTicks drives runner tick count");
Check(shortDefaultTicksBatch.scenarioSummaries.Length == 1, "manifest scenario list drives runner scenario count");
RomanValidationScenarioManifest unsupportedRunnerScenarioManifest = new RomanValidationScenarioManifest
{
    setId = "unsupported_runner_scenario_manifest",
    setVersion = "test",
    description = "unsupported runner scenario rejection probe",
    scenarios = new[] { ScenarioSpec("unknown_runner_trace") },
    invariants = new[] { BatchPassInvariant() }
};
CheckThrows<InvalidDataException>(
    () => RomanHeadlessScenarioRunner.CreateDefaultScenarios(unsupportedRunnerScenarioManifest),
    "runner rejects unsupported manifest scenario ids");
RomanValidationScenarioManifest reorderedScenarioManifest = new RomanValidationScenarioManifest
{
    setId = "reordered_scenario_manifest",
    setVersion = "test",
    description = "scenario id order mismatch probe",
    scenarios = new[]
    {
        defaultManifest.scenarios[1],
        defaultManifest.scenarios[0],
        defaultManifest.scenarios[2],
        defaultManifest.scenarios[3]
    },
    invariants = defaultManifest.invariants
};
RomanValidationScenarioReport reorderedValidation = RomanValidationScenarioSet.Evaluate(batch, reorderedScenarioManifest);
RomanValidationInvariantResult scenarioIdsInvariant = FindInvariant(reorderedValidation, "batch", "scenario_ids_match_manifest");
Check(!reorderedValidation.passed, "validation fails when scenario ids do not match manifest order");
Check(scenarioIdsInvariant != null && !scenarioIdsInvariant.passed, "scenario id mismatch invariant fails closed");
RomanHeadlessBatchResult brokenSquadDecisionBatch = RomanHeadlessScenarioRunner.RunBatch(
    RomanHeadlessScenarioRunner.CreateDefaultScenarios(defaultManifest));
for (int i = 0; i < brokenSquadDecisionBatch.tickRows.Length; i++)
{
    if (string.Equals(brokenSquadDecisionBatch.tickRows[i].scenarioId, "squad_cohesion_trace", StringComparison.Ordinal))
    {
        brokenSquadDecisionBatch.tickRows[i].decisionCount--;
        break;
    }
}
RomanValidationScenarioReport brokenSquadDecisionValidation = RomanValidationScenarioSet.EvaluateDefault(brokenSquadDecisionBatch);
RomanValidationInvariantResult squadDecisionInvariant = FindInvariant(
    brokenSquadDecisionValidation,
    "squad_cohesion_trace",
    "squad_decisions_each_tick");
Check(!brokenSquadDecisionValidation.passed, "validation fails when squad decision cardinality is broken");
Check(squadDecisionInvariant != null && !squadDecisionInvariant.passed, "squad decision cardinality invariant fails closed");
RomanHeadlessBatchResult brokenSquadStateBatch = RomanHeadlessScenarioRunner.RunBatch(
    RomanHeadlessScenarioRunner.CreateDefaultScenarios(defaultManifest));
for (int i = 0; i < brokenSquadStateBatch.tickRows.Length; i++)
{
    if (string.Equals(brokenSquadStateBatch.tickRows[i].scenarioId, "squad_cohesion_trace", StringComparison.Ordinal)
        && brokenSquadStateBatch.tickRows[i].tickIndex == 1)
    {
        brokenSquadStateBatch.tickRows[i].averageFatigue += 0.05f;
        break;
    }
}
RomanValidationScenarioReport brokenSquadStateValidation = RomanValidationScenarioSet.EvaluateDefault(brokenSquadStateBatch);
RomanValidationInvariantResult squadStateInvariant = FindInvariant(
    brokenSquadStateValidation,
    "squad_cohesion_trace",
    "squad_behavior_stable");
Check(!brokenSquadStateValidation.passed, "validation fails when squad behavior drifts on an intermediate tick");
Check(squadStateInvariant != null && !squadStateInvariant.passed, "squad behavior stable invariant checks intermediate ticks");
RomanValidationScenarioManifest singleInvariantManifest = new RomanValidationScenarioManifest
{
    setId = "test_validation_manifest",
    setVersion = "test",
    description = "custom manifest subset",
    scenarios = defaultManifest.scenarios,
    invariants = new[]
    {
        new RomanValidationInvariantSpec
        {
            scenarioId = "batch",
            invariantId = "batch_passes",
            category = "safety",
            expected = "passed=true",
            reason = "custom manifest subset"
        }
    }
};
RomanValidationScenarioReport singleInvariantValidation = RomanValidationScenarioSet.Evaluate(batch, singleInvariantManifest);
Check(singleInvariantValidation.setId == "test_validation_manifest", "custom validation manifest set id");
Check(singleInvariantValidation.totalInvariants == 1, "custom validation manifest controls invariant count");
Check(singleInvariantValidation.passed, "custom validation manifest subset passes");
RomanValidationScenarioManifest unsupportedInvariantManifest = new RomanValidationScenarioManifest
{
    setId = "unsupported_validation_manifest",
    setVersion = "test",
    description = "unsupported invariant fail-closed probe",
    scenarios = defaultManifest.scenarios,
    invariants = new[]
    {
        new RomanValidationInvariantSpec
        {
            scenarioId = "batch",
            invariantId = "unsupported_probe",
            category = "contract",
            expected = "supported invariant id",
            reason = "unsupported invariants must fail closed"
        }
    }
};
RomanValidationScenarioReport unsupportedInvariantValidation = RomanValidationScenarioSet.Evaluate(batch, unsupportedInvariantManifest);
Check(!unsupportedInvariantValidation.passed, "unsupported validation manifest invariant fails closed");
RomanValidationCoverageReport brokenCoverage = RomanValidationScenarioSet.BuildCoverageReport(unsupportedInvariantValidation);
Check(!brokenCoverage.passed, "validation coverage fails when validation report fails");
RomanValidationScenarioManifest duplicateScenarioManifest = new RomanValidationScenarioManifest
{
    setId = "duplicate_scenario_manifest",
    setVersion = "test",
    description = "duplicate scenario rejection probe",
    scenarios = new[]
    {
        ScenarioSpec("duplicate_trace"),
        ScenarioSpec("duplicate_trace")
    },
    invariants = new[] { BatchPassInvariant() }
};
CheckThrows<InvalidDataException>(
    () => RomanValidationScenarioSet.Evaluate(batch, duplicateScenarioManifest),
    "duplicate validation scenario ids are rejected");
RomanValidationScenarioManifest unknownScenarioManifest = new RomanValidationScenarioManifest
{
    setId = "unknown_scenario_manifest",
    setVersion = "test",
    description = "unknown scenario reference rejection probe",
    scenarios = new[] { ScenarioSpec("known_trace") },
    invariants = new[]
    {
        new RomanValidationInvariantSpec
        {
            scenarioId = "missing_trace",
            invariantId = "missing_reference",
            category = "coverage",
            expected = "known scenario",
            reason = "unknown scenario references must fail closed",
            enabled = true
        }
    }
};
CheckThrows<InvalidDataException>(
    () => RomanValidationScenarioSet.Evaluate(batch, unknownScenarioManifest),
    "validation invariant unknown scenario references are rejected");
RomanValidationScenarioManifest unknownCategoryManifest = new RomanValidationScenarioManifest
{
    setId = "unknown_category_manifest",
    setVersion = "test",
    description = "unknown category rejection probe",
    scenarios = new[] { ScenarioSpec("category_trace") },
    invariants = new[]
    {
        new RomanValidationInvariantSpec
        {
            scenarioId = "batch",
            invariantId = "unknown_category",
            category = "unknown",
            expected = "known category",
            reason = "unknown categories must fail closed",
            enabled = true
        }
    }
};
CheckThrows<InvalidDataException>(
    () => RomanValidationScenarioSet.Evaluate(batch, unknownCategoryManifest),
    "validation invariant unknown categories are rejected");
string missingSetIdManifestPath = Path.Combine(Path.GetTempPath(), "roman_validation_missing_setid_" + Guid.NewGuid().ToString("N") + ".json");
try
{
    File.WriteAllText(
        missingSetIdManifestPath,
        "{"
        + "\"format\":\"roman_validation_scenario_manifest_v1\","
        + "\"setVersion\":\"test\","
        + "\"description\":\"missing set id probe\","
        + "\"scenarios\":[{\"scenarioId\":\"shape_trace\",\"purpose\":\"shape probe\",\"defaultTicks\":1}],"
        + "\"invariants\":[{\"scenarioId\":\"batch\",\"invariantId\":\"batch_passes\",\"category\":\"safety\",\"expected\":\"passed=true\",\"reason\":\"shape probe\",\"enabled\":true}]"
        + "}");
    CheckThrows<InvalidDataException>(
        () => RomanValidationScenarioSet.LoadManifest(missingSetIdManifestPath),
        "validation manifest JSON missing setId is rejected");
}
finally
{
    if (File.Exists(missingSetIdManifestPath))
    {
        File.Delete(missingSetIdManifestPath);
    }
}
RomanHeadlessBatchResult brokenBatch = RomanHeadlessScenarioRunner.RunBatch(
    RomanHeadlessScenarioRunner.CreateDefaultScenarios());
brokenBatch.scenarioSummaries[0].mutationCount = 1;
RomanValidationScenarioReport brokenValidation = RomanValidationScenarioSet.EvaluateDefault(brokenBatch);
Check(!brokenValidation.passed, "validation fails when mutation invariant is broken");

if (failures > 0)
{
    Console.Error.WriteLine($"{failures} RomanAI.Core test(s) failed.");
    return 1;
}

Console.WriteLine("RomanAI.Core tests passed.");
return 0;
