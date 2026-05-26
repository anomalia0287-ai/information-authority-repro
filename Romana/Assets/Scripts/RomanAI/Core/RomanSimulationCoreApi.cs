using System;
using System.Collections.Generic;

namespace RomanAI
{
    public enum RomanSimulationExecutionMode
    {
        TraceOnly,
        ShadowDecision,
        ReferenceAdapter
    }

    public enum RomanDecisionIntentKind
    {
        None,
        Hold,
        Move,
        TakeCover,
        Observe,
        Search,
        Regroup,
        Retreat,
        SupportAlly,
        ReportContact
    }

    [Serializable]
    public sealed class RomanScenarioConfig
    {
        public string apiVersion = RomanSimulationCoreApi.ApiVersion;
        public string scenarioId = "default";
        public int randomSeed;
        public float fixedDeltaTimeSeconds = 0.1f;
        public int maxTicks = 600;
        public RomanSimulationExecutionMode executionMode = RomanSimulationExecutionMode.TraceOnly;
        public bool allowWorldMutation;
        public string safetyPolicy = RomanSimulationCoreApi.SafetyPolicy;

        public RomanScenarioConfig Clone()
        {
            return new RomanScenarioConfig
            {
                apiVersion = apiVersion,
                scenarioId = scenarioId,
                randomSeed = randomSeed,
                fixedDeltaTimeSeconds = fixedDeltaTimeSeconds,
                maxTicks = maxTicks,
                executionMode = executionMode,
                allowWorldMutation = allowWorldMutation,
                safetyPolicy = safetyPolicy
            };
        }
    }

    [Serializable]
    public sealed class RomanSimulationAgentState
    {
        public string unitId = "none";
        public RomanTeam team = RomanTeam.BLUFOR;
        public RomanSoldierState soldierState = RomanSoldierState.Unknown;
        public RomanCoreVector3 position;
        public RomanCoreVector3 forward;
        public float health01 = 1f;
        public int ammo;
        public bool isAlive = true;
        public string role = "none";
        public RomanSimulationBehaviorState behavior = new RomanSimulationBehaviorState();

        public static RomanSimulationAgentState FromUnitState(RomanSimulationUnitState unit)
        {
            if (unit == null)
            {
                return new RomanSimulationAgentState();
            }

            return new RomanSimulationAgentState
            {
                unitId = string.IsNullOrWhiteSpace(unit.unitId) ? "none" : unit.unitId,
                team = unit.team,
                soldierState = unit.soldierState,
                position = unit.position,
                forward = unit.forward,
                health01 = Clamp01(unit.health01),
                ammo = unit.ammo,
                isAlive = unit.isAlive,
                role = string.IsNullOrWhiteSpace(unit.role) ? "none" : unit.role
            };
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            return value > 1f ? 1f : value;
        }
    }

    [Serializable]
    public sealed class RomanObservedUnit
    {
        public string unitId = "none";
        public RomanTeam team = RomanTeam.OPFOR;
        public RomanCoreVector3 position;
        public float confidence01 = 1f;
        public float lastSeenSeconds;
        public bool hasLineOfSight;
        public RomanInformationAuthorityLevel authorityLevel = RomanInformationAuthorityLevel.PerceivedTruth;
        public RomanKnowledgeSource knowledgeSource = RomanKnowledgeSource.Sight;
        public bool privilegedOnly;
    }

    [Serializable]
    public sealed class RomanWorldObservation
    {
        public string observationId = "none";
        public string observerUnitId = "none";
        public float observedAtSeconds;
        public RomanObservedUnit[] visibleUnits = new RomanObservedUnit[0];
        public RomanCoreVector3 lastKnownContactPosition;
        public float uncertainty01;
        public string source = "synthetic_simulation";
    }

