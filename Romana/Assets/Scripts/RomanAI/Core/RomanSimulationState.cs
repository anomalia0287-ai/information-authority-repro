using System;

namespace RomanAI
{
    public enum RomanSoldierState
    {
        Unknown,
        Idle,
        Patrol,
        Suspicious,
        Search,
        InvestigateHide,
        Combat,
        MoveToCover,
        InCover,
        Reload,
        Reposition,
        Retreat,
        Dead
    }

    [Serializable]
    public struct RomanCoreVector3
    {
        public float x;
        public float y;
        public float z;

        public RomanCoreVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    [Serializable]
    public sealed class RomanSimulationUnitState
    {
        public string unitId = "none";
        public RomanTeam team = RomanTeam.BLUFOR;
        public RomanSoldierState soldierState = RomanSoldierState.Unknown;
        public string stateName = RomanSoldierStateContract.UnknownName;
        public string stateReason = "none";
        public string role = "none";
        public string objectiveMode = "hold";
        public string currentActionType = "hold";
        public string decisionReason = "none";
        public RomanCoreVector3 position;
        public RomanCoreVector3 forward;
        public float health;
        public float maxHealth;
        public float health01;
        public int ammo;
        public int maxAmmo;
        public bool isAlive;
        public RomanDebugWarningId debugWarning = RomanDebugWarningId.None;
    }

    public static class RomanSoldierStateContract
    {
        public const string UnknownName = "Unknown";
        public const string IdleName = "Idle";
        public const string PatrolName = "Patrol";
        public const string SuspiciousName = "Suspicious";
        public const string SearchName = "Search";
        public const string InvestigateHideName = "InvestigateHide";
        public const string CombatName = "Combat";
        public const string MoveToCoverName = "MoveToCover";
        public const string InCoverName = "InCover";
        public const string ReloadName = "Reload";
        public const string RepositionName = "Reposition";
        public const string RetreatName = "Retreat";
        public const string DeadName = "Dead";

        public static bool TryParseName(string name, out RomanSoldierState state)
        {
            switch (name)
            {
                case IdleName:
                    state = RomanSoldierState.Idle;
                    return true;
                case PatrolName:
                    state = RomanSoldierState.Patrol;
                    return true;
                case SuspiciousName:
                    state = RomanSoldierState.Suspicious;
                    return true;
                case SearchName:
                    state = RomanSoldierState.Search;
                    return true;
                case InvestigateHideName:
                    state = RomanSoldierState.InvestigateHide;
                    return true;
                case CombatName:
                    state = RomanSoldierState.Combat;
                    return true;
                case MoveToCoverName:
                    state = RomanSoldierState.MoveToCover;
                    return true;
                case InCoverName:
                    state = RomanSoldierState.InCover;
                    return true;
                case ReloadName:
                    state = RomanSoldierState.Reload;
                    return true;
                case RepositionName:
                    state = RomanSoldierState.Reposition;
                    return true;
                case RetreatName:
                    state = RomanSoldierState.Retreat;
                    return true;
                case DeadName:
                    state = RomanSoldierState.Dead;
                    return true;
                default:
                    state = RomanSoldierState.Unknown;
                    return false;
            }
        }

        public static string ToLogName(RomanSoldierState state)
        {
            switch (state)
            {
                case RomanSoldierState.Idle:
                    return IdleName;
                case RomanSoldierState.Patrol:
                    return PatrolName;
                case RomanSoldierState.Suspicious:
                    return SuspiciousName;
                case RomanSoldierState.Search:
                    return SearchName;
                case RomanSoldierState.InvestigateHide:
                    return InvestigateHideName;
                case RomanSoldierState.Combat:
                    return CombatName;
                case RomanSoldierState.MoveToCover:
                    return MoveToCoverName;
                case RomanSoldierState.InCover:
                    return InCoverName;
                case RomanSoldierState.Reload:
                    return ReloadName;
                case RomanSoldierState.Reposition:
                    return RepositionName;
                case RomanSoldierState.Retreat:
                    return RetreatName;
                case RomanSoldierState.Dead:
                    return DeadName;
                default:
                    return UnknownName;
            }
        }

        public static RomanSoldierState ResolveState(string action, string objective, string source, string requestedState)
        {
            if (TryParseName(requestedState, out RomanSoldierState explicitState))
            {
                return explicitState;
            }

            if (string.Equals(action, "dead", StringComparison.OrdinalIgnoreCase)
                || string.Equals(requestedState, DeadName, StringComparison.OrdinalIgnoreCase))
            {
                return RomanSoldierState.Dead;
            }

            if (string.Equals(action, "engage_visible_enemy", StringComparison.OrdinalIgnoreCase)
                || string.Equals(objective, "visible_enemy", StringComparison.OrdinalIgnoreCase))
            {
                return RomanSoldierState.Combat;
            }

            if (string.Equals(action, "bandaging", StringComparison.OrdinalIgnoreCase)
                || string.Equals(action, "player_reload", StringComparison.OrdinalIgnoreCase)
                || ContainsToken(requestedState, "Reload"))
            {
                return RomanSoldierState.Reload;
            }

            if (string.Equals(action, "medical_retreat", StringComparison.OrdinalIgnoreCase)
                || string.Equals(objective, "emotional_retreat", StringComparison.OrdinalIgnoreCase)
                || string.Equals(source, "own_base", StringComparison.OrdinalIgnoreCase)
                || ContainsToken(requestedState, "Retreat"))
            {
                return RomanSoldierState.Retreat;
            }

            if (string.Equals(action, "team_cover", StringComparison.OrdinalIgnoreCase)
                || string.Equals(source, "cover", StringComparison.OrdinalIgnoreCase)
                || ContainsToken(requestedState, "Cover"))
            {
                return RomanSoldierState.MoveToCover;
            }

            if (string.Equals(action, "local_self_preservation", StringComparison.OrdinalIgnoreCase)
                && (string.Equals(objective, "panic_hesitation", StringComparison.OrdinalIgnoreCase)
                    || ContainsToken(requestedState, "Hesitation")))
            {
                return RomanSoldierState.Suspicious;
            }

            if (string.Equals(action, "objective_move", StringComparison.OrdinalIgnoreCase)
                || string.Equals(action, "player_move", StringComparison.OrdinalIgnoreCase)
                || string.Equals(action, "recover_from_stuck", StringComparison.OrdinalIgnoreCase)
                || ContainsToken(requestedState, "Move")
                || ContainsToken(requestedState, "Advance")
                || ContainsToken(requestedState, "Push")
                || ContainsToken(requestedState, "Regroup")
                || ContainsToken(requestedState, "Role"))
            {
                return RomanSoldierState.Reposition;
            }

            if (string.Equals(action, "player_control", StringComparison.OrdinalIgnoreCase)
                || string.Equals(action, "hold", StringComparison.OrdinalIgnoreCase)
                || ContainsToken(requestedState, "Hold"))
            {
                return RomanSoldierState.Idle;
            }

            return RomanSoldierState.Unknown;
        }

        public static string NormalizeStateName(string action, string objective, string source, string requestedState)
        {
            return ToLogName(ResolveState(action, objective, source, requestedState));
        }

        public static string NormalizeStateReason(string requestedState)
        {
            return string.IsNullOrWhiteSpace(requestedState) ? "none" : requestedState;
        }

        private static bool ContainsToken(string value, string token)
        {
            return !string.IsNullOrEmpty(value)
                && value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
