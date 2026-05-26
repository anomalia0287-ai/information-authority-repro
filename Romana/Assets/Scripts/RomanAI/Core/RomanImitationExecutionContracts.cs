namespace RomanAI
{
    public static class RomanImitationDatasetContract
    {
        public const string Format = "roman_imitation_v1";
        public const string SchemaVersion = "1.0.0";
        public const string SchemaStatus = "candidate";
        public const string Producer = "unity_romana";

        public static readonly string[] CandidateActions =
        {
            "objective_move",
            "engage_visible_enemy",
            "bandaging",
            "medical_retreat",
            "team_cover",
            "recover_from_stuck",
            "local_self_preservation",
            "traveling_overwatch",
            "bounding_overwatch",
            "hold"
        };

        public static bool IsCandidateAction(string action)
        {
            if (string.IsNullOrEmpty(action))
            {
                return false;
            }

            for (int i = 0; i < CandidateActions.Length; i++)
            {
                if (CandidateActions[i] == action)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public enum RomanImitationActionExecutorStatus
    {
        NotRequested,
        Blocked,
        MappedNotApplied,
        Unsupported,
        NoOpApplied,
        ReviewedApplied
    }

    public sealed class RomanImitationActionExecutionPlan
    {
        public bool checkedExecutor = true;
        public RomanImitationActionExecutorStatus status = RomanImitationActionExecutorStatus.NotRequested;
        public string plannedAction = "none";
        public string plannedState = "none";
        public string plannedObjective = "none";
        public string plannedSource = "none";
        public string controllerMethod = "none";
        public string reason = "not_requested";
    }

    public sealed class RomanImitationActionExecutionRequest
    {
        public bool hasSoldier = true;
        public bool hasController = true;
        public RomanImitationDryRunPrediction prediction;
    }

    public static class RomanImitationActionExecutionContract
    {
        public static RomanImitationActionExecutionPlan NotRequested(string reason)
        {
            return new RomanImitationActionExecutionPlan
            {
                checkedExecutor = true,
                status = RomanImitationActionExecutorStatus.NotRequested,
                plannedAction = "none",
                plannedState = "none",
                plannedObjective = "none",
                plannedSource = "none",
                controllerMethod = "none",
                reason = string.IsNullOrEmpty(reason) ? "not_requested" : reason
            };
        }

        public static RomanImitationActionExecutionPlan Blocked(string reason, RomanImitationActionExecutionPlan plan = null)
        {
            RomanImitationActionExecutionPlan result = plan ?? NotRequested(reason);
            result.checkedExecutor = true;
            result.status = RomanImitationActionExecutorStatus.Blocked;
            result.reason = string.IsNullOrEmpty(reason) ? "blocked" : reason;
            return result;
        }

        public static RomanImitationActionExecutionPlan BuildPlan(RomanImitationActionExecutionRequest request)
        {
            if (request == null || !request.hasSoldier)
            {
                return Blocked("no_soldier");
            }

            if (!request.hasController)
            {
                return Blocked("no_controller");
            }

            RomanImitationDryRunPrediction prediction = request.prediction;
            if (prediction == null || !prediction.predicted)
            {
                return Blocked("missing_model_prediction");
            }

            if (!prediction.safetyPassed)
            {
                return Blocked("unsafe_prediction_" + SafeReason(prediction.safetyReason));
            }

            if (!RomanImitationDatasetContract.IsCandidateAction(prediction.action))
            {
                return Unsupported("invalid_candidate_action");
            }

            switch (prediction.action)
            {
                case "hold":
                    return Plan("hold", "ModelWouldHold", "model_hold", "hold", "HoldPosition");
                case "objective_move":
                    return Plan("objective_move", "ModelWouldObjectiveMove", "model_objective", "model_candidate", "SetMoveDestination");
                case "engage_visible_enemy":
                    return Plan("engage_visible_enemy", "ModelWouldEngage", "visible_enemy", "target_or_cover", "HoldPositionOrSetMoveDestination");
                case "bandaging":
                    return Plan("bandaging", "ModelWouldBandage", "model_bandaging", "self", "BandageState");
                case "medical_retreat":
                    return Plan("medical_retreat", "ModelWouldMedicalRetreat", "model_medical_retreat", "cover_or_own_base", "SetMoveDestination");
                case "team_cover":
                    return Plan("team_cover", "ModelWouldTeamCover", "model_team_cover", "cover", "SetMoveDestination");
                case "recover_from_stuck":
                    return Plan("recover_from_stuck", "ModelWouldRecoverFromStuck", "model_recover_from_stuck", "navmesh", "RecoveryState");
                case "local_self_preservation":
                    return Plan("local_self_preservation", "ModelWouldSelfPreserve", "model_self_preservation", "cover_or_own_base", "SetMoveDestination");
                case "traveling_overwatch":
                    return Plan("traveling_overwatch", "ModelWouldTravelingOverwatch", "doctrine_security", "doctrine", "DoctrineMovement");
                case "bounding_overwatch":
                    return Plan("bounding_overwatch", "ModelWouldBoundingOverwatch", "doctrine_security", "doctrine", "DoctrineMovement");
                default:
                    return Unsupported("executor_mapping_missing");
            }
        }

        private static RomanImitationActionExecutionPlan Unsupported(string reason)
        {
            return new RomanImitationActionExecutionPlan
            {
                checkedExecutor = true,
                status = RomanImitationActionExecutorStatus.Unsupported,
                reason = string.IsNullOrEmpty(reason) ? "unsupported" : reason,
                plannedAction = "none",
                plannedState = "none",
                plannedObjective = "none",
                plannedSource = "none",
                controllerMethod = "none"
            };
        }

        private static RomanImitationActionExecutionPlan Plan(
            string action,
            string state,
            string objective,
            string source,
            string controllerMethod)
        {
            return new RomanImitationActionExecutionPlan
            {
                checkedExecutor = true,
                status = RomanImitationActionExecutorStatus.MappedNotApplied,
                plannedAction = action,
                plannedState = state,
                plannedObjective = objective,
                plannedSource = source,
                controllerMethod = controllerMethod,
                reason = "mapped_not_applied"
            };
        }

        private static string SafeReason(string reason)
        {
            return string.IsNullOrEmpty(reason) ? "unknown" : reason;
        }
    }
}
