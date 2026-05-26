using System;

namespace RomanAI
{
    public enum RomanHumanPerformanceModifierKind
    {
        None = 0,
        StimulantLike = 1,
        AnalgesicLike = 2,
        AnxiolyticLike = 3,
        AdrenergicResponse = 4,
        CognitiveImpairment = 5,
        SubstanceUseState = 6,
        EmergencyCareSupport = 7,
        DissociativeAnalgesicLike = 8
    }

    [Serializable]
    public sealed class RomanHumanPerformanceModifierProfile
    {
        public RomanHumanPerformanceModifierKind kind = RomanHumanPerformanceModifierKind.None;
        public string id = "none";
        public float intensity01 = 1f;
        public float onsetSeconds = 1f;
        public float peakSeconds = 1f;
        public float halfLifeSeconds = 10f;
        public RomanBehaviorStateDelta peakDelta;

        public void Clamp()
        {
            intensity01 = RomanCoreMath.Clamp01(intensity01);
            onsetSeconds = Math.Max(0.001f, onsetSeconds);
            peakSeconds = Math.Max(0.001f, peakSeconds);
            halfLifeSeconds = Math.Max(0.001f, halfLifeSeconds);
        }
    }

    public static class RomanHumanPerformanceStateModel
    {
        public static RomanBehaviorStateDelta Evaluate(RomanHumanPerformanceModifierProfile profile, float elapsedSeconds)
        {
            if (profile == null || profile.kind == RomanHumanPerformanceModifierKind.None)
            {
                return new RomanBehaviorStateDelta();
            }

            profile.Clamp();
            float t = Math.Max(0f, elapsedSeconds);
            float onset = 1f - (float)Math.Exp(-t / profile.onsetSeconds);
            float decayStart = Math.Max(0f, t - profile.peakSeconds);
            float decay = (float)Math.Exp(-decayStart / profile.halfLifeSeconds);
            float effect = RomanCoreMath.Clamp01(profile.intensity01 * onset * decay);
            return profile.peakDelta.Scale(effect);
        }

        public static RomanCombatStress ApplyToStress(RomanCombatStress stress, RomanBehaviorStateDelta delta)
        {
            RomanSimulationBehaviorState behavior = RomanBehaviorStateModel.FromCombatStress(stress);
            return RomanBehaviorStateModel.ToCombatStress(RomanBehaviorStateModel.ApplyDelta(behavior, delta));
        }

        public static RomanSimulationBehaviorState ApplyToBehavior(
            RomanSimulationBehaviorState behavior,
            RomanBehaviorStateDelta delta)
        {
            return RomanBehaviorStateModel.ApplyDelta(behavior, delta);
        }

        public static RomanHumanPerformanceModifierProfile CreateSyntheticStimulantLikeProfile()
        {
            return new RomanHumanPerformanceModifierProfile
            {
                kind = RomanHumanPerformanceModifierKind.StimulantLike,
                id = "synthetic_stimulant_like",
                onsetSeconds = 30f,
                peakSeconds = 120f,
                halfLifeSeconds = 300f,
                peakDelta = new RomanBehaviorStateDelta
                {
                    fatigue = -0.35f,
                    confidence = 0.14f,
                    hesitation = -0.16f,
                    aggression = 0.10f
                }
            };
        }
    }
}
