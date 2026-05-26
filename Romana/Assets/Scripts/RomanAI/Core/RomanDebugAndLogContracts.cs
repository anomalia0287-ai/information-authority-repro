namespace RomanAI
{
    public enum RomanDebugUiMode
    {
        Off,
        Compact,
        UnitDetail,
        SquadCommand,
        AuthorityAudit,
        TacticalMap,
        LogProbe,
        LearningProbe
    }

    public enum RomanDebugWarningId
    {
        None,
        PrivilegedTruthDecision,
        OrderNoFailureReason,
        OrderNoFallback,
        BeliefWithoutAuthority,
        ModelUsedPrivilegedInput,
        UnknownUsedAsTarget,
        InvalidPathAccepted
    }

    public enum RomanLogSchemaStatus
    {
        Stable,
        Candidate,
        Deprecated
    }

    public static class RomanAdaptiveLogContract
    {
        public const string Format = "unit_step_v2";
        public const string SchemaVersion = "2.25.0";
        public const string SchemaStatus = "candidate";
        public const string Producer = "unity_romana";

        public static readonly string[] FeatureFlags =
        {
            "authority_levels",
            "enemy_belief",
            "squad_orders",
            "order_failure",
            "psychology",
            "tactical_nodes",
            "privileged_info",
            "verticality",
            "height_advantage",
            "human_player",
            RomanCoreFeatureFlags.EmotionAppraisalMemory,
            "public_doctrine_principles",
            "adaptive_policy_memory",
            "no_leader_fallback",
            "command_debug_audit",
            "imitation_inference_stub",
            "runtime_model_safety_audit",
            "imitation_rollout_activation_gate",
            "imitation_executor_skeleton",
            "imitation_hold_only_canary_policy",
            "imitation_hold_noop_executor",
            "imitation_mutation_diff_audit",
            "imitation_action_mutation_policy",
            "imitation_objective_move_canary_policy",
            "imitation_objective_move_reviewed_executor",
            "imitation_readiness_report_freshness_provenance_guard",
            "imitation_readiness_report_model_hash_guard",
            "imitation_medical_retreat_canary_policy",
            "imitation_medical_retreat_reviewed_executor",
            "imitation_readiness_report_quality_rollback_guard",
            "imitation_hold_live_canary_scope_guard",
            "imitation_hold_noop_evidence_run_set"
        };
    }
}