    [Serializable]
    public sealed class RomanSimulationTickInput
    {
        public string apiVersion = RomanSimulationCoreApi.ApiVersion;
        public RomanScenarioConfig scenario = new RomanScenarioConfig();
        public long tickIndex;
        public float timeSeconds;
        public float deltaTimeSeconds = 0.1f;
        public int deterministicSeed;
        public RomanSimulationAgentState[] agents = new RomanSimulationAgentState[0];
        public RomanWorldObservation[] observations = new RomanWorldObservation[0];
        public RomanSquadOrder[] orders = new RomanSquadOrder[0];
    }

    [Serializable]
    public sealed class RomanAgentDecision
    {
        public string unitId = "none";
        public RomanDecisionIntentKind intent = RomanDecisionIntentKind.Hold;
        public string intentName = "hold";
        public string objective = "none";
        public string source = "roman_core";
        public string reason = "none";
        public RomanCoreVector3 desiredPosition;
        public string relatedUnitId = "none";
        public float confidence01 = 1f;
        public RomanInformationAuthorityLevel authorityLevel = RomanInformationAuthorityLevel.Unknown;
        public RomanKnowledgeSource knowledgeSource = RomanKnowledgeSource.None;
        public bool privilegedTruthUsedForDecision;
        public bool adapterMayApply;
        public bool requiresHumanReview;
        public string safetyBoundary = RomanSimulationCoreApi.SafetyPolicy;
    }

    [Serializable]
    public sealed class RomanSimulationTickOutput
    {
        public string apiVersion = RomanSimulationCoreApi.ApiVersion;
        public string scenarioId = "default";
        public long tickIndex;
        public RomanSimulationExecutionMode executionMode = RomanSimulationExecutionMode.TraceOnly;
        public bool mutatedWorld;
        public RomanAgentDecision[] decisions = new RomanAgentDecision[0];
        public int privilegedTruthDecisions;
        public int blockedPrivilegedAccessAttempts;
        public string traceId = "none";
        public string reason = "none";
    }

    public sealed class RomanCoreValidationResult
    {
        public bool isValid = true;
        public string reason = "ok";

        public static RomanCoreValidationResult Ok()
        {
            return new RomanCoreValidationResult();
        }

        public static RomanCoreValidationResult Fail(string reason)
        {
            return new RomanCoreValidationResult
            {
                isValid = false,
                reason = string.IsNullOrWhiteSpace(reason) ? "invalid" : reason
            };
        }
    }

    public interface IRomanSimulationCore
    {
        RomanSimulationTickOutput Tick(RomanSimulationTickInput input);
    }

    public static class RomanSimulationCoreApi
    {
        public const string ApiVersion = "0.1.0";
        public const string SafetyPolicy = "synthetic_trace_only_no_direct_world_mutation";

        private static readonly string[] ProhibitedDecisionTokens =
        {
            "weapon",
            "fire",
            "shoot",
            "kill",
            "lethal",
            "target_selection",
            "live_fire",
            "surveillance_targeting",
            "drug_dosing",
            "prescription",
            "treatment"
        };

        public static RomanScenarioConfig CreateDefaultScenarioConfig(string scenarioId, int randomSeed)
        {
            return new RomanScenarioConfig
            {
                apiVersion = ApiVersion,
                scenarioId = string.IsNullOrWhiteSpace(scenarioId) ? "default" : scenarioId,
                randomSeed = randomSeed,
                fixedDeltaTimeSeconds = 0.1f,
                maxTicks = 600,
                executionMode = RomanSimulationExecutionMode.TraceOnly,
                allowWorldMutation = false,
                safetyPolicy = SafetyPolicy
            };
        }

        public static RomanSimulationTickInput CreateTickInput(
            RomanScenarioConfig scenario,
            long tickIndex,
            float timeSeconds,
            RomanSimulationAgentState[] agents,
            RomanWorldObservation[] observations = null,
            RomanSquadOrder[] orders = null)
        {
            RomanScenarioConfig safeScenario = scenario == null
                ? CreateDefaultScenarioConfig("default", 0)
                : scenario.Clone();

            return new RomanSimulationTickInput
            {
                apiVersion = ApiVersion,
                scenario = safeScenario,
                tickIndex = tickIndex,
                timeSeconds = timeSeconds,
                deltaTimeSeconds = safeScenario.fixedDeltaTimeSeconds,
                deterministicSeed = BuildDeterministicSeed(safeScenario, tickIndex),
                agents = agents ?? new RomanSimulationAgentState[0],
                observations = observations ?? new RomanWorldObservation[0],
                orders = orders ?? new RomanSquadOrder[0]
            };
        }

