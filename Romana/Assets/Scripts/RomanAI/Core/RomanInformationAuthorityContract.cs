namespace RomanAI
{
    public enum RomanInformationAuthorityLevel
    {
        PrivilegedTruth = 0,
        StaticTerrainTruth = 1,
        PerceivedTruth = 2,
        ReportedTruth = 3,
        InferredTruth = 4,
        StaleTruth = 5,
        Unknown = 6
    }

    public enum RomanKnowledgeSource
    {
        None,
        StaticTerrain,
        Sight,
        Sound,
        Damage,
        AllyReport,
        BehindCoverInference,
        ObjectiveInference,
        DebugPrivileged
    }

    public static class RomanInformationAuthority
    {
        public const string PrivilegedDecisionWarning = "PRIVILEGED_TRUTH_DECISION";

        public static bool CanDriveRuntimeDecision(RomanInformationAuthorityLevel level)
        {
            return level == RomanInformationAuthorityLevel.StaticTerrainTruth
                || level == RomanInformationAuthorityLevel.PerceivedTruth
                || level == RomanInformationAuthorityLevel.ReportedTruth
                || level == RomanInformationAuthorityLevel.InferredTruth
                || level == RomanInformationAuthorityLevel.StaleTruth;
        }

        public static bool CanDriveRuntimeDecision(
            RomanInformationAuthorityLevel level,
            RomanKnowledgeSource source,
            bool privilegedOnly)
        {
            return !IsPrivilegedTruth(level, source, privilegedOnly)
                && CanDriveRuntimeDecision(level);
        }

        public static bool CanDriveEnemyTargeting(RomanInformationAuthorityLevel level)
        {
            return level == RomanInformationAuthorityLevel.PerceivedTruth
                || level == RomanInformationAuthorityLevel.ReportedTruth
                || level == RomanInformationAuthorityLevel.InferredTruth
                || level == RomanInformationAuthorityLevel.StaleTruth;
        }

        public static bool CanDriveEnemyTargeting(
            RomanInformationAuthorityLevel level,
            RomanKnowledgeSource source,
            bool privilegedOnly)
        {
            return !IsPrivilegedTruth(level, source, privilegedOnly)
                && CanDriveEnemyTargeting(level);
        }

        public static bool IsPrivilegedTruth(
            RomanInformationAuthorityLevel level,
            RomanKnowledgeSource source,
            bool privilegedOnly)
        {
            return privilegedOnly
                || level == RomanInformationAuthorityLevel.PrivilegedTruth
                || source == RomanKnowledgeSource.DebugPrivileged;
        }

        public static string ToLogString(RomanInformationAuthorityLevel level)
        {
            return level.ToString();
        }

        public static string ToLogString(RomanKnowledgeSource source)
        {
            return source.ToString();
        }
    }
}
