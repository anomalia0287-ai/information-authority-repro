using System;

namespace RomanAI
{
    [Serializable]
    public sealed class RomanPsychologyProfile
    {
        public float bravery = 0.5f;
        public float discipline = 0.5f;
        public float aggression = 0.5f;
        public float selfPreservation = 0.5f;
        public float loyalty = 0.5f;
        public float stressTolerance = 0.5f;
        public float initiative = 0.5f;

        public static RomanPsychologyProfile CreateDefault(bool leader, int squadIndex, string role)
        {
            RomanPsychologyProfile profile = leader
                ? new RomanPsychologyProfile
                {
                    bravery = 0.78f,
                    discipline = 0.86f,
                    aggression = 0.45f,
                    selfPreservation = 0.46f,
                    loyalty = 0.82f,
                    stressTolerance = 0.84f,
                    initiative = 0.76f
                }
                : new RomanPsychologyProfile
                {
                    bravery = 0.42f + RomanCoreMath.Clamp01(squadIndex / 5f) * 0.14f,
                    discipline = 0.42f + RomanCoreMath.Clamp01(squadIndex / 5f) * 0.18f,
                    aggression = 0.40f + (squadIndex % 3) * 0.12f,
                    selfPreservation = 0.58f,
                    loyalty = 0.52f + RomanCoreMath.Clamp01(1f - squadIndex / 6f) * 0.14f,
                    stressTolerance = 0.38f + RomanCoreMath.Clamp01(squadIndex / 5f) * 0.16f,
                    initiative = 0.38f + (squadIndex % 2) * 0.13f
                };

            switch (role)
            {
                case "assault":
                    profile.aggression += 0.12f;
                    profile.bravery += 0.06f;
                    profile.selfPreservation -= 0.06f;
                    break;
                case "token_runner":
                    profile.initiative += 0.12f;
                    profile.selfPreservation += 0.05f;
                    break;
                case "support":
                    profile.discipline += 0.08f;
                    profile.loyalty += 0.08f;
                    profile.aggression -= 0.04f;
                    break;
                case "base_guard":
                    profile.discipline += 0.1f;
                    profile.stressTolerance += 0.06f;
                    profile.initiative -= 0.04f;
                    break;
                case "flank_south":
                    profile.bravery += 0.08f;
                    profile.initiative += 0.08f;
                    profile.selfPreservation -= 0.04f;
                    break;
            }

            profile.Clamp();
            return profile;
        }

        public void Clamp()
        {
            bravery = RomanCoreMath.Clamp01(bravery);
            discipline = RomanCoreMath.Clamp01(discipline);
            aggression = RomanCoreMath.Clamp01(aggression);
            selfPreservation = RomanCoreMath.Clamp01(selfPreservation);
            loyalty = RomanCoreMath.Clamp01(loyalty);
            stressTolerance = RomanCoreMath.Clamp01(stressTolerance);
            initiative = RomanCoreMath.Clamp01(initiative);
        }
    }

    [Serializable]
    public sealed class RomanCombatStress
    {
        public float fear;
        public float anger;
        public float morale = 0.75f;
        public float suppression;
        public float pain;
        public float panic;
        public float fatigue;
        public float confidence = 0.55f;
    }

    public sealed class RomanOrderComplianceResult
    {
        public RomanSquadOrderStatus status = RomanSquadOrderStatus.None;
        public RomanSquadOrderType effectiveOrderType = RomanSquadOrderType.None;
        public RomanSquadOrderFailureReason failureReason = RomanSquadOrderFailureReason.None;
        public RomanFallbackAction fallbackAction = RomanFallbackAction.None;
        public float acceptScore;
        public float delayDuration;
        public string decision = "none";
        public string reason = "none";
    }