        public static RomanCoreValidationResult ValidateTickInput(RomanSimulationTickInput input)
        {
            if (input == null)
            {
                return RomanCoreValidationResult.Fail("input_null");
            }

            if (input.apiVersion != ApiVersion)
            {
                return RomanCoreValidationResult.Fail("api_version_mismatch");
            }

            if (input.scenario == null)
            {
                return RomanCoreValidationResult.Fail("scenario_null");
            }

            if (input.scenario.apiVersion != ApiVersion)
            {
                return RomanCoreValidationResult.Fail("scenario_api_version_mismatch");
            }

            if (input.scenario.fixedDeltaTimeSeconds <= 0f)
            {
                return RomanCoreValidationResult.Fail("scenario_fixed_delta_non_positive");
            }

            if (input.scenario.maxTicks <= 0)
            {
                return RomanCoreValidationResult.Fail("scenario_max_ticks_non_positive");
            }

            if (!string.Equals(input.scenario.safetyPolicy, SafetyPolicy, StringComparison.Ordinal))
            {
                return RomanCoreValidationResult.Fail("scenario_safety_policy_mismatch");
            }

            if (input.tickIndex < 0)
            {
                return RomanCoreValidationResult.Fail("tick_index_negative");
            }

            if (input.tickIndex >= input.scenario.maxTicks)
            {
                return RomanCoreValidationResult.Fail("tick_index_exceeds_scenario_max");
            }

            if (input.timeSeconds < 0f)
            {
                return RomanCoreValidationResult.Fail("time_seconds_negative");
            }

            if (input.deltaTimeSeconds <= 0f)
            {
                return RomanCoreValidationResult.Fail("delta_time_non_positive");
            }

            if (Math.Abs(input.deltaTimeSeconds - input.scenario.fixedDeltaTimeSeconds) > 0.0001f)
            {
                return RomanCoreValidationResult.Fail("delta_time_scenario_mismatch");
            }

            if (input.deterministicSeed != BuildDeterministicSeed(input.scenario, input.tickIndex))
            {
                return RomanCoreValidationResult.Fail("deterministic_seed_mismatch");
            }

            if (input.agents == null)
            {
                return RomanCoreValidationResult.Fail("agents_null");
            }

            for (int i = 0; i < input.agents.Length; i++)
            {
                RomanSimulationAgentState agent = input.agents[i];
                if (agent == null)
                {
                    return RomanCoreValidationResult.Fail("agent_null");
                }

                if (string.IsNullOrWhiteSpace(agent.unitId) || agent.unitId == "none")
                {
                    return RomanCoreValidationResult.Fail("agent_unit_id_missing");
                }

                if (agent.health01 < 0f || agent.health01 > 1f)
                {
                    return RomanCoreValidationResult.Fail("agent_health01_out_of_range");
                }

                RomanCoreValidationResult behaviorValidation = RomanBehaviorStateModel.Validate(agent.behavior);
                if (!behaviorValidation.isValid)
                {
                    return RomanCoreValidationResult.Fail("agent_" + behaviorValidation.reason);
                }

                for (int j = i + 1; j < input.agents.Length; j++)
                {
                    RomanSimulationAgentState other = input.agents[j];
                    if (other != null && string.Equals(agent.unitId, other.unitId, StringComparison.Ordinal))
                    {
                        return RomanCoreValidationResult.Fail("agent_unit_id_duplicate");
                    }
                }
            }

            if (input.observations == null)
            {
                return RomanCoreValidationResult.Fail("observations_null");
            }

            for (int i = 0; i < input.observations.Length; i++)
            {
                RomanWorldObservation observation = input.observations[i];
                if (observation == null)
                {
                    return RomanCoreValidationResult.Fail("observation_null");
                }

                if (string.IsNullOrWhiteSpace(observation.observationId) || observation.observationId == "none")
                {
                    return RomanCoreValidationResult.Fail("observation_id_missing");
                }

                if (string.IsNullOrWhiteSpace(observation.observerUnitId) || observation.observerUnitId == "none")
                {
                    return RomanCoreValidationResult.Fail("observation_observer_unit_id_missing");
                }

                if (!AgentExists(input.agents, observation.observerUnitId, requireAlive: true))
                {
                    return RomanCoreValidationResult.Fail("observation_observer_unit_not_live_agent");
                }

                if (observation.observedAtSeconds < 0f || observation.observedAtSeconds > input.timeSeconds + 0.0001f)
                {
                    return RomanCoreValidationResult.Fail("observation_time_out_of_tick_range");
                }

                if (!IsUnitRange(observation.uncertainty01))
                {
                    return RomanCoreValidationResult.Fail("observation_uncertainty01_out_of_range");
                }

                if (observation.visibleUnits == null)
                {
                    return RomanCoreValidationResult.Fail("observation_visible_units_null");
                }

                for (int j = 0; j < observation.visibleUnits.Length; j++)
                {
                    RomanObservedUnit visibleUnit = observation.visibleUnits[j];
                    if (visibleUnit == null)
                    {
                        return RomanCoreValidationResult.Fail("observation_visible_unit_null");
                    }

                    if (string.IsNullOrWhiteSpace(visibleUnit.unitId) || visibleUnit.unitId == "none")
                    {
                        return RomanCoreValidationResult.Fail("observation_visible_unit_id_missing");
                    }

                    if (!IsUnitRange(visibleUnit.confidence01))
                    {
                        return RomanCoreValidationResult.Fail("observation_visible_unit_confidence01_out_of_range");
                    }

                    if (visibleUnit.lastSeenSeconds < 0f || visibleUnit.lastSeenSeconds > input.timeSeconds + 0.0001f)
                    {
                        return RomanCoreValidationResult.Fail("observation_visible_unit_last_seen_out_of_range");
                    }
                }
            }

            if (input.orders == null)
            {
                return RomanCoreValidationResult.Fail("orders_null");
            }

            return RomanCoreValidationResult.Ok();
        }

