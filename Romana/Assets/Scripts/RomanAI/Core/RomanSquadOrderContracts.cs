using System;

namespace RomanAI
{
    public enum RomanSquadIntent
    {
        None,
        Hold,
        Push,
        Flank,
        Regroup,
        Retreat,
        Search,
        Rescue
    }

    public enum RomanSquadOrderType
    {
        None,
        HoldPosition,
        TakeCover,
        Suppress,
        FlankLeft,
        FlankRight,
        Investigate,
        Regroup,
        Retreat,
        PushObjective,
        RescueWounded,
        CoverMedic
    }

    public enum RomanSquadOrderStatus
    {
        None,
        Issued,
        Received,
        Accepted,
        Delayed,
        Modified,
        Refused,
        Executing,
        Interrupted,
        Completed,
        Failed,
        Expired,
        Cancelled,
        Superseded
    }

    public enum RomanSquadOrderFailureReason
    {
        None,
        NotReceived,
        ReceiverDead,
        IssuerDead,
        Expired,
        InvalidTarget,
        NoPath,
        UnsafePath,
        TargetLost,
        BeliefTooUncertain,
        Suppressed,
        FearRefusal,
        Panic,
        Pain,
        LowMorale,
        LowTrust,
        ConflictingOrder,
        FriendlyFireRisk,
        OutOfAmmo,
        NeedsMedical,
        LeaderIncapacitated,
        NoViableLeader
    }

    public enum RomanFallbackAction
    {
        None,
        ContinueCurrentBehavior,
        ReassignOrder,
        CheckSecondInCommand,
        ReturnToRole,
        RequestUpdatedBelief,
        PickNearestReachableNode,
        TakeCover,
        InvestigateOrSearch,
        WatchChokepoint,
        SuppressBack,
        ReportRefusal,
        RetreatToSafeNode,
        MedicalRetreat,
        RegroupNearAlly,
        DelayOrModifySafer,
        HoldFire,
        ReloadSafe,
        TriggerSecondInCommand,
        LocalDoctrine
    }

    [Serializable]
    public sealed class RomanSquadOrder
    {
        public string orderId = "none";
        public string issuerId = "none";
        public string targetUnitId = "none";
        public string targetTeam = "none";
        public RomanSquadIntent intent = RomanSquadIntent.None;
        public RomanSquadOrderType orderType = RomanSquadOrderType.None;
        public RomanCoreVector3 targetPosition;
        public string targetNodeId = "none";
        public string beliefId = "none";
        public int priority;
        public float issuedAt;
        public float expiresAt;
        public string reason = "none";
        public float fearCost;
        public float trustRequirement;
        public bool allowModification = true;
        public RomanInformationAuthorityLevel authorityLevel = RomanInformationAuthorityLevel.Unknown;
        public RomanSquadOrderFailureReason failureReason = RomanSquadOrderFailureReason.None;
        public RomanFallbackAction fallbackAction = RomanFallbackAction.None;

        public bool IsActive(float now)
        {
            return orderType != RomanSquadOrderType.None && (expiresAt <= 0f || now < expiresAt);
        }

        public RomanSquadOrder Clone()
        {
            return new RomanSquadOrder
            {
                orderId = orderId,
                issuerId = issuerId,
                targetUnitId = targetUnitId,
                targetTeam = targetTeam,
                intent = intent,
                orderType = orderType,
                targetPosition = targetPosition,
                targetNodeId = targetNodeId,
                beliefId = beliefId,
                priority = priority,
                issuedAt = issuedAt,
                expiresAt = expiresAt,
                reason = reason,
                fearCost = fearCost,
                trustRequirement = trustRequirement,
                allowModification = allowModification,
                authorityLevel = authorityLevel,
                failureReason = failureReason,
                fallbackAction = fallbackAction
            };
        }

        public static RomanSquadOrder None()
        {
            return new RomanSquadOrder();
        }
    }

    [Serializable]
    public sealed class RomanOrderResponse
    {
        public RomanSquadOrderStatus status = RomanSquadOrderStatus.None;
        public RomanSquadOrderType acceptedOrderType = RomanSquadOrderType.None;
        public RomanSquadOrderType modifiedOrderType = RomanSquadOrderType.None;
        public float orderAcceptScore;
        public string orderRefusalReason = "none";
        public float trustLeader = 0.5f;
        public RomanInformationAuthorityLevel authorityLevel = RomanInformationAuthorityLevel.Unknown;
        public RomanSquadOrderFailureReason failureReason = RomanSquadOrderFailureReason.None;
        public RomanFallbackAction fallbackAction = RomanFallbackAction.None;

        public void Reset()
        {
            status = RomanSquadOrderStatus.None;
            acceptedOrderType = RomanSquadOrderType.None;
            modifiedOrderType = RomanSquadOrderType.None;
            orderAcceptScore = 0f;
            orderRefusalReason = "none";
            authorityLevel = RomanInformationAuthorityLevel.Unknown;
            failureReason = RomanSquadOrderFailureReason.None;
            fallbackAction = RomanFallbackAction.None;
        }
    }

    public static class RomanOrderFailurePolicy
    {
        public static RomanFallbackAction GetDefaultFallback(RomanSquadOrderFailureReason reason)
        {
            switch (reason)
            {
                case RomanSquadOrderFailureReason.NotReceived:
                    return RomanFallbackAction.ContinueCurrentBehavior;
                case RomanSquadOrderFailureReason.ReceiverDead:
                    return RomanFallbackAction.ReassignOrder;
                case RomanSquadOrderFailureReason.IssuerDead:
                    return RomanFallbackAction.CheckSecondInCommand;
                case RomanSquadOrderFailureReason.Expired:
                    return RomanFallbackAction.ReturnToRole;
                case RomanSquadOrderFailureReason.InvalidTarget:
                    return RomanFallbackAction.RequestUpdatedBelief;
                case RomanSquadOrderFailureReason.NoPath:
                case RomanSquadOrderFailureReason.UnsafePath:
                    return RomanFallbackAction.PickNearestReachableNode;
                case RomanSquadOrderFailureReason.TargetLost:
                    return RomanFallbackAction.InvestigateOrSearch;
                case RomanSquadOrderFailureReason.BeliefTooUncertain:
                    return RomanFallbackAction.WatchChokepoint;
                case RomanSquadOrderFailureReason.Suppressed:
                case RomanSquadOrderFailureReason.FearRefusal:
                    return RomanFallbackAction.TakeCover;
                case RomanSquadOrderFailureReason.Panic:
                    return RomanFallbackAction.RetreatToSafeNode;
                case RomanSquadOrderFailureReason.Pain:
                case RomanSquadOrderFailureReason.NeedsMedical:
                    return RomanFallbackAction.MedicalRetreat;
                case RomanSquadOrderFailureReason.LowMorale:
                    return RomanFallbackAction.RegroupNearAlly;
                case RomanSquadOrderFailureReason.LowTrust:
                case RomanSquadOrderFailureReason.ConflictingOrder:
                    return RomanFallbackAction.DelayOrModifySafer;
                case RomanSquadOrderFailureReason.FriendlyFireRisk:
                    return RomanFallbackAction.HoldFire;
                case RomanSquadOrderFailureReason.OutOfAmmo:
                    return RomanFallbackAction.ReloadSafe;
                case RomanSquadOrderFailureReason.LeaderIncapacitated:
                    return RomanFallbackAction.TriggerSecondInCommand;
                case RomanSquadOrderFailureReason.NoViableLeader:
                    return RomanFallbackAction.LocalDoctrine;
                default:
                    return RomanFallbackAction.None;
            }
        }
    }
}
