using System;

namespace RomanAI
{
    public enum RomanProductTrack
    {
        HumanBehaviorSimulationCore = 0,
        PublicGameAsset = 1,
        TrainingSimulation = 2
    }

    public static class RomanProductContract
    {
        public const string ProductName = "Roman AI";
        public const RomanProductTrack DefaultTrack = RomanProductTrack.HumanBehaviorSimulationCore;
        public const string VerticalSliceScenePath = "Assets/Scenes/RomanAISandbox1.unity";
        public const string VerticalSliceMapId = "training_compound_alpha";
        public const string VerticalSliceMapPrefabName = "TrainingCompoundAlpha";
        public const int VerticalSliceSoldiersPerTeam = 5;
        public const bool VerticalSliceHumanPlayerEnabled = true;
        public const RomanTeam VerticalSliceHumanPlayerTeam = RomanTeam.BLUFOR;
        public const int VerticalSliceHumanPlayerSquadIndex = 1;
        public const string VerticalSliceBluforVisualPrefabName = "CA_BLUFOR_Visual";
        public const string VerticalSliceOpforVisualPrefabName = "CA_OPFOR_Visual";
        public const string InfrastructureSourceOfTruth = "headless_simulation_core";
        public const string UnityReferenceAdapterSource = "authored_scene_and_map_prefab";

        public static readonly string[] AllowedUseCases =
        {
            "synthetic_human_behavior_simulation",
            "human_performance_research",
            "education",
            "synthetic_training",
            "after_action_review",
            "public_game_asset",
            "game_ai_middleware",
            "non_clinical_medical_training_simulation"
        };

        public static readonly string[] ReviewRequiredUseCases =
        {
            "defense_training",
            "security_training",
            "institutional_training_procurement",
            "dual_use_distribution",
            "clinical_training_procurement",
            "ems_training_procurement",
            "named_physiological_modifier_distribution"
        };

        public static readonly string[] ProhibitedUseCases =
        {
            "lethal_decision_automation",
            "real_world_target_selection",
            "weapon_system_control",
            "operational_orders",
            "live_fire_control",
            "surveillance_targeting",
            "clinical_diagnosis",
            "clinical_treatment_advice",
            "drug_dosing_recommendation",
            "drug_use_recommendation",
            "medical_prescription_support"
        };

        public static bool IsAllowedUseCase(string useCase)
        {
            return ContainsUseCase(AllowedUseCases, useCase);
        }

        public static bool RequiresReview(string useCase)
        {
            return ContainsUseCase(ReviewRequiredUseCases, useCase);
        }

        public static bool IsProhibitedUseCase(string useCase)
        {
            return ContainsUseCase(ProhibitedUseCases, useCase);
        }

        private static bool ContainsUseCase(string[] useCases, string useCase)
        {
            if (string.IsNullOrWhiteSpace(useCase))
            {
                return false;
            }

            for (int i = 0; i < useCases.Length; i++)
            {
                if (string.Equals(useCases[i], useCase, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