        public static RomanSimulationTickOutput BuildTraceOnlyHoldOutput(RomanSimulationTickInput input, string reason)
        {
            List<RomanAgentDecision> decisions = new List<RomanAgentDecision>();
            int blockedPrivilegedAccessAttempts = 0;

            if (input != null && input.agents != null)
            {
                for (int i = 0; i < input.agents.Length; i++)
                {
                    RomanSimulationAgentState agent = input.agents[i];
                    if (agent == null || !agent.isAlive)
                    {
                        continue;
                    }

                    blockedPrivilegedAccessAttempts += CountBlockedPrivilegedEnemyAccessAttempts(input, agent);

                    decisions.Add(new RomanAgentDecision
                    {
                        unitId = agent.unitId,
                        intent = RomanDecisionIntentKind.Hold,
                        intentName = "hold",
                        objective = "maintain_state",
                        source = "roman_core_trace_only",
                        reason = string.IsNullOrWhiteSpace(reason) ? "trace_only_default" : reason,
                        desiredPosition = agent.position,
                        relatedUnitId = "none",
                        confidence01 = 1f,
                        adapterMayApply = false,
                        requiresHumanReview = false,
                        safetyBoundary = SafetyPolicy
                    });
                }
            }

            return new RomanSimulationTickOutput
            {
                apiVersion = ApiVersion,
                scenarioId = input != null && input.scenario != null ? input.scenario.scenarioId : "default",
                tickIndex = input != null ? input.tickIndex : 0,
                executionMode = RomanSimulationExecutionMode.TraceOnly,
                mutatedWorld = false,
                decisions = decisions.ToArray(),
                privilegedTruthDecisions = 0,
                blockedPrivilegedAccessAttempts = blockedPrivilegedAccessAttempts,
                traceId = BuildTraceId(input),
                reason = string.IsNullOrWhiteSpace(reason) ? "trace_only_default" : reason
            };
        }