    public static class RomanOrderCompliance
    {
        public static RomanOrderComplianceResult Evaluate(RomanSquadOrder order, RomanPsychologyProfile profile, RomanCombatStress stress, float trustLeader, float now)
        {
            RomanOrderComplianceResult result = new RomanOrderComplianceResult();
            if (order == null || order.orderType == RomanSquadOrderType.None)
            {
                result.status = RomanSquadOrderStatus.Refused;
                result.failureReason = RomanSquadOrderFailureReason.InvalidTarget;
                result.fallbackAction = RomanOrderFailurePolicy.GetDefaultFallback(result.failureReason);
                result.decision = "refused";
                result.reason = "order_refused_invalid_order";
                return result;
            }

            if (order.expiresAt > 0f && now >= order.expiresAt)
            {
                result.status = RomanSquadOrderStatus.Expired;
                result.failureReason = RomanSquadOrderFailureReason.Expired;
                result.fallbackAction = RomanOrderFailurePolicy.GetDefaultFallback(result.failureReason);
                result.decision = "expired";
                result.reason = "order_expired_before_receive";
                return result;
            }

            if (order.authorityLevel == RomanInformationAuthorityLevel.PrivilegedTruth
                || !RomanInformationAuthority.CanDriveRuntimeDecision(order.authorityLevel))
            {
                result.status = RomanSquadOrderStatus.Refused;
                result.failureReason = RomanSquadOrderFailureReason.InvalidTarget;
                result.fallbackAction = RomanOrderFailurePolicy.GetDefaultFallback(result.failureReason);
                result.decision = "refused";
                result.reason = "order_refused_invalid_authority";
                return result;
            }

            profile ??= new RomanPsychologyProfile();
            stress ??= new RomanCombatStress();
            profile.Clamp();
            trustLeader = RomanCoreMath.Clamp01(trustLeader);
            result.acceptScore = CalculateAcceptScore(order, profile, stress, trustLeader);

            if (result.acceptScore >= 0.46f)
            {
                result.status = RomanSquadOrderStatus.Accepted;
                result.effectiveOrderType = order.orderType;
                result.decision = "accepted";
                result.reason = "order_accepted_" + order.orderType;
                return result;
            }

            RomanSquadOrderFailureReason failure = SelectFailureReason(order, profile, stress, trustLeader);
            if (result.acceptScore >= 0.31f && order.allowModification)
            {
                result.status = RomanSquadOrderStatus.Modified;
                result.effectiveOrderType = SelectSaferOrder(order.orderType, stress);
                result.failureReason = failure;
                result.fallbackAction = RomanOrderFailurePolicy.GetDefaultFallback(failure);
                result.decision = "modified";
                result.reason = "order_modified_" + order.orderType + "_to_" + result.effectiveOrderType;
                return result;
            }

            if (result.acceptScore >= 0.18f && stress.panic < 0.55f)
            {
                result.status = RomanSquadOrderStatus.Delayed;
                result.effectiveOrderType = order.orderType;
                result.failureReason = failure;
                result.fallbackAction = RomanFallbackAction.ContinueCurrentBehavior;
                result.delayDuration = RomanCoreMath.Lerp(1.25f, 3.25f, RomanCoreMath.Clamp01(stress.fear + stress.suppression));
                result.decision = "delayed";
                result.reason = "order_delayed_" + failure;
                return result;
            }

            result.status = RomanSquadOrderStatus.Refused;
            result.effectiveOrderType = RomanSquadOrderType.None;
            result.failureReason = failure;
            result.fallbackAction = RomanOrderFailurePolicy.GetDefaultFallback(failure);
            result.decision = "refused";
            result.reason = "order_refused_" + failure;
            return result;
        }

