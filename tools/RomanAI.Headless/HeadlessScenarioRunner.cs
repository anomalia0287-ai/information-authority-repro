using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using RomanAI;

namespace RomanAI.Headless
{
    public sealed class RomanHeadlessRunOptions
    {
        public string outputDirectory = string.Empty;
        public string validationManifestPath = string.Empty;
        public bool writeFiles;
        public bool emitJson;
        public bool paperExperiment;
        public bool paperReadyBundle;
        public int paperRepetitions = RomanPaperExperimentRunner.MinimumPaperRepetitions;
        public int paperSeedStart = 1001;
        public string paperManifestPath = string.Empty;
        public string paperBroaderManifestPath = string.Empty;
        public bool paperNegativeControls;
        public int tickOverride;

        public static RomanHeadlessRunOptions Parse(string[] args)
        {
            RomanHeadlessRunOptions options = new RomanHeadlessRunOptions();

            for (int i = 0; args != null && i < args.Length; i++)
            {
                string arg = args[i];
                if (string.Equals(arg, "--json", StringComparison.OrdinalIgnoreCase))
                {
                    options.emitJson = true;
                    continue;
                }

                if (string.Equals(arg, "--paper-experiment", StringComparison.OrdinalIgnoreCase))
                {
                    options.paperExperiment = true;
                    continue;
                }

                if (string.Equals(arg, "--paper-ready-bundle", StringComparison.OrdinalIgnoreCase))
                {
                    options.paperReadyBundle = true;
                    continue;
                }

                if (string.Equals(arg, "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    options.outputDirectory = args[++i];
                    options.writeFiles = !string.IsNullOrWhiteSpace(options.outputDirectory);
                    continue;
                }

                if (string.Equals(arg, "--validation-manifest", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    options.validationManifestPath = args[++i];
                    continue;
                }

                if (string.Equals(arg, "--ticks", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    if (int.TryParse(args[++i], NumberStyles.Integer, CultureInfo.InvariantCulture, out int ticks))
                    {
                        options.tickOverride = ticks < 0 ? 0 : ticks;
                    }
                    continue;
                }

                if (string.Equals(arg, "--paper-repetitions", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    if (int.TryParse(args[++i], NumberStyles.Integer, CultureInfo.InvariantCulture, out int repetitions))
                    {
                        options.paperRepetitions = repetitions < 1 ? RomanPaperExperimentRunner.MinimumPaperRepetitions : repetitions;
                    }

                    continue;
                }

                if (string.Equals(arg, "--paper-seed-start", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    if (int.TryParse(args[++i], NumberStyles.Integer, CultureInfo.InvariantCulture, out int seedStart))
                    {
                        options.paperSeedStart = seedStart;
                    }

                    continue;
                }

                if (string.Equals(arg, "--paper-heldout-manifest", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    options.paperManifestPath = args[++i];
                    continue;
                }

                if (string.Equals(arg, "--paper-broader-manifest", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    options.paperBroaderManifestPath = args[++i];
                    continue;
                }

                if (string.Equals(arg, "--paper-negative-controls", StringComparison.OrdinalIgnoreCase))
                {
                    options.paperNegativeControls = true;
                    continue;
                }
            }

            return options;
        }
    }

    public sealed class RomanHeadlessScenarioSpec
    {
        public string scenarioId = "default";
        public int randomSeed;
        public int ticks = 5;
        public int agentCount = 1;
        public float baselineFatigue = 0.35f;
        public float baselineConfidence = 0.55f;
        public bool applySyntheticPerformanceModifier;
        public bool includeContactObservation;
        public bool includePrivilegedBaitObservation;
        public string paperNegativeControlFault = string.Empty;

        public RomanScenarioConfig ToScenarioConfig()
        {
            RomanScenarioConfig config = RomanSimulationCoreApi.CreateDefaultScenarioConfig(scenarioId, randomSeed);
            config.maxTicks = ticks;
            return config;
        }
    }

    public sealed class RomanHeadlessTickRow
    {
        public string scenarioId = "default";
        public long tickIndex;
        public float timeSeconds;
        public int agentCount;
        public int observationCount;
        public int decisionCount;
        public int mutationCount;
        public int privilegedTruthDecisions;
        public int blockedPrivilegedAccessAttempts;
        public float averageFatigue;
        public float averageConfidence;
        public string validation = "ok";
        public string traceId = "none";
    }

    public sealed class RomanHeadlessScenarioSummary
    {
        public string scenarioId = "default";
        public int randomSeed;
        public int ticks;
        public int agentCount;
        public int totalObservations;
        public int totalDecisions;
        public int mutationCount;
        public int invalidInputCount;
        public int invalidOutputCount;
        public int traceOnlyDecisionCount;
        public int privilegedTruthDecisions;
        public int blockedPrivilegedAccessAttempts;
        public float averageFatigue;
        public float averageConfidence;
        public string firstTraceId = "none";
        public string lastTraceId = "none";
        public string status = "passed";
    }

    public sealed class RomanHeadlessBatchResult
    {
        public string runnerVersion = RomanHeadlessScenarioRunner.RunnerVersion;
        public string apiVersion = RomanSimulationCoreApi.ApiVersion;
        public bool passed;
        public string reason = "ok";
        public RomanHeadlessScenarioSummary[] scenarioSummaries = new RomanHeadlessScenarioSummary[0];
        public RomanHeadlessTickRow[] tickRows = new RomanHeadlessTickRow[0];
    }

    public static class RomanHeadlessScenarioRunner
    {
        public const string RunnerVersion = "0.1.0";

        public static RomanHeadlessScenarioSpec[] CreateDefaultScenarios(int tickOverride = 0)
        {
            return CreateDefaultScenarios(RomanValidationScenarioSet.LoadDefaultManifest(), tickOverride);
        }

        public static RomanHeadlessScenarioSpec[] CreateDefaultScenarios(
            RomanValidationScenarioManifest manifest,
            int tickOverride = 0)
        {
            return CreateScenariosFromManifest(
                manifest,
                scenario => ResolveTicks(tickOverride, scenario.defaultTicks));
        }

        public static RomanHeadlessScenarioSpec[] CreateReplayProfileScenarios(
            RomanValidationScenarioManifest manifest,
            RomanValidationReplayProfileSpec replayProfile)
        {
            if (replayProfile == null)
            {
                throw new InvalidDataException("validation replay profile is required");
            }

            if (replayProfile.tickMultiplier <= 0)
            {
                throw new InvalidDataException("validation replay profile tickMultiplier must be > 0: " + replayProfile.profileId);
            }

            if (replayProfile.minimumTicks <= 0)
            {
                throw new InvalidDataException("validation replay profile minimumTicks must be > 0: " + replayProfile.profileId);
            }

            return CreateScenariosFromManifest(
                manifest,
                scenario => Math.Max(scenario.defaultTicks * replayProfile.tickMultiplier, replayProfile.minimumTicks));
        }

        private static RomanHeadlessScenarioSpec[] CreateScenariosFromManifest(
            RomanValidationScenarioManifest manifest,
            Func<RomanValidationScenarioManifestScenario, int> resolveTicks)
        {
            if (manifest == null || manifest.scenarios == null || manifest.scenarios.Length == 0)
            {
                throw new InvalidDataException("validation scenario manifest has no scenarios");
            }

            List<RomanHeadlessScenarioSpec> scenarios = new List<RomanHeadlessScenarioSpec>();
            for (int i = 0; i < manifest.scenarios.Length; i++)
            {
                RomanValidationScenarioManifestScenario scenario = manifest.scenarios[i];
                if (scenario == null)
                {
                    throw new InvalidDataException("validation scenario manifest contains a null scenario");
                }

                scenarios.Add(CreateScenarioTemplate(
                    scenario.scenarioId,
                    resolveTicks == null ? scenario.defaultTicks : resolveTicks(scenario)));
            }

            return scenarios.ToArray();
        }

        public static RomanHeadlessScenarioSpec[] CreateScenariosFromSummaries(RomanHeadlessScenarioSummary[] summaries)
        {
            if (summaries == null || summaries.Length == 0)
            {
                throw new InvalidDataException("headless scenario summaries are required");
            }

            List<RomanHeadlessScenarioSpec> scenarios = new List<RomanHeadlessScenarioSpec>();
            for (int i = 0; i < summaries.Length; i++)
            {
                RomanHeadlessScenarioSummary summary = summaries[i];
                if (summary == null)
                {
                    throw new InvalidDataException("headless scenario summary is null");
                }

                scenarios.Add(CreateScenarioTemplate(summary.scenarioId, summary.ticks));
            }

            return scenarios.ToArray();
        }

        private static RomanHeadlessScenarioSpec CreateScenarioTemplate(string scenarioId, int ticks)
        {
            switch (scenarioId)
            {
                case "baseline_trace_hold":
                    return new RomanHeadlessScenarioSpec
                    {
                        scenarioId = "baseline_trace_hold",
                        randomSeed = 101,
                        ticks = ticks,
                        agentCount = 2,
                        baselineFatigue = 0.25f,
                        baselineConfidence = 0.65f
                    };
                case "contact_uncertainty_trace":
                    return new RomanHeadlessScenarioSpec
                    {
                        scenarioId = "contact_uncertainty_trace",
                        randomSeed = 202,
                        ticks = ticks,
                        agentCount = 2,
                        baselineFatigue = 0.4f,
                        baselineConfidence = 0.5f,
                        includeContactObservation = true
                    };
                case "performance_state_trace":
                    return new RomanHeadlessScenarioSpec
                    {
                        scenarioId = "performance_state_trace",
                        randomSeed = 303,
                        ticks = ticks,
                        agentCount = 1,
                        baselineFatigue = 0.7f,
                        baselineConfidence = 0.45f,
                        applySyntheticPerformanceModifier = true
                    };
                case "squad_cohesion_trace":
                    return new RomanHeadlessScenarioSpec
                    {
                        scenarioId = "squad_cohesion_trace",
                        randomSeed = 404,
                        ticks = ticks,
                        agentCount = 4,
                        baselineFatigue = 0.45f,
                        baselineConfidence = 0.55f
                    };
                case "authority_privileged_bait":
                    return new RomanHeadlessScenarioSpec
                    {
                        scenarioId = "authority_privileged_bait",
                        randomSeed = 505,
                        ticks = ticks,
                        agentCount = 1,
                        baselineFatigue = 0.35f,
                        baselineConfidence = 0.60f,
                        includePrivilegedBaitObservation = true
                    };
                default:
                    throw new InvalidDataException("unsupported headless validation scenario: " + scenarioId);
            }
        }

        public static RomanHeadlessBatchResult RunBatch(RomanHeadlessScenarioSpec[] scenarios)
        {
            List<RomanHeadlessScenarioSummary> summaries = new List<RomanHeadlessScenarioSummary>();
            List<RomanHeadlessTickRow> rows = new List<RomanHeadlessTickRow>();
            int failedScenarios = 0;

            if (scenarios == null || scenarios.Length == 0)
            {
                return new RomanHeadlessBatchResult
                {
                    passed = false,
                    reason = "no_scenarios"
                };
            }

            for (int i = 0; i < scenarios.Length; i++)
            {
                RomanHeadlessScenarioSummary summary = RunScenario(scenarios[i], rows);
                summaries.Add(summary);

                if (!string.Equals(summary.status, "passed", StringComparison.Ordinal))
                {
                    failedScenarios++;
                }
            }

            return new RomanHeadlessBatchResult
            {
                passed = failedScenarios == 0,
                reason = failedScenarios == 0 ? "ok" : "failed_scenarios_" + failedScenarios.ToString(CultureInfo.InvariantCulture),
                scenarioSummaries = summaries.ToArray(),
                tickRows = rows.ToArray()
            };
        }

        public static string ToSummaryCsv(RomanHeadlessBatchResult result)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("scenario_id,seed,ticks,agents,total_observations,total_decisions,mutation_count,invalid_inputs,invalid_outputs,trace_only_decisions,privileged_truth_decisions,blocked_privileged_access_attempts,avg_fatigue,avg_confidence,status");

            if (result != null && result.scenarioSummaries != null)
            {
                for (int i = 0; i < result.scenarioSummaries.Length; i++)
                {
                    RomanHeadlessScenarioSummary item = result.scenarioSummaries[i];
                    builder.Append(Csv(item.scenarioId)).Append(',');
                    builder.Append(item.randomSeed.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(item.ticks.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(item.agentCount.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(item.totalObservations.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(item.totalDecisions.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(item.mutationCount.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(item.invalidInputCount.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(item.invalidOutputCount.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(item.traceOnlyDecisionCount.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(item.privilegedTruthDecisions.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(item.blockedPrivilegedAccessAttempts.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(FormatFloat(item.averageFatigue)).Append(',');
                    builder.Append(FormatFloat(item.averageConfidence)).Append(',');
                    builder.AppendLine(Csv(item.status));
                }
            }

            return builder.ToString();
        }

        public static string ToTickCsv(RomanHeadlessBatchResult result)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("scenario_id,tick,time_s,agents,observations,decisions,mutations,privileged_truth_decisions,blocked_privileged_access_attempts,avg_fatigue,avg_confidence,validation,trace_id");

            if (result != null && result.tickRows != null)
            {
                for (int i = 0; i < result.tickRows.Length; i++)
                {
                    RomanHeadlessTickRow row = result.tickRows[i];
                    builder.Append(Csv(row.scenarioId)).Append(',');
                    builder.Append(row.tickIndex.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(FormatFloat(row.timeSeconds)).Append(',');
                    builder.Append(row.agentCount.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(row.observationCount.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(row.decisionCount.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(row.mutationCount.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(row.privilegedTruthDecisions.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(row.blockedPrivilegedAccessAttempts.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(FormatFloat(row.averageFatigue)).Append(',');
                    builder.Append(FormatFloat(row.averageConfidence)).Append(',');
                    builder.Append(Csv(row.validation)).Append(',');
                    builder.AppendLine(Csv(row.traceId));
                }
            }

            return builder.ToString();
        }

        public static string ToJson(RomanHeadlessBatchResult result)
        {
            return JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            });
        }

        public static void WriteResultFiles(RomanHeadlessBatchResult result, string outputDirectory)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                return;
            }

            Directory.CreateDirectory(outputDirectory);
            File.WriteAllText(Path.Combine(outputDirectory, "scenario_summary.csv"), ToSummaryCsv(result), Encoding.UTF8);
            File.WriteAllText(Path.Combine(outputDirectory, "tick_trace.csv"), ToTickCsv(result), Encoding.UTF8);
            File.WriteAllText(Path.Combine(outputDirectory, "batch_result.json"), ToJson(result), Encoding.UTF8);
        }

        private static RomanHeadlessScenarioSummary RunScenario(
            RomanHeadlessScenarioSpec spec,
            List<RomanHeadlessTickRow> rows)
        {
            RomanScenarioConfig config = spec.ToScenarioConfig();
            RomanHeadlessScenarioSummary summary = new RomanHeadlessScenarioSummary
            {
                scenarioId = config.scenarioId,
                randomSeed = config.randomSeed,
                ticks = spec.ticks,
                agentCount = spec.agentCount
            };

            float fatigueSum = 0f;
            float confidenceSum = 0f;
            int stateSamples = 0;

            for (int tick = 0; tick < spec.ticks; tick++)
            {
                float timeSeconds = tick * config.fixedDeltaTimeSeconds;
                RomanSimulationAgentState[] agents = BuildAgents(spec, tick, timeSeconds);
                RomanWorldObservation[] observations = BuildObservations(spec, tick, timeSeconds);
                RomanSimulationTickInput input = RomanSimulationCoreApi.CreateTickInput(
                    config,
                    tick,
                    timeSeconds,
                    agents,
                    observations);
                ApplyPaperNegativeControlInputFault(spec, input);

                RomanCoreValidationResult inputValidation = RomanSimulationCoreApi.ValidateTickInput(input);
                RomanSimulationTickOutput output = string.Equals(
                        spec.paperNegativeControlFault,
                        "privileged_truth_violation",
                        StringComparison.Ordinal)
                    ? RomanSimulationCoreApi.BuildTraceOnlyPrivilegedViolationOutput(input, "privileged_truth_violation")
                    : RomanSimulationCoreApi.BuildTraceOnlyHoldOutput(input, "headless_batch");
                ApplyPaperNegativeControlOutputFault(spec, output);
                RomanCoreValidationResult outputValidation = RomanSimulationCoreApi.ValidateTickOutput(output, input);

                if (!inputValidation.isValid)
                {
                    summary.invalidInputCount++;
                }

                if (!outputValidation.isValid)
                {
                    summary.invalidOutputCount++;
                }

                int mutationCount = output.mutatedWorld ? 1 : 0;
                int traceOnlyDecisionCount = CountTraceOnlyDecisions(output);
                float tickFatigue = AverageFatigue(agents);
                float tickConfidence = AverageConfidence(agents);

                summary.totalObservations += observations.Length;
                summary.totalDecisions += output.decisions.Length;
                summary.mutationCount += mutationCount;
                summary.traceOnlyDecisionCount += traceOnlyDecisionCount;
                summary.privilegedTruthDecisions += output.privilegedTruthDecisions;
                summary.blockedPrivilegedAccessAttempts += output.blockedPrivilegedAccessAttempts;
                fatigueSum += tickFatigue;
                confidenceSum += tickConfidence;
                stateSamples++;

                if (tick == 0)
                {
                    summary.firstTraceId = output.traceId;
                }

                summary.lastTraceId = output.traceId;

                rows.Add(new RomanHeadlessTickRow
                {
                    scenarioId = config.scenarioId,
                    tickIndex = tick,
                    timeSeconds = timeSeconds,
                    agentCount = agents.Length,
                    observationCount = observations.Length,
                    decisionCount = output.decisions.Length,
                    mutationCount = mutationCount,
                    privilegedTruthDecisions = output.privilegedTruthDecisions,
                    blockedPrivilegedAccessAttempts = output.blockedPrivilegedAccessAttempts,
                    averageFatigue = tickFatigue,
                    averageConfidence = tickConfidence,
                    validation = inputValidation.isValid && outputValidation.isValid ? "ok" : inputValidation.reason + "|" + outputValidation.reason,
                    traceId = output.traceId
                });
            }

            summary.averageFatigue = stateSamples == 0 ? 0f : fatigueSum / stateSamples;
            summary.averageConfidence = stateSamples == 0 ? 0f : confidenceSum / stateSamples;
            summary.status = summary.invalidInputCount == 0
                && summary.invalidOutputCount == 0
                && summary.mutationCount == 0
                ? "passed"
                : "failed";
            return summary;
        }

        private static RomanSimulationAgentState[] BuildAgents(RomanHeadlessScenarioSpec spec, int tick, float timeSeconds)
        {
            int count = spec.agentCount <= 0 ? 1 : spec.agentCount;
            RomanSimulationAgentState[] agents = new RomanSimulationAgentState[count];

            for (int i = 0; i < count; i++)
            {
                RomanSimulationBehaviorState behavior = BuildBehavior(spec, timeSeconds);
                agents[i] = new RomanSimulationAgentState
                {
                    unitId = spec.scenarioId + "-blue-" + (i + 1).ToString(CultureInfo.InvariantCulture),
                    team = RomanTeam.BLUFOR,
                    soldierState = RomanSoldierState.Idle,
                    position = new RomanCoreVector3(i * 1.5f, 0f, tick * 0.25f),
                    forward = new RomanCoreVector3(0f, 0f, 1f),
                    health01 = 1f,
                    ammo = 30,
                    isAlive = true,
                    role = i == 0 ? "lead" : "rifle",
                    behavior = behavior
                };
            }

            return agents;
        }

        private static RomanSimulationBehaviorState BuildBehavior(RomanHeadlessScenarioSpec spec, float timeSeconds)
        {
            RomanSimulationBehaviorState behavior = RomanBehaviorStateModel.CreateBaseline(
                spec.baselineFatigue,
                spec.baselineConfidence);
            behavior.hesitation = 0.1f;
            behavior.aggression = 0.15f;
            behavior.morale = RomanCoreMath.Clamp01(0.6f + behavior.confidence * 0.2f);
            behavior.selfPreservation = 0.5f;
            behavior.cohesion = 0.65f;
            RomanBehaviorStateModel.Normalize(behavior);

            if (spec.applySyntheticPerformanceModifier)
            {
                RomanHumanPerformanceModifierProfile profile = RomanHumanPerformanceStateModel.CreateSyntheticStimulantLikeProfile();
                RomanBehaviorStateDelta delta = RomanHumanPerformanceStateModel.Evaluate(profile, timeSeconds * 60f);
                behavior = RomanHumanPerformanceStateModel.ApplyToBehavior(behavior, delta);
            }

            return behavior;
        }

        private static RomanWorldObservation[] BuildObservations(RomanHeadlessScenarioSpec spec, int tick, float timeSeconds)
        {
            if (!spec.includeContactObservation && !spec.includePrivilegedBaitObservation)
            {
                return new RomanWorldObservation[0];
            }

            if (spec.includePrivilegedBaitObservation)
            {
                return new[]
                {
                    new RomanWorldObservation
                    {
                        observationId = spec.scenarioId + "-privileged-bait-" + tick.ToString(CultureInfo.InvariantCulture),
                        observerUnitId = spec.scenarioId + "-blue-1",
                        observedAtSeconds = timeSeconds,
                        uncertainty01 = 0f,
                        source = "privileged_truth_bait",
                        lastKnownContactPosition = new RomanCoreVector3(8f, 0f, 4f + tick * 0.1f),
                        visibleUnits = new[]
                        {
                            new RomanObservedUnit
                            {
                                unitId = spec.scenarioId + "-opfor-privileged-1",
                                team = RomanTeam.OPFOR,
                                position = new RomanCoreVector3(8f, 0f, 4f + tick * 0.1f),
                                confidence01 = 1f,
                                lastSeenSeconds = timeSeconds,
                                hasLineOfSight = false,
                                authorityLevel = RomanInformationAuthorityLevel.PrivilegedTruth,
                                knowledgeSource = RomanKnowledgeSource.DebugPrivileged,
                                privilegedOnly = true
                            }
                        }
                    }
                };
            }

            return new[]
            {
                new RomanWorldObservation
                {
                    observationId = spec.scenarioId + "-obs-" + tick.ToString(CultureInfo.InvariantCulture),
                    observerUnitId = spec.scenarioId + "-blue-1",
                    observedAtSeconds = timeSeconds,
                    uncertainty01 = tick == 0 ? 0.35f : 0.2f,
                    lastKnownContactPosition = new RomanCoreVector3(5f, 0f, 3f + tick * 0.1f),
                    visibleUnits = new[]
                    {
                        new RomanObservedUnit
                        {
                            unitId = spec.scenarioId + "-opfor-1",
                            team = RomanTeam.OPFOR,
                            position = new RomanCoreVector3(5f, 0f, 3f + tick * 0.1f),
                            confidence01 = tick == 0 ? 0.65f : 0.8f,
                            lastSeenSeconds = timeSeconds,
                            hasLineOfSight = tick > 0
                        }
                    }
                }
            };
        }

        private static void ApplyPaperNegativeControlInputFault(
            RomanHeadlessScenarioSpec spec,
            RomanSimulationTickInput input)
        {
            if (spec == null
                || input == null
                || !string.Equals(spec.paperNegativeControlFault, "invalid_input_seed", StringComparison.Ordinal))
            {
                return;
            }

            input.deterministicSeed++;
        }

        private static void ApplyPaperNegativeControlOutputFault(
            RomanHeadlessScenarioSpec spec,
            RomanSimulationTickOutput output)
        {
            if (spec == null || output == null)
            {
                return;
            }

            if (string.Equals(spec.paperNegativeControlFault, "mutated_world", StringComparison.Ordinal))
            {
                output.mutatedWorld = true;
                return;
            }

            if (string.Equals(spec.paperNegativeControlFault, "adapter_apply", StringComparison.Ordinal))
            {
                if (output.decisions != null && output.decisions.Length > 0)
                {
                    output.decisions[0].adapterMayApply = true;
                }

                return;
            }

            if (string.Equals(spec.paperNegativeControlFault, "duplicate_decision", StringComparison.Ordinal))
            {
                if (output.decisions == null || output.decisions.Length == 0)
                {
                    return;
                }

                List<RomanAgentDecision> decisions = new List<RomanAgentDecision>(output.decisions);
                decisions.Add(output.decisions[0]);
                output.decisions = decisions.ToArray();
            }
        }

        private static int CountTraceOnlyDecisions(RomanSimulationTickOutput output)
        {
            if (output == null || output.decisions == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < output.decisions.Length; i++)
            {
                RomanAgentDecision decision = output.decisions[i];
                if (decision != null
                    && output.executionMode == RomanSimulationExecutionMode.TraceOnly
                    && !decision.adapterMayApply)
                {
                    count++;
                }
            }

            return count;
        }

        private static float AverageFatigue(RomanSimulationAgentState[] agents)
        {
            if (agents == null || agents.Length == 0)
            {
                return 0f;
            }

            float sum = 0f;
            int count = 0;
            for (int i = 0; i < agents.Length; i++)
            {
                if (agents[i] != null && agents[i].behavior != null)
                {
                    sum += agents[i].behavior.fatigue;
                    count++;
                }
            }

            return count == 0 ? 0f : sum / count;
        }

        private static float AverageConfidence(RomanSimulationAgentState[] agents)
        {
            if (agents == null || agents.Length == 0)
            {
                return 0f;
            }

            float sum = 0f;
            int count = 0;
            for (int i = 0; i < agents.Length; i++)
            {
                if (agents[i] != null && agents[i].behavior != null)
                {
                    sum += agents[i].behavior.confidence;
                    count++;
                }
            }

            return count == 0 ? 0f : sum / count;
        }

        private static int ResolveTicks(int overrideTicks, int defaultTicks)
        {
            return overrideTicks > 0 ? overrideTicks : defaultTicks;
        }

        private static string FormatFloat(float value)
        {
            return value.ToString("0.000", CultureInfo.InvariantCulture);
        }

        private static string Csv(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0)
            {
                return value;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
