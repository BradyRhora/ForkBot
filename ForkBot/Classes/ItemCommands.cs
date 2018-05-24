using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace ForkBot
{
    public class ItemCommands : ModuleBase
    {
        Random rdm = new Random();

        public bool Check(ICommandContext Context, string item, bool remove = true)
        {
            var user = Functions.GetUser(Context.User);
            if (user.GetItemList().Contains(item))
            {
                if (remove) user.RemoveItem(item);
                return false;
            }
            return true;
        }

        [Command("poop")]
        public async Task Poop()
        {
            if (Check(Context, "poop")) return;
            int r = rdm.Next(100) + 1;
            var user = Functions.GetUser(Context.User);
            string msg;
            if (r < 50)
            {
                msg = "You take a successful poop. It flushes nicely and you wash your hands.\n**Hygiene+20**";
                user.AddData("stat.hygiene", 20);
            }
            else if (r < 75)
            {
                msg = "You've been holding this in for a while.. Poop gets everywhere and is a pain to clean up.\n**Hygiene-10**";
                user.AddData("stat.hygiene", -10);
            }
            else
            {
                msg = "This... this is the best poop you've ever had in your life! You feel fantastic!\n**Hygiene+40 Happiness+20";
                user.AddData("stat.hygiene", 40);
                user.AddData("stat.happiness", 20);
            }
            await Context.Channel.SendMessageAsync(":poop: | " + msg);
        }

        [Command("shirt")]
        public async Task Shirt()
        {
            if (Check(Context, "shirt")) return;
            await Context.Channel.SendMessageAsync(":shirt: Lookin' good!\n**Fashion+10**");
            Functions.GetUser(Context.User).AddData("stat.fashion", 10);
        }

        [Command("dress")]
        public async Task Dress()
        {
            if (Check(Context, "dress")) return;
            await Context.Channel.SendMessageAsync(":dress: So beautiful!\n**Fashion+25**");
            Functions.GetUser(Context.User).AddData("stat.fashion", 25);
        }

        [Command("high_heel")]
        public async Task HighHeel()
        {
            if (Check(Context, "high_heel")) return;
            await Context.Channel.SendMessageAsync(":high_heel: You feel fabulous.\n**Fashion+10**");
            Functions.GetUser(Context.User).AddData("stat.fashion", 10);
        }
        
        [Command("athletic_shoe")]
        public async Task AthleticShoe()
        {
            if (Check(Context, "athletic_shoe")) return;
            await Context.Channel.SendMessageAsync(":athletic_shoe: You go for a nice run!\n**Fitness+20**");
            Functions.GetUser(Context.User).AddData("stat.fitness", 20);
        }
        
        [Command("dark_sunglasses")]
        public async Task DarkSunglasses()
        {
            if (Check(Context, "dark_sunglasses")) return;
            await Context.Channel.SendMessageAsync(":dark_sunglasses: You equip your sunglasses... and get a whole lot cooler.\n**Fashion+20**");
            Functions.GetUser(Context.User).AddData("stat.fashion", 20);
        }
        
        /*[Command("eyeglasses")]
        public async Task ITEM()
        {
            if (Check(Context, "item")) return;
            await Context.Channel.SendMessageAsync(":item: msg\n**Stat+0**");
            Functions.GetUser(Context.User).AddData("stat.stat", 0);
        }
        
        [Command("umbrella2")]
        public async Task ITEM()
        {
            if (Check(Context, "item")) return;
            await Context.Channel.SendMessageAsync(":item: msg\n**Stat+0**");
            Functions.GetUser(Context.User).AddData("stat.stat", 0);
        }*/

        [Command("briefcase")]
        public async Task Briefcase()
        {
            if (Check(Context, "briefcase")) return;

            int r = rdm.Next(50)+1;
            string msg;
            int amount;
            if (r < 20) amount = rdm.Next(100) + 1;
            else amount = rdm.Next(100, 1000) + 1;

            await Context.Channel.SendMessageAsync($":briefcase: There's money inside the briefcase!\n**+{amount} coins**");
            Functions.GetUser(Context.User).GiveCoins(amount);
        }

        [Command("purse")]
        public async Task Purse()
        {
            if (Check(Context, "purse")) return;

            int r = rdm.Next(50) + 1;
            string msg;
            int amount;
            if (r < 20) amount = rdm.Next(50) + 1;
            else amount = rdm.Next(50, 200) + 1;

            await Context.Channel.SendMessageAsync($":purse: There's money inside! You also look great!\n**+{amount} coins Fashion+5**");
            var u = Functions.GetUser(Context.User);
            u.GiveCoins(amount);
            u.AddData("stat.fashion", 5);
        }

        [Command("gift")]
        public async Task Gift()
        {
            if (Check(Context, "gift")) return;
            var user = Functions.GetUser(Context.User);

            await Context.Channel.SendMessageAsync($"{Context.User.Username}! You got...");
            var presents = Functions.GetItemList();
            int presRDM = rdm.Next(presents.Count());
            var presentData = presents[presRDM].Split('|');
            Var.present = presentData[0];
            Var.rPresent = Var.present;
            var presentName = Var.present.Replace('_', ' ');
            var pMessage = presentData[1];
            await Context.Channel.SendMessageAsync($"A {Func.ToTitleCase(presentName)}! {Functions.GetItemEmote(presents[presRDM])} {pMessage}");
            if (Var.present == "santa")
            {
                await Context.Channel.SendMessageAsync("You got...");
                string sMessage = "";
                for (int i = 0; i < 5; i++)
                {
                    var sPresentData = presents[rdm.Next(presents.Count())];
                    string sPresentName = sPresentData.Split('|')[0];
                    user.GiveItem(sPresentName);
                    sMessage += $"A {Func.ToTitleCase(sPresentName)}! {Functions.GetItemEmote(sPresentData)} {sPresentData.Split('|')[1]}\n";
                }
                await Context.Channel.SendMessageAsync(sMessage);

                Var.replaceable = false;
            }
            else user.GiveItem(Var.present);
        }

        [Command("roll")]
        public async Task Roll(int max = 6)
        {
            if (Check(Context, "game_die", false)) return;
            await Context.Channel.SendMessageAsync(":game_die: " + Convert.ToString(rdm.Next(max) + 1));
        }

        [Command("8ball")]
        public async Task EightBall([Remainder] string question = "")
        {
            if (Check(Context, "8ball", false)) return;
            string[] answers = { "Yes", "No", "Unlikely", "Chances good", "Likely", "Lol no", "If you believe", "Ask Brady" };
            await Context.Channel.SendMessageAsync(":8ball: " + answers[rdm.Next(answers.Count())]);
            
        }

        [Command("gun"), Alias(new string[] { "rob" })]
        public async Task Gun() { if (Check(Context, "gun", false)) return; await Context.Channel.SendMessageAsync("Choose someone to rob with `;gun [user]`..."); }

        [Command("gun"), Alias(new string[] { "rob" })]
        public async Task Gun(IUser user)
        {
            if (Check(Context, "gun")) return;
            var u1 = Functions.GetUser(Context.User);
            var u2 = Functions.GetUser(user);

            if (rdm.Next(100) > 80)
            {
                int amount;
                do amount = rdm.Next(500);
                while (amount > Convert.ToInt32(u2.GetData("coins")));
                u1.GiveCoins(amount);
                u2.GiveCoins(-amount);
                await Context.Channel.SendMessageAsync($":gun: {(user as IGuildUser).Mention}! {(Context.User as IGuildUser).Mention} has stolen {amount} coins from you!");
            }
            else
            {
                string item = u2.GetItemList()[rdm.Next(u2.GetItemList().Count())];
                u1.GiveItem(item);
                u2.RemoveItem(item);
                await Context.Channel.SendMessageAsync($":gun: {(user as IGuildUser).Mention}! {(Context.User as IGuildUser).Mention} has stolen your {item} from you!");
            }
        }

        [Command("cat")]
        public async Task Cat()
        {
            if (Check(Context, "cat")) return;
            await Context.Channel.SendMessageAsync(":cat: You pet your kitty :blush:\n**Happiness+30**");
            Functions.GetUser(Context.User).AddData("stat.happiness", 30);
        }

        [Command("dog")]
        public async Task Dog()
        {
            if (Check(Context, "dog")) return;
            await Context.Channel.SendMessageAsync(":dog: You pet your pupper :blush:\n**Happiness+30**");
            Functions.GetUser(Context.User).AddData("stat.happiness", 30);
        }


    }
}
