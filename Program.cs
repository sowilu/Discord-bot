using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using HSNXT.DSharpPlus.Extended.Emoji;
using DSharpPlus.Interactivity;
using unirest_net.http;
using Newtonsoft.Json;
using DSharpPlus.Entities;
using System.Linq;
using System.Collections.Generic;

namespace _3PL4_bot
{
    class Program
    {
        static DiscordClient discord;
        static CommandsNextModule commands;
        static InteractivityModule interactivity;

        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {

            discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = "",//TODO insert your bot token here
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            });

            discord.MessageCreated += async e =>
            {
                if (e.Message.Content.ToLower().Contains("ping"))
                    await e.Message.RespondAsync("pong!");
            };

            commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefix = ";;",
                CaseSensitive = false
            });
            commands.RegisterCommands<MyCommands>();

            interactivity = discord.UseInteractivity(new InteractivityConfiguration());

            await discord.ConnectAsync();

            await Task.Delay(-1);
        }
    }
}
