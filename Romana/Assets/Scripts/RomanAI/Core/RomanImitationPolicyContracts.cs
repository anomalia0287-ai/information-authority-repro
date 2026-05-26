using System;

namespace RomanAI
{
    public enum RomanImitationHoldOnlyCanaryStatus
    {
        NotRequested,
        Blocked,
        Eligible
    }

    public sealed class RomanImitationHoldOnlyCanaryAudit
    {
        public bool checkedCanary = true;
        public RomanImitationHoldOnlyCanaryStatus status = RomanImitationHoldOnlyCanaryStatus.NotRequested;
        public string allowedAction = "none";
        public string reason = "not_requested";
    }

    public sealed class RomanImitationDryRunPrediction
    {
        public bool predicted;
        public string reason = "not_run";
        public string action = "none";
        public float confidence;
        public bool safetyPassed;
        public string safetyReason = "not_checked";
    }

    public enum RomanImitationAllowedMutationPolicyStatus
    {
        NotRequested,
        NotDefined,
        NoMutationOnly,
        DefinedForFutureCanary,
        ReviewedCanaryAllowed
    }

    public sealed class RomanImitationAllowedMutationPolicyDefinition
    {
        public string action = "none";
        public RomanImitationAllowedMutationPolicyStatus status = RomanImitationAllowedMutationPolicyStatus.NotRequested;
        public bool currentRuntimeAllowed;
        public bool requiresSeparateCanary = true;
        public string reason = "not_requested";
        public string allowedFields = "none";

        public bool AllowsField(string field)
        {
            if (string.IsNullOrEmpty(field) || allowedFields == "none")
            {
                return false;
            }

            string[] fields = allowedFields.Split('|');
            return Array.IndexOf(fields, field) >= 0;
        }
    }

    public static class RomanImitationAllowedMutationPolicy
    {
        private static readonly string[] MovementFields =
        {
            "currentActionType",
            "currentState",
            "objectiveMode",
            "objectiveSource",
            "objectiveKnowledge",
            "moveTargetSource",
            "reservedTargetSource",
            "decisionReason",
            "knowledgeAuthorityLevel",
            "knowledgeSource",
            "knowledgeConfidence",
            "knowledgeUncertaintyRadius",
            "navHasPath",
            "navIsStopped",
            "navStoppingDistance",
            "moveTarget"
        };

        private static readonly string[] CombatPositioningFields =
        {
            "currentActionType",
            "currentState",
            "objectiveMode",
            "objectiveSource",
            "objectiveKnowledge",
            "moveTargetSource",
            "reservedTargetSource",
            "reservedTargetSlot",
            "decisionReason",
            "knowledgeAuthorityLevel",
            "knowledgeSource",
            "knowledgeConfidence",
            "knowledgeUncertaintyRadius",
            "navHasPath",
            "navIsStopped",
            "navStoppingDistance",
            "moveTarget"
        };

        private static readonly string[] StateOnlyFields =
        {
            "currentActionType",
            "currentState",
            "objectiveMode",
            "objectiveSource",
            "objectiveKnowledge",
            "moveTargetSource",
            "reservedTargetSource",
            "decisionReason",
            "knowledgeAuthorityLevel",
            "knowledgeSource",
            "knowledgeConfidence",
            "knowledgeUncertaintyRadius",
            "navIsStopped"
        };

