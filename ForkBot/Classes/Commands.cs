using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
//using VideoLibrary;

namespace ForkBot
{
    public class Commands : ModuleBase
    {
        Random rdm = new Random();

        [Command("help"), Summary("Displays commands and descriptions.")]
        public async Task Help()
        {
            JEmbed emb = new JEmbed();
            emb.Author.Name = "ForkBot Commands";
            emb.ThumbnailUrl = Context.User.AvatarId;

            if (Context.Guild != null) emb.ColorStripe = Functions.GetColor(Context.User);
            else emb.ColorStripe = Constants.Colours.DEFAULT_COLOUR;

            foreach (CommandInfo command in Bot.commands.Commands)
            {
                emb.Fields.Add(new JEmbedField(x =>
                {
                    string header = command.Name;
                    foreach (String alias in command.Aliases)
                    {
                        if (alias != command.Name) header += " (;" + alias + ") ";
                    }

                    foreach (ParameterInfo parameter in command.Parameters)
                    {
                        header += " [" + parameter.Name + "]";
                    }
                    x.Header = header;
                    x.Text = command.Summary;
                }));
            }

            await Context.Channel.SendMessageAsync("", embed: emb.Build());
            //await Context.User.SendMessageAsync("", embed: emb.Build());
            //await Context.Channel.SendMessageAsync("Commands have been sent to you privately!");
            
        }

        /*[Command("play"), Summary("Play a song from Youtube.")]
        public async Task Play(string song)
        {

            var youTube = YouTube.Default; // starting point for YouTube actions
            var video = await youTube.GetVideoAsync(song); // gets a Video object with info about the video
            File.WriteAllBytes(@"Video\" + video.FullName, video.GetBytes());
            
        }*/


        [Command("hangman"), Summary("Play a game of Hangman with the bot."), Alias(new string[] {"hm"})]
        public async Task HangMan()
        {
            if (!Bot.hangman)
            {
                var wordList = File.ReadAllLines("Files/wordlist.txt");
                Bot.hmWord = wordList[(rdm.Next(wordList.Count()))].ToLower();
                Bot.hangman = true;
                Bot.hmCount = 0;
                Bot.hmErrors = 0;
                Bot.guessedChars.Clear();
                await HangMan("");
            }
            else
            {
                await Context.Channel.SendMessageAsync("There is already a game of HangMan running.");
            }
        }

        [Command("hangman"), Alias(new string[] { "hm" })]
        public async Task HangMan(string guess)
        {
            guess = guess.ToLower();
            if (guess != "" && Bot.guessedChars.Contains(guess[0]) && guess.Count() == 1) await Context.Channel.SendMessageAsync("You've already guessed " + Char.ToUpper(guess[0]));
            else
            {
                if (guess.Count() == 1 && !Bot.guessedChars.Contains(guess[0])) Bot.guessedChars.Add(guess[0]);
                if (guess != "" && ((!Bot.hmWord.Contains(guess[0]) && guess.Count() == 1) || (Bot.hmWord != guess && guess.Count() > 1))) Bot.hmErrors++;
                

                string[] hang = {
            "       ______   " ,    //0
            "      /      \\  " ,   //1
            "     |          " ,    //2
            "     |          " ,    //3
            "     |          " ,    //4
            "     |          " ,    //5
            "     |          " ,    //6
            "_____|_____     " };   //7

                for (int i = 0; i < Bot.hmWord.Count(); i++)
                {
                    if (Bot.guessedChars.Contains(Bot.hmWord[i])) hang[6] += Char.ToUpper(Convert.ToChar(Bot.hmWord[i])) + " ";
                    else hang[6] += "_ ";
                }

                for (int i = 0; i < Bot.hmErrors; i++) 
                {
                    if (i == 0)
                    {
                        var line = hang[2].ToCharArray();
                        line[13] = 'O';
                        hang[2] = new string(line);
                    }
                    if (i == 1)
                    {
                        var line = hang[3].ToCharArray();
                        line[13] = '|';
                        hang[3] = new string(line);
                    }
                    if (i == 2)
                    {
                        var line = hang[4].ToCharArray();
                        line[12] = '/';
                        hang[4] = new string(line);
                    }
                    if (i == 3)
                    {
                        var line = hang[4].ToCharArray();
                        line[14] = '\\';
                        hang[4] = new string(line);
                    }
                    if (i == 4)
                    {
                        var line = hang[3].ToCharArray();
                        line[12] = '/';
                        hang[3] = new string(line);
                    }
                    if (i == 5)
                    {
                        var line = hang[3].ToCharArray();
                        line[14] = '\\';
                        hang[3] = new string(line);
                    }
                }

                if (!hang[6].Contains("_") || Bot.hmWord == guess) //win
                {
                    await Context.Channel.SendMessageAsync("You did it!");
                    Bot.hangman = false;

                    var u = Functions.GetUser(Context.User);
                    u.Coins += 10;
                    Functions.SaveUsers();
                }

                if (Bot.hmErrors == 6)
                {
                    await Context.Channel.SendMessageAsync("You lose! The word was: " + Bot.hmWord);
                    Bot.hangman = false;
                }

                string msg = "```\n";
                foreach (String s in hang) msg += s + "\n";
                msg += "```";
                if (Bot.hangman)
                {
                    msg += "Guessed letters: ";
                    foreach (char c in Bot.guessedChars) msg += char.ToUpper(c) + " ";
                    msg += "\nUse `;hangman [guess]` to guess a character or the entire word.\n ~~hint hint the word is " + Bot.hmWord + "~~";

                }
                await Context.Channel.SendMessageAsync(msg);
            }

        }

        [Command("profile"), Summary("View the your or another users profile.")]
        public async Task Profile(IUser user)
        {
            var u = Functions.GetUser(user);

            var emb = new JEmbed();
            emb.Author.Name = user.Username;
            emb.Author.IconUrl = user.GetAvatarUrl();

            emb.ColorStripe = Functions.GetColor(user);

            emb.Fields.Add(new JEmbedField(x =>
            {
                x.Header = "Coins:";
                x.Text = Convert.ToString(u.Coins);
                x.Inline = true;
            }));

            emb.Fields.Add(new JEmbedField(x =>
            {
                x.Header = "Roles:";
                string text = "";
                foreach(ulong id in (Context.User as IGuildUser).RoleIds)
                {
                    text += Context.Guild.GetRole(id).Name + "\n";
                }

                x.Text = Convert.ToString(text);
                x.Inline = true;
            }));

            await Context.Channel.SendMessageAsync("", embed: emb.Build());
        }

        [Command("profile")]
        public async Task Profile() { await Profile(Context.User); }

        [Command("listusers")]
        public async Task Listusers()
        {
            string msg = "```\n";
            foreach(User u in Bot.users)
            {
                msg += u.Username() + " " + u.ID + ". Coins: " + u.Coins + "\n";
            }
            msg += "```";
            await Context.Channel.SendMessageAsync(msg);
        }
    }
}
