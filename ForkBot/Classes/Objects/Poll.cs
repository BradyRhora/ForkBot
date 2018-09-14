using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ForkBot
{
    public class Poll
    {
        string question;
        string[] options;
        int[] votes;
        int minutes;
        DateTime startTime;
        public List<ulong> voted = new List<ulong>();
        public bool completed = false;
        Timer endTimer;
        Discord.IMessageChannel pollChannel;

        public Poll(Discord.IMessageChannel channel, int minutes,  string question, string[] options)
        {
            completed = false;
            this.minutes = minutes;
            this.question = question;
            this.options = options;
            pollChannel = channel;
            startTime = Var.CurrentDate();
            votes = new int[options.Count()];
            for(int i = 0; i < votes.Count(); i++) votes[i] = 0;
            endTimer = new Timer(new TimerCallback(End), null, 1000 * 60 * minutes, Timeout.Infinite);
        }

        public Discord.Embed GenerateEmbed()
        {
            JEmbed emb = new JEmbed();
            emb.Author.Name = "POLL";
            emb.ColorStripe = Constants.Colours.YORK_RED;
            emb.Description = question;

            char letter = 'a';
            foreach(string o in options)
            {
                emb.Description += $"\n:regional_indicator_{letter}:: {o} - [{votes[Char.ToLower(letter) - 'a']} votes]";
                letter++;
            }

            if (!completed) emb.Footer.Text = "Vote with ;poll vote [choice]!";
            else emb.Footer.Text = "Poll completed.";
            return emb.Build();
        }

        public void Vote(char choice)
        {
            int index = Char.ToLower(choice) - 'a';
            votes[index]++;
        }

        public bool HasVoted(ulong id)
        {
            if (voted.Contains(id)) return true;
            else return false;
        }

        public async void End(object state)
        {
            completed = true;
            await pollChannel.SendMessageAsync("Poll Completed! Here are the results.", embed: GenerateEmbed());
        }
    }
}
