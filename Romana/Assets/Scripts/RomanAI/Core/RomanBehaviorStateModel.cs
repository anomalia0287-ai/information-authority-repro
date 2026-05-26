using System;

namespace RomanAI
{
    [Serializable]
    public sealed class RomanSimulationBehaviorState
    {
        public float fear;
        public float anger;
        public float morale = 0.5f;
        public float suppression;
        public float pain;
        public float panic;
        public float fatigue;
        public float confidence = 0.5f;
        public float selfPreservation = 0.5f;
        public float aggression;
        public float cohesion = 0.5f;
        public float hesitation;
    }

    [Serializable]
    public struct RomanBehaviorStateDelta
    {
        public float fear;
        public float anger;
        public float morale;
        public float suppression;
        public float pain;
        public float panic;
        public float fatigue;
        public float confidence;
        public float selfPreservation;
        public float aggression;
        public float cohesion;
        public float hesitation;

        public static RomanBehaviorStateDelta operator +(RomanBehaviorStateDelta left, RomanBehaviorStateDelta right)
        {
            return new RomanBehaviorStateDelta
            {
                fear = left.fear + right.fear,
                anger = left.anger + right.anger,
                morale = left.morale + right.morale,
                suppression = left.suppression + right.suppression,
                pain = left.pain + right.pain,
                panic = left.panic + right.panic,
                fatigue = left.fatigue + right.fatigue,
                confidence = left.confidence + right.confidence,
                selfPreservation = left.selfPreservation + right.selfPreservation,
                aggression = left.aggression + right.aggression,
                cohesion = left.cohesion + right.cohesion,
                hesitation = left.hesitation + right.hesitation
            };
        }

        public RomanBehaviorStateDelta Scale(float value)
        {
            return new RomanBehaviorStateDelta
            {
                fear = fear * value,
                anger = anger * value,
                morale = morale * value,
                suppression = suppression * value,
                pain = pain * value,
                panic = panic * value,
                fatigue = fatigue * value,
                confidence = confidence * value,
                selfPreservation = selfPreservation * value,
                aggression = aggression * value,
                cohesion = cohesion * value,
                hesitation = hesitation * value
            };
        }
    }

    public static class RomanBehaviorStateModel
    {
        public const int DimensionCount = 12;
        public const string Version = "0.1.0";

        public static RomanSimulationBehaviorState CreateNeutral()
        {
            return new RomanSimulationBehaviorState
            {
                fear = 0f,
                anger = 0f,
                morale = 0.5f,
                suppression = 0f,
                pain = 0f,
                panic = 0f,
                fatigue = 0f,
                confidence = 0.5f,
                selfPreservation = 0.5f,
                aggression = 0f,
                cohesion = 0.5f,
                hesitation = 0f
            };
        }

        public static RomanSimulationBehaviorState CreateBaseline(
            float fatigue,
            float confidence,
            float morale = 0.6f,
            float cohesion = 0.65f)
        {
            RomanSimulationBehaviorState state = CreateNeutral();
            state.fatigue = fatigue;
            state.confidence = confidence;
            state.morale = morale;
            state.cohesion = cohesion;
            return Normalize(state);
        }

        public static RomanSimulationBehaviorState Clone(RomanSimulationBehaviorState state)
        {
            state ??= CreateNeutral();
            return new RomanSimulationBehaviorState
            {
                fear = state.fear,
                anger = state.anger,
                morale = state.morale,
                suppression = state.suppression,
                pain = state.pain,
                panic = state.panic,
                fatigue = state.fatigue,
                confidence = state.confidence,
                selfPreservation = state.selfPreservation,
                aggression = state.aggression,
                cohesion = state.cohesion,
                hesitation = state.hesitation
            };
        }

