namespace ForkBot.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;

    public interface IBotBase
    {
        DiscordSocketClient Client { get; set; }
        ICommandHandler Commands { get; set; }

        Task StartAsync<T>() where T : IBotConfig, new();
        Task HandleConfigsAsync<T>() where T : IBotConfig, new();
        Task InstallCommandsAsync();
        Task LoginAndConnectAsync(TokenType tokenType);
    }

    public interface IBotConfig
    {
        #region Public Fields + Properties

        string BotToken { get; set; }
        ulong LogChannel { get; set; }

        #endregion Public Fields + Properties
    }

    public interface ICommandHandler
    {

        #region Public Fields + Properties

        DiscordSocketClient Client { get; set; }
        CommandService Service { get; set; }

        #endregion Public Fields + Properties

        #region Public Methods

        Task HandleCommandAsync(SocketMessage msg);
        

        #endregion Public Methods
    }

    public interface IServerConfig
    {
        #region Public Fields + Properties

        string CommandPrefix { get; set; }
        Dictionary<string, string> Tags { get; set; }

        #endregion Public Fields + Properties
    }
}