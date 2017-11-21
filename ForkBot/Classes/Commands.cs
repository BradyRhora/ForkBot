using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
//using VideoLibrary;
using DuckDuckGo.Net;
using OxfordDictionariesAPI;
using HtmlAgilityPack;

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
        
        [Command("hangman"), Summary("Play a game of Hangman with the bot."), Alias(new string[] { "hm" })]
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
            if (Bot.hangman)
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
                        msg += "\nUse `;hangman [guess]` to guess a character or the entire word.";

                    }
                    await Context.Channel.SendMessageAsync(msg);
                }
            }
            else
            {
                await HangMan();
                await HangMan(guess);
            }
        }

        [Command("profile"), Summary("View your or another users profile.")]
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
                foreach (ulong id in (Context.User as IGuildUser).RoleIds)
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
            foreach (User u in Bot.users)
            {
                msg += u.Username() + " " + u.ID + ". Coins: " + u.Coins + "\n";
            }
            msg += "```";
            await Context.Channel.SendMessageAsync(msg);
        }

        [Command("present"), Summary("Get a cool gift!")]
        public async Task Present()
        {
            if (!Bot.presentWaiting)
            {
                Bot.presentNum = rdm.Next(10);
                await Context.Channel.SendMessageAsync($"A present appears! :gift: Press {Bot.presentNum} to open it!");
                Bot.presentWaiting = true;
                Bot.replacing = false;
                Bot.replaceable = true;
            }
        }

        [Command("whatis"), Alias(new string[] { "wi" }), Summary("Don't know what something is? Find out!")]
        public async Task WhatIs([Remainder]string thing)
        {
            var results = new Search().Query(thing, "ForkBot");
            QueryResult result = null;
            if (results.Abstract == "" && results.RelatedTopics.Count > 0) result = results.RelatedTopics[0];

            if (result != null)
            {
                JEmbed emb = new JEmbed();
                emb.Title = thing;
                emb.Description = result.Text;
                emb.ImageUrl = result.Icon.Url;
                await Context.Channel.SendMessageAsync("", embed: emb.Build());
            }
            else await Context.Channel.SendMessageAsync("No results found!");

        }

        [Command("define"), Summary("Returns the definiton for the inputted word.")]
        public async Task Define([Remainder]string word)
        {
            OxfordDictionaryClient client = new OxfordDictionaryClient("45278ea9", "c4dcdf7c03df65ac5791b67874d956ce");
            var result = await client.SearchEntries(word, CancellationToken.None);
            if (result != null)
            {
                var senses = result.Results[0].LexicalEntries[0].Entries[0].Senses[0];

                JEmbed emb = new JEmbed();
                emb.Title = func.ToTitleCase(word);
                emb.Description = senses.Definitions[0];
                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Examples:";
                    string text = "";
                    foreach (OxfordDictionariesAPI.Models.Example eg in senses.Examples)
                    {
                        text += $"\"{eg.Text}\"\n";
                    }
                    x.Text = text;
                }));

                await Context.Channel.SendMessageAsync("", embed: emb.Build());
            }
            else await Context.Channel.SendMessageAsync($"Could not find definition for: {word}.");
        }

        [Command("professor"), Alias(new string[] {"prof","rmp"}), Summary("Check out a professors rating from RateMyProfessors.com!")]
        public async Task Professor([Remainder]string name)
        {
            HtmlWeb web = new HtmlWeb();
            
            string link = "http://www.ratemyprofessors.com/search.jsp?query=" + name.Replace(" ","%20");
            var page = web.Load(link);
            var node = page.DocumentNode.SelectSingleNode("//*[@id=\"searchResultsBox\"]/div[2]/ul/li[1]");
            if (node != null)
            {
                string tid = Functions.GetTID(node.InnerHtml);

                var newLink = "http://www.ratemyprofessors.com/ShowRatings.jsp?tid=" + tid;
                page = web.Load(newLink);

                var rating = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[3]/div[1]/div/div[1]/div/div/div").InnerText;
                var takeAgain = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[3]/div[1]/div/div[2]/div[1]/div").InnerText;
                var difficulty = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[3]/div[1]/div/div[2]/div[2]/div").InnerText;
                var imageNode = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[1]/div[2]/div[1]/div[1]/img");
                var titleText = page.DocumentNode.SelectSingleNode("/html/head/title").InnerText;
                string profName = titleText.Split(' ')[0] + " "+ titleText.Split(' ')[1];

                var tagsNode = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[3]/div[2]/div[2]");
                List<string> tags = new List<string>();
                for (int i = 0; i < tagsNode.ChildNodes.Count(); i++)
                {
                    if (tagsNode.ChildNodes[i].Name == "span") tags.Add(tagsNode.ChildNodes[i].InnerText);
                }

                var hotness = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[3]/div[1]/div/div[2]/div[3]/div/figure/img").Attributes[0].Value;
                var hotnessIMG = "http://www.ratemyprofessors.com" + hotness;
                string imageURL = null;
                if (imageNode != null) imageURL = imageNode.Attributes[0].Value;

                var commentsNode = page.DocumentNode.SelectSingleNode("/ html[1] / body[1] / div[2] / div[4] / div[3] / div[1] / div[7] / table[1]");

                List<string> comments = new List<string>();
                for (int i = 3; i<commentsNode.ChildNodes.Count();i++)
                {
                    if (commentsNode.ChildNodes[i].Name=="tr" && commentsNode.ChildNodes[i].Attributes.Count() ==2)
                    {
                        comments.Add(commentsNode.ChildNodes[i].ChildNodes[5].ChildNodes[3].InnerText.Replace("\r\n               ",""));
                    }
                }
                List<string> words = new List<string>();
                List<int> counts = new List<int>();

                foreach (string comment in comments)
                {
                    foreach (string word in comment.Split(' '))
                    {
                        if (words.Contains(word)) counts[words.IndexOf(word)]++;
                        else
                        {
                            words.Add(word);
                            counts.Add(1);
                        }
                    }
                }

                List<string> OrderedWords = new List<string>();
                for (int i = counts.Max(); i >= 0; i--)
                {
                    for (int c = 0; c < counts.Count(); c++)
                    {
                        if (counts[c] == i)
                        {
                            OrderedWords.Add(words[counts.IndexOf(counts[c])]);
                        }
                    }
                }


                JEmbed emb = new JEmbed();

                emb.Title = profName;
                if (imageURL != null) emb.ImageUrl = imageURL;
                emb.ThumbnailUrl = hotnessIMG;
                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Rating:";
                    x.Text = rating;
                    x.Inline = true;
                }));

                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Difficulty:";
                    x.Text = difficulty;
                    x.Inline = true;
                }));

                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Would take again?:";
                    x.Text = takeAgain;
                    x.Inline = true;
                }));

                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Top Tags:";
                    string text = "";
                    foreach(string s in tags)
                    {
                        text += s;
                    }
                    x.Text = text;
                    x.Inline = false;
                }));

                emb.ColorStripe = Constants.Colours.YORK_RED;
                await Context.Channel.SendMessageAsync("", embed: emb.Build());
            } 
            else
            {
                await Context.Channel.SendMessageAsync("Professor not found!");
            }
        }
    }
}
