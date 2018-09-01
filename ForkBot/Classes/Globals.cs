namespace ForkBot
{
    using System.Collections.Generic;
    using Interfaces;

    public static class Globals
    {
        public static List<string> EvalImports { get; } = new List<string> {
            "Discord",
            "Discord.API",
            "Discord.Commands",
            "Discord.WebSocket",
            "System",
            "System.Collections",
            "System.Collections.Generic",
            "System.Diagnostics",
            "System.IO",
            "System.Linq",
            "System.Math",
            "System.Reflection",
            "System.Runtime",
            "System.Threading.Tasks",
            "ForkBot"
        };
    }
}