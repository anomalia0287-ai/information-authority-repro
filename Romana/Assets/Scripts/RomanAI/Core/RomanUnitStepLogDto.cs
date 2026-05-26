using System;

namespace RomanAI
{
    [Serializable]
    public sealed class RomanUnitStepLogDto
    {
        public RomanSimulationUnitState unit = new RomanSimulationUnitState();
        public RomanLogActionDto action = new RomanLogActionDto();
        public RomanLogWeaponDto weapon = new RomanLogWeaponDto();
        public RomanLogPoseDto pose = new RomanLogPoseDto();
        public RomanLogKnowledgeDto knowledge = new RomanLogKnowledgeDto();
        public RomanLogBeliefDto belief = new RomanLogBeliefDto();
        public RomanLogOrderDto order = new RomanLogOrderDto();
        public RomanLogCommandDto command = new RomanLogCommandDto();
        public RomanLogEmotionDto emotion = new RomanLogEmotionDto();
        public RomanLogHumanUtilityDto humanUtility = new RomanLogHumanUtilityDto();
        public RomanLogPsychologyDto psychology = new RomanLogPsychologyDto();
        public RomanLogModelAuditDto model = new RomanLogModelAuditDto();
    }

    [Serializable]
    public sealed class RomanLogActionDto
    {
        public string type = "hold";
        public string state = RomanSoldierStateContract.UnknownName;
        public string stateReason = "none";
        public string objective = "hold";
        public string source = "none";
        public string knowledge = "unknown";
        public string moveTargetSource = "none";
        public string laneNode = "none";
        public RomanCoreVector3 moveTarget;
        public string reservation = "none";
        public int reservationSlot;
        public string decisionReason = "none";
    }

    [Serializable]
    public sealed class RomanLogWeaponDto
    {
        public int ammo;
        public float fireTimer;
        public float reloadTimer;
        public bool weaponReady;
    }

    [Serializable]
    public sealed class RomanLogPoseDto
    {
        public string stance = RomanStance.Standing.ToString();
        public string lean = RomanLean.Center.ToString();
        public float exposure = 1f;
        public RomanCoreVector3 coverEdge;
        public RomanCoreVector3 muzzle;
    }

    [Serializable]
    public sealed class RomanLogKnowledgeDto
    {
        public string authorityLevel = RomanInformationAuthorityLevel.Unknown.ToString();
        public string source = RomanKnowledgeSource.None.ToString();
        public float confidence;
        public float uncertaintyRadius;
        public int teamReportCount;
        public bool privilegedTruthUsedForDecision;
        public RomanDebugWarningId debugWarning = RomanDebugWarningId.None;
    }

    [Serializable]
    public sealed class RomanLogBeliefDto
    {
        public string id = "none";
        public string targetId = "none";
        public string authorityLevel = RomanInformationAuthorityLevel.Unknown.ToString();
        public string source = RomanKnowledgeSource.None.ToString();
        public bool hasVisibleTarget;
        public bool hasLastKnownPosition;
        public bool hasSuspectedPosition;
        public float confidence;
        public float uncertaintyRadius;
        public RomanCoreVector3 position;
        public float age = -1f;
        public string reportId = "none";
        public string reportSourceUnitId = "none";
        public string originalAuthorityLevel = RomanInformationAuthorityLevel.Unknown.ToString();
        public string originalSource = RomanKnowledgeSource.None.ToString();
    }

    [Serializable]
    public sealed class RomanLogOrderDto
    {
        public string currentOrder = RomanSquadOrderType.None.ToString();
        public string activeOrderId = "none";
        public string orderIssuerId = "none";
        public string squadIntent = RomanSquadIntent.None.ToString();
        public string orderStatus = RomanSquadOrderStatus.None.ToString();
        public string orderFailureReason = RomanSquadOrderFailureReason.None.ToString();
        public string fallbackAction = RomanFallbackAction.None.ToString();
        public float orderAcceptScore;
        public float trustLeader = 0.5f;
        public string orderComplianceDecision = "none";
        public float orderDelayRemaining;
    }

    [Serializable]
    public sealed class RomanLogCommandDto
    {
        public float squadMorale = 0.75f;
        public float commandConfidence = 0.65f;
        public string currentLeaderId = "none";
        public string secondInCommandId = "none";
        public string commandState = "None";
        public bool isSecondInCommand;
        public float leadershipScore;
        public string moraleEvent = "none";
        public string moraleEventSourceUnitId = "none";
        public string moraleEventTargetUnitId = "none";
        public float moraleDelta;
        public float trustDelta;
    }

    [Serializable]
    public sealed class RomanLogEmotionDto
    {
        public string emotionCause = "none";
        public string emotionAppraisal = "steady";
        public float emotionArousal;
        public float emotionValence = 0.55f;
        public float threatMemory;
        public float allyLossMemory;
        public float successMemory;
        public float orderStressMemory;
    }

    [Serializable]
    public sealed class RomanLogHumanUtilityDto
    {
        public float selfPreservationUtility;
        public float aggressionUtility;
        public float cohesionUtility;
        public float hesitationUtility;
        public float hesitationRemaining;
    }

    [Serializable]
    public sealed class RomanLogPsychologyDto
    {
        public float fear;
        public float anger;
        public float morale = 0.75f;
        public float suppression;
        public float pain;
        public float panic;
        public float bravery = 0.5f;
        public float discipline = 0.5f;
        public float aggression = 0.5f;
        public float selfPreservation = 0.5f;
        public float loyalty = 0.5f;
        public float stressTolerance = 0.5f;
        public float initiative = 0.5f;
        public float fatigue;
        public float confidence = 0.55f;
    }

    [Serializable]
    public sealed class RomanLogModelAuditDto
    {
        public string recommendedAction = "none";
        public float confidence;
        public string acceptedAction = "none";
        public string rejectionReason = "inference_disabled";
        public string auditStatus = "Disabled";
        public bool safetyChecked = true;
        public bool usedPrivilegedInput;
        public bool artifactChecked = true;
        public bool artifactValid;
        public string artifactReason = "inference_disabled";
        public bool dryRun;
        public bool dryRunSafetyPassed;
        public string dryRunSafetyReason = "not_run";
        public bool executorChecked = true;
        public string executorStatus = "NotRequested";
        public string executorPlannedAction = "none";
        public string executorPlannedState = "none";
        public string executorReason = "not_requested";
        public bool canaryChecked = true;
        public string canaryStatus = "NotRequested";
        public string canaryAllowedAction = "none";
        public string canaryReason = "not_requested";
        public bool mutationChecked = true;
        public string mutationStatus = "NotRequested";
        public string mutationReason = "not_requested";
        public string mutationChangedFields = "none";
        public string mutationPolicyAction = "none";
        public string mutationPolicyStatus = "NotRequested";
        public string mutationPolicyAllowedFields = "none";
    }
}