        public static RomanSimulationTickOutput BuildTraceOnlyPrivilegedViolationOutput(
            RomanSimulationTickInput input,
            string reason)
        {
            List<RomanAgentDecision> decisions = new List<RomanAgentDecision>();
            int privilegedTruthDecisions = 0;

            if (input != null && input.agents != null)
            {
                for (int i = 0; i < input.agents.Length; i++)
                {
                    RomanSimulationAgentState agent = input.agents[i];
                    if (agent == null || !agent.isAlive)
                    {
                        continue;
                    }

                    RomanObservedUnit privilegedContact = FindFirstPrivilegedEnemyKnowledge(input, agent);
                    if (privilegedContact == null)
                    {
                        decisions.Add(BuildHoldDecision(agent, reason));
                        continue;
                    }

                    privilegedTruthDecisions++;
                    decisions.Add(new RomanAgentDecision
                    {
                        unitId = agent.unitId,
                        intent = RomanDecisionIntentKind.ReportContact,
                        intentName = "report_contact",
                        objective = "authority_probe_contact",
                        source = "roman_core_authority_negative_control",
                        reason = string.IsNullOrWhiteSpace(reason) ? "privileged_truth_violation" : reason,
                        desiredPosition = privilegedContact.position,
                        relatedUnitId = privilegedContact.unitId,
                        confidence01 = privilegedContact.confidence01,
                        authorityLevel = privilegedContact.authorityLevel,
                        knowledgeSource = privilegedContact.knowledgeSource,
                        privilegedTruthUsedForDecision = true,
                        adapterMayApply = false,
                        requiresHumanReview = false,
                        safetyBoundary = SafetyPolicy
                    });
                }
            }

            return new RomanSimulationTickOutput
            {
                apiVersion = ApiVersion,
                scenarioId = input != null && input.scenario != null ? input.scenario.scenarioId : "default",
                tickIndex = input != null ? input.tickIndex : 0,
                executionMode = RomanSimulationExecutionMode.TraceOnly,
                mutatedWorld = false,
                decisions = decisions.ToArray(),
                privilegedTruthDecisions = privilegedTruthDecisions,
                blockedPrivilegedAccessAttempts = 0,
                traceId = BuildTraceId(input),
                reason = string.IsNullOrWhiteSpace(reason) ? "privileged_truth_violation" : reason
            };
        }

