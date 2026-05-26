using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;

namespace RomanAI.Headless
{
    public sealed class RomanValidationScenarioManifest
    {
        public string format = RomanValidationScenarioSet.ManifestFormat;
        public string setId = RomanValidationScenarioSet.SetId;
        public string setVersion = RomanValidationScenarioSet.SetVersion;
        public string description = string.Empty;
        public RomanValidationScenarioManifestScenario[] scenarios = new RomanValidationScenarioManifestScenario[0];
        public RomanValidationReplayProfileSpec[] replayProfiles = new RomanValidationReplayProfileSpec[0];
        public RomanValidationInvariantSpec[] invariants = new RomanValidationInvariantSpec[0];
    }

    public sealed class RomanValidationScenarioManifestScenario
    {
        public string scenarioId = "default";
        public string purpose = string.Empty;
        public int defaultTicks;
    }

    public sealed class RomanValidationReplayProfileSpec
    {
        public string profileId = "default";
        public string purpose = string.Empty;
        public int tickMultiplier = 1;
        public int minimumTicks = 1;
    }

    public sealed class RomanValidationInvariantSpec
    {
        public string scenarioId = "batch";
        public string invariantId = "none";
        public string category = "none";
        public string expected = "none";
        public string reason = "none";
        public bool enabled = true;
    }

    public sealed class RomanValidationInvariantResult
    {
        public string scenarioId = "batch";
        public string invariantId = "none";
        public string category = "none";
        public bool passed;
        public string expected = "none";
        public string observed = "none";
        public string reason = "none";
    }

    public sealed class RomanValidationScenarioReport
    {
        public string setId = RomanValidationScenarioSet.SetId;
        public string setVersion = RomanValidationScenarioSet.SetVersion;
        public string manifestFormat = RomanValidationScenarioSet.ManifestFormat;
        public string manifestPath = string.Empty;
        public string runnerVersion = RomanHeadlessScenarioRunner.RunnerVersion;
        public string apiVersion = RomanSimulationCoreApi.ApiVersion;
        public bool passed;
        public int manifestScenarioCount;
        public int totalInvariants;
        public int passedInvariants;
        public int failedInvariants;
        public RomanValidationInvariantResult[] invariants = new RomanValidationInvariantResult[0];
    }

    public sealed class RomanValidationCoverageRow
    {
        public string scenarioId = "batch";
        public string category = "none";
        public int totalInvariants;
        public int passedInvariants;
        public int failedInvariants;
        public bool passed;
    }

    public sealed class RomanValidationCoverageReport
    {
        public string setId = RomanValidationScenarioSet.SetId;
        public string setVersion = RomanValidationScenarioSet.SetVersion;
        public string manifestFormat = RomanValidationScenarioSet.ManifestFormat;
        public string runnerVersion = RomanHeadlessScenarioRunner.RunnerVersion;
        public string apiVersion = RomanSimulationCoreApi.ApiVersion;
        public bool passed;
        public int totalRows;
        public RomanValidationCoverageRow[] rows = new RomanValidationCoverageRow[0];
    }

    public static class RomanValidationScenarioSet
    {
        public const string ManifestFormat = "roman_validation_scenario_manifest_v1";
        public const string DefaultManifestFileName = "validation_scenarios_manifest.json";
        public const string SetId = "roman_core_validation_scenarios";
        public const string SetVersion = "0.3.2";

        public static RomanValidationScenarioReport EvaluateDefault(RomanHeadlessBatchResult batch)
        {
            string manifestPath = ResolveDefaultManifestPath();
            return Evaluate(batch, LoadManifest(manifestPath), manifestPath);
        }

        public static RomanValidationScenarioReport Evaluate(
            RomanHeadlessBatchResult batch,
            RomanValidationScenarioManifest manifest)
        {
            return Evaluate(batch, manifest, "inline");
        }

        public static RomanValidationScenarioReport Evaluate(
            RomanHeadlessBatchResult batch,
            RomanValidationScenarioManifest manifest,
            string manifestPath)
        {
            ValidateManifest(manifest);

            List<RomanValidationInvariantResult> results = new List<RomanValidationInvariantResult>();
            for (int i = 0; i < manifest.invariants.Length; i++)
            {
                RomanValidationInvariantSpec spec = manifest.invariants[i];
                if (spec == null || !spec.enabled)
                {
                    continue;
                }

                InvariantEvaluation evaluation = EvaluateInvariant(batch, manifest, spec);
                Add(results, spec, evaluation.passed, evaluation.observed);
            }

            return BuildReport(results, manifest, manifestPath);
        }

        public static RomanValidationScenarioManifest LoadDefaultManifest()
        {
            return LoadManifest(ResolveDefaultManifestPath());
        }

        public static RomanValidationScenarioManifest LoadManifest(string manifestPath)
        {
            if (string.IsNullOrWhiteSpace(manifestPath))
            {
                throw new ArgumentException("manifest path is required", nameof(manifestPath));
            }

            if (!File.Exists(manifestPath))
            {
                throw new FileNotFoundException("validation scenario manifest was not found", manifestPath);
            }

            string manifestJson = File.ReadAllText(manifestPath, Encoding.UTF8);
            ValidateManifestJsonShape(manifestJson, manifestPath);
            RomanValidationScenarioManifest manifest = JsonSerializer.Deserialize<RomanValidationScenarioManifest>(
                manifestJson,
                JsonOptions());
            ValidateManifest(manifest);
            return manifest;
        }

