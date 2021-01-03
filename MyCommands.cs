using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using HSNXT.DSharpPlus.Extended.Emoji;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using unirest_net.http;

namespace _3PL4_bot
{
    public class MyCommands
    {

        [Command("weather")]
        [Description("Tells weather in selected city")]
        public async Task Weather(CommandContext commandInfo, [Description("City name")] string name)
        {
            var response = await Unirest.get(@"http://api.openweathermap.org/data/2.5/weather?q=" + name.ToLower() + @"&units=metric&appid=37d362013655b35b20a7d2f18907b3a2")
                .header("Accept", "application/json")
                .asJsonAsync<String>();

            var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Body);

            var weather = JsonConvert.DeserializeObject<Dictionary<string, string>>(values["weather"].ToString().Replace('[', ' ').Replace(']', ' '));

            var main = JsonConvert.DeserializeObject<Dictionary<string, string>>(values["main"].ToString());

            var description = $"**Description:** {weather["description"]}\n**Temperature:** {main["temp"]} C\n**Feels like:** {main["feels_like"]}C";

            var weatherEmbed = new DiscordEmbedBuilder
            {
                Title = $"{name.ToUpper()} - {weather["main"]}",
                Description = description,
                Color = DiscordColor.CornflowerBlue
            };

            await commandInfo.RespondAsync(embed: weatherEmbed);
            await commandInfo.Message.DeleteAsync();
        }

        [Command("Hi")]
        [Aliases("GutenTag", "Hey", "Yo")]
        [Description("Greets user")]
        public async Task Hi(CommandContext commandInfo)
        {
            await commandInfo.RespondAsync($"{Emoji.Hand} Hi, {commandInfo.User.Mention}!");

            var interactivity = commandInfo.Client.GetInteractivityModule();

            var msg = await interactivity.WaitForMessageAsync(response => response.Author.Id == commandInfo.User.Id && response.Content.ToLower() == "how are you?", TimeSpan.FromMinutes(1));

            if (msg != null)
            {
                await commandInfo.TriggerTypingAsync();
                await Task.Delay(TimeSpan.FromSeconds(3));

                await commandInfo.RespondAsync($"I'm fine, thank you {Emoji.SmileyCat}");
            }


        }

        [Command("random")]
        [Description("Creates random number with given limits")]
        public async Task Random(CommandContext commandInfo, [Description("Smallest possible number")] int min, [Description("Biggest possible number")] int max)
        {
            var rnd = new Random();

            await commandInfo.RespondAsync($"{Emoji.FlowerPlayingCards} Your random number is {rnd.Next(min, max)}");
        }

        [Command("dad")]
        [Aliases("joke", "telljoke")]
        [Description("Writes a cheesy dad joke")]
        public async Task DadJoke(CommandContext commandInfo)
        {
            var response = await Unirest.get(@"https://icanhazdadjoke.com/")
                .header("Accept", "application/json")
                .asJsonAsync<String>();

            DadJoke dad = JsonConvert.DeserializeObject<DadJoke>(response.Body);

            await commandInfo.TriggerTypingAsync();
            await Task.Delay(TimeSpan.FromSeconds(3));

            await commandInfo.RespondAsync($"{Emoji.OldMan} {dad.joke} {Emoji.Laughing}");


            var interactivity = commandInfo.Client.GetInteractivityModule();

            while (true)
            {
                var reactionResult = await interactivity.WaitForReactionAsync(emoji =>
                {
                    Console.WriteLine(emoji.Name == Emoji.Thumbsup);
                    return emoji.Name == Emoji.Thumbsup;
                },
                           TimeSpan.FromSeconds(60)
                            );

                if (reactionResult != null)
                {
                    await commandInfo.RespondAsync($"{Emoji.CocktailGlass} {reactionResult.User.Mention} has good taste!");
                }
            }

        }

        [Command("play")]
        [Aliases("game")]
        [Description("Creates a random number, user has to guess it")]
        public async Task Game(CommandContext commandInfo)
        {
            var number = new Random().Next(0, 100);

            await commandInfo.RespondAsync($"{Emoji.GameDie} I have a number in mind");

            var interactivity = commandInfo.Client.GetInteractivityModule();

            while (true)
            {
                var msg = await interactivity.WaitForMessageAsync(response => response.Author.Id == commandInfo.User.Id, TimeSpan.FromMinutes(5));

                if (msg != null)
                {
                    var guess = Convert.ToInt32(msg.Message.Content);

                    if (guess == number)
                    {
                        await commandInfo.RespondAsync($"{Emoji.Cake} You win!!!");
                        return;
                    }
                    else if (guess > number)
                    {
                        await commandInfo.RespondAsync("Lower");
                    }
                    else
                    {
                        await commandInfo.RespondAsync("Higher");
                    }
                }
            }
        }