        public static float CalculateAcceptScore(RomanSquadOrder order, RomanPsychologyProfile profile, RomanCombatStress stress, float trustLeader)
        {
            profile ??= new RomanPsychologyProfile();
            stress ??= new RomanCombatStress();
            profile.Clamp();

            float score =
                profile.discipline * 0.24f
                + RomanCoreMath.Clamp01(trustLeader) * 0.18f
                + stress.morale * 0.14f
                + profile.bravery * 0.12f
                + profile.stressTolerance * 0.10f
                + profile.initiative * 0.08f
                + profile.loyalty * 0.08f
                + profile.aggression * 0.06f;

            score -= stress.fear * order.fearCost * RomanCoreMath.Lerp(0.85f, 0.45f, profile.bravery);
            score -= stress.panic * RomanCoreMath.Lerp(0.36f, 0.18f, profile.stressTolerance);
            score -= stress.pain * 0.14f;
            score -= stress.suppression * 0.12f;
            score -= Math.Max(0f, order.trustRequirement - trustLeader) * 0.45f;

            switch (order.orderType)
            {
                case RomanSquadOrderType.TakeCover:
                    score += stress.fear * 0.28f + profile.selfPreservation * 0.08f;
                    break;
                case RomanSquadOrderType.Retreat:
                    score += stress.fear * 0.20f + profile.selfPreservation * 0.06f - profile.discipline * 0.04f;
                    break;
                case RomanSquadOrderType.FlankLeft:
                case RomanSquadOrderType.FlankRight:
                case RomanSquadOrderType.PushObjective:
                    score += profile.aggression * 0.10f + stress.anger * 0.08f - profile.selfPreservation * 0.15f;
                    break;
                case RomanSquadOrderType.RescueWounded:
                case RomanSquadOrderType.CoverMedic:
                    score += profile.loyalty * 0.14f - stress.panic * 0.08f;
                    break;
                case RomanSquadOrderType.HoldPosition:
                    score += profile.discipline * 0.06f - stress.fear * 0.06f;
                    break;
                case RomanSquadOrderType.Suppress:
                    score += profile.discipline * 0.05f + profile.aggression * 0.04f - stress.suppression * 0.04f;
                    break;
                case RomanSquadOrderType.Investigate:
                    score += profile.initiative * 0.07f - stress.fear * 0.04f;
                    break;
            }

            if (stress.morale < 0.35f)
            {
                score -= (0.35f - stress.morale) * 0.18f;
            }

            return RomanCoreMath.Clamp01(score);
        }

        public static RomanSquadOrderType SelectSaferOrder(RomanSquadOrderType orderType, RomanCombatStress stress)
        {
            switch (orderType)
            {
                case RomanSquadOrderType.FlankLeft:
                case RomanSquadOrderType.FlankRight:
                case RomanSquadOrderType.PushObjective:
                case RomanSquadOrderType.Investigate:
                    return RomanSquadOrderType.TakeCover;
                case RomanSquadOrderType.RescueWounded:
                    return RomanSquadOrderType.CoverMedic;
                case RomanSquadOrderType.Suppress:
                    return stress != null && stress.suppression > 0.5f ? RomanSquadOrderType.TakeCover : RomanSquadOrderType.Suppress;
                default:
                    return RomanSquadOrderType.TakeCover;
            }
        }

        public static RomanSquadOrderFailureReason SelectFailureReason(RomanSquadOrder order, RomanPsychologyProfile profile, RomanCombatStress stress, float trustLeader)
        {
            profile ??= new RomanPsychologyProfile();
            stress ??= new RomanCombatStress();
            if (stress.panic > 0.62f)
            {
                return RomanSquadOrderFailureReason.Panic;
            }

            if (stress.pain > 0.68f)
            {
                return RomanSquadOrderFailureReason.Pain;
            }

            if (stress.morale < 0.25f)
            {
                return RomanSquadOrderFailureReason.LowMorale;
            }

            if (trustLeader < order.trustRequirement)
            {
                return RomanSquadOrderFailureReason.LowTrust;
            }

            if (stress.suppression > 0.62f)
            {
                return RomanSquadOrderFailureReason.Suppressed;
            }

            if (stress.fear * order.fearCost > Math.Max(0.18f, profile.discipline * 0.52f + profile.bravery * 0.18f))
            {
                return RomanSquadOrderFailureReason.FearRefusal;
            }

            return RomanSquadOrderFailureReason.Suppressed;
        }
    }

    internal static class RomanCoreMath
    {
        public static float Clamp01(float value)
        {
            if (value <= 0f)
            {
                return 0f;
            }

            return value >= 1f ? 1f : value;
        }

        public static float Lerp(float from, float to, float t)
        {
            return from + (to - from) * Clamp01(t);
        }
    }
}