        public static RomanSimulationBehaviorState Normalize(RomanSimulationBehaviorState state)
        {
            state ??= CreateNeutral();
            state.fear = RomanCoreMath.Clamp01(state.fear);
            state.anger = RomanCoreMath.Clamp01(state.anger);
            state.morale = RomanCoreMath.Clamp01(state.morale);
            state.suppression = RomanCoreMath.Clamp01(state.suppression);
            state.pain = RomanCoreMath.Clamp01(state.pain);
            state.panic = RomanCoreMath.Clamp01(state.panic);
            state.fatigue = RomanCoreMath.Clamp01(state.fatigue);
            state.confidence = RomanCoreMath.Clamp01(state.confidence);
            state.selfPreservation = RomanCoreMath.Clamp01(state.selfPreservation);
            state.aggression = RomanCoreMath.Clamp01(state.aggression);
            state.cohesion = RomanCoreMath.Clamp01(state.cohesion);
            state.hesitation = RomanCoreMath.Clamp01(state.hesitation);
            return state;
        }

        public static RomanSimulationBehaviorState ApplyDelta(
            RomanSimulationBehaviorState state,
            RomanBehaviorStateDelta delta)
        {
            RomanSimulationBehaviorState result = Clone(state);
            result.fear += delta.fear;
            result.anger += delta.anger;
            result.morale += delta.morale;
            result.suppression += delta.suppression;
            result.pain += delta.pain;
            result.panic += delta.panic;
            result.fatigue += delta.fatigue;
            result.confidence += delta.confidence;
            result.selfPreservation += delta.selfPreservation;
            result.aggression += delta.aggression;
            result.cohesion += delta.cohesion;
            result.hesitation += delta.hesitation;
            return Normalize(result);
        }

        public static RomanSimulationBehaviorState FromCombatStress(
            RomanCombatStress stress,
            float selfPreservation = 0.5f,
            float aggression = 0f,
            float cohesion = 0.5f,
            float hesitation = 0f)
        {
            stress ??= new RomanCombatStress();
            return Normalize(new RomanSimulationBehaviorState
            {
                fear = stress.fear,
                anger = stress.anger,
                morale = stress.morale,
                suppression = stress.suppression,
                pain = stress.pain,
                panic = stress.panic,
                fatigue = stress.fatigue,
                confidence = stress.confidence,
                selfPreservation = selfPreservation,
                aggression = aggression,
                cohesion = cohesion,
                hesitation = hesitation
            });
        }

        public static RomanCombatStress ToCombatStress(RomanSimulationBehaviorState state)
        {
            RomanSimulationBehaviorState normalized = Normalize(Clone(state));
            return new RomanCombatStress
            {
                fear = normalized.fear,
                anger = normalized.anger,
                morale = normalized.morale,
                suppression = normalized.suppression,
                pain = normalized.pain,
                panic = normalized.panic,
                fatigue = normalized.fatigue,
                confidence = normalized.confidence
            };
        }

        public static RomanCoreValidationResult Validate(RomanSimulationBehaviorState state)
        {
            if (state == null)
            {
                return RomanCoreValidationResult.Fail("behavior_state_null");
            }

            if (!IsUnitRange(state.fear)
                || !IsUnitRange(state.anger)
                || !IsUnitRange(state.morale)
                || !IsUnitRange(state.suppression)
                || !IsUnitRange(state.pain)
                || !IsUnitRange(state.panic)
                || !IsUnitRange(state.fatigue)
                || !IsUnitRange(state.confidence)
                || !IsUnitRange(state.selfPreservation)
                || !IsUnitRange(state.aggression)
                || !IsUnitRange(state.cohesion)
                || !IsUnitRange(state.hesitation))
            {
                return RomanCoreValidationResult.Fail("behavior_state_dimension_out_of_range");
            }

            return RomanCoreValidationResult.Ok();
        }

        private static bool IsUnitRange(float value)
        {
            return !float.IsNaN(value)
                && !float.IsInfinity(value)
                && value >= 0f
                && value <= 1f;
        }
    }
}