        public static RomanCoreValidationResult ValidateTickOutput(
            RomanSimulationTickOutput output,
            RomanSimulationTickInput input = null)
        {
            if (output == null)
            {
                return RomanCoreValidationResult.Fail("output_null");
            }

            if (output.apiVersion != ApiVersion)
            {
                return RomanCoreValidationResult.Fail("output_api_version_mismatch");
            }

            if (input != null && output.tickIndex != input.tickIndex)
            {
                return RomanCoreValidationResult.Fail("output_tick_index_mismatch");
            }

            if (input != null)
            {
                if (!string.Equals(output.scenarioId, input.scenario.scenarioId, StringComparison.Ordinal))
                {
                    return RomanCoreValidationResult.Fail("output_scenario_id_mismatch");
                }

                if (output.executionMode != input.scenario.executionMode)
                {
                    return RomanCoreValidationResult.Fail("output_execution_mode_mismatch");
                }

                if (!string.Equals(output.traceId, BuildTraceId(input), StringComparison.Ordinal))
                {
                    return RomanCoreValidationResult.Fail("output_trace_id_mismatch");
                }
            }

            if (output.mutatedWorld && (input == null || !input.scenario.allowWorldMutation))
            {
                return RomanCoreValidationResult.Fail("output_mutation_not_authorized");
            }

            if (output.mutatedWorld && output.executionMode == RomanSimulationExecutionMode.TraceOnly)
            {
                return RomanCoreValidationResult.Fail("trace_only_output_mutated_world");
            }

            if (output.privilegedTruthDecisions < 0 || output.blockedPrivilegedAccessAttempts < 0)
            {
                return RomanCoreValidationResult.Fail("output_authority_counter_negative");
            }

            if (output.privilegedTruthDecisions > 0)
            {
                return RomanCoreValidationResult.Fail("output_privileged_truth_decision");
            }

            if (output.decisions == null)
            {
                return RomanCoreValidationResult.Fail("decisions_null");
            }

            for (int i = 0; i < output.decisions.Length; i++)
            {
                RomanAgentDecision decision = output.decisions[i];
                if (decision == null)
                {
                    return RomanCoreValidationResult.Fail("decision_null");
                }

                if (string.IsNullOrWhiteSpace(decision.unitId) || decision.unitId == "none")
                {
                    return RomanCoreValidationResult.Fail("decision_unit_id_missing");
                }

                if (input != null && !AgentExists(input.agents, decision.unitId, requireAlive: true))
                {
                    return RomanCoreValidationResult.Fail("decision_unit_not_live_agent");
                }

                if (decision.intent == RomanDecisionIntentKind.None)
                {
                    return RomanCoreValidationResult.Fail("decision_intent_none");
                }

                if (string.IsNullOrWhiteSpace(decision.intentName) || decision.intentName == "none")
                {
                    return RomanCoreValidationResult.Fail("decision_intent_name_missing");
                }

                if (decision.confidence01 < 0f || decision.confidence01 > 1f)
                {
                    return RomanCoreValidationResult.Fail("decision_confidence01_out_of_range");
                }

                if (!string.Equals(decision.safetyBoundary, SafetyPolicy, StringComparison.Ordinal))
                {
                    return RomanCoreValidationResult.Fail("decision_safety_boundary_mismatch");
                }

                if (decision.privilegedTruthUsedForDecision
                    || RomanInformationAuthority.IsPrivilegedTruth(
                        decision.authorityLevel,
                        decision.knowledgeSource,
                        decision.privilegedTruthUsedForDecision))
                {
                    return RomanCoreValidationResult.Fail("decision_privileged_truth_used");
                }

                if (output.executionMode == RomanSimulationExecutionMode.TraceOnly && decision.adapterMayApply)
                {
                    return RomanCoreValidationResult.Fail("trace_only_decision_adapter_may_apply");
                }

                if (decision.adapterMayApply
                    && (input == null
                        || output.executionMode != RomanSimulationExecutionMode.ReferenceAdapter
                        || !input.scenario.allowWorldMutation))
                {
                    return RomanCoreValidationResult.Fail("decision_adapter_application_not_authorized");
                }

                if (ContainsProhibitedDecisionToken(decision.intentName)
                    || ContainsProhibitedDecisionToken(decision.objective)
                    || ContainsProhibitedDecisionToken(decision.source)
                    || ContainsProhibitedDecisionToken(decision.reason)
                    || ContainsProhibitedDecisionToken(decision.safetyBoundary))
                {
                    return RomanCoreValidationResult.Fail("decision_contains_prohibited_token");
                }

                for (int j = i + 1; j < output.decisions.Length; j++)
                {
                    RomanAgentDecision other = output.decisions[j];
                    if (other != null && string.Equals(decision.unitId, other.unitId, StringComparison.Ordinal))
                    {
                        return RomanCoreValidationResult.Fail("decision_unit_id_duplicate");
                    }
                }
            }

            return RomanCoreValidationResult.Ok();
        }

