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
                        header += "(;" + alias + ") ";
                    }

                    foreach (ParameterInfo parameter in command.Parameters)
                    {
                        header += " [" + parameter.Name + "]";
                    }
                    x.Header = header;
                    x.Text = command.Summary;
                }));
            }

            await Context.User.SendMessageAsync("", embed: emb.Build());
            await Context.Channel.SendMessageAsync("Commands have been sent to you privately!");

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
                Bot.hmWord = wordList[(rdm.Next(wordList.Count()))];
                Bot.hangman = true;
                Bot.hmCount = 0;
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

            if (guess != "" && Bot.guessedChars.Contains(guess[0])) await Context.Channel.SendMessageAsync("You've already guessed " + Char.ToUpper(guess[0]));
            else
            {
                if (guess.Count() == 1 && !Bot.guessedChars.Contains(guess[0])) Bot.guessedChars.Add(guess[0]);
                else if (Bot.hmWord == guess)
                {
                    Bot.hangman = false;
                    await Context.Channel.SendMessageAsync("You did it!");
                    //add coins to user
                }

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

                if (!hang[6].Contains("_")) //win
                {
                    await Context.Channel.SendMessageAsync("You did it!");
                    Bot.hangman = false;
                    //add coins to user
                }

                string msg = "```\n";
                foreach (String s in hang) msg += s + "\n";
                msg += "```";
                if (Bot.hangman)
                {
                    msg += "Guessed letters: ";
                    foreach (char c in Bot.guessedChars) msg += char.ToUpper(c) + " ";
                    msg += "\nUse `;hangman [guess]` to guess a character or the entire word.";

                }
                await Context.Channel.SendMessageAsync(msg);
            }

        }
    }
}
