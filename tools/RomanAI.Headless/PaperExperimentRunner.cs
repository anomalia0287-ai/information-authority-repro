using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RomanAI.Headless
{
    public sealed class RomanPaperExperimentManifest
    {
        public string format = RomanPaperExperimentRunner.ManifestFormat;
        public string setId = RomanPaperExperimentRunner.DefaultManifestSetId;
        public string setVersion = "0.1.0";
        public string description = string.Empty;
        public RomanPaperExperimentConditionSpec[] conditions = new RomanPaperExperimentConditionSpec[0];
    }

    public sealed class RomanPaperExperimentConditionSpec
    {
        public string ablationGroup = string.Empty;
        public string conditionId = string.Empty;
        public string scenarioId = string.Empty;
        public int ticks = 4;
        public int agentCount = 1;
        public float baselineFatigue = 0.45f;
        public float baselineConfidence = 0.55f;
        public bool includeContactObservation;
        public bool includePrivilegedBaitObservation;
        public bool applySyntheticPerformanceModifier;
    }

    public sealed class RomanPaperExperimentRunRow
    {
        public string experimentId = RomanPaperExperimentRunner.ExperimentId;
        public string manifestSetId = string.Empty;
        public string manifestSetVersion = string.Empty;
        public string ablationGroup = string.Empty;
        public string conditionId = string.Empty;
        public string scenarioId = string.Empty;
        public int repetitionIndex;
        public int seed;
        public int ticks;
        public int agentCount;
        public bool includeContactObservation;
        public bool applySyntheticPerformanceModifier;
        public bool passed;
        public string failureBucket = "none";
        public bool deterministicReplayMatched;
        public int mutationCount;
        public int invalidInputCount;
        public int invalidOutputCount;
        public int totalObservations;
        public int totalDecisions;
        public int traceOnlyDecisions;
        public int decisionCardinalityErrors;
        public int privilegedTruthDecisions;
        public int blockedPrivilegedAccessAttempts;
        public double observationsPerTick;
        public double traceOnlyDecisionRate;
        public double fatigueDelta;
        public double confidenceDelta;
    }

    public sealed class RomanPaperExperimentSummary
    {
        public string experimentId = RomanPaperExperimentRunner.ExperimentId;
        public string manifestSetId = string.Empty;
        public string manifestSetVersion = string.Empty;
        public string ablationGroup = string.Empty;
        public string conditionId = string.Empty;
        public int repetitions;
        public int passedRepetitions;
        public int failedRepetitions;
        public int agentCount;
        public bool includeContactObservation;
        public bool applySyntheticPerformanceModifier;
        public bool passed;
        public double passRate;
        public double determinismMatchRate;
        public double meanMutationCount;
        public double meanInvalidTickValidations;
        public double meanDecisionCardinalityErrors;
        public double meanPrivilegedTruthDecisions;
        public double sdPrivilegedTruthDecisions;
        public double ci95PrivilegedTruthDecisions;
        public double meanBlockedPrivilegedAccessAttempts;
        public double sdBlockedPrivilegedAccessAttempts;
        public double ci95BlockedPrivilegedAccessAttempts;
        public double meanObservationsPerTick;
        public double meanTraceOnlyDecisionRate;
        public double meanFatigueDelta;
        public double sdFatigueDelta;
        public double ci95FatigueDelta;
        public double meanConfidenceDelta;
        public double sdConfidenceDelta;
        public double ci95ConfidenceDelta;
    }

    public sealed class RomanPaperNegativeControlSummary
    {
        public string experimentId = RomanPaperExperimentRunner.ExperimentId;
        public string negativeControlId = string.Empty;
        public string failureBucket = string.Empty;
        public int expectedFailedRows;
        public int observedFailedRows;
        public double failClosedRate;
        public bool passed;
        public string source = "runner_level_failed_execution";
    }

    public sealed class RomanPaperFailureTaxonomyRow
    {
        public string experimentId = RomanPaperExperimentRunner.ExperimentId;
        public string failureBucket = string.Empty;
        public string source = string.Empty;
        public int observedFailures;
        public int expectedFailures;
        public bool passed;
    }

    public sealed class RomanPaperExperimentResult
    {
        public string experimentId = RomanPaperExperimentRunner.ExperimentId;
        public string runnerVersion = RomanPaperExperimentRunner.RunnerVersion;
        public string manifestFormat = RomanPaperExperimentRunner.ManifestFormat;
        public string manifestSetId = string.Empty;
        public string manifestSetVersion = string.Empty;
        public string manifestPath = string.Empty;
        public int requestedRepetitions;
        public int actualRepetitions;
        public int seedStart;
        public bool negativeControlsEnabled;
        public bool passed;
        public RomanPaperExperimentSummary[] summaries = new RomanPaperExperimentSummary[0];
        public RomanPaperExperimentRunRow[] rows = new RomanPaperExperimentRunRow[0];
        public RomanPaperExperimentRunRow[] negativeControlRows = new RomanPaperExperimentRunRow[0];
        public RomanPaperNegativeControlSummary[] negativeControls = new RomanPaperNegativeControlSummary[0];
        public RomanPaperFailureTaxonomyRow[] failureTaxonomy = new RomanPaperFailureTaxonomyRow[0];
    }

    public sealed class RomanPaperReadinessGate
    {
        public string gateId = string.Empty;
        public bool passed;
        public string status = string.Empty;
        public string evidence = string.Empty;
        public string blocker = string.Empty;
    }

    public sealed class RomanPaperArtifactBundleFile
    {
        public string path = string.Empty;
        public string role = string.Empty;
        public long bytes;
        public string sha256 = string.Empty;
    }

    public sealed class RomanPaperArtifactBundleManifest
    {
        public string format = RomanPaperExperimentRunner.ArtifactBundleManifestFormat;
        public string bundleId = RomanPaperExperimentRunner.PaperReadyBundleId;
        public string experimentId = RomanPaperExperimentRunner.ExperimentId;
        public string runnerVersion = RomanPaperExperimentRunner.RunnerVersion;
        public bool evidenceBundlePassed;
        public bool paperCompletionReady;
        public string status = string.Empty;
        public string defaultManifestSetId = string.Empty;
        public string heldOutManifestSetId = string.Empty;
        public string broaderManifestSetId = string.Empty;
        public string authorityManifestSetId = string.Empty;
        public int repetitions;
        public int defaultSeedStart;
        public int heldOutSeedStart;
        public int broaderSeedStart;
        public string[] commands = new string[0];
        public RomanPaperArtifactBundleFile[] files = new RomanPaperArtifactBundleFile[0];
        public RomanPaperReadinessGate[] readinessGates = new RomanPaperReadinessGate[0];
    }

    public sealed class RomanPaperReadinessReport
    {
        public string format = RomanPaperExperimentRunner.PaperReadinessReportFormat;
        public string bundleId = RomanPaperExperimentRunner.PaperReadyBundleId;
        public string experimentId = RomanPaperExperimentRunner.ExperimentId;
        public bool evidenceBundlePassed;
        public bool paperCompletionReady;
        public string status = string.Empty;
        public RomanPaperReadinessGate[] gates = new RomanPaperReadinessGate[0];
    }

    public sealed class RomanPaperReadyBundleResult
    {
        public string bundleId = RomanPaperExperimentRunner.PaperReadyBundleId;
        public string runnerVersion = RomanPaperExperimentRunner.RunnerVersion;
        public bool passed;
        public bool paperCompletionReady;
        public string status = string.Empty;
        public RomanPaperExperimentResult defaultExperiment = new RomanPaperExperimentResult();
        public RomanPaperExperimentResult heldOutExperiment = new RomanPaperExperimentResult();
        public RomanPaperExperimentResult broaderExperiment = new RomanPaperExperimentResult();
        public RomanPaperExperimentResult authorityExperiment = new RomanPaperExperimentResult();
        public RomanPaperReadinessGate[] readinessGates = new RomanPaperReadinessGate[0];
    }

    public static class RomanPaperExperimentRunner
    {
        public const string ExperimentId = "unity_free_paper_ablation_v0_2";
        public const string RunnerVersion = "0.2.0";
        public const string ManifestFormat = "roman_paper_experiment_manifest_v1";
        public const string ArtifactBundleManifestFormat = "roman_paper_artifact_bundle_manifest_v1";
        public const string PaperReadinessReportFormat = "roman_paper_readiness_report_v1";
        public const string PaperReadyBundleId = "unity_free_paper_ready_bundle_v0_1";
        public const string DefaultManifestSetId = "unity_free_paper_default_conditions";
        public const string AuthorityManifestSetId = "unity_free_information_authority_no_cheat";
        public const string HeldOutManifestFileName = "paper_heldout_manifest.json";
        public const string BroaderManifestFileName = "paper_broader_manifest.json";
        public const int MinimumPaperRepetitions = 30;

        private const double Epsilon = 0.000001d;

        private sealed class PaperNegativeControlSpec
        {
            public string controlId = string.Empty;
            public string failureBucket = string.Empty;
            public string fault = string.Empty;
            public int replaySeedOffset;
        }

        public static RomanPaperExperimentResult Run(int repetitions, int seedStart)
        {
            return Run(repetitions, seedStart, null, false);
        }

        public static RomanPaperExperimentResult Run(
            int repetitions,
            int seedStart,
            string manifestPath,
            bool includeNegativeControls)
        {
            RomanPaperExperimentManifest manifest = string.IsNullOrWhiteSpace(manifestPath)
                ? CreateDefaultManifest()
                : LoadManifest(manifestPath);
            string resolvedManifestPath = string.IsNullOrWhiteSpace(manifestPath)
                ? "built_in_default"
                : Path.GetFullPath(manifestPath);
            return Run(repetitions, seedStart, manifest, resolvedManifestPath, includeNegativeControls);
        }

        public static RomanPaperExperimentResult Run(
            int repetitions,
            int seedStart,
            RomanPaperExperimentManifest manifest,
            string manifestPath,
            bool includeNegativeControls)
        {
            ValidateManifest(manifest);
            int actualRepetitions = Math.Max(repetitions, MinimumPaperRepetitions);
            RomanPaperExperimentConditionSpec[] conditions = manifest.conditions;
            List<RomanPaperExperimentRunRow> rows = new List<RomanPaperExperimentRunRow>();

            for (int conditionIndex = 0; conditionIndex < conditions.Length; conditionIndex++)
            {
                RomanPaperExperimentConditionSpec condition = conditions[conditionIndex];
                for (int repetition = 0; repetition < actualRepetitions; repetition++)
                {
                    int seed = seedStart + repetition;
                    rows.Add(RunCondition(manifest, condition, repetition, seed));
                }
            }

            RomanPaperExperimentRunRow[] normalRows = rows.ToArray();
            RomanPaperExperimentRunRow[] negativeControlRows = includeNegativeControls
                ? RunNegativeControlRows(manifest, seedStart + 4000)
                : new RomanPaperExperimentRunRow[0];
            RomanPaperNegativeControlSummary[] negativeControls = includeNegativeControls
                ? BuildNegativeControlSummaries(negativeControlRows)
                : new RomanPaperNegativeControlSummary[0];
            RomanPaperFailureTaxonomyRow[] taxonomy = BuildFailureTaxonomy(normalRows, negativeControlRows, negativeControls);
            RomanPaperExperimentSummary[] summaries = BuildSummaries(manifest, conditions, normalRows);
            bool negativeControlsPassed = !includeNegativeControls || AllNegativeControlsPassed(negativeControls);
            return new RomanPaperExperimentResult
            {
                manifestSetId = manifest.setId,
                manifestSetVersion = manifest.setVersion,
                manifestPath = string.IsNullOrWhiteSpace(manifestPath) ? "inline" : manifestPath,
                requestedRepetitions = repetitions,
                actualRepetitions = actualRepetitions,
                seedStart = seedStart,
                negativeControlsEnabled = includeNegativeControls,
                rows = normalRows,
                negativeControlRows = negativeControlRows,
                summaries = summaries,
                negativeControls = negativeControls,
                failureTaxonomy = taxonomy,
                passed = AllSummariesPassed(summaries) && negativeControlsPassed && AllTaxonomyRowsPassed(taxonomy)
            };
        }

        public static RomanPaperExperimentManifest CreateDefaultManifest()
        {
            return new RomanPaperExperimentManifest
            {
                setId = DefaultManifestSetId,
                setVersion = "0.1.0",
                description = "Built-in Unity-free paper experiment conditions for matched-seed ablations.",
                conditions = new[]
                {
                    Condition("contact_ablation", "contact_off", "paper_contact_off", 4, 2, 0.40f, 0.50f, false, false),
                    Condition("contact_ablation", "contact_on", "paper_contact_on", 4, 2, 0.40f, 0.50f, true, false),
                    Condition("synthetic_modifier_ablation", "modifier_off", "paper_modifier_off", 5, 1, 0.70f, 0.45f, false, false),
                    Condition("synthetic_modifier_ablation", "modifier_on", "paper_modifier_on", 5, 1, 0.70f, 0.45f, false, true),
                    Condition("squad_size_ablation", "squad_size_1", "paper_squad_size_1", 4, 1, 0.45f, 0.55f, false, false),
                    Condition("squad_size_ablation", "squad_size_2", "paper_squad_size_2", 4, 2, 0.45f, 0.55f, false, false),
                    Condition("squad_size_ablation", "squad_size_4", "paper_squad_size_4", 4, 4, 0.45f, 0.55f, false, false),
                    Condition("squad_size_ablation", "squad_size_8", "paper_squad_size_8", 4, 8, 0.45f, 0.55f, false, false)
                }
            };
        }

        public static RomanPaperExperimentManifest CreateAuthorityNoCheatManifest()
        {
            return new RomanPaperExperimentManifest
            {
                setId = AuthorityManifestSetId,
                setVersion = "0.1.0",
                description = "Unity-free information-authority bait condition proving privileged truth is blocked from decision use.",
                conditions = new[]
                {
                    Condition(
                        "authority_no_cheat",
                        "authority_privileged_bait",
                        "authority_privileged_bait",
                        4,
                        1,
                        0.35f,
                        0.60f,
                        false,
                        false,
                        includePrivilegedBaitObservation: true)
                }
            };
        }

        public static RomanPaperExperimentManifest LoadManifest(string manifestPath)
        {
            if (string.IsNullOrWhiteSpace(manifestPath))
            {
                throw new ArgumentException("paper experiment manifest path is required", nameof(manifestPath));
            }

            if (!File.Exists(manifestPath))
            {
                throw new FileNotFoundException("paper experiment manifest was not found", manifestPath);
            }

            string json = File.ReadAllText(manifestPath, Encoding.UTF8);
            RomanPaperExperimentManifest manifest = JsonSerializer.Deserialize<RomanPaperExperimentManifest>(
                json,
                JsonOptions());
            ValidateManifest(manifest);
            return manifest;
        }

        public static string ResolveDefaultHeldOutManifestPath()
        {
            return ResolveDefaultPaperManifestPath(HeldOutManifestFileName);
        }

        public static string ResolveDefaultBroaderManifestPath()
        {
            return ResolveDefaultPaperManifestPath(BroaderManifestFileName);
        }

        private static string ResolveDefaultPaperManifestPath(string fileName)
        {
            string relativePath = Path.Combine("tools", "RomanAI.Headless", fileName);
            string[] candidates = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), relativePath),
                Path.Combine(Directory.GetCurrentDirectory(), fileName),
                Path.Combine(AppContext.BaseDirectory, fileName)
            };

            for (int i = 0; i < candidates.Length; i++)
            {
                if (File.Exists(candidates[i]))
                {
                    return Path.GetFullPath(candidates[i]);
                }
            }

            string found = FindUpwards(Directory.GetCurrentDirectory(), relativePath);
            if (!string.IsNullOrWhiteSpace(found))
            {
                return found;
            }

            found = FindUpwards(AppContext.BaseDirectory, relativePath);
            if (!string.IsNullOrWhiteSpace(found))
            {
                return found;
            }

            throw new FileNotFoundException("default paper manifest was not found", relativePath);
        }

        public static string ToSummaryCsv(RomanPaperExperimentResult result)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("experiment_id,manifest_set_id,manifest_set_version,ablation_group,condition_id,repetitions,passed_repetitions,failed_repetitions,agent_count,contact,modifier,pass_rate,determinism_match_rate,mean_mutation_count,mean_invalid_tick_validations,mean_decision_cardinality_errors,mean_privileged_truth_decisions,sd_privileged_truth_decisions,ci95_privileged_truth_decisions,mean_blocked_privileged_access_attempts,sd_blocked_privileged_access_attempts,ci95_blocked_privileged_access_attempts,mean_observations_per_tick,mean_trace_only_decision_rate,mean_fatigue_delta,sd_fatigue_delta,ci95_fatigue_delta,mean_confidence_delta,sd_confidence_delta,ci95_confidence_delta,passed");

            if (result != null && result.summaries != null)
            {
                for (int i = 0; i < result.summaries.Length; i++)
                {
                    RomanPaperExperimentSummary summary = result.summaries[i];
                    builder.Append(Csv(summary.experimentId)).Append(',');
                    builder.Append(Csv(summary.manifestSetId)).Append(',');
                    builder.Append(Csv(summary.manifestSetVersion)).Append(',');
                    builder.Append(Csv(summary.ablationGroup)).Append(',');
                    builder.Append(Csv(summary.conditionId)).Append(',');
                    builder.Append(summary.repetitions.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(summary.passedRepetitions.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(summary.failedRepetitions.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(summary.agentCount.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(summary.includeContactObservation ? "true" : "false").Append(',');
                    builder.Append(summary.applySyntheticPerformanceModifier ? "true" : "false").Append(',');
                    builder.Append(FormatDouble(summary.passRate)).Append(',');
                    builder.Append(FormatDouble(summary.determinismMatchRate)).Append(',');
                    builder.Append(FormatDouble(summary.meanMutationCount)).Append(',');
                    builder.Append(FormatDouble(summary.meanInvalidTickValidations)).Append(',');
                    builder.Append(FormatDouble(summary.meanDecisionCardinalityErrors)).Append(',');
                    builder.Append(FormatDouble(summary.meanPrivilegedTruthDecisions)).Append(',');
                    builder.Append(FormatDouble(summary.sdPrivilegedTruthDecisions)).Append(',');
                    builder.Append(FormatDouble(summary.ci95PrivilegedTruthDecisions)).Append(',');
                    builder.Append(FormatDouble(summary.meanBlockedPrivilegedAccessAttempts)).Append(',');
                    builder.Append(FormatDouble(summary.sdBlockedPrivilegedAccessAttempts)).Append(',');
                    builder.Append(FormatDouble(summary.ci95BlockedPrivilegedAccessAttempts)).Append(',');
                    builder.Append(FormatDouble(summary.meanObservationsPerTick)).Append(',');
                    builder.Append(FormatDouble(summary.meanTraceOnlyDecisionRate)).Append(',');
                    builder.Append(FormatDouble(summary.meanFatigueDelta)).Append(',');
                    builder.Append(FormatDouble(summary.sdFatigueDelta)).Append(',');
                    builder.Append(FormatDouble(summary.ci95FatigueDelta)).Append(',');
                    builder.Append(FormatDouble(summary.meanConfidenceDelta)).Append(',');
                    builder.Append(FormatDouble(summary.sdConfidenceDelta)).Append(',');
                    builder.Append(FormatDouble(summary.ci95ConfidenceDelta)).Append(',');
                    builder.AppendLine(summary.passed ? "true" : "false");
                }
            }

            return builder.ToString();
        }

        public static string ToRowsCsv(RomanPaperExperimentResult result)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("experiment_id,manifest_set_id,manifest_set_version,ablation_group,condition_id,scenario_id,repetition,seed,ticks,agents,contact,modifier,passed,failure_bucket,deterministic_replay_matched,mutations,invalid_inputs,invalid_outputs,observations,decisions,trace_only_decisions,decision_cardinality_errors,privileged_truth_decisions,blocked_privileged_access_attempts,observations_per_tick,trace_only_decision_rate,fatigue_delta,confidence_delta");

            if (result != null)
            {
                AppendRunRowsCsv(builder, result.rows);
                AppendRunRowsCsv(builder, result.negativeControlRows);
            }

            return builder.ToString();
        }

        private static void AppendRunRowsCsv(StringBuilder builder, RomanPaperExperimentRunRow[] rows)
        {
            for (int i = 0; rows != null && i < rows.Length; i++)
            {
                RomanPaperExperimentRunRow row = rows[i];
                builder.Append(Csv(row.experimentId)).Append(',');
                builder.Append(Csv(row.manifestSetId)).Append(',');
                builder.Append(Csv(row.manifestSetVersion)).Append(',');
                builder.Append(Csv(row.ablationGroup)).Append(',');
                builder.Append(Csv(row.conditionId)).Append(',');
                builder.Append(Csv(row.scenarioId)).Append(',');
                builder.Append(row.repetitionIndex.ToString(CultureInfo.InvariantCulture)).Append(',');
                builder.Append(row.seed.ToString(CultureInfo.InvariantCulture)).Append(',');
                builder.Append(row.ticks.ToString(CultureInfo.InvariantCulture)).Append(',');
                builder.Append(row.agentCount.ToString(CultureInfo.InvariantCulture)).Append(',');
                builder.Append(row.includeContactObservation ? "true" : "false").Append(',');
                builder.Append(row.applySyntheticPerformanceModifier ? "true" : "false").Append(',');
                builder.Append(row.passed ? "true" : "false").Append(',');
                builder.Append(Csv(row.failureBucket)).Append(',');
                builder.Append(row.deterministicReplayMatched ? "true" : "false").Append(',');
                builder.Append(row.mutationCount.ToString(CultureInfo.InvariantCulture)).Append(',');
                builder.Append(row.invalidInputCount.ToString(CultureInfo.InvariantCulture)).Append(',');
                builder.Append(row.invalidOutputCount.ToString(CultureInfo.InvariantCulture)).Append(',');
                builder.Append(row.totalObservations.ToString(CultureInfo.InvariantCulture)).Append(',');
                builder.Append(row.totalDecisions.ToString(CultureInfo.InvariantCulture)).Append(',');
                builder.Append(row.traceOnlyDecisions.ToString(CultureInfo.InvariantCulture)).Append(',');
                builder.Append(row.decisionCardinalityErrors.ToString(CultureInfo.InvariantCulture)).Append(',');
                builder.Append(row.privilegedTruthDecisions.ToString(CultureInfo.InvariantCulture)).Append(',');
                builder.Append(row.blockedPrivilegedAccessAttempts.ToString(CultureInfo.InvariantCulture)).Append(',');
                builder.Append(FormatDouble(row.observationsPerTick)).Append(',');
                builder.Append(FormatDouble(row.traceOnlyDecisionRate)).Append(',');
                builder.Append(FormatDouble(row.fatigueDelta)).Append(',');
                builder.AppendLine(FormatDouble(row.confidenceDelta));
            }
        }

        public static string ToNegativeControlsCsv(RomanPaperExperimentResult result)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("experiment_id,negative_control_id,failure_bucket,expected_failed_rows,observed_failed_rows,fail_closed_rate,passed,source");

            if (result != null && result.negativeControls != null)
            {
                for (int i = 0; i < result.negativeControls.Length; i++)
                {
                    RomanPaperNegativeControlSummary item = result.negativeControls[i];
                    builder.Append(Csv(item.experimentId)).Append(',');
                    builder.Append(Csv(item.negativeControlId)).Append(',');
                    builder.Append(Csv(item.failureBucket)).Append(',');
                    builder.Append(item.expectedFailedRows.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(item.observedFailedRows.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(FormatDouble(item.failClosedRate)).Append(',');
                    builder.Append(item.passed ? "true" : "false").Append(',');
                    builder.AppendLine(Csv(item.source));
                }
            }

            return builder.ToString();
        }

        public static string ToFailureTaxonomyCsv(RomanPaperExperimentResult result)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("experiment_id,failure_bucket,source,observed_failures,expected_failures,passed");

            if (result != null && result.failureTaxonomy != null)
            {
                for (int i = 0; i < result.failureTaxonomy.Length; i++)
                {
                    RomanPaperFailureTaxonomyRow row = result.failureTaxonomy[i];
                    builder.Append(Csv(row.experimentId)).Append(',');
                    builder.Append(Csv(row.failureBucket)).Append(',');
                    builder.Append(Csv(row.source)).Append(',');
                    builder.Append(row.observedFailures.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(row.expectedFailures.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.AppendLine(row.passed ? "true" : "false");
                }
            }

            return builder.ToString();
        }

        public static string ToJson(RomanPaperExperimentResult result)
        {
            return JsonSerializer.Serialize(result, JsonOptions(writeIndented: true));
        }

        public static void WriteResultFiles(RomanPaperExperimentResult result, string outputDirectory)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                return;
            }

            Directory.CreateDirectory(outputDirectory);
            File.WriteAllText(Path.Combine(outputDirectory, "paper_experiment_summary.csv"), ToSummaryCsv(result), Encoding.UTF8);
            File.WriteAllText(Path.Combine(outputDirectory, "paper_experiment_runs.csv"), ToRowsCsv(result), Encoding.UTF8);
            File.WriteAllText(Path.Combine(outputDirectory, "paper_experiment_negative_controls.csv"), ToNegativeControlsCsv(result), Encoding.UTF8);
            File.WriteAllText(Path.Combine(outputDirectory, "paper_experiment_failure_taxonomy.csv"), ToFailureTaxonomyCsv(result), Encoding.UTF8);
            File.WriteAllText(Path.Combine(outputDirectory, "paper_experiment_result.json"), ToJson(result), Encoding.UTF8);
        }

        public static RomanPaperReadyBundleResult CreatePaperReadyBundle(
            int repetitions,
            int seedStart,
            string heldOutManifestPath)
        {
            return CreatePaperReadyBundle(repetitions, seedStart, heldOutManifestPath, null);
        }

        public static RomanPaperReadyBundleResult CreatePaperReadyBundle(
            int repetitions,
            int seedStart,
            string heldOutManifestPath,
            string broaderManifestPath)
        {
            string resolvedHeldOutManifestPath = string.IsNullOrWhiteSpace(heldOutManifestPath)
                ? ResolveDefaultHeldOutManifestPath()
                : Path.GetFullPath(heldOutManifestPath);
            string resolvedBroaderManifestPath = string.IsNullOrWhiteSpace(broaderManifestPath)
                ? ResolveDefaultBroaderManifestPath()
                : Path.GetFullPath(broaderManifestPath);
            RomanPaperExperimentResult defaultExperiment = Run(repetitions, seedStart, null, false);
            RomanPaperExperimentResult heldOutExperiment = Run(
                repetitions,
                seedStart + 8000,
                resolvedHeldOutManifestPath,
                includeNegativeControls: true);
            RomanPaperExperimentResult broaderExperiment = Run(
                repetitions,
                seedStart + 16000,
                resolvedBroaderManifestPath,
                includeNegativeControls: false);
            RomanPaperExperimentResult authorityExperiment = Run(
                repetitions,
                seedStart + 24000,
                CreateAuthorityNoCheatManifest(),
                "built_in_authority_no_cheat",
                includeNegativeControls: false);
            RomanPaperReadinessGate[] gates = BuildReadinessGates(
                defaultExperiment,
                heldOutExperiment,
                broaderExperiment,
                authorityExperiment);
            bool paperCompletionReady = AllReadinessGatesPassed(gates);
            bool evidenceBundlePassed = defaultExperiment.passed
                && heldOutExperiment.passed
                && broaderExperiment.passed
                && authorityExperiment.passed;
            return new RomanPaperReadyBundleResult
            {
                defaultExperiment = defaultExperiment,
                heldOutExperiment = heldOutExperiment,
                broaderExperiment = broaderExperiment,
                authorityExperiment = authorityExperiment,
                readinessGates = gates,
                passed = evidenceBundlePassed,
                paperCompletionReady = paperCompletionReady,
                status = paperCompletionReady
                    ? "paper_completion_ready"
                    : "evidence_bundle_passed_but_paper_completion_incomplete"
            };
        }

        public static string ToPaperResultsMarkdown(RomanPaperReadyBundleResult bundle)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Paper-Ready Results Table");
            builder.AppendLine();
            builder.AppendLine("Status: evidence bundle generated; paper completion is not claimed unless every readiness gate passes.");
            builder.AppendLine();
            builder.AppendLine("## Default Ablation Results");
            AppendSummaryMarkdownTable(builder, bundle == null ? null : bundle.defaultExperiment);
            builder.AppendLine();
            builder.AppendLine("## Held-Out Results");
            AppendSummaryMarkdownTable(builder, bundle == null ? null : bundle.heldOutExperiment);
            builder.AppendLine();
            builder.AppendLine("## Broader Unseen Results");
            AppendSummaryMarkdownTable(builder, bundle == null ? null : bundle.broaderExperiment);
            builder.AppendLine();
            builder.AppendLine("## Information Authority No-Cheat Evidence");
            AppendSummaryMarkdownTable(builder, bundle == null ? null : bundle.authorityExperiment);
            builder.AppendLine();
            builder.AppendLine("## Negative Controls");
            AppendNegativeControlsMarkdownTable(builder, bundle == null ? null : bundle.heldOutExperiment);
            builder.AppendLine();
            builder.AppendLine("## Failure Taxonomy");
            AppendFailureTaxonomyMarkdownTable(builder, bundle == null ? null : bundle.heldOutExperiment);
            builder.AppendLine();
            builder.AppendLine("## Claim Boundary");
            builder.AppendLine();
            builder.AppendLine("These results support Unity-free systems/reproducibility claims only: deterministic trace-only execution, manifest-driven held-out and broader unseen conditions, runner-level fail-closed negative controls, non-mutating safety counters inside the covered synthetic scenario families, and architecture-level blocking of privileged-truth use in the information-authority bait condition.");
            builder.AppendLine();
            builder.AppendLine("These results do not support tactical superiority, human-realistic behavior validity, live movement/combat approval, medical validity, targeting validity, or production/default learned policy control.");
            return builder.ToString();
        }

        public static string ToPaperLimitationsMarkdown(RomanPaperReadyBundleResult bundle)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Paper Limitations");
            builder.AppendLine();
            builder.AppendLine("This section constrains the paper track to the evidence generated by the Unity-free core runner.");
            builder.AppendLine();
            builder.AppendLine("## Supported Claims");
            builder.AppendLine();
            builder.AppendLine("- The core simulation contract can run without Unity.");
            builder.AppendLine("- Covered default, held-out, and broader unseen synthetic scenarios produce deterministic, trace-only, non-mutating outputs.");
            builder.AppendLine("- Broader unseen scenarios cover combined contact/modifier, long-horizon modifier, and dense squad contact families.");
            builder.AppendLine("- Contact, synthetic modifier, and squad-size ablations have repeated-run summary statistics and 95 percent confidence intervals.");
            builder.AppendLine("- Runner-level negative controls fail closed for safety, contract, and determinism buckets.");
            builder.AppendLine("- The information-authority bait condition reports zero privileged-truth decisions and positive blocked privileged-access attempts in the Unity-free runner.");
            builder.AppendLine("- The privileged-truth violation negative control fails closed in a distinct authority_failure bucket.");
            builder.AppendLine();
            builder.AppendLine("The no-cheat result is architecture-enforcement evidence only: it shows that privileged-truth records are blocked at the decision boundary in this reproducible Unity-free audit path.");
            builder.AppendLine();
            builder.AppendLine("## Not Supported Claims");
            builder.AppendLine();
            builder.AppendLine("- Human-realistic behavior validity.");
            builder.AppendLine("- Tactical superiority over another system or baseline.");
            builder.AppendLine("- Real-world operational, targeting, live-fire, surveillance, or weapon-control validity.");
            builder.AppendLine("- Medical diagnosis, treatment, dosing, impairment prediction, or clinical decision support.");
            builder.AppendLine("- Learned movement, combat, objective_move, or medical_retreat live action approval.");
            builder.AppendLine("- Production/default model action application beyond separately approved hold_noop_only no-op scope.");
            builder.AppendLine();
            builder.AppendLine("## Remaining Paper Blockers");
            builder.AppendLine();
            AppendReadinessMarkdownList(builder, bundle);
            return builder.ToString();
        }

        public static string ToReadinessCsv(RomanPaperReadyBundleResult bundle)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("gate_id,passed,status,evidence,blocker");
            RomanPaperReadinessGate[] gates = bundle == null ? null : bundle.readinessGates;
            for (int i = 0; gates != null && i < gates.Length; i++)
            {
                RomanPaperReadinessGate gate = gates[i];
                builder.Append(Csv(gate.gateId)).Append(',');
                builder.Append(gate.passed ? "true" : "false").Append(',');
                builder.Append(Csv(gate.status)).Append(',');
                builder.Append(Csv(gate.evidence)).Append(',');
                builder.AppendLine(Csv(gate.blocker));
            }

            return builder.ToString();
        }

        public static string ToReadinessJson(RomanPaperReadyBundleResult bundle)
        {
            RomanPaperReadinessReport report = new RomanPaperReadinessReport
            {
                evidenceBundlePassed = bundle != null && bundle.passed,
                paperCompletionReady = bundle != null && bundle.paperCompletionReady,
                status = bundle == null ? "missing_bundle" : bundle.status,
                gates = bundle == null ? new RomanPaperReadinessGate[0] : bundle.readinessGates
            };
            return JsonSerializer.Serialize(report, JsonOptions(writeIndented: true));
        }

        public static string ToPaperReadyBundleJson(RomanPaperReadyBundleResult bundle)
        {
            return JsonSerializer.Serialize(bundle, JsonOptions(writeIndented: true));
        }

        public static void WritePaperReadyBundle(RomanPaperReadyBundleResult bundle, string outputDirectory)
        {
            if (bundle == null || string.IsNullOrWhiteSpace(outputDirectory))
            {
                return;
            }

            Directory.CreateDirectory(outputDirectory);
            string defaultDirectory = Path.Combine(outputDirectory, "default_experiment");
            string heldOutDirectory = Path.Combine(outputDirectory, "heldout_negative_experiment");
            string broaderDirectory = Path.Combine(outputDirectory, "broader_unseen_experiment");
            string authorityDirectory = Path.Combine(outputDirectory, "authority_no_cheat_experiment");
            WriteResultFiles(bundle.defaultExperiment, defaultDirectory);
            WriteResultFiles(bundle.heldOutExperiment, heldOutDirectory);
            WriteResultFiles(bundle.broaderExperiment, broaderDirectory);
            WriteResultFiles(bundle.authorityExperiment, authorityDirectory);

            WriteBundleText(outputDirectory, "paper_results_table.md", ToPaperResultsMarkdown(bundle));
            WriteBundleText(outputDirectory, "paper_limitations.md", ToPaperLimitationsMarkdown(bundle));
            WriteBundleText(outputDirectory, "paper_readiness_report.csv", ToReadinessCsv(bundle));
            WriteBundleText(outputDirectory, "paper_readiness_report.json", ToReadinessJson(bundle));

            string[] files = new[]
            {
                "paper_results_table.md",
                "paper_limitations.md",
                "paper_readiness_report.csv",
                "paper_readiness_report.json",
                Path.Combine("default_experiment", "paper_experiment_summary.csv"),
                Path.Combine("default_experiment", "paper_experiment_runs.csv"),
                Path.Combine("default_experiment", "paper_experiment_negative_controls.csv"),
                Path.Combine("default_experiment", "paper_experiment_failure_taxonomy.csv"),
                Path.Combine("default_experiment", "paper_experiment_result.json"),
                Path.Combine("heldout_negative_experiment", "paper_experiment_summary.csv"),
                Path.Combine("heldout_negative_experiment", "paper_experiment_runs.csv"),
                Path.Combine("heldout_negative_experiment", "paper_experiment_negative_controls.csv"),
                Path.Combine("heldout_negative_experiment", "paper_experiment_failure_taxonomy.csv"),
                Path.Combine("heldout_negative_experiment", "paper_experiment_result.json"),
                Path.Combine("broader_unseen_experiment", "paper_experiment_summary.csv"),
                Path.Combine("broader_unseen_experiment", "paper_experiment_runs.csv"),
                Path.Combine("broader_unseen_experiment", "paper_experiment_negative_controls.csv"),
                Path.Combine("broader_unseen_experiment", "paper_experiment_failure_taxonomy.csv"),
                Path.Combine("broader_unseen_experiment", "paper_experiment_result.json"),
                Path.Combine("authority_no_cheat_experiment", "paper_experiment_summary.csv"),
                Path.Combine("authority_no_cheat_experiment", "paper_experiment_runs.csv"),
                Path.Combine("authority_no_cheat_experiment", "paper_experiment_negative_controls.csv"),
                Path.Combine("authority_no_cheat_experiment", "paper_experiment_failure_taxonomy.csv"),
                Path.Combine("authority_no_cheat_experiment", "paper_experiment_result.json")
            };

            RomanPaperArtifactBundleManifest manifest = BuildArtifactBundleManifest(bundle, outputDirectory, files);
            File.WriteAllText(
                Path.Combine(outputDirectory, "paper_artifact_bundle_manifest.json"),
                JsonSerializer.Serialize(manifest, JsonOptions(writeIndented: true)),
                Encoding.UTF8);
        }

        private static RomanPaperReadinessGate[] BuildReadinessGates(
            RomanPaperExperimentResult defaultExperiment,
            RomanPaperExperimentResult heldOutExperiment,
            RomanPaperExperimentResult broaderExperiment,
            RomanPaperExperimentResult authorityExperiment)
        {
            bool researchQuestionPresent = RepositoryFileContains(
                Path.Combine("docs", "paper_track_research_protocol.md"),
                "Research Question");
            bool reproductionScriptPresent = RepositoryFileContains(
                Path.Combine("Romana", "Tools", "Test-PaperReadyBundle.ps1"),
                "paper_ready_bundle");
            bool narrativeDraftPresent = RepositoryFileContains(
                Path.Combine("docs", "paper_track_systems_reproducibility_draft.md"),
                "Claim Boundary");
            List<RomanPaperReadinessGate> gates = new List<RomanPaperReadinessGate>();
            gates.Add(Gate(
                "research_question_fixed",
                researchQuestionPresent,
                "present",
                "docs/paper_track_research_protocol.md records the systems/reproducibility question.",
                "Research protocol must exist and include the fixed research question."));
            gates.Add(Gate(
                "baseline_ablation_matrix_implemented",
                defaultExperiment != null && defaultExperiment.passed && defaultExperiment.summaries.Length >= 8,
                "implemented",
                "Default experiment covers contact, synthetic modifier, and squad-size ablations.",
                "Default ablation matrix must pass."));
            gates.Add(Gate(
                "held_out_manifest_implemented",
                heldOutExperiment != null
                    && heldOutExperiment.passed
                    && string.Equals(heldOutExperiment.manifestSetId, "unity_free_paper_heldout_conditions", StringComparison.Ordinal),
                "implemented",
                "tools/RomanAI.Headless/paper_heldout_manifest.json drives held-out conditions.",
                "Held-out manifest must exist and pass."));
            gates.Add(Gate(
                "statistical_repetition_runner",
                defaultExperiment != null
                    && heldOutExperiment != null
                    && defaultExperiment.actualRepetitions >= MinimumPaperRepetitions
                    && heldOutExperiment.actualRepetitions >= MinimumPaperRepetitions,
                "implemented",
                "Runner enforces N >= 30 repetitions per condition.",
                "Repetitions must be at least 30."));
            gates.Add(Gate(
                "confidence_intervals_reported",
                HasConfidenceIntervals(defaultExperiment) && HasConfidenceIntervals(heldOutExperiment),
                "implemented",
                "paper_experiment_summary.csv includes mean, SD, and ci95 columns.",
                "Summary statistics must include confidence intervals."));
            gates.Add(Gate(
                "negative_controls_fail_closed",
                heldOutExperiment != null
                    && heldOutExperiment.negativeControlsEnabled
                    && heldOutExperiment.negativeControlRows.Length >= 5
                    && heldOutExperiment.negativeControls.Length >= 5
                    && AllNegativeControlsPassed(heldOutExperiment.negativeControls),
                "implemented",
                "Held-out bundle runs runner-level failed executions for safety, contract, and determinism negative controls.",
                "Negative controls must fail closed."));
            gates.Add(Gate(
                "failure_taxonomy_populated",
                heldOutExperiment != null
                    && heldOutExperiment.failureTaxonomy.Length >= 10
                    && AllTaxonomyRowsPassed(heldOutExperiment.failureTaxonomy),
                "implemented",
                "Failure taxonomy records normal passing rows and runner-level negative-control failed rows.",
                "Failure taxonomy must be populated and passing."));
            gates.Add(Gate(
                "no_cheat_invariant_evidence",
                AuthorityNoCheatEvidencePassed(authorityExperiment)
                    && AuthorityNegativeControlPassed(heldOutExperiment),
                "implemented",
                "Authority bait runs report privileged_truth_decisions=0 with blocked_privileged_access_attempts>0, and the privileged_truth_violation control fails closed.",
                "Unity-free authority bait evidence and negative control must pass."));
            gates.Add(Gate(
                "artifact_bundle_generated",
                defaultExperiment != null
                    && heldOutExperiment != null
                    && authorityExperiment != null
                    && defaultExperiment.passed
                    && heldOutExperiment.passed
                    && authorityExperiment.passed,
                "implemented",
                "Bundle writer emits result table, limitation section, readiness report, manifest, and raw CSV/JSON outputs.",
                "Artifact bundle must be generated from passing experiments."));
            gates.Add(Gate(
                "limitations_section_present",
                true,
                "implemented",
                "paper_limitations.md is generated with supported and unsupported claim boundaries.",
                string.Empty));
            gates.Add(Gate(
                "broader_unseen_scenario_families",
                broaderExperiment != null
                    && broaderExperiment.passed
                    && string.Equals(broaderExperiment.manifestSetId, "unity_free_paper_broader_unseen_conditions", StringComparison.Ordinal)
                    && broaderExperiment.summaries.Length >= 6,
                "implemented",
                "tools/RomanAI.Headless/paper_broader_manifest.json adds combined contact/modifier, long-horizon modifier, and dense squad contact families.",
                "Add broader unseen scenario families before claiming paper completion."));
            gates.Add(Gate(
                "clean_machine_reproduction_script",
                reproductionScriptPresent,
                "implemented",
                "Romana/Tools/Test-PaperReadyBundle.ps1 runs the Unity-free paper bundle verification path and writes a verification report.",
                "Paper bundle verification script must exist and call the paper-ready bundle path."));
            gates.Add(Gate(
                "paper_narrative_draft_present",
                narrativeDraftPresent,
                "implemented",
                "docs/paper_track_systems_reproducibility_draft.md records the draft claim matrix, method, results, limitations, and review checklist.",
                "Paper narrative draft must exist and include the claim matrix."));
            gates.Add(Gate(
                "independent_clean_machine_reproduction",
                false,
                "blocked",
                "Current evidence is local to this workspace.",
                "Run the bundle from a clean checkout on an independent machine and record the reproduction log."));
            gates.Add(Gate(
                "paper_narrative_external_review",
                false,
                "blocked",
                "Result tables and limitations are generated, but no final paper narrative review has been completed.",
                "Write and review the paper narrative against the claim boundary."));
            return gates.ToArray();
        }

        private static RomanPaperReadinessGate Gate(
            string gateId,
            bool passed,
            string status,
            string evidence,
            string blocker)
        {
            return new RomanPaperReadinessGate
            {
                gateId = gateId,
                passed = passed,
                status = passed ? status : "blocked",
                evidence = evidence,
                blocker = passed ? string.Empty : blocker
            };
        }

        private static bool HasConfidenceIntervals(RomanPaperExperimentResult result)
        {
            if (result == null || result.summaries == null || result.summaries.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < result.summaries.Length; i++)
            {
                RomanPaperExperimentSummary summary = result.summaries[i];
                if (summary == null || summary.repetitions < MinimumPaperRepetitions)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool AuthorityNoCheatEvidencePassed(RomanPaperExperimentResult result)
        {
            if (result == null
                || !result.passed
                || !string.Equals(result.manifestSetId, AuthorityManifestSetId, StringComparison.Ordinal)
                || result.summaries == null
                || result.summaries.Length != 1)
            {
                return false;
            }

            RomanPaperExperimentSummary summary = result.summaries[0];
            return summary != null
                && string.Equals(summary.conditionId, "authority_privileged_bait", StringComparison.Ordinal)
                && Math.Abs(summary.meanPrivilegedTruthDecisions) < Epsilon
                && summary.meanBlockedPrivilegedAccessAttempts > Epsilon;
        }

        private static bool AuthorityNegativeControlPassed(RomanPaperExperimentResult result)
        {
            if (result == null || result.negativeControls == null || result.negativeControlRows == null)
            {
                return false;
            }

            RomanPaperExperimentRunRow row = FindNegativeControlRow(
                result.negativeControlRows,
                "privileged_truth_violation");
            if (row == null
                || row.passed
                || row.privilegedTruthDecisions <= 0
                || !string.Equals(row.failureBucket, "authority_failure", StringComparison.Ordinal))
            {
                return false;
            }

            for (int i = 0; i < result.negativeControls.Length; i++)
            {
                RomanPaperNegativeControlSummary control = result.negativeControls[i];
                if (control != null
                    && string.Equals(control.negativeControlId, "privileged_truth_violation", StringComparison.Ordinal)
                    && string.Equals(control.failureBucket, "authority_failure", StringComparison.Ordinal)
                    && control.passed
                    && control.observedFailedRows == 1)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool AllReadinessGatesPassed(RomanPaperReadinessGate[] gates)
        {
            if (gates == null || gates.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < gates.Length; i++)
            {
                if (gates[i] == null || !gates[i].passed)
                {
                    return false;
                }
            }

            return true;
        }

        private static void AppendSummaryMarkdownTable(StringBuilder builder, RomanPaperExperimentResult result)
        {
            builder.AppendLine();
            builder.AppendLine("| Manifest | Condition | N | Pass Rate | Determinism | Mutations | Invalid Ticks | Cardinality Errors | Privileged Decisions | Blocked Privileged Attempts | Trace-Only Rate | Fatigue Delta Mean +/- CI95 | Confidence Delta Mean +/- CI95 | Passed |");
            builder.AppendLine("| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |");
            RomanPaperExperimentSummary[] summaries = result == null ? null : result.summaries;
            for (int i = 0; summaries != null && i < summaries.Length; i++)
            {
                RomanPaperExperimentSummary summary = summaries[i];
                builder.Append("| ")
                    .Append(summary.manifestSetId)
                    .Append(" | ")
                    .Append(summary.conditionId)
                    .Append(" | ")
                    .Append(summary.repetitions.ToString(CultureInfo.InvariantCulture))
                    .Append(" | ")
                    .Append(FormatDouble(summary.passRate))
                    .Append(" | ")
                    .Append(FormatDouble(summary.determinismMatchRate))
                    .Append(" | ")
                    .Append(FormatDouble(summary.meanMutationCount))
                    .Append(" | ")
                    .Append(FormatDouble(summary.meanInvalidTickValidations))
                    .Append(" | ")
                    .Append(FormatDouble(summary.meanDecisionCardinalityErrors))
                    .Append(" | ")
                    .Append(FormatDouble(summary.meanPrivilegedTruthDecisions))
                    .Append(" | ")
                    .Append(FormatDouble(summary.meanBlockedPrivilegedAccessAttempts))
                    .Append(" | ")
                    .Append(FormatDouble(summary.meanTraceOnlyDecisionRate))
                    .Append(" | ")
                    .Append(FormatDouble(summary.meanFatigueDelta))
                    .Append(" +/- ")
                    .Append(FormatDouble(summary.ci95FatigueDelta))
                    .Append(" | ")
                    .Append(FormatDouble(summary.meanConfidenceDelta))
                    .Append(" +/- ")
                    .Append(FormatDouble(summary.ci95ConfidenceDelta))
                    .Append(" | ")
                    .Append(summary.passed ? "true" : "false")
                    .AppendLine(" |");
            }
        }

        private static void AppendNegativeControlsMarkdownTable(StringBuilder builder, RomanPaperExperimentResult result)
        {
            builder.AppendLine();
            builder.AppendLine("| Control | Bucket | Expected Failed Rows | Observed Failed Rows | Fail-Closed Rate | Passed |");
            builder.AppendLine("| --- | --- | ---: | ---: | ---: | --- |");
            RomanPaperNegativeControlSummary[] controls = result == null ? null : result.negativeControls;
            for (int i = 0; controls != null && i < controls.Length; i++)
            {
                RomanPaperNegativeControlSummary control = controls[i];
                builder.Append("| ")
                    .Append(control.negativeControlId)
                    .Append(" | ")
                    .Append(control.failureBucket)
                    .Append(" | ")
                    .Append(control.expectedFailedRows.ToString(CultureInfo.InvariantCulture))
                    .Append(" | ")
                    .Append(control.observedFailedRows.ToString(CultureInfo.InvariantCulture))
                    .Append(" | ")
                    .Append(FormatDouble(control.failClosedRate))
                    .Append(" | ")
                    .Append(control.passed ? "true" : "false")
                    .AppendLine(" |");
            }
        }

        private static void AppendFailureTaxonomyMarkdownTable(StringBuilder builder, RomanPaperExperimentResult result)
        {
            builder.AppendLine();
            builder.AppendLine("| Bucket | Source | Observed Failures | Expected Failures | Passed |");
            builder.AppendLine("| --- | --- | ---: | ---: | --- |");
            RomanPaperFailureTaxonomyRow[] rows = result == null ? null : result.failureTaxonomy;
            for (int i = 0; rows != null && i < rows.Length; i++)
            {
                RomanPaperFailureTaxonomyRow row = rows[i];
                builder.Append("| ")
                    .Append(row.failureBucket)
                    .Append(" | ")
                    .Append(row.source)
                    .Append(" | ")
                    .Append(row.observedFailures.ToString(CultureInfo.InvariantCulture))
                    .Append(" | ")
                    .Append(row.expectedFailures.ToString(CultureInfo.InvariantCulture))
                    .Append(" | ")
                    .Append(row.passed ? "true" : "false")
                    .AppendLine(" |");
            }
        }

        private static void AppendReadinessMarkdownList(StringBuilder builder, RomanPaperReadyBundleResult bundle)
        {
            RomanPaperReadinessGate[] gates = bundle == null ? null : bundle.readinessGates;
            for (int i = 0; gates != null && i < gates.Length; i++)
            {
                RomanPaperReadinessGate gate = gates[i];
                if (gate.passed)
                {
                    continue;
                }

                builder.Append("- ")
                    .Append(gate.gateId)
                    .Append(": ")
                    .Append(gate.blocker)
                    .AppendLine();
            }
        }

        private static void WriteBundleText(string outputDirectory, string fileName, string content)
        {
            File.WriteAllText(Path.Combine(outputDirectory, fileName), content, Encoding.UTF8);
        }

        private static RomanPaperArtifactBundleManifest BuildArtifactBundleManifest(
            RomanPaperReadyBundleResult bundle,
            string outputDirectory,
            string[] relativePaths)
        {
            List<RomanPaperArtifactBundleFile> files = new List<RomanPaperArtifactBundleFile>();
            for (int i = 0; relativePaths != null && i < relativePaths.Length; i++)
            {
                string relativePath = relativePaths[i];
                string fullPath = Path.Combine(outputDirectory, relativePath);
                FileInfo info = new FileInfo(fullPath);
                files.Add(new RomanPaperArtifactBundleFile
                {
                    path = relativePath.Replace('\\', '/'),
                    role = ResolveArtifactRole(relativePath),
                    bytes = info.Exists ? info.Length : 0L,
                    sha256 = info.Exists ? ComputeSha256(fullPath) : string.Empty
                });
            }

            return new RomanPaperArtifactBundleManifest
            {
                evidenceBundlePassed = bundle.passed,
                paperCompletionReady = bundle.paperCompletionReady,
                status = bundle.status,
                defaultManifestSetId = bundle.defaultExperiment.manifestSetId,
                heldOutManifestSetId = bundle.heldOutExperiment.manifestSetId,
                broaderManifestSetId = bundle.broaderExperiment.manifestSetId,
                authorityManifestSetId = bundle.authorityExperiment.manifestSetId,
                repetitions = bundle.defaultExperiment.actualRepetitions,
                defaultSeedStart = bundle.defaultExperiment.seedStart,
                heldOutSeedStart = bundle.heldOutExperiment.seedStart,
                broaderSeedStart = bundle.broaderExperiment.seedStart,
                commands = new[]
                {
                    BuildPaperReadyBundleCommand(bundle, outputDirectory),
                    BuildPaperExperimentCommand(bundle.defaultExperiment, Path.Combine(outputDirectory, "default_experiment"), string.Empty, false),
                    BuildPaperExperimentCommand(bundle.heldOutExperiment, Path.Combine(outputDirectory, "heldout_negative_experiment"), "--paper-heldout-manifest", true),
                    BuildPaperExperimentCommand(bundle.broaderExperiment, Path.Combine(outputDirectory, "broader_unseen_experiment"), "--paper-broader-manifest", false)
                },
                files = files.ToArray(),
                readinessGates = bundle.readinessGates
            };
        }

        private static string BuildPaperReadyBundleCommand(
            RomanPaperReadyBundleResult bundle,
            string outputDirectory)
        {
            List<string> args = BaseDotnetRunArgs();
            args.Add("--paper-ready-bundle");
            args.Add("--paper-repetitions");
            args.Add(bundle.defaultExperiment.actualRepetitions.ToString(CultureInfo.InvariantCulture));
            args.Add("--paper-seed-start");
            args.Add(bundle.defaultExperiment.seedStart.ToString(CultureInfo.InvariantCulture));
            AddManifestArgument(args, "--paper-heldout-manifest", bundle.heldOutExperiment.manifestPath);
            AddManifestArgument(args, "--paper-broader-manifest", bundle.broaderExperiment.manifestPath);
            args.Add("--out");
            args.Add(outputDirectory);
            return JoinCommand(args);
        }

        private static string BuildPaperExperimentCommand(
            RomanPaperExperimentResult result,
            string outputDirectory,
            string manifestArgumentName,
            bool includeNegativeControls)
        {
            List<string> args = BaseDotnetRunArgs();
            args.Add("--paper-experiment");
            args.Add("--paper-repetitions");
            args.Add(result.actualRepetitions.ToString(CultureInfo.InvariantCulture));
            args.Add("--paper-seed-start");
            args.Add(result.seedStart.ToString(CultureInfo.InvariantCulture));
            AddManifestArgument(args, manifestArgumentName, result.manifestPath);
            if (includeNegativeControls)
            {
                args.Add("--paper-negative-controls");
            }

            args.Add("--out");
            args.Add(outputDirectory);
            return JoinCommand(args);
        }

        private static List<string> BaseDotnetRunArgs()
        {
            return new List<string>
            {
                "dotnet",
                "run",
                "--project",
                "tools\\RomanAI.Headless\\RomanAI.Headless.csproj",
                "--configfile",
                "NuGet.Config",
                "--no-build",
                "--"
            };
        }

        private static void AddManifestArgument(List<string> args, string argumentName, string manifestPath)
        {
            if (string.IsNullOrWhiteSpace(argumentName)
                || string.IsNullOrWhiteSpace(manifestPath)
                || string.Equals(manifestPath, "built_in_default", StringComparison.Ordinal)
                || string.Equals(manifestPath, "inline", StringComparison.Ordinal))
            {
                return;
            }

            args.Add(argumentName);
            args.Add(manifestPath);
        }

        private static string JoinCommand(List<string> args)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; args != null && i < args.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(CommandArgument(args[i]));
            }

            return builder.ToString();
        }

        private static string CommandArgument(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "\"\"";
            }

            if (value.IndexOfAny(new[] { ' ', '\t', '\r', '\n', '"' }) < 0)
            {
                return value;
            }

            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }

        private static string ResolveArtifactRole(string relativePath)
        {
            string normalized = relativePath.Replace('\\', '/');
            if (normalized.EndsWith("paper_results_table.md", StringComparison.Ordinal))
            {
                return "paper_ready_results_table";
            }

            if (normalized.EndsWith("paper_limitations.md", StringComparison.Ordinal))
            {
                return "limitations_section";
            }

            if (normalized.EndsWith("paper_readiness_report.csv", StringComparison.Ordinal)
                || normalized.EndsWith("paper_readiness_report.json", StringComparison.Ordinal))
            {
                return "paper_readiness_gate_report";
            }

            if (normalized.Contains("default_experiment", StringComparison.Ordinal))
            {
                return "default_ablation_evidence";
            }

            if (normalized.Contains("heldout_negative_experiment", StringComparison.Ordinal))
            {
                return "heldout_and_negative_control_evidence";
            }

            if (normalized.Contains("broader_unseen_experiment", StringComparison.Ordinal))
            {
                return "broader_unseen_family_evidence";
            }

            if (normalized.Contains("authority_no_cheat_experiment", StringComparison.Ordinal))
            {
                return "information_authority_no_cheat_evidence";
            }

            return "paper_bundle_evidence";
        }

        private static string ComputeSha256(string path)
        {
            using SHA256 sha256 = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            byte[] hash = sha256.ComputeHash(stream);
            StringBuilder builder = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("x2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        private static RomanPaperExperimentRunRow RunCondition(
            RomanPaperExperimentManifest manifest,
            RomanPaperExperimentConditionSpec condition,
            int repetitionIndex,
            int seed,
            string paperNegativeControlFault = "",
            int replaySeedOffset = 0,
            string ablationGroupOverride = "",
            string conditionIdOverride = "")
        {
            RomanHeadlessScenarioSpec spec = BuildScenario(condition, seed);
            spec.paperNegativeControlFault = paperNegativeControlFault;
            if (string.Equals(paperNegativeControlFault, "privileged_truth_violation", StringComparison.Ordinal))
            {
                spec.includePrivilegedBaitObservation = true;
            }

            RomanHeadlessBatchResult batch = RomanHeadlessScenarioRunner.RunBatch(new[] { spec });
            RomanHeadlessScenarioSpec replaySpec = BuildScenario(condition, seed + replaySeedOffset);
            replaySpec.paperNegativeControlFault = paperNegativeControlFault;
            if (string.Equals(paperNegativeControlFault, "privileged_truth_violation", StringComparison.Ordinal))
            {
                replaySpec.includePrivilegedBaitObservation = true;
            }

            RomanHeadlessBatchResult replay = RomanHeadlessScenarioRunner.RunBatch(new[] { replaySpec });
            RomanHeadlessScenarioSummary summary = batch.scenarioSummaries.Length > 0 ? batch.scenarioSummaries[0] : null;
            RomanHeadlessTickRow firstTick = FindFirstTick(batch);
            RomanHeadlessTickRow lastTick = FindLastTick(batch);

            int mutationCount = summary == null ? 1 : summary.mutationCount;
            int invalidInputCount = summary == null ? 1 : summary.invalidInputCount;
            int invalidOutputCount = summary == null ? 1 : summary.invalidOutputCount;
            int totalDecisions = summary == null ? 0 : summary.totalDecisions;
            int traceOnlyDecisions = summary == null ? 0 : summary.traceOnlyDecisionCount;
            int totalObservations = summary == null ? 0 : summary.totalObservations;
            int privilegedTruthDecisions = summary == null ? 1 : summary.privilegedTruthDecisions;
            int blockedPrivilegedAccessAttempts = summary == null ? 0 : summary.blockedPrivilegedAccessAttempts;
            int cardinalityErrors = CountDecisionCardinalityErrors(batch, condition.agentCount);
            bool deterministicReplayMatched = string.Equals(
                    RomanHeadlessScenarioRunner.ToSummaryCsv(batch),
                    RomanHeadlessScenarioRunner.ToSummaryCsv(replay),
                    StringComparison.Ordinal)
                && string.Equals(
                    RomanHeadlessScenarioRunner.ToTickCsv(batch),
                    RomanHeadlessScenarioRunner.ToTickCsv(replay),
                    StringComparison.Ordinal);

            double fatigueDelta = firstTick == null || lastTick == null ? 0d : lastTick.averageFatigue - firstTick.averageFatigue;
            double confidenceDelta = firstTick == null || lastTick == null ? 0d : lastTick.averageConfidence - firstTick.averageConfidence;
            double observationsPerTick = summary == null || summary.ticks <= 0 ? 0d : (double)totalObservations / summary.ticks;
            double traceOnlyDecisionRate = totalDecisions <= 0 ? 0d : (double)traceOnlyDecisions / totalDecisions;
            bool passed = batch.passed
                && deterministicReplayMatched
                && mutationCount == 0
                && invalidInputCount == 0
                && invalidOutputCount == 0
                && cardinalityErrors == 0
                && privilegedTruthDecisions == 0
                && Math.Abs(traceOnlyDecisionRate - 1d) < Epsilon
                && ConditionSpecificCheck(
                    condition,
                    observationsPerTick,
                    fatigueDelta,
                    confidenceDelta,
                    privilegedTruthDecisions,
                    blockedPrivilegedAccessAttempts);

            RomanPaperExperimentRunRow row = new RomanPaperExperimentRunRow
            {
                manifestSetId = manifest.setId,
                manifestSetVersion = manifest.setVersion,
                ablationGroup = string.IsNullOrWhiteSpace(ablationGroupOverride) ? condition.ablationGroup : ablationGroupOverride,
                conditionId = string.IsNullOrWhiteSpace(conditionIdOverride) ? condition.conditionId : conditionIdOverride,
                scenarioId = condition.scenarioId,
                repetitionIndex = repetitionIndex,
                seed = seed,
                ticks = condition.ticks,
                agentCount = condition.agentCount,
                includeContactObservation = condition.includeContactObservation,
                applySyntheticPerformanceModifier = condition.applySyntheticPerformanceModifier,
                passed = passed,
                deterministicReplayMatched = deterministicReplayMatched,
                mutationCount = mutationCount,
                invalidInputCount = invalidInputCount,
                invalidOutputCount = invalidOutputCount,
                totalObservations = totalObservations,
                totalDecisions = totalDecisions,
                traceOnlyDecisions = traceOnlyDecisions,
                decisionCardinalityErrors = cardinalityErrors,
                privilegedTruthDecisions = privilegedTruthDecisions,
                blockedPrivilegedAccessAttempts = blockedPrivilegedAccessAttempts,
                observationsPerTick = observationsPerTick,
                traceOnlyDecisionRate = traceOnlyDecisionRate,
                fatigueDelta = fatigueDelta,
                confidenceDelta = confidenceDelta
            };
            row.failureBucket = ClassifyFailure(row);
            return row;
        }

        private static RomanHeadlessScenarioSpec BuildScenario(RomanPaperExperimentConditionSpec condition, int seed)
        {
            return new RomanHeadlessScenarioSpec
            {
                scenarioId = condition.scenarioId,
                randomSeed = seed,
                ticks = condition.ticks,
                agentCount = condition.agentCount,
                baselineFatigue = Clamp01(condition.baselineFatigue + SeedJitter(seed, 17, 0.04f)),
                baselineConfidence = Clamp01(condition.baselineConfidence + SeedJitter(seed, 31, 0.04f)),
                includeContactObservation = condition.includeContactObservation,
                includePrivilegedBaitObservation = condition.includePrivilegedBaitObservation,
                applySyntheticPerformanceModifier = condition.applySyntheticPerformanceModifier
            };
        }

        private static RomanPaperExperimentSummary[] BuildSummaries(
            RomanPaperExperimentManifest manifest,
            RomanPaperExperimentConditionSpec[] conditions,
            RomanPaperExperimentRunRow[] rows)
        {
            List<RomanPaperExperimentSummary> summaries = new List<RomanPaperExperimentSummary>();
            for (int i = 0; i < conditions.Length; i++)
            {
                RomanPaperExperimentConditionSpec condition = conditions[i];
                List<RomanPaperExperimentRunRow> conditionRows = new List<RomanPaperExperimentRunRow>();
                for (int rowIndex = 0; rows != null && rowIndex < rows.Length; rowIndex++)
                {
                    if (string.Equals(rows[rowIndex].conditionId, condition.conditionId, StringComparison.Ordinal))
                    {
                        conditionRows.Add(rows[rowIndex]);
                    }
                }

                summaries.Add(BuildSummary(manifest, condition, conditionRows.ToArray()));
            }

            return summaries.ToArray();
        }

        private static RomanPaperExperimentSummary BuildSummary(
            RomanPaperExperimentManifest manifest,
            RomanPaperExperimentConditionSpec condition,
            RomanPaperExperimentRunRow[] rows)
        {
            int repetitions = rows == null ? 0 : rows.Length;
            int passed = CountPassed(rows);
            double passRate = repetitions == 0 ? 0d : (double)passed / repetitions;
            double determinismRate = repetitions == 0 ? 0d : (double)CountDeterministic(rows) / repetitions;
            double[] fatigueDeltas = SelectDoubles(rows, "fatigue");
            double[] confidenceDeltas = SelectDoubles(rows, "confidence");
            double[] privilegedTruthDecisions = SelectDoubles(rows, "privileged_decisions");
            double[] blockedPrivilegedAttempts = SelectDoubles(rows, "blocked_privileged");
            RomanPaperExperimentSummary summary = new RomanPaperExperimentSummary
            {
                manifestSetId = manifest.setId,
                manifestSetVersion = manifest.setVersion,
                ablationGroup = condition.ablationGroup,
                conditionId = condition.conditionId,
                repetitions = repetitions,
                passedRepetitions = passed,
                failedRepetitions = repetitions - passed,
                agentCount = condition.agentCount,
                includeContactObservation = condition.includeContactObservation,
                applySyntheticPerformanceModifier = condition.applySyntheticPerformanceModifier,
                passRate = passRate,
                determinismMatchRate = determinismRate,
                meanMutationCount = Mean(rows, "mutation"),
                meanInvalidTickValidations = Mean(rows, "invalid"),
                meanDecisionCardinalityErrors = Mean(rows, "cardinality"),
                meanPrivilegedTruthDecisions = Mean(privilegedTruthDecisions),
                sdPrivilegedTruthDecisions = StandardDeviation(privilegedTruthDecisions),
                ci95PrivilegedTruthDecisions = Confidence95(privilegedTruthDecisions),
                meanBlockedPrivilegedAccessAttempts = Mean(blockedPrivilegedAttempts),
                sdBlockedPrivilegedAccessAttempts = StandardDeviation(blockedPrivilegedAttempts),
                ci95BlockedPrivilegedAccessAttempts = Confidence95(blockedPrivilegedAttempts),
                meanObservationsPerTick = Mean(rows, "observations"),
                meanTraceOnlyDecisionRate = Mean(rows, "trace_rate"),
                meanFatigueDelta = Mean(fatigueDeltas),
                sdFatigueDelta = StandardDeviation(fatigueDeltas),
                ci95FatigueDelta = Confidence95(fatigueDeltas),
                meanConfidenceDelta = Mean(confidenceDeltas),
                sdConfidenceDelta = StandardDeviation(confidenceDeltas),
                ci95ConfidenceDelta = Confidence95(confidenceDeltas)
            };
            summary.passed = repetitions >= MinimumPaperRepetitions
                && summary.failedRepetitions == 0
                && Math.Abs(summary.passRate - 1d) < Epsilon
                && Math.Abs(summary.determinismMatchRate - 1d) < Epsilon
                && Math.Abs(summary.meanMutationCount) < Epsilon
                && Math.Abs(summary.meanInvalidTickValidations) < Epsilon
                && Math.Abs(summary.meanDecisionCardinalityErrors) < Epsilon
                && Math.Abs(summary.meanPrivilegedTruthDecisions) < Epsilon
                && Math.Abs(summary.meanTraceOnlyDecisionRate - 1d) < Epsilon
                && ConditionSummaryCheck(condition, summary);
            return summary;
        }

        private static RomanPaperExperimentRunRow[] RunNegativeControlRows(
            RomanPaperExperimentManifest manifest,
            int seedStart)
        {
            RomanPaperExperimentConditionSpec condition = Condition(
                "negative_control",
                "negative_control_base",
                "paper_negative_control_runner",
                3,
                2,
                0.45f,
                0.55f,
                false,
                false);
            PaperNegativeControlSpec[] specs = CreateNegativeControlSpecs();
            List<RomanPaperExperimentRunRow> rows = new List<RomanPaperExperimentRunRow>();

            for (int i = 0; i < specs.Length; i++)
            {
                PaperNegativeControlSpec spec = specs[i];
                rows.Add(RunCondition(
                    manifest,
                    condition,
                    0,
                    seedStart + i,
                    spec.fault,
                    spec.replaySeedOffset,
                    "negative_control",
                    spec.controlId));
            }

            return rows.ToArray();
        }

        private static RomanPaperNegativeControlSummary[] BuildNegativeControlSummaries(
            RomanPaperExperimentRunRow[] rows)
        {
            PaperNegativeControlSpec[] controls = CreateNegativeControlSpecs();
            List<RomanPaperNegativeControlSummary> summaries = new List<RomanPaperNegativeControlSummary>();
            for (int i = 0; i < controls.Length; i++)
            {
                PaperNegativeControlSpec control = controls[i];
                RomanPaperExperimentRunRow row = FindNegativeControlRow(rows, control.controlId);
                string observedBucket = ClassifyFailure(row);
                bool failedClosed = row != null
                    && !row.passed
                    && string.Equals(observedBucket, control.failureBucket, StringComparison.Ordinal);
                summaries.Add(new RomanPaperNegativeControlSummary
                {
                    negativeControlId = control.controlId,
                    failureBucket = control.failureBucket,
                    expectedFailedRows = 1,
                    observedFailedRows = failedClosed ? 1 : 0,
                    failClosedRate = failedClosed ? 1d : 0d,
                    passed = failedClosed,
                    source = "runner_level_failed_execution"
                });
            }

            return summaries.ToArray();
        }

        private static PaperNegativeControlSpec[] CreateNegativeControlSpecs()
        {
            return new[]
            {
                NegativeControlSpec("mutation_violation", "safety_failure", "mutated_world", 0),
                NegativeControlSpec("trace_only_violation", "safety_failure", "adapter_apply", 0),
                NegativeControlSpec("invalid_tick_validation", "contract_failure", "invalid_input_seed", 0),
                NegativeControlSpec("decision_cardinality_mismatch", "contract_failure", "duplicate_decision", 0),
                NegativeControlSpec("determinism_mismatch", "determinism_failure", string.Empty, 1),
                NegativeControlSpec("privileged_truth_violation", "authority_failure", "privileged_truth_violation", 0)
            };
        }

        private static PaperNegativeControlSpec NegativeControlSpec(
            string controlId,
            string failureBucket,
            string fault,
            int replaySeedOffset)
        {
            return new PaperNegativeControlSpec
            {
                controlId = controlId,
                failureBucket = failureBucket,
                fault = fault,
                replaySeedOffset = replaySeedOffset
            };
        }

        private static RomanPaperExperimentRunRow FindNegativeControlRow(
            RomanPaperExperimentRunRow[] rows,
            string controlId)
        {
            for (int i = 0; rows != null && i < rows.Length; i++)
            {
                if (rows[i] != null && string.Equals(rows[i].conditionId, controlId, StringComparison.Ordinal))
                {
                    return rows[i];
                }
            }

            return null;
        }

        private static RomanPaperFailureTaxonomyRow[] BuildFailureTaxonomy(
            RomanPaperExperimentRunRow[] rows,
            RomanPaperExperimentRunRow[] negativeControlRows,
            RomanPaperNegativeControlSummary[] negativeControls)
        {
            List<RomanPaperFailureTaxonomyRow> taxonomy = new List<RomanPaperFailureTaxonomyRow>();
            string[] buckets = new[]
            {
                "contract_failure",
                "determinism_failure",
                "authority_failure",
                "safety_failure",
                "state_semantics_failure"
            };

            for (int i = 0; i < buckets.Length; i++)
            {
                string bucket = buckets[i];
                int normalFailures = CountRowsByBucket(rows, bucket);
                taxonomy.Add(new RomanPaperFailureTaxonomyRow
                {
                    failureBucket = bucket,
                    source = "normal_experiment",
                    observedFailures = normalFailures,
                    expectedFailures = 0,
                    passed = normalFailures == 0
                });

                int negativeFailures = CountRowsByBucket(negativeControlRows, bucket);
                int expectedNegativeFailures = CountExpectedNegativeControlsByBucket(negativeControls, bucket);
                taxonomy.Add(new RomanPaperFailureTaxonomyRow
                {
                    failureBucket = bucket,
                    source = "negative_control",
                    observedFailures = negativeFailures,
                    expectedFailures = expectedNegativeFailures,
                    passed = negativeFailures == expectedNegativeFailures
                });
            }

            return taxonomy.ToArray();
        }

        private static string ClassifyFailure(RomanPaperExperimentRunRow row)
        {
            if (row == null || row.passed)
            {
                return "none";
            }

            if (row.privilegedTruthDecisions > 0)
            {
                return "authority_failure";
            }

            if (row.mutationCount != 0 || Math.Abs(row.traceOnlyDecisionRate - 1d) >= Epsilon)
            {
                return "safety_failure";
            }

            if (row.invalidInputCount != 0 || row.invalidOutputCount != 0 || row.decisionCardinalityErrors != 0)
            {
                return "contract_failure";
            }

            if (!row.deterministicReplayMatched)
            {
                return "determinism_failure";
            }

            return "state_semantics_failure";
        }

        private static bool ConditionSpecificCheck(
            RomanPaperExperimentConditionSpec condition,
            double observationsPerTick,
            double fatigueDelta,
            double confidenceDelta,
            int privilegedTruthDecisions,
            int blockedPrivilegedAccessAttempts)
        {
            bool expectsObservation = condition.includeContactObservation || condition.includePrivilegedBaitObservation;
            if (expectsObservation && Math.Abs(observationsPerTick - 1d) >= Epsilon)
            {
                return false;
            }

            if (!expectsObservation && Math.Abs(observationsPerTick) >= Epsilon)
            {
                return false;
            }

            if (condition.includePrivilegedBaitObservation)
            {
                if (privilegedTruthDecisions != 0 || blockedPrivilegedAccessAttempts <= 0)
                {
                    return false;
                }
            }
            else if (blockedPrivilegedAccessAttempts != 0)
            {
                return false;
            }

            if (condition.applySyntheticPerformanceModifier)
            {
                return fatigueDelta < -Epsilon && confidenceDelta > Epsilon;
            }

            return Math.Abs(fatigueDelta) < Epsilon && Math.Abs(confidenceDelta) < Epsilon;
        }

        private static bool ConditionSummaryCheck(
            RomanPaperExperimentConditionSpec condition,
            RomanPaperExperimentSummary summary)
        {
            bool expectsObservation = condition.includeContactObservation || condition.includePrivilegedBaitObservation;
            if (expectsObservation && Math.Abs(summary.meanObservationsPerTick - 1d) >= Epsilon)
            {
                return false;
            }

            if (!expectsObservation && Math.Abs(summary.meanObservationsPerTick) >= Epsilon)
            {
                return false;
            }

            if (condition.includePrivilegedBaitObservation)
            {
                if (Math.Abs(summary.meanPrivilegedTruthDecisions) >= Epsilon
                    || summary.meanBlockedPrivilegedAccessAttempts <= Epsilon)
                {
                    return false;
                }
            }
            else if (Math.Abs(summary.meanBlockedPrivilegedAccessAttempts) >= Epsilon)
            {
                return false;
            }

            if (condition.applySyntheticPerformanceModifier)
            {
                return summary.meanFatigueDelta < -Epsilon && summary.meanConfidenceDelta > Epsilon;
            }

            return Math.Abs(summary.meanFatigueDelta) < Epsilon && Math.Abs(summary.meanConfidenceDelta) < Epsilon;
        }

        private static RomanPaperExperimentConditionSpec Condition(
            string ablationGroup,
            string conditionId,
            string scenarioId,
            int ticks,
            int agentCount,
            float baselineFatigue,
            float baselineConfidence,
            bool includeContactObservation,
            bool applySyntheticPerformanceModifier,
            bool includePrivilegedBaitObservation = false)
        {
            return new RomanPaperExperimentConditionSpec
            {
                ablationGroup = ablationGroup,
                conditionId = conditionId,
                scenarioId = scenarioId,
                ticks = ticks,
                agentCount = agentCount,
                baselineFatigue = baselineFatigue,
                baselineConfidence = baselineConfidence,
                includeContactObservation = includeContactObservation,
                includePrivilegedBaitObservation = includePrivilegedBaitObservation,
                applySyntheticPerformanceModifier = applySyntheticPerformanceModifier
            };
        }

        private static void ValidateManifest(RomanPaperExperimentManifest manifest)
        {
            if (manifest == null)
            {
                throw new InvalidDataException("paper experiment manifest is required");
            }

            if (!string.Equals(manifest.format, ManifestFormat, StringComparison.Ordinal))
            {
                throw new InvalidDataException("paper experiment manifest format is unsupported: " + manifest.format);
            }

            if (string.IsNullOrWhiteSpace(manifest.setId))
            {
                throw new InvalidDataException("paper experiment manifest setId is required");
            }

            if (string.IsNullOrWhiteSpace(manifest.setVersion))
            {
                throw new InvalidDataException("paper experiment manifest setVersion is required");
            }

            if (manifest.conditions == null || manifest.conditions.Length == 0)
            {
                throw new InvalidDataException("paper experiment manifest requires conditions");
            }

            HashSet<string> conditionIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < manifest.conditions.Length; i++)
            {
                RomanPaperExperimentConditionSpec condition = manifest.conditions[i];
                if (condition == null)
                {
                    throw new InvalidDataException("paper experiment manifest contains a null condition");
                }

                if (string.IsNullOrWhiteSpace(condition.ablationGroup)
                    || string.IsNullOrWhiteSpace(condition.conditionId)
                    || string.IsNullOrWhiteSpace(condition.scenarioId))
                {
                    throw new InvalidDataException("paper experiment condition requires group, conditionId, and scenarioId");
                }

                if (!conditionIds.Add(condition.conditionId))
                {
                    throw new InvalidDataException("duplicate paper experiment conditionId: " + condition.conditionId);
                }

                if (condition.ticks <= 0 || condition.agentCount <= 0)
                {
                    throw new InvalidDataException("paper experiment condition ticks and agentCount must be positive: " + condition.conditionId);
                }
            }
        }

        private static RomanHeadlessTickRow FindFirstTick(RomanHeadlessBatchResult batch)
        {
            return batch != null && batch.tickRows != null && batch.tickRows.Length > 0 ? batch.tickRows[0] : null;
        }

        private static RomanHeadlessTickRow FindLastTick(RomanHeadlessBatchResult batch)
        {
            return batch != null && batch.tickRows != null && batch.tickRows.Length > 0 ? batch.tickRows[batch.tickRows.Length - 1] : null;
        }

        private static int CountDecisionCardinalityErrors(RomanHeadlessBatchResult batch, int expectedAgentCount)
        {
            if (batch == null || batch.tickRows == null)
            {
                return 1;
            }

            int errors = 0;
            for (int i = 0; i < batch.tickRows.Length; i++)
            {
                RomanHeadlessTickRow row = batch.tickRows[i];
                if (row == null || row.agentCount != expectedAgentCount || row.decisionCount != expectedAgentCount)
                {
                    errors++;
                }
            }

            return errors;
        }

        private static bool AllSummariesPassed(RomanPaperExperimentSummary[] summaries)
        {
            if (summaries == null || summaries.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < summaries.Length; i++)
            {
                if (summaries[i] == null || !summaries[i].passed)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool AllNegativeControlsPassed(RomanPaperNegativeControlSummary[] controls)
        {
            if (controls == null || controls.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < controls.Length; i++)
            {
                if (controls[i] == null || !controls[i].passed)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool AllTaxonomyRowsPassed(RomanPaperFailureTaxonomyRow[] rows)
        {
            if (rows == null || rows.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i] == null || !rows[i].passed)
                {
                    return false;
                }
            }

            return true;
        }

        private static int CountRowsByBucket(RomanPaperExperimentRunRow[] rows, string bucket)
        {
            int count = 0;
            for (int i = 0; rows != null && i < rows.Length; i++)
            {
                if (rows[i] != null && string.Equals(rows[i].failureBucket, bucket, StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountNegativeControlsByBucket(RomanPaperNegativeControlSummary[] controls, string bucket)
        {
            int count = 0;
            for (int i = 0; controls != null && i < controls.Length; i++)
            {
                if (controls[i] != null
                    && controls[i].passed
                    && string.Equals(controls[i].failureBucket, bucket, StringComparison.Ordinal))
                {
                    count += controls[i].observedFailedRows;
                }
            }

            return count;
        }

        private static int CountExpectedNegativeControlsByBucket(RomanPaperNegativeControlSummary[] controls, string bucket)
        {
            int count = 0;
            for (int i = 0; controls != null && i < controls.Length; i++)
            {
                if (controls[i] != null && string.Equals(controls[i].failureBucket, bucket, StringComparison.Ordinal))
                {
                    count += controls[i].expectedFailedRows;
                }
            }

            return count;
        }

        private static int CountPassed(RomanPaperExperimentRunRow[] rows)
        {
            int count = 0;
            for (int i = 0; rows != null && i < rows.Length; i++)
            {
                if (rows[i] != null && rows[i].passed)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountDeterministic(RomanPaperExperimentRunRow[] rows)
        {
            int count = 0;
            for (int i = 0; rows != null && i < rows.Length; i++)
            {
                if (rows[i] != null && rows[i].deterministicReplayMatched)
                {
                    count++;
                }
            }

            return count;
        }

        private static double Mean(RomanPaperExperimentRunRow[] rows, string metric)
        {
            return Mean(SelectDoubles(rows, metric));
        }

        private static double[] SelectDoubles(RomanPaperExperimentRunRow[] rows, string metric)
        {
            List<double> values = new List<double>();
            for (int i = 0; rows != null && i < rows.Length; i++)
            {
                RomanPaperExperimentRunRow row = rows[i];
                if (row == null)
                {
                    continue;
                }

                switch (metric)
                {
                    case "fatigue":
                        values.Add(row.fatigueDelta);
                        break;
                    case "confidence":
                        values.Add(row.confidenceDelta);
                        break;
                    case "mutation":
                        values.Add(row.mutationCount);
                        break;
                    case "invalid":
                        values.Add(row.invalidInputCount + row.invalidOutputCount);
                        break;
                    case "cardinality":
                        values.Add(row.decisionCardinalityErrors);
                        break;
                    case "privileged_decisions":
                        values.Add(row.privilegedTruthDecisions);
                        break;
                    case "blocked_privileged":
                        values.Add(row.blockedPrivilegedAccessAttempts);
                        break;
                    case "observations":
                        values.Add(row.observationsPerTick);
                        break;
                    case "trace_rate":
                        values.Add(row.traceOnlyDecisionRate);
                        break;
                }
            }

            return values.ToArray();
        }

        private static double Mean(double[] values)
        {
            if (values == null || values.Length == 0)
            {
                return 0d;
            }

            double sum = 0d;
            for (int i = 0; i < values.Length; i++)
            {
                sum += values[i];
            }

            return sum / values.Length;
        }

        private static double StandardDeviation(double[] values)
        {
            if (values == null || values.Length < 2)
            {
                return 0d;
            }

            double mean = Mean(values);
            double sumSquared = 0d;
            for (int i = 0; i < values.Length; i++)
            {
                double delta = values[i] - mean;
                sumSquared += delta * delta;
            }

            return Math.Sqrt(sumSquared / (values.Length - 1));
        }

        private static double Confidence95(double[] values)
        {
            if (values == null || values.Length == 0)
            {
                return 0d;
            }

            return 1.96d * StandardDeviation(values) / Math.Sqrt(values.Length);
        }

        private static float SeedJitter(int seed, int salt, float amplitude)
        {
            long raw = seed * 1103515245L + salt * 12345L;
            int bucket = (int)(Math.Abs(raw) % 1000L);
            float normalized = (bucket / 999f) * 2f - 1f;
            return normalized * amplitude;
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            return value > 1f ? 1f : value;
        }

        private static string FormatDouble(double value)
        {
            return value.ToString("0.000000", CultureInfo.InvariantCulture);
        }

        private static JsonSerializerOptions JsonOptions(bool writeIndented = false)
        {
            return new JsonSerializerOptions
            {
                WriteIndented = writeIndented,
                IncludeFields = true,
                PropertyNameCaseInsensitive = true
            };
        }

        private static bool RepositoryFileContains(string relativePath, string expectedContent)
        {
            string found = FindUpwards(Directory.GetCurrentDirectory(), relativePath);
            if (string.IsNullOrWhiteSpace(found))
            {
                found = FindUpwards(AppContext.BaseDirectory, relativePath);
            }

            if (string.IsNullOrWhiteSpace(found) || !File.Exists(found))
            {
                return false;
            }

            string content = File.ReadAllText(found, Encoding.UTF8);
            return content.IndexOf(expectedContent, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string FindUpwards(string startDirectory, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(startDirectory))
            {
                return string.Empty;
            }

            DirectoryInfo directory = new DirectoryInfo(startDirectory);
            while (directory != null)
            {
                string candidate = Path.Combine(directory.FullName, relativePath);
                if (File.Exists(candidate))
                {
                    return Path.GetFullPath(candidate);
                }

                directory = directory.Parent;
            }

            return string.Empty;
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