        private static RomanAgentDecision BuildHoldDecision(RomanSimulationAgentState agent, string reason)
        {
            return new RomanAgentDecision
            {
                unitId = agent.unitId,
                intent = RomanDecisionIntentKind.Hold,
                intentName = "hold",
                objective = "maintain_state",
                source = "roman_core_trace_only",
                reason = string.IsNullOrWhiteSpace(reason) ? "trace_only_default" : reason,
                desiredPosition = agent.position,
                relatedUnitId = "none",
                confidence01 = 1f,
                adapterMayApply = false,
                requiresHumanReview = false,
                safetyBoundary = SafetyPolicy
            };
        }

        private static int CountBlockedPrivilegedEnemyAccessAttempts(
            RomanSimulationTickInput input,
            RomanSimulationAgentState agent)
        {
            if (input == null || agent == null || input.observations == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < input.observations.Length; i++)
            {
                RomanWorldObservation observation = input.observations[i];
                if (observation == null
                    || !string.Equals(observation.observerUnitId, agent.unitId, StringComparison.Ordinal)
                    || observation.visibleUnits == null)
                {
                    continue;
                }

                for (int j = 0; j < observation.visibleUnits.Length; j++)
                {
                    RomanObservedUnit visibleUnit = observation.visibleUnits[j];
                    if (IsBlockedPrivilegedEnemyKnowledge(visibleUnit, agent))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static RomanObservedUnit FindFirstPrivilegedEnemyKnowledge(
            RomanSimulationTickInput input,
            RomanSimulationAgentState agent)
        {
            if (input == null || agent == null || input.observations == null)
            {
                return null;
            }

            for (int i = 0; i < input.observations.Length; i++)
            {
                RomanWorldObservation observation = input.observations[i];
                if (observation == null
                    || !string.Equals(observation.observerUnitId, agent.unitId, StringComparison.Ordinal)
                    || observation.visibleUnits == null)
                {
                    continue;
                }

                for (int j = 0; j < observation.visibleUnits.Length; j++)
                {
                    RomanObservedUnit visibleUnit = observation.visibleUnits[j];
                    if (IsBlockedPrivilegedEnemyKnowledge(visibleUnit, agent))
                    {
                        return visibleUnit;
                    }
                }
            }

            return null;
        }

        private static bool IsBlockedPrivilegedEnemyKnowledge(
            RomanObservedUnit visibleUnit,
            RomanSimulationAgentState agent)
        {
            return visibleUnit != null
                && agent != null
                && visibleUnit.team != agent.team
                && !RomanInformationAuthority.CanDriveEnemyTargeting(
                    visibleUnit.authorityLevel,
                    visibleUnit.knowledgeSource,
                    visibleUnit.privilegedOnly)
                && RomanInformationAuthority.IsPrivilegedTruth(
                    visibleUnit.authorityLevel,
                    visibleUnit.knowledgeSource,
                    visibleUnit.privilegedOnly);
        }

        private static int BuildDeterministicSeed(RomanScenarioConfig scenario, long tickIndex)
        {
            return (scenario != null ? scenario.randomSeed : 0) ^ (int)(tickIndex & 0x7fffffff);
        }

        private static bool AgentExists(
            RomanSimulationAgentState[] agents,
            string unitId,
            bool requireAlive)
        {
            if (agents == null || string.IsNullOrWhiteSpace(unitId))
            {
                return false;
            }

            for (int i = 0; i < agents.Length; i++)
            {
                RomanSimulationAgentState agent = agents[i];
                if (agent != null
                    && string.Equals(agent.unitId, unitId, StringComparison.Ordinal)
                    && (!requireAlive || agent.isAlive))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsUnitRange(float value)
        {
            return !float.IsNaN(value)
                && !float.IsInfinity(value)
                && value >= 0f
                && value <= 1f;
        }

        private static string BuildTraceId(RomanSimulationTickInput input)
        {
            if (input == null || input.scenario == null)
            {
                return "default:0";
            }

            return input.scenario.scenarioId + ":" + input.tickIndex;
        }

        private static bool ContainsProhibitedDecisionToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            for (int i = 0; i < ProhibitedDecisionTokens.Length; i++)
            {
                if (value.IndexOf(ProhibitedDecisionTokens[i], StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
