namespace ForkBot
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Discord.Commands;
    using Discord.WebSocket;
    using Microsoft.CodeAnalysis.CSharp.Scripting;
    using Microsoft.CodeAnalysis.Scripting;

    #endregion

    public static class EvalService
    {
        #region Private Fields + Properties

        public static IEnumerable<Assembly> Assemblies => GetAssemblies();
        public static IEnumerable<string> Imports => ForkBot.Globals.EvalImports;

        #endregion Private Fields + Properties   

        #region Public Methods

        public static async Task<string> EvaluateAsync(CommandContext Context, string script)
        {
           
            using (Context.Channel.EnterTypingState())
            {
                var options = ScriptOptions.Default.AddReferences(Assemblies).AddImports(Imports);
                var _globals = new ScriptGlobals { client = Context.Client as DiscordSocketClient, Context = Context };
                script = script.Trim('`');
                try
                {
                    var eval = await CSharpScript.EvaluateAsync(script, options, _globals, typeof(ScriptGlobals));
                    return eval.ToString();
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            }
        }

        public static IEnumerable<Assembly> GetAssemblies()
        {
            var Assemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies();
            foreach (var a in Assemblies)
            {
                var asm = Assembly.Load(a);
                yield return asm;
            }
            yield return Assembly.GetEntryAssembly();
            yield return typeof(ILookup<string, string>).GetTypeInfo().Assembly;
        }

        #endregion Public Methods
    }

    public class ScriptGlobals
    {
        #region Public Fields + Properties

        public SocketGuildChannel channel => Context.Channel as SocketGuildChannel;
        public DiscordSocketClient client { get; internal set; }
        public CommandContext Context { get; internal set; }
        public SocketGuild guild => Context.Guild as SocketGuild;
        public SocketMessage msg => Context.Message as SocketMessage;

        #endregion Public Fields + Properties
    }
}