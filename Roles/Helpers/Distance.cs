namespace TownOfHost.RoleHelpers
{
    public enum Distance
    {
        None = 0,
        Short = 1,
        Normal = 2,
        Long = 3,
        ExtraLong = 4,
        Infinite = 5
    }

    public class DistanceHelper
    {
        public static readonly string[] distanceModes =
        {
            "Distance.None",
            "Distance.Short",
            "Distance.Normal",
            "Distance.Long",
            "Distance.ExtraLong",
            "Distance.Infinite"
            //"SuffixMode.Dev"
        };
        public static Distance GetDistanceFromOption(CustomOption? option)
        {
            return (Distance)option?.GetSelection();
        }
        public static int GetIntFromDistance(Distance distance)
        {
            return (int)distance;
        }
    }
}