        public static RomanImitationAllowedMutationPolicyDefinition ForAction(string action)
        {
            if (string.IsNullOrEmpty(action) || action == "none")
            {
                return Definition(
                    "none",
                    RomanImitationAllowedMutationPolicyStatus.NotRequested,
                    false,
                    false,
                    "mutation_policy_not_requested",
                    "none");
            }

            switch (action)
            {
                case "hold":
                    return Definition(
                        action,
                        RomanImitationAllowedMutationPolicyStatus.NoMutationOnly,
                        true,
                        true,
                        "hold_noop_only",
                        "none");
                case "objective_move":
                case "medical_retreat":
                case "team_cover":
                case "recover_from_stuck":
                case "local_self_preservation":
                case "traveling_overwatch":
                case "bounding_overwatch":
                    return Definition(
                        action,
                        RomanImitationAllowedMutationPolicyStatus.DefinedForFutureCanary,
                        false,
                        true,
                        "future_movement_canary_required",
                        JoinFields(MovementFields));
                case "engage_visible_enemy":
                    return Definition(
                        action,
                        RomanImitationAllowedMutationPolicyStatus.DefinedForFutureCanary,
                        false,
                        true,
                        "future_combat_canary_required",
                        JoinFields(CombatPositioningFields));
                case "bandaging":
                    return Definition(
                        action,
                        RomanImitationAllowedMutationPolicyStatus.DefinedForFutureCanary,
                        false,
                        true,
                        "future_state_canary_required",
                        JoinFields(StateOnlyFields));
                default:
                    return Definition(
                        action,
                        RomanImitationAllowedMutationPolicyStatus.NotDefined,
                        false,
                        true,
                        "mutation_policy_not_defined",
                        "none");
            }
        }

        public static RomanImitationAllowedMutationPolicyDefinition ForReviewedObjectiveMoveCanary()
        {
            return Definition(
                "objective_move",
                RomanImitationAllowedMutationPolicyStatus.ReviewedCanaryAllowed,
                true,
                true,
                "reviewed_objective_move_canary",
                JoinFields(MovementFields));
        }

        public static RomanImitationAllowedMutationPolicyDefinition ForReviewedMedicalRetreatCanary()
        {
            return Definition(
                "medical_retreat",
                RomanImitationAllowedMutationPolicyStatus.ReviewedCanaryAllowed,
                true,
                true,
                "reviewed_medical_retreat_canary",
                JoinFields(MovementFields));
        }

        public static bool AreChangedFieldsAllowed(string action, string changedFields, out string reason)
        {
            RomanImitationAllowedMutationPolicyDefinition policy = ForAction(action);
            return AreChangedFieldsAllowed(policy, changedFields, out reason);
        }

        public static bool AreChangedFieldsAllowed(
            RomanImitationAllowedMutationPolicyDefinition policy,
            string changedFields,
            out string reason)
        {
            reason = "no_mutation";
            if (string.IsNullOrEmpty(changedFields) || changedFields == "none")
            {
                return true;
            }

            if (policy == null || policy.status == RomanImitationAllowedMutationPolicyStatus.NotDefined)
            {
                reason = "mutation_policy_not_defined";
                return false;
            }

            if (!policy.currentRuntimeAllowed)
            {
                reason = policy.reason;
                return false;
            }

            if (policy.allowedFields == "none")
            {
                reason = "mutation_policy_allows_no_fields";
                return false;
            }

            string[] changed = changedFields.Split('|');
            for (int i = 0; i < changed.Length; i++)
            {
                if (!policy.AllowsField(changed[i]))
                {
                    reason = "mutation_field_not_allowed_" + changed[i];
                    return false;
                }
            }

            reason = "mutation_fields_allowed";
            return true;
        }

        private static RomanImitationAllowedMutationPolicyDefinition Definition(
            string action,
            RomanImitationAllowedMutationPolicyStatus status,
            bool currentRuntimeAllowed,
            bool requiresSeparateCanary,
            string reason,
            string allowedFields)
        {
            return new RomanImitationAllowedMutationPolicyDefinition
            {
                action = string.IsNullOrEmpty(action) ? "none" : action,
                status = status,
                currentRuntimeAllowed = currentRuntimeAllowed,
                requiresSeparateCanary = requiresSeparateCanary,
                reason = string.IsNullOrEmpty(reason) ? "mutation_policy_missing_reason" : reason,
                allowedFields = string.IsNullOrEmpty(allowedFields) ? "none" : allowedFields
            };
        }

        private static string JoinFields(string[] fields)
        {
            return fields == null || fields.Length == 0 ? "none" : string.Join("|", fields);
        }
    }
}