        public static string ResolveDefaultManifestPath()
        {
            string relativePath = Path.Combine("tools", "RomanAI.Headless", DefaultManifestFileName);
            string[] directCandidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, DefaultManifestFileName),
                Path.Combine(Directory.GetCurrentDirectory(), relativePath),
                Path.Combine(Directory.GetCurrentDirectory(), DefaultManifestFileName)
            };

            for (int i = 0; i < directCandidates.Length; i++)
            {
                if (File.Exists(directCandidates[i]))
                {
                    return Path.GetFullPath(directCandidates[i]);
                }
            }

            string found = FindUpwards(AppContext.BaseDirectory, relativePath);
            if (!string.IsNullOrWhiteSpace(found))
            {
                return found;
            }

            found = FindUpwards(Directory.GetCurrentDirectory(), relativePath);
            if (!string.IsNullOrWhiteSpace(found))
            {
                return found;
            }

            throw new FileNotFoundException("default validation scenario manifest was not found", relativePath);
        }

        public static string ToCsv(RomanValidationScenarioReport report)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("set_id,set_version,scenario_id,invariant_id,category,passed,expected,observed,reason");

            if (report != null && report.invariants != null)
            {
                for (int i = 0; i < report.invariants.Length; i++)
                {
                    RomanValidationInvariantResult item = report.invariants[i];
                    builder.Append(Csv(report.setId)).Append(',');
                    builder.Append(Csv(report.setVersion)).Append(',');
                    builder.Append(Csv(item.scenarioId)).Append(',');
                    builder.Append(Csv(item.invariantId)).Append(',');
                    builder.Append(Csv(item.category)).Append(',');
                    builder.Append(item.passed ? "true" : "false").Append(',');
                    builder.Append(Csv(item.expected)).Append(',');
                    builder.Append(Csv(item.observed)).Append(',');
                    builder.AppendLine(Csv(item.reason));
                }
            }

            return builder.ToString();
        }

        public static string ToJson(RomanValidationScenarioReport report)
        {
            return JsonSerializer.Serialize(report, JsonOptions(writeIndented: true));
        }

        public static string ToManifestJson(RomanValidationScenarioManifest manifest)
        {
            return JsonSerializer.Serialize(manifest, JsonOptions(writeIndented: true));
        }

        public static RomanValidationCoverageReport BuildCoverageReport(RomanValidationScenarioReport report)
        {
            if (report == null)
            {
                return new RomanValidationCoverageReport
                {
                    passed = false
                };
            }

            List<RomanValidationCoverageRow> rows = new List<RomanValidationCoverageRow>();
            Dictionary<string, RomanValidationCoverageRow> rowsByKey = new Dictionary<string, RomanValidationCoverageRow>(StringComparer.Ordinal);

            if (report.invariants != null)
            {
                for (int i = 0; i < report.invariants.Length; i++)
                {
                    RomanValidationInvariantResult invariant = report.invariants[i];
                    if (invariant == null)
                    {
                        continue;
                    }

                    string scenarioId = string.IsNullOrWhiteSpace(invariant.scenarioId) ? "unknown" : invariant.scenarioId;
                    string category = string.IsNullOrWhiteSpace(invariant.category) ? "unknown" : invariant.category;
                    string key = scenarioId + "\n" + category;

                    if (!rowsByKey.TryGetValue(key, out RomanValidationCoverageRow row))
                    {
                        row = new RomanValidationCoverageRow
                        {
                            scenarioId = scenarioId,
                            category = category
                        };
                        rowsByKey.Add(key, row);
                        rows.Add(row);
                    }

                    row.totalInvariants++;
                    if (invariant.passed)
                    {
                        row.passedInvariants++;
                    }
                    else
                    {
                        row.failedInvariants++;
                    }

                    row.passed = row.failedInvariants == 0;
                }
            }

            RomanValidationCoverageReport coverage = new RomanValidationCoverageReport
            {
                setId = report.setId,
                setVersion = report.setVersion,
                manifestFormat = report.manifestFormat,
                runnerVersion = report.runnerVersion,
                apiVersion = report.apiVersion,
                rows = rows.ToArray()
            };
            coverage.totalRows = coverage.rows.Length;
            coverage.passed = report.passed && AllCoverageRowsPassed(coverage.rows);
            return coverage;
        }

        public static string ToCoverageCsv(RomanValidationCoverageReport coverage)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("set_id,set_version,scenario_id,category,total_invariants,passed_invariants,failed_invariants,passed");

            if (coverage != null && coverage.rows != null)
            {
                for (int i = 0; i < coverage.rows.Length; i++)
                {
                    RomanValidationCoverageRow row = coverage.rows[i];
                    builder.Append(Csv(coverage.setId)).Append(',');
                    builder.Append(Csv(coverage.setVersion)).Append(',');
                    builder.Append(Csv(row.scenarioId)).Append(',');
                    builder.Append(Csv(row.category)).Append(',');
                    builder.Append(row.totalInvariants.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(row.passedInvariants.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.Append(row.failedInvariants.ToString(CultureInfo.InvariantCulture)).Append(',');
                    builder.AppendLine(row.passed ? "true" : "false");
                }
            }

            return builder.ToString();
        }

        public static string ToCoverageJson(RomanValidationCoverageReport coverage)
        {
            return JsonSerializer.Serialize(coverage, JsonOptions(writeIndented: true));
        }

        public static void WriteReportFiles(RomanValidationScenarioReport report, string outputDirectory)
        {
            WriteReportFiles(report, null, outputDirectory);
        }

        public static void WriteReportFiles(
            RomanValidationScenarioReport report,
            RomanValidationScenarioManifest manifest,
            string outputDirectory)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                return;
            }

            Directory.CreateDirectory(outputDirectory);
            File.WriteAllText(Path.Combine(outputDirectory, "validation_report.csv"), ToCsv(report), Encoding.UTF8);
            File.WriteAllText(Path.Combine(outputDirectory, "validation_report.json"), ToJson(report), Encoding.UTF8);
            RomanValidationCoverageReport coverage = BuildCoverageReport(report);
            File.WriteAllText(Path.Combine(outputDirectory, "validation_coverage.csv"), ToCoverageCsv(coverage), Encoding.UTF8);
            File.WriteAllText(Path.Combine(outputDirectory, "validation_coverage.json"), ToCoverageJson(coverage), Encoding.UTF8);
            if (manifest != null)
            {
                File.WriteAllText(Path.Combine(outputDirectory, "validation_manifest.json"), ToManifestJson(manifest), Encoding.UTF8);
            }
        }

        private static InvariantEvaluation EvaluateInvariant(
            RomanHeadlessBatchResult batch,
            RomanValidationScenarioManifest manifest,
            RomanValidationInvariantSpec spec)
        {
            string scenarioId = spec.scenarioId;
            RomanHeadlessScenarioSummary summary = FindSummary(batch, scenarioId);

            switch (spec.invariantId)
            {
                case "batch_passes":
                    return Result(batch != null && batch.passed, batch == null ? "batch=null" : "passed=" + batch.passed);
                case "scenario_count":
                    return Result(
                        CountSummaries(batch) == CountManifestScenarios(manifest),
                        CountSummaries(batch).ToString(CultureInfo.InvariantCulture));
                case "scenario_ids_match_manifest":
                    return Result(
                        ScenarioIdsMatchManifest(batch, manifest),
                        DescribeScenarioIds(batch));
                case "all_tick_validations_ok":
                    return Result(AllTickValidationsOk(batch), CountInvalidTickRows(batch).ToString(CultureInfo.InvariantCulture));
                case "no_world_mutation":
                    return Result(TotalMutations(batch) == 0, TotalMutations(batch).ToString(CultureInfo.InvariantCulture));
                case "trace_only_decisions_cover_outputs":
                    return Result(
                        TotalTraceOnlyDecisions(batch) == TotalDecisions(batch),
                        TotalTraceOnlyDecisions(batch).ToString(CultureInfo.InvariantCulture));
                case "deterministic_replay":
                    return Result(IsDeterministicReplay(batch), "compared summary and tick CSV");
                case "long_replay_profile_deterministic":
                    return Result(
                        IsDeterministicReplayProfile(manifest, "long_replay"),
                        DescribeReplayProfile(manifest, "long_replay"));
                case "baseline_present":
                case "contact_present":
                case "performance_present":
                case "squad_present":
                    return Result(summary != null, summary == null ? "missing" : "present");
                case "baseline_no_observations":
                    return Result(
                        summary != null && summary.totalObservations == 0,
                        summary == null ? "missing" : summary.totalObservations.ToString(CultureInfo.InvariantCulture));
                case "baseline_behavior_stable":
                    return Result(BaselineBehaviorStable(batch, scenarioId), DescribeFirstLastState(batch, scenarioId));
                case "contact_observations_each_tick":
                    return Result(
                        summary != null && summary.totalObservations == summary.ticks,
                        summary == null ? "missing" : summary.totalObservations.ToString(CultureInfo.InvariantCulture));
                case "contact_no_mutation":
                    return Result(
                        summary != null && summary.mutationCount == 0,
                        summary == null ? "missing" : summary.mutationCount.ToString(CultureInfo.InvariantCulture));
                case "performance_fatigue_decreases":
                    return Result(PerformanceFatigueDecreases(batch, scenarioId), DescribeFirstLast(batch, scenarioId, fatigue: true));
                case "performance_confidence_increases":
                    return Result(PerformanceConfidenceIncreases(batch, scenarioId), DescribeFirstLast(batch, scenarioId, fatigue: false));
                case "squad_agent_count":
                    return Result(
                        summary != null && summary.agentCount == 4,
                        summary == null ? "missing" : summary.agentCount.ToString(CultureInfo.InvariantCulture));
                case "squad_decisions_each_tick":
                    return Result(
                        DecisionsMatchAgentsEachTick(batch, scenarioId, summary),
                        DescribeDecisionCardinality(summary));
                case "squad_behavior_stable":
                    return Result(BaselineBehaviorStable(batch, scenarioId), DescribeFirstLastState(batch, scenarioId));
                default:
                    return Result(false, "unsupported_invariant:" + spec.invariantId);
            }
        }

        private static RomanValidationScenarioReport BuildReport(
            List<RomanValidationInvariantResult> results,
            RomanValidationScenarioManifest manifest,
            string manifestPath)
        {
            RomanValidationScenarioReport report = new RomanValidationScenarioReport
            {
                setId = string.IsNullOrWhiteSpace(manifest.setId) ? SetId : manifest.setId,
                setVersion = string.IsNullOrWhiteSpace(manifest.setVersion) ? SetVersion : manifest.setVersion,
                manifestFormat = string.IsNullOrWhiteSpace(manifest.format) ? ManifestFormat : manifest.format,
                manifestPath = manifestPath ?? string.Empty,
                manifestScenarioCount = CountManifestScenarios(manifest),
                invariants = results.ToArray()
            };

            report.totalInvariants = report.invariants.Length;
            for (int i = 0; i < report.invariants.Length; i++)
            {
                if (report.invariants[i].passed)
                {
                    report.passedInvariants++;
                }
                else
                {
                    report.failedInvariants++;
                }
            }

            report.passed = report.failedInvariants == 0;
            return report;
        }

        private static void Add(
            List<RomanValidationInvariantResult> results,
            RomanValidationInvariantSpec spec,
            bool passed,
            string observed)
        {
            results.Add(new RomanValidationInvariantResult
            {
                scenarioId = spec.scenarioId,
                invariantId = spec.invariantId,
                category = spec.category,
                passed = passed,
                expected = spec.expected,
                observed = observed,
                reason = spec.reason
            });
        }

        private static void ValidateManifest(RomanValidationScenarioManifest manifest)
        {
            if (manifest == null)
            {
                throw new InvalidDataException("validation scenario manifest is empty");
            }

            if (!string.Equals(manifest.format, ManifestFormat, StringComparison.Ordinal))
            {
                throw new InvalidDataException("unexpected validation scenario manifest format: " + manifest.format);
            }

            if (string.IsNullOrWhiteSpace(manifest.setId))
            {
                throw new InvalidDataException("validation scenario manifest setId is required");
            }

            if (string.IsNullOrWhiteSpace(manifest.setVersion))
            {
                throw new InvalidDataException("validation scenario manifest setVersion is required");
            }

            if (string.IsNullOrWhiteSpace(manifest.description))
            {
                throw new InvalidDataException("validation scenario manifest description is required");
            }

            if (manifest.scenarios == null || manifest.scenarios.Length == 0)
            {
                throw new InvalidDataException("validation scenario manifest has no scenarios");
            }

            if (manifest.invariants == null || manifest.invariants.Length == 0)
            {
                throw new InvalidDataException("validation scenario manifest has no invariants");
            }

            Dictionary<string, bool> scenarioIds = new Dictionary<string, bool>(StringComparer.Ordinal);
            for (int i = 0; i < manifest.scenarios.Length; i++)
            {
                RomanValidationScenarioManifestScenario scenario = manifest.scenarios[i];
                if (scenario == null)
                {
                    throw new InvalidDataException("validation scenario manifest contains a null scenario");
                }

                if (string.IsNullOrWhiteSpace(scenario.scenarioId))
                {
                    throw new InvalidDataException("validation scenario manifest scenarioId is required");
                }

                if (string.Equals(scenario.scenarioId, "batch", StringComparison.Ordinal))
                {
                    throw new InvalidDataException("validation scenario manifest scenarioId is reserved: batch");
                }

                if (scenarioIds.ContainsKey(scenario.scenarioId))
                {
                    throw new InvalidDataException("duplicate validation scenario id: " + scenario.scenarioId);
                }

                if (string.IsNullOrWhiteSpace(scenario.purpose))
                {
                    throw new InvalidDataException("validation scenario manifest purpose is required: " + scenario.scenarioId);
                }

                if (scenario.defaultTicks <= 0)
                {
                    throw new InvalidDataException("validation scenario manifest defaultTicks must be > 0: " + scenario.scenarioId);
                }

                scenarioIds.Add(scenario.scenarioId, true);
            }

            Dictionary<string, bool> replayProfileIds = new Dictionary<string, bool>(StringComparer.Ordinal);
            if (manifest.replayProfiles != null)
            {
                for (int i = 0; i < manifest.replayProfiles.Length; i++)
                {
                    RomanValidationReplayProfileSpec profile = manifest.replayProfiles[i];
                    if (profile == null)
                    {
                        throw new InvalidDataException("validation scenario manifest contains a null replay profile");
                    }

                    if (string.IsNullOrWhiteSpace(profile.profileId))
                    {
                        throw new InvalidDataException("validation replay profile profileId is required");
                    }

                    if (replayProfileIds.ContainsKey(profile.profileId))
                    {
                        throw new InvalidDataException("duplicate validation replay profile id: " + profile.profileId);
                    }

                    if (string.IsNullOrWhiteSpace(profile.purpose))
                    {
                        throw new InvalidDataException("validation replay profile purpose is required: " + profile.profileId);
                    }

                    if (profile.tickMultiplier <= 0)
                    {
                        throw new InvalidDataException("validation replay profile tickMultiplier must be > 0: " + profile.profileId);
                    }

                    if (profile.minimumTicks <= 0)
                    {
                        throw new InvalidDataException("validation replay profile minimumTicks must be > 0: " + profile.profileId);
                    }

                    replayProfileIds.Add(profile.profileId, true);
                }
            }

            Dictionary<string, bool> invariantIds = new Dictionary<string, bool>(StringComparer.Ordinal);
            int enabledInvariantCount = 0;
            for (int i = 0; i < manifest.invariants.Length; i++)
            {
                RomanValidationInvariantSpec invariant = manifest.invariants[i];
                if (invariant == null)
                {
                    throw new InvalidDataException("validation scenario manifest contains a null invariant");
                }

                if (string.IsNullOrWhiteSpace(invariant.scenarioId))
                {
                    throw new InvalidDataException("validation invariant scenarioId is required");
                }

                if (!string.Equals(invariant.scenarioId, "batch", StringComparison.Ordinal)
                    && !scenarioIds.ContainsKey(invariant.scenarioId))
                {
                    throw new InvalidDataException("validation invariant references unknown scenario: " + invariant.scenarioId);
                }

                if (string.IsNullOrWhiteSpace(invariant.invariantId))
                {
                    throw new InvalidDataException("validation invariant invariantId is required");
                }

                string invariantKey = invariant.scenarioId + "::" + invariant.invariantId;
                if (invariantIds.ContainsKey(invariantKey))
                {
                    throw new InvalidDataException("duplicate validation invariant id: " + invariantKey);
                }

                if (string.IsNullOrWhiteSpace(invariant.category))
                {
                    throw new InvalidDataException("validation invariant category is required: " + invariantKey);
                }

                if (!IsAllowedInvariantCategory(invariant.category))
                {
                    throw new InvalidDataException("unsupported validation invariant category: " + invariant.category);
                }

                if (string.IsNullOrWhiteSpace(invariant.expected))
                {
                    throw new InvalidDataException("validation invariant expected value is required: " + invariantKey);
                }

                if (string.IsNullOrWhiteSpace(invariant.reason))
                {
                    throw new InvalidDataException("validation invariant reason is required: " + invariantKey);
                }

                if (invariant.enabled)
                {
                    enabledInvariantCount++;
                }

                invariantIds.Add(invariantKey, true);
            }

            if (enabledInvariantCount == 0)
            {
                throw new InvalidDataException("validation scenario manifest has no enabled invariants");
            }
        }

        private static void ValidateManifestJsonShape(string manifestJson, string manifestPath)
        {
            if (string.IsNullOrWhiteSpace(manifestJson))
            {
                throw new InvalidDataException("validation scenario manifest JSON is empty: " + manifestPath);
            }

            using (JsonDocument document = JsonDocument.Parse(manifestJson))
            {
                JsonElement root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                {
                    throw new InvalidDataException("validation scenario manifest JSON root must be an object: " + manifestPath);
                }

                RequireStringProperty(root, "format", manifestPath);
                RequireStringProperty(root, "setId", manifestPath);
                RequireStringProperty(root, "setVersion", manifestPath);
                RequireStringProperty(root, "description", manifestPath);

                JsonElement scenarios = RequireArrayProperty(root, "scenarios", manifestPath);
                for (int i = 0; i < scenarios.GetArrayLength(); i++)
                {
                    JsonElement scenario = scenarios[i];
                    if (scenario.ValueKind != JsonValueKind.Object)
                    {
                        throw new InvalidDataException("validation scenario manifest scenario must be an object: " + manifestPath);
                    }

                    RequireStringProperty(scenario, "scenarioId", manifestPath);
                    RequireStringProperty(scenario, "purpose", manifestPath);
                    RequireIntProperty(scenario, "defaultTicks", manifestPath);
                }

                if (root.TryGetProperty("replayProfiles", out JsonElement replayProfiles))
                {
                    if (replayProfiles.ValueKind != JsonValueKind.Array)
                    {
                        throw new InvalidDataException("validation scenario manifest requires array property replayProfiles: " + manifestPath);
                    }

                    for (int i = 0; i < replayProfiles.GetArrayLength(); i++)
                    {
                        JsonElement profile = replayProfiles[i];
                        if (profile.ValueKind != JsonValueKind.Object)
                        {
                            throw new InvalidDataException("validation scenario manifest replay profile must be an object: " + manifestPath);
                        }

                        RequireStringProperty(profile, "profileId", manifestPath);
                        RequireStringProperty(profile, "purpose", manifestPath);
                        RequireIntProperty(profile, "tickMultiplier", manifestPath);
                        RequireIntProperty(profile, "minimumTicks", manifestPath);
                    }
                }

                JsonElement invariants = RequireArrayProperty(root, "invariants", manifestPath);
                for (int i = 0; i < invariants.GetArrayLength(); i++)
                {
                    JsonElement invariant = invariants[i];
                    if (invariant.ValueKind != JsonValueKind.Object)
                    {
                        throw new InvalidDataException("validation scenario manifest invariant must be an object: " + manifestPath);
                    }

                    RequireStringProperty(invariant, "scenarioId", manifestPath);
                    RequireStringProperty(invariant, "invariantId", manifestPath);
                    RequireStringProperty(invariant, "category", manifestPath);
                    RequireStringProperty(invariant, "expected", manifestPath);
                    RequireStringProperty(invariant, "reason", manifestPath);
                    RequireBoolProperty(invariant, "enabled", manifestPath);
                }
            }
        }

        private static JsonElement RequireArrayProperty(JsonElement element, string propertyName, string manifestPath)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement property) || property.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidDataException("validation scenario manifest requires array property " + propertyName + ": " + manifestPath);
            }

            return property;
        }

        private static void RequireStringProperty(JsonElement element, string propertyName, string manifestPath)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement property)
                || property.ValueKind != JsonValueKind.String
                || string.IsNullOrWhiteSpace(property.GetString()))
            {
                throw new InvalidDataException("validation scenario manifest requires string property " + propertyName + ": " + manifestPath);
            }
        }

        private static void RequireIntProperty(JsonElement element, string propertyName, string manifestPath)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement property)
                || property.ValueKind != JsonValueKind.Number
                || !property.TryGetInt32(out _))
            {
                throw new InvalidDataException("validation scenario manifest requires integer property " + propertyName + ": " + manifestPath);
            }
        }

        private static void RequireBoolProperty(JsonElement element, string propertyName, string manifestPath)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement property)
                || (property.ValueKind != JsonValueKind.True && property.ValueKind != JsonValueKind.False))
            {
                throw new InvalidDataException("validation scenario manifest requires boolean property " + propertyName + ": " + manifestPath);
            }
        }

        private static bool IsAllowedInvariantCategory(string category)
        {
            return string.Equals(category, "determinism", StringComparison.Ordinal)
                || string.Equals(category, "safety", StringComparison.Ordinal)
                || string.Equals(category, "contract", StringComparison.Ordinal)
                || string.Equals(category, "coverage", StringComparison.Ordinal)
                || string.Equals(category, "perception", StringComparison.Ordinal)
                || string.Equals(category, "state", StringComparison.Ordinal);
        }

        private static bool AllCoverageRowsPassed(RomanValidationCoverageRow[] rows)
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

        private static RomanHeadlessScenarioSummary FindSummary(RomanHeadlessBatchResult batch, string scenarioId)
        {
            if (batch == null || batch.scenarioSummaries == null)
            {
                return null;
            }

            for (int i = 0; i < batch.scenarioSummaries.Length; i++)
            {
                RomanHeadlessScenarioSummary summary = batch.scenarioSummaries[i];
                if (summary != null && string.Equals(summary.scenarioId, scenarioId, StringComparison.Ordinal))
                {
                    return summary;
                }
            }

            return null;
        }

        private static int CountManifestScenarios(RomanValidationScenarioManifest manifest)
        {
            return manifest != null && manifest.scenarios != null ? manifest.scenarios.Length : 0;
        }

        private static int CountSummaries(RomanHeadlessBatchResult batch)
        {
            return batch != null && batch.scenarioSummaries != null ? batch.scenarioSummaries.Length : 0;
        }

        private static bool AllTickValidationsOk(RomanHeadlessBatchResult batch)
        {
            return CountInvalidTickRows(batch) == 0;
        }

        private static int CountInvalidTickRows(RomanHeadlessBatchResult batch)
        {
            if (batch == null || batch.tickRows == null)
            {
                return 1;
            }

            int invalid = 0;
            for (int i = 0; i < batch.tickRows.Length; i++)
            {
                RomanHeadlessTickRow row = batch.tickRows[i];
                if (row == null || !string.Equals(row.validation, "ok", StringComparison.Ordinal))
                {
                    invalid++;
                }
            }

            return invalid;
        }

        private static int TotalMutations(RomanHeadlessBatchResult batch)
        {
            if (batch == null || batch.scenarioSummaries == null)
            {
                return -1;
            }

            int total = 0;
            for (int i = 0; i < batch.scenarioSummaries.Length; i++)
            {
                if (batch.scenarioSummaries[i] != null)
                {
                    total += batch.scenarioSummaries[i].mutationCount;
                }
            }

            return total;
        }

        private static int TotalTraceOnlyDecisions(RomanHeadlessBatchResult batch)
        {
            if (batch == null || batch.scenarioSummaries == null)
            {
                return -1;
            }

            int total = 0;
            for (int i = 0; i < batch.scenarioSummaries.Length; i++)
            {
                if (batch.scenarioSummaries[i] != null)
                {
                    total += batch.scenarioSummaries[i].traceOnlyDecisionCount;
                }
            }

            return total;
        }

        private static int TotalDecisions(RomanHeadlessBatchResult batch)
        {
            if (batch == null || batch.scenarioSummaries == null)
            {
                return -1;
            }

            int total = 0;
            for (int i = 0; i < batch.scenarioSummaries.Length; i++)
            {
                if (batch.scenarioSummaries[i] != null)
                {
                    total += batch.scenarioSummaries[i].totalDecisions;
                }
            }

            return total;
        }

        private static bool IsDeterministicReplay(RomanHeadlessBatchResult batch)
        {
            if (batch == null)
            {
                return false;
            }

            RomanHeadlessBatchResult replay = RomanHeadlessScenarioRunner.RunBatch(
                RomanHeadlessScenarioRunner.CreateScenariosFromSummaries(batch.scenarioSummaries));
            return string.Equals(RomanHeadlessScenarioRunner.ToSummaryCsv(batch), RomanHeadlessScenarioRunner.ToSummaryCsv(replay), StringComparison.Ordinal)
                && string.Equals(RomanHeadlessScenarioRunner.ToTickCsv(batch), RomanHeadlessScenarioRunner.ToTickCsv(replay), StringComparison.Ordinal);
        }

        private static bool IsDeterministicReplayProfile(
            RomanValidationScenarioManifest manifest,
            string profileId)
        {
            RomanValidationReplayProfileSpec profile = FindReplayProfile(manifest, profileId);
            if (profile == null)
            {
                return false;
            }

            RomanHeadlessBatchResult batch = RomanHeadlessScenarioRunner.RunBatch(
                RomanHeadlessScenarioRunner.CreateReplayProfileScenarios(manifest, profile));
            if (batch == null || !batch.passed)
            {
                return false;
            }

            RomanHeadlessBatchResult replay = RomanHeadlessScenarioRunner.RunBatch(
                RomanHeadlessScenarioRunner.CreateScenariosFromSummaries(batch.scenarioSummaries));
            return string.Equals(RomanHeadlessScenarioRunner.ToSummaryCsv(batch), RomanHeadlessScenarioRunner.ToSummaryCsv(replay), StringComparison.Ordinal)
                && string.Equals(RomanHeadlessScenarioRunner.ToTickCsv(batch), RomanHeadlessScenarioRunner.ToTickCsv(replay), StringComparison.Ordinal);
        }

        private static RomanValidationReplayProfileSpec FindReplayProfile(
            RomanValidationScenarioManifest manifest,
            string profileId)
        {
            if (manifest == null || manifest.replayProfiles == null)
            {
                return null;
            }

            for (int i = 0; i < manifest.replayProfiles.Length; i++)
            {
                RomanValidationReplayProfileSpec profile = manifest.replayProfiles[i];
                if (profile != null && string.Equals(profile.profileId, profileId, StringComparison.Ordinal))
                {
                    return profile;
                }
            }

            return null;
        }

        private static string DescribeReplayProfile(
            RomanValidationScenarioManifest manifest,
            string profileId)
        {
            RomanValidationReplayProfileSpec profile = FindReplayProfile(manifest, profileId);
            if (profile == null)
            {
                return "missing_profile:" + profileId;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("profile=").Append(profile.profileId);
            builder.Append("; tickMultiplier=").Append(profile.tickMultiplier.ToString(CultureInfo.InvariantCulture));
            builder.Append("; minimumTicks=").Append(profile.minimumTicks.ToString(CultureInfo.InvariantCulture));
            builder.Append("; scenarioTicks=");

            if (manifest == null || manifest.scenarios == null)
            {
                builder.Append("missing");
                return builder.ToString();
            }

            for (int i = 0; i < manifest.scenarios.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append('|');
                }

                RomanValidationScenarioManifestScenario scenario = manifest.scenarios[i];
                if (scenario == null)
                {
                    builder.Append("null");
                    continue;
                }

                int ticks = Math.Max(scenario.defaultTicks * profile.tickMultiplier, profile.minimumTicks);
                builder.Append(scenario.scenarioId).Append(':').Append(ticks.ToString(CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        private static bool ScenarioIdsMatchManifest(
            RomanHeadlessBatchResult batch,
            RomanValidationScenarioManifest manifest)
        {
            if (batch == null
                || batch.scenarioSummaries == null
                || manifest == null
                || manifest.scenarios == null
                || batch.scenarioSummaries.Length != manifest.scenarios.Length)
            {
                return false;
            }

            for (int i = 0; i < manifest.scenarios.Length; i++)
            {
                RomanHeadlessScenarioSummary summary = batch.scenarioSummaries[i];
                RomanValidationScenarioManifestScenario scenario = manifest.scenarios[i];
                if (summary == null
                    || scenario == null
                    || !string.Equals(summary.scenarioId, scenario.scenarioId, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private static string DescribeScenarioIds(RomanHeadlessBatchResult batch)
        {
            if (batch == null || batch.scenarioSummaries == null)
            {
                return "missing";
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < batch.scenarioSummaries.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append('|');
                }

                builder.Append(batch.scenarioSummaries[i] == null ? "null" : batch.scenarioSummaries[i].scenarioId);
            }

            return builder.ToString();
        }

        private static bool BaselineBehaviorStable(RomanHeadlessBatchResult batch, string scenarioId)
        {
            RomanHeadlessTickRow first = FindFirstTick(batch, scenarioId);
            if (first == null || batch == null || batch.tickRows == null)
            {
                return false;
            }

            int rows = 0;
            for (int i = 0; i < batch.tickRows.Length; i++)
            {
                RomanHeadlessTickRow row = batch.tickRows[i];
                if (row == null || !string.Equals(row.scenarioId, scenarioId, StringComparison.Ordinal))
                {
                    continue;
                }

                rows++;
                if (Math.Abs(first.averageFatigue - row.averageFatigue) >= 0.0001f
                    || Math.Abs(first.averageConfidence - row.averageConfidence) >= 0.0001f)
                {
                    return false;
                }
            }

            return rows > 0;
        }

        private static bool PerformanceFatigueDecreases(RomanHeadlessBatchResult batch, string scenarioId)
        {
            RomanHeadlessTickRow first = FindFirstTick(batch, scenarioId);
            RomanHeadlessTickRow last = FindLastTick(batch, scenarioId);
            return first != null && last != null && last.averageFatigue < first.averageFatigue;
        }

        private static bool PerformanceConfidenceIncreases(RomanHeadlessBatchResult batch, string scenarioId)
        {
            RomanHeadlessTickRow first = FindFirstTick(batch, scenarioId);
            RomanHeadlessTickRow last = FindLastTick(batch, scenarioId);
            return first != null && last != null && last.averageConfidence > first.averageConfidence;
        }

        private static bool DecisionsMatchAgentsEachTick(
            RomanHeadlessBatchResult batch,
            string scenarioId,
            RomanHeadlessScenarioSummary summary)
        {
            if (batch == null || batch.tickRows == null || summary == null || summary.ticks <= 0 || summary.agentCount <= 0)
            {
                return false;
            }

            int matchingRows = 0;
            for (int i = 0; i < batch.tickRows.Length; i++)
            {
                RomanHeadlessTickRow row = batch.tickRows[i];
                if (row == null || !string.Equals(row.scenarioId, scenarioId, StringComparison.Ordinal))
                {
                    continue;
                }

                matchingRows++;
                if (row.agentCount != summary.agentCount || row.decisionCount != row.agentCount)
                {
                    return false;
                }
            }

            return matchingRows == summary.ticks
                && summary.totalDecisions == summary.ticks * summary.agentCount;
        }

        private static string DescribeDecisionCardinality(RomanHeadlessScenarioSummary summary)
        {
            if (summary == null)
            {
                return "missing";
            }

            return "ticks="
                + summary.ticks.ToString(CultureInfo.InvariantCulture)
                + "; agents="
                + summary.agentCount.ToString(CultureInfo.InvariantCulture)
                + "; decisions="
                + summary.totalDecisions.ToString(CultureInfo.InvariantCulture);
        }

        private static RomanHeadlessTickRow FindFirstTick(RomanHeadlessBatchResult batch, string scenarioId)
        {
            if (batch == null || batch.tickRows == null)
            {
                return null;
            }

            for (int i = 0; i < batch.tickRows.Length; i++)
            {
                RomanHeadlessTickRow row = batch.tickRows[i];
                if (row != null && string.Equals(row.scenarioId, scenarioId, StringComparison.Ordinal))
                {
                    return row;
                }
            }

            return null;
        }

        private static RomanHeadlessTickRow FindLastTick(RomanHeadlessBatchResult batch, string scenarioId)
        {
            if (batch == null || batch.tickRows == null)
            {
                return null;
            }

            RomanHeadlessTickRow result = null;
            for (int i = 0; i < batch.tickRows.Length; i++)
            {
                RomanHeadlessTickRow row = batch.tickRows[i];
                if (row != null && string.Equals(row.scenarioId, scenarioId, StringComparison.Ordinal))
                {
                    result = row;
                }
            }

            return result;
        }

        private static string DescribeFirstLastState(RomanHeadlessBatchResult batch, string scenarioId)
        {
            RomanHeadlessTickRow first = FindFirstTick(batch, scenarioId);
            RomanHeadlessTickRow last = FindLastTick(batch, scenarioId);
            if (first == null || last == null)
            {
                return "missing";
            }

            return "fatigue "
                + first.averageFatigue.ToString("0.000", CultureInfo.InvariantCulture)
                + "->"
                + last.averageFatigue.ToString("0.000", CultureInfo.InvariantCulture)
                + "; confidence "
                + first.averageConfidence.ToString("0.000", CultureInfo.InvariantCulture)
                + "->"
                + last.averageConfidence.ToString("0.000", CultureInfo.InvariantCulture);
        }

        private static string DescribeFirstLast(RomanHeadlessBatchResult batch, string scenarioId, bool fatigue)
        {
            RomanHeadlessTickRow first = FindFirstTick(batch, scenarioId);
            RomanHeadlessTickRow last = FindLastTick(batch, scenarioId);
            if (first == null || last == null)
            {
                return "missing";
            }

            float firstValue = fatigue ? first.averageFatigue : first.averageConfidence;
            float lastValue = fatigue ? last.averageFatigue : last.averageConfidence;
            return firstValue.ToString("0.000", CultureInfo.InvariantCulture) + "->" + lastValue.ToString("0.000", CultureInfo.InvariantCulture);
        }

        private static InvariantEvaluation Result(bool passed, string observed)
        {
            return new InvariantEvaluation
            {
                passed = passed,
                observed = observed
            };
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

        private sealed class InvariantEvaluation
        {
            public bool passed;
            public string observed = string.Empty;
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
