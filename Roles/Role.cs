
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace TownOfHost
{
    public abstract class Role
    {
        protected internal bool Loaded { get; set; }
    }
    public abstract class Crewmate : Role
    {
    }
    public class Medium : Crewmate
    {
        public Medium(PlayerControl player)
        {
            Loaded = true;
        }
    }
}