        [Command("spoll")]
        [Description("Creates a simple poll with emoji reactions")]
        public async Task SimplePoll(CommandContext commandInfo, [Description("Time for reactions")] TimeSpan duration, [Description("Emoji list")] params DiscordEmoji[] emojiOptions)
        {
            var interactivity = commandInfo.Client.GetInteractivityModule();

            var options = emojiOptions.Select(e => e.ToString());

            var pollEmbed = new DiscordEmbedBuilder
            {
                Title = "Poll",
                Description = string.Join(' ', options),
                Color = DiscordColor.CornflowerBlue
            };

            //give named parameter
            var pollMessage = await commandInfo.Channel.SendMessageAsync(embed: pollEmbed);

            //put possible emojis
            foreach (var option in emojiOptions)
            {
                await pollMessage.CreateReactionAsync(option);
            }

            //store results
            var results = await interactivity.CollectReactionsAsync(pollMessage, duration);

            var resultEmojis = results.Reactions.Select(e => $"{e.Key} {e.Value}");

            await commandInfo.Channel.SendMessageAsync(string.Join('\n', resultEmojis));
        }


        [Command("poll")]
        [Description("Creates a yes/no or multiple choise poll")]
        public async Task Poll(CommandContext commandInfo, [Description("Time for reactions")] TimeSpan duration, [Description("Poll question")] string question, [Description("Asnwer options, non mandatory for yes no questions")] params string[] answers)
        {
            var interactivity = commandInfo.Client.GetInteractivityModule();

            List<DiscordEmoji> optionEmojis = new List<DiscordEmoji>();

            var pollEmbed = new DiscordEmbedBuilder
            {
                Title = $"Poll: {question}",
                Color = DiscordColor.Aquamarine
            };

            if (answers.Length == 0)
            {
                optionEmojis.Add(DiscordEmoji.FromUnicode(commandInfo.Client, Emoji.Thumbsup));
                optionEmojis.Add(DiscordEmoji.FromUnicode(commandInfo.Client, Emoji.Thumbsdown));
            }
            else
            {
                //add emojis
                optionEmojis.Add(DiscordEmoji.FromName(commandInfo.Client, ":one:"));
                optionEmojis.Add(DiscordEmoji.FromName(commandInfo.Client, ":two:"));
                optionEmojis.Add(DiscordEmoji.FromName(commandInfo.Client, ":three:"));
                optionEmojis.Add(DiscordEmoji.FromName(commandInfo.Client, ":four:"));
                optionEmojis.Add(DiscordEmoji.FromName(commandInfo.Client, ":five:"));
                optionEmojis.Add(DiscordEmoji.FromName(commandInfo.Client, ":six:"));
                optionEmojis.Add(DiscordEmoji.FromName(commandInfo.Client, ":seven:"));
                optionEmojis.Add(DiscordEmoji.FromName(commandInfo.Client, ":eight:"));

                var description = "";
                var size = (answers.Length >= 8) ? 8 : answers.Length;

                for (int i = 0; i < size; i++)
                {
                    description += $"{optionEmojis[i]} {answers[i]}\n";
                }

                pollEmbed.Description = description;

                optionEmojis.RemoveRange(size, optionEmojis.Count - size);
            }

            //add toime to poll end
            pollEmbed.Description += duration;

            var pollMessage = await commandInfo.Channel.SendMessageAsync(embed: pollEmbed);

            //put possible emojis
            foreach (var option in optionEmojis)
            {
                await pollMessage.CreateReactionAsync(option);
            }

            //store results
            var result = interactivity.CollectReactionsAsync(pollMessage, duration);

            //count down
            while (!result.IsCompleted)
            {
                var index = pollEmbed.Description.LastIndexOf('\n');

                duration = duration.Subtract(TimeSpan.FromSeconds(1));

                pollEmbed.Description = $"{pollEmbed.Description.Substring(0, index)}\n{duration}";

                pollMessage.ModifyAsync(embed: pollEmbed);

                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            var values = result.Result;
            var statistics = values.Reactions.Select(e => $"{e.Key} {e.Value}");
            await commandInfo.Channel.SendMessageAsync(string.Join('\n', statistics));
        }


        [Command("dm_me")]
        [Description("Dms user a greeting")]
        public async Task Dm(CommandContext commandInfo)
        {
            var dm = await commandInfo.Client.CreateDmAsync(commandInfo.User);

            await dm.SendMessageAsync($"Hey there! {Emoji.WavingHand}");
        }

        [Command("dm")]
        [Description("Dms some user given message")]
        public async Task Dm(CommandContext commandInfo, [Description("Message to be send")] string message, [Description("Recipient")] DiscordUser user)
        {
            var dm = await commandInfo.Client.CreateDmAsync(user);

            await dm.SendMessageAsync(message);
            await commandInfo.Message.DeleteAsync();
        }


        [Command("corona")]
        [Description("Get info about corona cases in selected country")]
        public async Task Corona(CommandContext commandInfo, [Description("Country name")] string name)
        {
            var response = await Unirest.get(@"https://api.covid19api.com/live/country/" + name.ToLower())
                .header("Accept", "application/json")
                .asJsonAsync<String>();

            var countries = JsonConvert.DeserializeObject<List<CoronaCountry>>(response.Body);

            var sumDead = countries.Sum(c => c.Deaths);
            var sumRecovered = countries.Sum(c => c.Recovered);
            var sumActive = countries.Sum(c => c.Active);
            var sumConfirmed = countries.Sum(c => c.Confirmed);

            var country = countries[0];

            await commandInfo.RespondAsync($"{country.Country} :flag_{country.CountryCode.ToLower()}:\n{Emoji.SneezingFace}Active:{sumActive} \n{Emoji.Skull}Died: {sumDead} \n{Emoji.Mask}Recovered:{sumRecovered}\n{country.Date}");


        }
    }
}
