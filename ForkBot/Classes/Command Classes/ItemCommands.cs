using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ImageProcessor;
using System.Net;
using PokeAPI;
using System.Data.SQLite;

namespace ForkBot
{
    public class ItemCommands : ModuleBase
    {
        Random rdm = new Random();

        public bool Check(ICommandContext Context, string item, bool remove = true)
        {
            var user = Functions.GetUser(Context.User);
            if (user.HasItem(DBFunctions.GetItemID(item)))
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
                user.AddStat("hygiene", 20);
            }
            else if (r < 75)
            {
                msg = "You've been holding this in for a while.. Poop gets everywhere and is a pain to clean up.\n**Hygiene-50**";
                user.AddStat("hygiene", -50);
            }
            else
            {
                msg = "This... this is the best poop you've ever had in your life! You feel fantastic!\n**Hygiene+40 Happiness+20**";
                user.AddStat("hygiene", 40);
                user.AddStat("happiness", 20);
            }
            await Context.Channel.SendMessageAsync(":poop: " + msg);
        }

        [Command("shirt")]
        public async Task Shirt()
        {
            if (Check(Context, "shirt")) return;
            await Context.Channel.SendMessageAsync(":shirt: Lookin' good!\n**Fashion+10**");
            Functions.GetUser(Context.User).AddStat("fashion", 10);
        }

        [Command("dress")]
        public async Task Dress()
        {
            if (Check(Context, "dress")) return;
            await Context.Channel.SendMessageAsync(":dress: So beautiful!\n**Fashion+25**");
            Functions.GetUser(Context.User).AddStat("fashion", 25);
        }

        [Command("high_heel")]
        public async Task HighHeel()
        {
            if (Check(Context, "high_heel")) return;
            await Context.Channel.SendMessageAsync(":high_heel: You feel fabulous.\n**Fashion+10**");
            Functions.GetUser(Context.User).AddStat("fashion", 10);
        }

        [Command("athletic_shoe")]
        public async Task AthleticShoe()
        {
            if (Check(Context, "athletic_shoe")) return;
            await Context.Channel.SendMessageAsync(":athletic_shoe: You go for a nice run!\n**Fitness+20**");
            Functions.GetUser(Context.User).AddStat("fitness", 20);
        }

        [Command("dark_sunglasses")]
        public async Task DarkSunglasses()
        {
            if (Check(Context, "dark_sunglasses")) return;
            await Context.Channel.SendMessageAsync(":dark_sunglasses: You equip your sunglasses... and get a whole lot cooler.\n**Fashion+20**");
            Functions.GetUser(Context.User).AddStat("fashion", 20);
        }

        [Command("eyeglasses")]
        public async Task Eyeglasses(string item)
        {
            if (Check(Context, "eyeglasses",false)) return;
            User u = Functions.GetUser(Context.User);
            if (u.HasItem(DBFunctions.GetItemID(item)))
            {
                List<ItemCombo> possible = new List<ItemCombo>();
                foreach (ItemCombo ic in ItemCombo.ItemCombos)
                {
                    if (ic.Items.Contains(item)) possible.Add(ic);
                }
                if (possible.Count != 0)
                {
                    string msg = "Recipes:\n";
                    foreach (ItemCombo ic in possible)
                    {
                        foreach (string i in ic.Items)
                        {
                            if (u.HasItem(i)) msg += DBFunctions.GetItemEmote(i);
                            else msg += ":question:";
                            msg += " + ";
                        }
                        msg = msg.Substring(0, msg.Length - 3);
                        msg += " = ";
                        if (u.HasItem(ic.Result)) msg += DBFunctions.GetItemEmote(ic.Result);
                        else msg += ":question:";
                        msg += "\n";
                    }
                    await ReplyAsync(msg);
                    u.RemoveItem("eyeglasses");
                }
                else
                {
                    await ReplyAsync("No recipes available for this item!");
                }
            }
            else
            {
                await ReplyAsync("You don't have an item with that name!");
            }
        }

        [Command("briefcase")]
        public async Task Briefcase()
        {
            if (Check(Context, "briefcase")) return;

            int r = rdm.Next(50) + 1;
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
            int amount;
            if (r < 20) amount = rdm.Next(50) + 1;
            else amount = rdm.Next(50, 200) + 1;

            await Context.Channel.SendMessageAsync($":purse: There's money inside! You also look great!\n**+{amount} coins Fashion+5**");
            var u = Functions.GetUser(Context.User);
            u.GiveCoins(amount);
            u.AddStat("fashion", 5);
        }

        [Command("gift")]
        public async Task Gift()
        {
            if (Check(Context, "gift")) return;
            var user = Functions.GetUser(Context.User);

            var presents = DBFunctions.GetItemIDList();
            int presRDM;
            do {
                presRDM = rdm.Next(presents.Count());
            }
            while (!DBFunctions.ItemIsPresentable(presents[presRDM]));

            var itemID = presents[presRDM];

            Var.present = DBFunctions.GetItemName(itemID);
            Var.rPresent = Var.present;
            var presentName = Var.present;
            var pMessage = DBFunctions.GetItemDescription(itemID);

            await Context.Channel.SendMessageAsync($"You got...\nA {Func.ToTitleCase(presentName.Replace('_', ' '))}! {DBFunctions.GetItemEmote(itemID)} {pMessage}");
            if (Var.present == "santa")
            {
                string sMessage = "You got...\n";
                for (int i = 0; i < 5; i++)
                {
                    var sPresentID = presents[rdm.Next(presents.Count())];
                    if (!DBFunctions.ItemIsPresentable(sPresentID))
                    {
                        i--;
                        continue;
                    }
                    string sPresentName = DBFunctions.GetItemName(itemID);
                    user.GiveItem(sPresentName);
                    sMessage += $"A {Func.ToTitleCase(sPresentName)}! {DBFunctions.GetItemEmote(itemID)} {DBFunctions.GetItemDescription(itemID)}\n";
                }
                await Context.Channel.SendMessageAsync(sMessage);

                Var.replaceable = false;
            }
            else user.GiveItem(Var.present);

        }

        [Command("roll"), Alias(new string[] {"game_die"})]
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

        [Command("gun"), Alias("rob")]
        public async Task Gun() { if (Check(Context, "gun", false)) return; await Context.Channel.SendMessageAsync("Choose someone to rob with `;gun [user]`..."); }

        [Command("gun"), Alias("rob")]
        public async Task Gun(IUser user)
        {
            if (Check(Context, "gun")) return;
            var u1 = Functions.GetUser(Context.User);
            var u2 = Functions.GetUser(user);

            if (u2.GetData<bool>("active_gnome"))
            {
                u2.SetData("active_gnome", "0");
                await ReplyAsync(DBFunctions.GetItemEmote(DBFunctions.GetItemID("gnome")) + $" Uh oh! {user.Mention} just gnomed you! Your gun had no effect!\n{Constants.Values.GNOME_VID}");
            }
            else
            {

                if (rdm.Next(100) > 80)
                {
                    int amount;
                    do amount = rdm.Next(500);
                    while (amount > u2.GetCoins());
                    u1.GiveCoins(amount);
                    u2.GiveCoins(-amount);
                    await Context.Channel.SendMessageAsync($":gun: {(user as IGuildUser).Mention}! {(Context.User as IGuildUser).Mention} has stolen {amount} coins from you!");
                    if (rdm.Next(100) < 20) DBFunctions.AddNews("ROBBED AT GUNPOINT!", $"On {Var.CurrentDate().ToString("dddd, MMMM dd")} at {Var.CurrentDate().ToString("h:mm tt")}, a person was seen" +
                            $"robbing local person {user.Username} using a gun! Fearing for their life, {user.Username} handed over {amount} coins to the culprit who then escaped. Police are " +
                            $"currently investigating but currently have no leads!");
                }
                else
                {
                    var items = u2.GetItemList();
                    if (items.Count() == 0)
                    {
                        await Context.Channel.SendMessageAsync($"You try to steal an item from {user.Username}... but they have nothing!" +
                                                               $" You drop your gun and run before the police arrive. {(user as IGuildUser).Mention} picks up the gun!");
                        u2.GiveItem("gun");
                        if (rdm.Next(100) < 70) DBFunctions.AddNews("ATTEMPTED ARMED ROBBERY!", $"At {Var.CurrentDate().ToString("h: mm tt")} on {Var.CurrentDate().ToString("dddd, MMMM dd")}, {Context.User.Username} " +
                            $"was caught in an attempt to rob {user.Username} using a pistol. {user.Username} was able to disarm the culprit and didn't lose anything. It is currently unclear if " +
                            $"{Context.User.Username} was under the influence of any drugs or alcohol at this time, but it is believed to be so.");
                    }
                    else
                    {
                        var itemID = items.ElementAt(rdm.Next(items.Count())).Key;
                        var item = DBFunctions.GetItemName(itemID);
                        u1.GiveItem(item);
                        u2.RemoveItem(item);
                        await Context.Channel.SendMessageAsync($":gun: {(user as IGuildUser).Mention}! {(Context.User as IGuildUser).Mention} has stolen your {item} from you!");
                        if (rdm.Next(100) < 20) DBFunctions.AddNews("ROBBED AT GUNPOINT!", $"On {Var.CurrentDate().ToString("dddd, MMMM dd")} at {Var.CurrentDate().ToString("h:mm tt")}, a person was seen" +
                            $"robbing local person {user.Username} using a gun! Fearing for their life, {user.Username} handed over their {item} to the culprit who then escaped. Police are " +
                            $"currently investigating but currently have no leads!");
                    }
                }
            }
        }

        [Command("cat")]
        public async Task Cat()
        {
            if (Check(Context, "cat")) return;
            await Context.Channel.SendMessageAsync(":cat: You pet your kitty :blush:\n**Happiness+30**");
            Functions.GetUser(Context.User).AddStat("happiness", 30);
        }

        [Command("dog")]
        public async Task Dog()
        {
            if (Check(Context, "dog")) return;
            await Context.Channel.SendMessageAsync(":dog: You pet your pupper :blush:\n**Happiness+30**");
            Functions.GetUser(Context.User).AddStat("happiness", 30);
        }

        [Command("unicorn")]
        public async Task Unicorn()
        {
            if (Check(Context, "unicorn", false)) return;
            await Context.Channel.SendMessageAsync(":unicorn: A magical unicorn appears! Make a wish!\n" +
                                                   "`;wish 1` *\"I want to be rich!\"*\n" +
                                                   "`;wish 2` *\"Make me beautiful!\"*\n" +
                                                   "`;wish 3` *\"Give me some items!\"*\n" +
                                                   "`;wish 4` *\"Make me happy!\"*\n" +
                                                   "`;wish 5` *\"You decide!\"*");
        }

        [Command("wish")]
        public async Task Wish(int choice)
        {
            if (choice <= 0 || choice > 5) return;
            if (Check(Context, "unicorn")) return;
            var user = Functions.GetUser(Context.User);
            string msg = ":unicorn: Your wish is my command!\n";
            bool repeat;
            do
            {
                repeat = false;
                switch (choice)
                {
                    case 1:
                        int coinAmount = rdm.Next(1000,3000)+1;
                        user.GiveCoins(coinAmount);
                        msg += $"**+{coinAmount} coins!**";
                        break;
                    case 2:
                        int amount = rdm.Next(2000, 5000)+1;
                        msg += $"**Fashion+{amount}**";
                        user.AddStat("fashion", amount);
                        break;
                    case 3:
                        await Context.Channel.SendMessageAsync("You got...");
                        var presents = DBFunctions.GetItemIDList();
                        var list = presents.ToList();
                        presents = list.ToArray();
                        for (int i = 0; i < 10; i++)
                        {
                            var sPresentID = presents[rdm.Next(presents.Count())];
                            if (!DBFunctions.ItemIsPresentable(sPresentID))
                            {
                                i--;
                                continue;
                            }
                            string sPresentName = DBFunctions.GetItemName(sPresentID);
                            user.GiveItem(sPresentName);
                            msg += $"A {Func.ToTitleCase(sPresentName)}! {DBFunctions.GetItemEmote(sPresentName)} {DBFunctions.GetItemDescription(sPresentID)}\n";
                        }
                        break;
                    case 4:
                        int hAmount = rdm.Next(2000, 5000)+1;
                        msg += $"**Happiness+{hAmount}**";
                        user.AddStat("happiness", hAmount);
                        break;
                    case 5:
                        choice = rdm.Next(4) + 1;
                        repeat = true;
                        break;
                }
            } while (repeat);
            await Context.Channel.SendMessageAsync(msg);
        }

        [Command("crystal_ball"), Alias("crystalball", "nextpresent")]
        public async Task CrystalBall()
        {
            if (Check(Context, "crystal_ball")) return;
            await Context.Channel.SendMessageAsync("You gaze into the crystal ball and... learn the exact time until the next present!");
            var nextPres = (Var.presentTime + Var.presentWait) - Var.CurrentDate();
            await Context.User.SendMessageAsync($"The next present will be ready in {nextPres.Hours} hours {nextPres.Minutes} minutes, and {nextPres.Seconds} seconds!");
        }

        [Command("eggplant")]
        public async Task Eggplant()
        {
            if (Check(Context, "eggplant")) return;
            await Context.Channel.SendMessageAsync(":eggplant: You eat the eggplant. What were you expecting?\n**Fullness+10**");
            Functions.GetUser(Context.User).AddStat("fullness", 10);
        }

        [Command("apple")]
        public async Task Apple()
        {
            if (Check(Context, "apple")) return;
            await Context.Channel.SendMessageAsync(":apple: Keeps the doctor away!\n**Healthiness+5 Fullness+10**");
            Functions.GetUser(Context.User).AddStat("fullness", 10);
            Functions.GetUser(Context.User).AddStat("healthiness", 5);
        }

        [Command("egg")]
        public async Task Egg()
        {
            if (Check(Context, "egg")) return;
            int rand = rdm.Next(100) + 1;
            string eggType;
            if (rand < 30) eggType = "Boiled";
            else if (rand < 60) eggType = "Scrambled";
            else if (rand < 90) eggType = "Sunny Side Up";
            else eggType = "Raw";

            string msg;
            if (eggType == "Raw")
            {
                msg = ":egg: Eugh! You eat the egg.. Raw!\n**Happiness-20 Fullness-10**";
                Functions.GetUser(Context.User).AddStat("fullness", -10);
                Functions.GetUser(Context.User).AddStat("happiness", -20);
            }
            else
            {
                msg = ":cooking: Yum! " + eggType + " eggs!\n**Fullness+20**";
                Functions.GetUser(Context.User).AddStat("fullness", 20);
            }
            await ReplyAsync(msg);
        }

        [Command("egg")]
        public async Task Egg(IUser user)
        {
            if (Check(Context, "egg")) return;
            var u = Functions.GetUser(user);
            if (u.GetData<bool>("active_gnome"))
            {
                u.SetData("active_gnome", "0");
                await ReplyAsync(DBFunctions.GetItemEmote("gnome") + $" Uh oh! {user.Mention} just gnomed you! Your egg had no effect!\n{Constants.Values.GNOME_VID}");
            }
            else
            {
                await Context.Channel.SendMessageAsync($":egg: {Context.User.Username} throws their egg at {user.Username}!\n**Their Happiness-10**");
                u.AddStat("happiness", -10);
            }
        }

        [Command("ramen")]
        public async Task Ramen()
        {
            if (Check(Context, "ramen")) return;
            await Context.Channel.SendMessageAsync(":ramen: Sweet sweet ramen...\n**Fullness+50**");
            Functions.GetUser(Context.User).AddStat("fullness", 50);
        }

        [Command("goose")]
        public async Task Goose()
        {
            if (Check(Context, "goose")) return;
            await Context.Channel.SendMessageAsync("<:goose:369992347314028554> A herd of geese fly by... eugh!\n**Happiness-35**");
            Functions.GetUser(Context.User).AddStat("happiness", -35);
            string msg = "You got...\n:poop: A goose poop!";
            var user = Functions.GetUser(Context.User);
            user.GiveItem("poop");
            int poopCount = rdm.Next(5);
            for (int i = 0; i < poopCount; i++)
            {
                msg += ":poop: Another goose poop!";
                user.GiveItem("poop");
            }
            await Context.Channel.SendMessageAsync(msg);
        }

        [Command("youness")]
        public async Task Youness()
        {
            if (Check(Context, "youness")) return;

            int rand = rdm.Next(50) + 1;
            string msg;
            var user = Functions.GetUser(Context.User);
            if (rand < 10)
            {
                msg = "Youness winks at you.\n**Happiness+25**";
                user.AddStat("happiness", 25);
            }
            else if (rand < 20)
            {
                msg = "Youness returns your test... You passed!\n**Happiness+20**";
                user.AddStat("happiness", 20);
            }
            else if (rand < 30)
            {
                msg = "Youness returns your test... You failed...\n**Happiness-30**";
                user.AddStat("happiness", -30);
            }
            else if (rand < 40)
            {
                msg = "Youness yells at the next class for coming in early. You sense his manliness.\n**Happiness+30**";
                user.AddStat("happiness", 30);
            }
            else
            {
                msg = "Youness tells you there *is* one thing you can do to get that A+ you need...\n**Fullness+10(inches) Happiness+5**";
                user.AddStat("fullness", 10);
                user.AddStat("happiness", 5);
            }

            await Context.Channel.SendMessageAsync($"<:youness:373579959899258880> {msg}");
        }

        [Command("moneybag")]
        public async Task Moneybag()
        {
            if (Check(Context, "moneybag")) return;

            int r = rdm.Next(50) + 1;
            int amount = rdm.Next(100, 1000) + 1;

            await Context.Channel.SendMessageAsync($":moneybag: Loads of coins come out of the bag!!\n**+{amount} coins**");
            Functions.GetUser(Context.User).GiveCoins(amount);
        }

        [Command("shopping_cart"), Alias("shoppingcart", "cart")]
        public async Task ShoppingCart()
        {
            if (Check(Context, "shopping_cart")) return;
            Var.currentShop = new Shop();
            await Context.Channel.SendMessageAsync(":shopping_cart: The items for sale in `;shop` have changed!");
        }

        [Command("knife"), Alias("stab")]
        public async Task Knife() { if (Check(Context, "knife", false)) return; await Context.Channel.SendMessageAsync("Choose someone to rob with `;knife [user]`..."); }

        [Command("knife"), Alias("stab")]
        public async Task Knife(IUser user)
        {
            if (Check(Context, "knife")) return;
            var u1 = Functions.GetUser(Context.User);
            var u2 = Functions.GetUser(user);

            if (u2.GetData<bool>("active_gnome"))
            {
                u2.SetData("active_gnome", "0");
                await ReplyAsync(DBFunctions.GetItemEmote("gnome") + $" Uh oh! {user.Mention} just gnomed you! Your knife had no effect!\n{Constants.Values.GNOME_VID}");
            }
            else
            {
                if (rdm.Next(100) < 60)
                {
                    if (rdm.Next(100) > 80)
                    {
                        int amount;
                        do amount = rdm.Next(500);
                        while (amount > u2.GetCoins());
                        u1.GiveCoins(amount);
                        u2.GiveCoins(-amount);
                        await Context.Channel.SendMessageAsync($":knife: {(user as IGuildUser).Mention}! {(Context.User as IGuildUser).Mention} has stolen {amount} coins from you!");
                        if (rdm.Next(100) < 20)DBFunctions.AddNews("ROBBED AT KNIFEPOINT!", $"On {Var.CurrentDate().ToString("dddd, MMMM dd")} at {Var.CurrentDate().ToString("h:mm tt")}, a person was seen " +
                            $"robbing local person {user.Username} using a knife! Fearing for their life, {user.Username} handed over {amount} coins to the culprit, who then escaped. Police are " +
                            $"currently investigating but currently have no leads!");
                    }
                    else
                    {
                        var items = u2.GetItemList();
                        if (items.Count() == 0)
                        {
                            await Context.Channel.SendMessageAsync($"You try to steal an item from {user.Username}... but they have nothing!" +
                                                                   $" You drop your knife and run before the police arrive. {(user as IGuildUser).Mention} picks up the knife!");
                            u2.GiveItem("knife");

                            if (rdm.Next(100) < 70) DBFunctions.AddNews("ATTEMPTED ARMED ROBBERY WITH A SHARP WEAPON", $"At {Var.CurrentDate().ToString("h: mm tt")} on {Var.CurrentDate().ToString("dddd, MMMM dd")}, {Context.User.Username} " +
                            $"was caught in an attempt to rob {user.Username} using a knife. {user.Username} was able to disarm the culprit and didn't lose anything. It is currently unclear if " +
                            $"{Context.User.Username} was under the influence of any drugs or alcohol at this time, but it is believed to be so.");
                        }
                        else
                        {
                            var itemID = items.ElementAt(rdm.Next(items.Count())).Key;
                            var item = DBFunctions.GetItemName(itemID);
                            u1.GiveItem(item);
                            u2.RemoveItem(item);
                            await Context.Channel.SendMessageAsync($":knife: {(user as IGuildUser).Mention}! {(Context.User as IGuildUser).Mention} has stolen your {item} from you!");
                            if (rdm.Next(100) < 20) DBFunctions.AddNews("ROBBED AT KNIFEPOINT!", $"On {Var.CurrentDate().ToString("dddd, MMMM dd")} at {Var.CurrentDate().ToString("h:mm tt")}, a person was seen" +
                            $"robbing local person {user.Username} using a knife! Fearing for their life, {user.Username} handed over their {item} to the culprit, who then escaped. Police are " +
                            $"currently investigating but currently have no leads!");
                        }
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync($":knife: Your attempt to rob {user.Username} fails! You get nothing.");
                    if (rdm.Next(100) < 70) DBFunctions.AddNews("ATTEMPTED ARMED ROBBERY WITH A SHARP WEAPON", $"At {Var.CurrentDate().ToString("h: mm tt")} on {Var.CurrentDate().ToString("dddd, MMMM dd")}, {Context.User.Username} " +
                            $"was caught in an attempt to rob {user.Username} using a knife. {user.Username} was able to disarm the culprit and didn't lose anything. It is currently unclear if " +
                            $"{Context.User.Username} was under the influence of any drugs or alcohol at this time, but it is believed to be so.");

                }
            }
        }

        [Command("beer")]
        public async Task Beer()
        {
            if (Check(Context, "beer")) return;
            await Context.Channel.SendMessageAsync(":beer: You drink the beer and feel a little tipsy.\n**Sobriety-5 Happiness+20**");
            Functions.GetUser(Context.User).AddStat("sobriety", -5);
            Functions.GetUser(Context.User).AddStat("happiness", 20);
        }

        [Command("paintbrush")]
        public async Task Paintbrush()
        {
            try
            {
                if (Context.Message.Attachments.Count() <= 0) await Paintbrush(Context.User);
                else if (Context.Message.Attachments.First().Size < 500000)
                    await Paintbrush(Context.Message.Attachments.First().Url);
            } catch (Exception e)
            {
                Console.WriteLine("Paintbrush Error:\n" + e.StackTrace);
            }
        }

        [Command("paintbrush")]
        public async Task Paintbrush(IUser user)
        {
            try
            {
                await Paintbrush(user.GetAvatarUrl());
            }
            catch (Exception e)
            {
                Console.WriteLine("Paintbrush Error:\n" + e.StackTrace);
            }
}
        
        [Command("paintbrush")]
        public async Task Paintbrush(string url)
        {
            try
            {
                if (Check(Context, "paintbrush", false)) return;
                using (ImageFactory fact = new ImageFactory())
                {
                    using (WebClient web = new WebClient())
                    {
                        bool downloaded = false;
                        int timeOutCounter = 0;
                        while (!downloaded)
                        {
                            timeOutCounter++;
                            if (timeOutCounter >= 20)
                            {
                                await ReplyAsync("Timed out, make sure username or url is correct!");
                                return;
                            }
                            if (url == null)
                            {
                                await ReplyAsync("You must use a valid picture to use this command.");
                                return;
                            }
                            try { web.DownloadFile(url, @"Files\paintbrush.png"); downloaded = true; }
                            catch (Exception) { }
                        }
                        fact.Load(@"Files\paintbrush.png");
                    }
                    int[] effects = new int[5];
                    for (int i = 0; i < effects.Count(); i++)
                    {
                        effects[i] = rdm.Next(10);
                    }
                    System.Drawing.Color colour = System.Drawing.Color.FromArgb(rdm.Next(255) + 1, rdm.Next(255) + 1, rdm.Next(255) + 1);
                    foreach (int effect in effects)
                    {
                        switch (effect)
                        {
                            case 0:
                                fact.Hue(rdm.Next(359) + 1);
                                break;
                            case 1:
                                fact.Brightness(rdm.Next(100) + 1);
                                break;
                            case 2:
                                fact.Contrast(rdm.Next(100) + 1);
                                break;
                            case 3:
                                fact.Gamma(rdm.Next(5) + (rdm.Next(10) / 2));
                                break;
                            case 4:
                                fact.Halftone(true);
                                break;
                            case 5:
                                fact.Pixelate(rdm.Next(fact.Image.Size.Height / 10));
                                break;
                            case 6:
                                fact.Saturation(rdm.Next(100) + 1);
                                break;
                            case 7:
                                fact.Vignette(colour);
                                break;
                            case 8:
                                fact.Tint(colour);
                                break;
                            case 9:
                                fact.ReplaceColor(colour, System.Drawing.Color.FromArgb(rdm.Next(255) + 1, rdm.Next(255) + 1, rdm.Next(255) + 1), 50);
                                break;

                        }
                    }
                    fact.Save(@"Files\paintbrush_edited.png");
                }
                await Context.Channel.SendFileAsync(@"Files\paintbrush_edited.png");
            }
            catch (Exception e)
            {
                Console.WriteLine("Paintbrush Error:\n" + e.StackTrace);
            }
        }
        
        [Command("santa")]
        public async Task Santa()
        {
            if (Check(Context, "santa")) return;

            await Context.Channel.SendMessageAsync("You got...");
            string sMessage = ""; var user = Functions.GetUser(Context.User);
            var presents = DBFunctions.GetItemIDList();
            for (int i = 0; i < 5; i++)
            {
                var sPresentID = presents[rdm.Next(presents.Count())];
                if (!DBFunctions.ItemIsPresentable(sPresentID))
                {
                    i--;
                    continue;
                }
                string sPresentName = DBFunctions.GetItemName(sPresentID);
                user.GiveItem(sPresentName);
                sMessage += $"A {Func.ToTitleCase(sPresentName)}! {DBFunctions.GetItemEmote(sPresentName)} {DBFunctions.GetItemDescription(sPresentID)}\n";
            }
            await Context.Channel.SendMessageAsync(sMessage);

            Var.replaceable = false;
        }

        [Command("watch")]
        public async Task Watch()
        {
            if (Check(Context, "watch", false)) return;
            await Context.Channel.SendMessageAsync(":watch: `" + (DateTime.UtcNow - new TimeSpan(4, 0, 0)).ToLocalTime() + "`");
        }

        [Command("mag"), Alias("burn", "magnifyingglass","magnifying_glass")]
        public async Task Mag() { if (Check(Context, "mag", false)) return; await Context.Channel.SendMessageAsync("Choose someone to burn with `;mag [user]`..."); }

        [Command("mag"), Alias("burn", "magnifyingglass", "magnifying_glass")]
        public async Task Mag(IUser user)
        {
            if (user.Id == Constants.Users.FORKBOT)
            {
                await ReplyAsync("This user cannot be magged!");
                return;
            }
            if (Check(Context, "mag")) return;
            var u1 = Functions.GetUser(Context.User);
            var u2 = Functions.GetUser(user);

            if (u2.GetData<bool>("active_gnome"))
            {
                u2.SetData("active_gnome", "0");
                await ReplyAsync(DBFunctions.GetItemEmote("gnome") + $" Uh oh! {user.Mention} just gnomed you! Your mag had no effect!\n{Constants.Values.GNOME_VID}");
            }
            else
            {
                if (rdm.Next(100) > 80)
                {
                    int amount;
                    do amount = rdm.Next(500);
                    while (amount > u2.GetCoins());
                    u2.GiveCoins(-amount);
                    await Context.Channel.SendMessageAsync($":mag: {(user as IGuildUser).Mention}! {(Context.User as IGuildUser).Mention} has burned {amount} of your coins!");
                }
                else
                {
                    var items = u2.GetItemList();
                    if (items.Count() == 0)
                    {
                        await Context.Channel.SendMessageAsync($"You try to burn one of {user.Username}'s items... but they have nothing!");
                    }
                    else
                    {
                        var itemID = items.ElementAt(rdm.Next(items.Count())).Key;
                        var item = DBFunctions.GetItemName(itemID);
                        u2.RemoveItem(item);
                        await Context.Channel.SendMessageAsync($":mag: {(user as IGuildUser).Mention}! {(Context.User as IGuildUser).Mention} has burnt your {item}!");
                    }
                }
            }
        }

        [Command("bomb")]
        public async Task Bomb()
        {
            if (Check(Context, "bomb")) return;
            await Context.Message.DeleteAsync();
            await Context.User.SendMessageAsync("You have successfully rigged the next present! Make sure you're not the one to open it!");
            Var.presentRigged = true;
            Var.presentRigger = Context.User;
        }

        [Command("slots"), Alias(new string[] { "slot", "slot_machine" })]
        public async Task Slots(int bet = 0)
        {
            if (Check(Context, "slot_machine", false)) return;
            try
            {
                if (bet < 100) { await ReplyAsync("You need to bet at least 100 coins to use this! `;slots [bet]`"); return; }
                var user = Functions.GetUser(Context.User);

                if (user.GetCoins() < bet) await Context.Channel.SendMessageAsync(":slot_machine: | You do not have that many coins!");
                else if (bet <= 0) await Context.Channel.SendMessageAsync(":slot_machine: | Your bet must be above 0.");
                else
                {
                    Properties.Settings.Default.jackpot += bet;
                    Properties.Settings.Default.Save();
                    user.GiveCoins(-bet);
                    SlotMachine sm = new SlotMachine(Context.User, bet);
                    var result = sm.Spin();
                    JEmbed emb = new JEmbed();
                    emb.Description = sm.Generate() + "\n" + result;
                    emb.Footer.Text = $"You have: {user.GetCoins()} coins.";
                    emb.ColorStripe = Functions.GetColor(Context.User);
                    var msg = await Context.Channel.SendMessageAsync("",embed:emb.Build());

                    if ((rdm.Next(100) + 1) <= 5)
                    {
                        await ReplyAsync("Oh no! Your slot machine broke!");
                        user.RemoveItem("slot_machine");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Slots error!\n" + e.Message);
                await ReplyAsync("Slots error.\n" + e.Message);
            }
        }

        [Command("ticket")]
        public async Task Ticket()
        {
            if (Check(Context, "ticket")) return;
            Var.presentCount += 2;
            await Context.Channel.SendMessageAsync(":ticket: The present count has increased by 2!");
        }

        [Command("key"), Alias(new string[] { "package", "lootbox" })]
        public async Task Key()
        {
            User u = Functions.GetUser(Context.User);
            if (u.HasItem("key") && u.HasItem("package"))
            {
                string[] lootboxItems = { "knife", "poop", "bomb", "ticket", "slot_machine", "mag", "moneybag", "purse", "briefcase", "shopping_cart", "gift", "crystal_ball", "gnome" };

                u.RemoveItem("key");
                u.RemoveItem("package");

                int[] items = new int[5];

                for (int i = 0; i < items.Count(); i++) items[i] = rdm.Next(lootboxItems.Count());

                string msg = "Your lootbox bursts open!\n:sparkles: ";
                foreach(int i in items)
                {
                    msg += DBFunctions.GetItemEmote(lootboxItems[i]) + " " + lootboxItems[i] + ":sparkles: ";
                    u.GiveItem(lootboxItems[i]);
                }

                await ReplyAsync(msg);
            }
            else if (u.HasItem("key")) await ReplyAsync("You have nothing to open with this key!");
            else if (u.HasItem("package")) await ReplyAsync("It's locked! You need a key to open it.");
        }

        [Command("postbox")]
        public async Task Postbox()
        {
            User u = Functions.GetUser(Context.User);
            if (u.HasItem("key2") && u.HasItem("postbox"))
            {
                string[] lootboxItems = { "pouch", "lock", "key", "moneybag","briefcase", "gun", "knife", "mag", "ticket", "stopwatch", "gift","unicorn","watch", "stopwatch","santa","shopping_cart","poop" };

                u.RemoveItem("key2");
                u.RemoveItem("postbox");

                int[] items = new int[10];

                for (int i = 0; i < items.Count(); i++) items[i] = rdm.Next(lootboxItems.Count());

                string msg = "Your lootbox bursts open!\n:sparkles: ";
                foreach (int i in items)
                {
                    msg += DBFunctions.GetItemEmote(lootboxItems[i]) + " " + lootboxItems[i] + ":sparkles: ";
                    u.GiveItem(lootboxItems[i]);
                }

                await ReplyAsync(msg);
            }
            else if (u.HasItem("key2")) await ReplyAsync("You have nothing to open with this key2!");
            else if (u.HasItem("postbox")) await ReplyAsync("It's locked! You need a key2 to open it.");
        }

        [Command("key2")]
        public async Task Key2()
        {
            User u = Functions.GetUser(Context.User);
            if (u.HasItem("key2") && u.HasItem("postbox"))
            {
                await Postbox();
            }
            else if (u.HasItem("key2") && u.HasItem("package"))
            {
                u.GiveItem("key");
                u.GiveItem("key");
                u.RemoveItem("key2");
                await ReplyAsync("Your key2 turns into a key! :key2: :arrow_right: :key:");
                await Key();
            }
            else if (u.HasItem("key2")) await ReplyAsync("You have nothing to open with this key!");
        }

        [Command("stopwatch")]
        public async Task Stopwatch()
        {
            if (Check(Context, "stopwatch")) return;
            await Context.Channel.SendMessageAsync(":stopwatch: The present time has decreased by 75%!");
            Var.presentWait -= new TimeSpan(0,0,Convert.ToInt32(Var.presentWait.TotalSeconds * .75));
        }

        [Command("baby_symbol")]
        public async Task BabySymbol()
        {
            if (Check(Context, "baby_symbol")) return;
            await Context.Channel.SendMessageAsync(":pregnant_woman: Her water broke!");
            string msg = "You got...\n:baby: A baby!";
            var user = Functions.GetUser(Context.User);
            user.GiveItem("baby");
            int babyCount = rdm.Next(8);
            for (int i = 0; i < babyCount; i++)
            {
                msg += ":baby: Another baby!";
                user.GiveItem("baby");
            }
            msg += "\nCongratulations! :older_woman:";
            user.GiveItem("older_woman");
            await Context.Channel.SendMessageAsync(msg);

            string[] twins = { "A BABY", "TWINS", "TRIPLETS", "QUADRUPLETS", "QUINTUPLETS", "SEXTUPLETS", "SEPTUPLETS", "OCTUPLETS" };


            if (babyCount+1 > 4)
            DBFunctions.AddNews($"{Context.User.Username.ToUpper()} GIVES BIRTH TO {twins[babyCount]}!", "We here at The Daily Fork would like to extend our deepest " +
                $"congratulations over to {Context.User.Username} for their succesful delivery of {babyCount+1} beautiful babies! The delivery took place at {Var.CurrentDate().ToString("h:mm tt")} on {Var.CurrentDate().ToString("dddd, MMMM dd")}. " +
                $"Good work to everyone involved!");
        }

        [Command("jack_o_lantern"), Alias(new string[] {"jackolantern","pumpkin","jack-o-lantern" })]
        public async Task JackOLantern()
        {
            if (Check(Context, "jack_o_lantern")) return;
            var dt = Var.CurrentDate();
            string msg = "";
            User u = Functions.GetUser(Context.User);
            if (dt.Month == 10 && dt.Day == 31)
            {
                msg += "Happy halloween!";
                int candyCount = rdm.Next(11) + 5;
                for(int i = 0; i < candyCount; i++)
                {
                    u.GiveItem("candy");
                }
                msg += $" You got {candyCount} candies!";
                await ReplyAsync(msg);
            }
            else
            {
                u.GiveItem("candy");
                u.GiveItem("candy");
                await ReplyAsync("You got 2 pieces of candy! :candy: Maybe if you used this at a different time it would be better..");
            }
        }
        
        [Command("candy")]
        public async Task Candy()
        {
            if (Check(Context,"candy")) return;
            await Context.Channel.SendMessageAsync(":candy: Don't forget to check for razors!\n**Fullness+10 Happiness+15**");
            Functions.GetUser(Context.User).AddStat("fullness", 10);
            Functions.GetUser(Context.User).AddStat("happiness", 15);
        }
        
        [Command("gnome")]
        public async Task Gnome()
        {
            if (Check(Context, "gnome", false)) return;
            var user = Functions.GetUser(Context.User);
            if (user.GetData<bool>("active_gnome")) await ReplyAsync(DBFunctions.GetItemEmote(DBFunctions.GetItemID("gnome")) + " Hohohohoho! You already have gnome protection!");
            else
            {
                user.RemoveItem("gnome");
                user.SetData("active_gnome", "1");
                await ReplyAsync(DBFunctions.GetItemEmote(DBFunctions.GetItemID("gnome")) + " Hohohohohoho! You've gnought to worry! I'll protect you!");
            }
        }
        
        [Command("makekey")]
        public async Task MakeKey()
        {
            int packageCount = 0;
            var user = Functions.GetUser(Context.User);
            var uitems = user.GetItemList();
            
            if (user.HasItem(DBFunctions.GetItemID("package"),5))
            {
                for (int i = 0; i < 5; i++)
                {
                    user.RemoveItem("package");
                }
                user.GiveItem("key");
                await ReplyAsync("You have succesfully converted 5 packages into 1 key! :key:");
            }
            else await ReplyAsync("You must have 5 packages to make a key!");
            
        }

        [Command("dividers"), Alias(new string[] { "sort", "divider" })]
        public async Task Dividers()
        {
            await ReplyAsync("this command is being deleted soon. divider sell price has increased, sell it while you can!");
        }
        
        [Command("meat_on_bone"), Alias("meat")]
        public async Task MeatOnBone()
        {
            if (Check(Context, "meat_on_bone")) return;
            int stat = rdm.Next(35, 51);
            await Context.Channel.SendMessageAsync($":meat_on_bone: So well cooked!\n**Fullness+{stat}**");
            Functions.GetUser(Context.User).AddStat("fullness", stat);
        }

        [Command("hole")]
        public async Task Hole()
        {
            if (Check(Context, "hole")) return;
            try { await Context.Message.DeleteAsync(); } catch { } 
            Functions.GetUser(Context.User).SetData("has_bm", "true");
            await (Bot.client.GetChannel(Constants.Channels.ELITE) as IMessageChannel).SendMessageAsync($":spy: @everyone, {Context.User.Mention} has entered the Black Market.");
            await Context.User.SendMessageAsync(":spy: Psst... hey.... you've been granted access to the black market. **Don't** tell anyone about this... Or you'll regret it.\nUse `;bm` to access and buy from it just like the shop.\nKeep it in private messages..");
            DBFunctions.AddNews("🕵️", $"Welcome, {Context.User.Username}.");
        }
        
        [Command("spy")]
        public async Task Spy()
        {
            if (Check(Context, "spy", false)) return;
            if (Var.presentRigged)
            {
                if (rdm.Next(100)+1 < 10)
                {
                    await ReplyAsync(":spy: Oh fu-! :boom::boom::boom:\nYour spy accidentally activated the bomb and died!");
                }
                else await ReplyAsync($":spy: Watch out.. There's a bomb hidden here.. Looks like {Functions.GetName(Var.presentRigger as IGuildUser)} planted it.");
            }
            else await ReplyAsync(":spy: Nope, no bombs here.");
        }

        [Command("unlock")]
        public async Task Unlock()
        {
            if (Check(Context, "unlock",false)) return;
            var u = Functions.GetUser(Context.User);
            if (u.HasItem("iphone"))
            {
                u.RemoveItem("iphone");
                u.RemoveItem("unlock");
                u.GiveItem("calling");
                await ReplyAsync("You unlocked your IPhone! :iphone: :calling: There's a number you can dial, try it!");
            }
            else await ReplyAsync("You need an IPhone to use this.");
        }
        
        [Command("calling"), Alias("call")]
        public async Task Calling()
        {
            if (Check(Context, "calling")) return;
            int index = rdm.Next(4);
            string msg = "";
            var u = Functions.GetUser(Context.User);
            switch (index)
            {
                case 0:
                    msg = ":mrs_claus: Mrs. Claus answer the phone! \"Christmas is early this year!!\"\nYou got:";
                    var items = DBFunctions.GetItemIDList();
                    for (int i = 0; i < 4; i++)
                    {
                        var itemI = rdm.Next(items.Count());
                        if (!DBFunctions.ItemIsPresentable(items[itemI]))
                        {
                            i--;
                            continue;
                        }
                        string itemName = DBFunctions.GetItemName(items[itemI]);
                        msg += "\nA(n) " + DBFunctions.GetItemEmote(itemName) + " " + itemName;
                        u.GiveItem(itemName);
                    }
                    break;
                case 1:
                    msg = ":unicorn: A Unicorn answered the phone! \"NEIGHHH, NIEGHHHHHH!!!\"\nYou got a Unicorn! :unicorn:";
                    u.GiveItem("unicorn");
                    break;
                case 2:
                    msg = "<:rhonda:504902977069383681> Rhonda Lenton answered the phone! \"It's me, Rhonda. Here's a scholarship!\"";
                    int coins = rdm.Next(1500, 3000);
                    msg += $"\nYou got {coins} coins!";
                    u.GiveCoins(coins);
                    break;
                case 3:
                    msg = ":smiling_imp: Oh no, Satan answered the phone! \"Ur going to hell bitch.\" You hear a dab over the phone.";
                    int coins2 = rdm.Next(500);
                    if (coins2 > u.GetCoins()) coins2 = u.GetCoins();
                    msg += $"\nOh no! You lost {coins2} coins!";
                    u.GiveCoins(-coins2);
                    break;
            }
            await ReplyAsync(msg);
        }

        [Command("weed")]
        public async Task Weed()
        {
            if (Check(Context, "weed")) return;
            int r = rdm.Next(100) + 1;
            var user = Functions.GetUser(Context.User);
            string msg;
            if (r < 70)
            {
                msg = "You had a good time. You spent the evening watching \"Family Guy Funny Moments 2019\" on youtube and then had some Popeyes.\n**Sobriety-20Happiness+80**";
                user.AddStat("happiness", 80);
                user.AddStat("sobriety", -20);
            }
            else if (r < 90)
            {
                msg = "This was a terrible high. You were so paranoid you called 911 while hiding in the fridge.\n**Sobriety-50Happiness-90**";
                user.AddStat("sobriety", -50);
                user.AddStat("happiness", -70);
            }
            else
            {
                msg = "This was the best high of your life! You finally figured out the meaning of life. Sadly you forgot it.\n**Sobriety-30Happiness+110**";
                user.AddStat("sobriety", -30);
                user.AddStat("happiness", 110);
            }
            await Context.Channel.SendMessageAsync($"<:weed:506117312823427082> {msg}");
        }

        [Command("wine_glass"), Alias("wine", "wine_glass")]
        public async Task Wine_glass()
        {
            if (Check(Context, "wine_glass")) return;
            int r = rdm.Next(5) + 1;

            if (r < 5)
            {
                await Context.Channel.SendMessageAsync(":wine_glass: Cheers! You had some cheap wine.\n**Sobriety-3 Happiness+13**");
                Functions.GetUser(Context.User).AddStat("sobriety", -3);
                Functions.GetUser(Context.User).AddStat("happiness", 13);
            }

            else
            {
                await Context.Channel.SendMessageAsync(":wine_glass: A votre santé! You had some imported wine!\n**Sobriety-3 Happiness+18**");
                Functions.GetUser(Context.User).AddStat("sobriety", -3);
                Functions.GetUser(Context.User).AddStat("happiness", 18);
            }
        }
        
        [Command("bow_and_arrow"),Alias("bowandarrow","bow")]
        public async Task Bow_And_Arrow(string target)
        {
            if (Check(Context, "bow_and_arrow", false)) return;
            var user = Functions.GetUser(Context.User);
            if (!user.HasItem(DBFunctions.GetItemID(target))) target = "null";
            switch (target)
            {
                case "tiger":
                    int chance = rdm.Next(100);
                    user.RemoveItem("tiger");
                    user.RemoveItem("bow_and_arrow");
                    int coinLoss = 0, coinGain = 0;
                    if (chance < 45)
                    {
                        coinLoss = rdm.Next(300);
                        int itemLossID = user.GetItemList()[rdm.Next(user.GetItemList().Count())];
                        user.RemoveItem(itemLossID);
                        await ReplyAsync($"The tiger looks towards you, and it looks *pissed*. It lunges at you!\nYou lost {coinLoss} coins and your {DBFunctions.GetItemName(itemLossID)}!");
                    }
                    else
                    {
                        coinGain = rdm.Next(100)+100;
                        await ReplyAsync($"The tiger falls to the ground, and you take some nice pictures! You're also able to sell its pelt for {coinGain} coins.");
                    }

                    user.GiveCoins(coinGain-coinLoss);
                    break;
                case "man":
                    int coins = rdm.Next(50, 200);
                    user.GiveCoins(coins);
                    user.RemoveItem("bow_and_arrow");
                    user.RemoveItem("man");
                    await ReplyAsync("The arrow pierces through his body and he collapses to the ground. His wallet slides out of his pocket as his face collides with the concrete.\n" +
                        $"You got {coins} coins! :moneybag:");
                    break;
                case "older_woman":
                    user.RemoveItem("older_woman");
                    user.RemoveItem("bow_and_arrow");
                    user.GiveItem("ring");
                    await ReplyAsync("The arrow pierces through her frail body and she collapses to the ground.\nOh, that's a nice ring!\nYou got a ring! :ring:");
                    break;
                case "null":
                    await ReplyAsync("You don't have one of those.");
                    break;
                default:
                    await ReplyAsync("You feel like you shouldn't shoot at that. (Yet)");
                    break;
            }
            
        }

        [Command("pouch")]
        public async Task Pouch()
        {
            if (Check(Context, "pouch",false)) return;
            var user = Functions.GetUser(Context.User);
            if (!user.GetData<bool>("active_pouch"))
            {
                user.SetData("active_pouch", "1");
                await ReplyAsync("You equip your pouch! Plenty of space for an extra present now.\n(You can claim an extra present with `;present` one time now!)");
                user.RemoveItem("pouch");
            }
            else await ReplyAsync("You try to equip your pouch... But you've already got one on!");
        }

        [Command("newspaper"), Alias("news")]
        public async Task Newspaper()
        {
            if (Check(Context, "newspaper")) return;
            JEmbed emb = new JEmbed();
            emb.Title = "THE DAILY FORK";
            emb.Description = "Serving the people the news they need to hear, on an easy to ingest plate!";
            emb.ColorStripe = new Color(254, 254, 254);

            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "SELECT * FROM NEWSPAPER WHERE DATE_PUBLISHED > @newsDate";
                using (var com = new SQLiteCommand(stm, con))
                {
                    com.Parameters.AddWithValue("@newsDate", Var.CurrentDate().AddHours(-24));
                    using (var reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            emb.Fields.Add(new JEmbedField(x =>
                            {
                                x.Header = reader.GetString(0);
                                x.Text = reader.GetDateTime(2).AddHours(5).ToString("dddd, MMMM dd yyyy") + "\n" + reader.GetString(1);
                            }));
                        }
                    }
                }
            }

            await ReplyAsync("", embed: emb.Build());
        }

        [Command("poopbucket"), Alias("poop_bucket")]
        public async Task PoopBucket(IUser target)
        {
            if (Check(Context, "Poop Bucket")) return;
            var u2 = Functions.GetUser(target);

            if (u2.GetData<bool>("active_gnome"))
            {
                u2.SetData("active_gnome", "0");
                await ReplyAsync(DBFunctions.GetItemEmote("gnome") + $" Uh oh! {target.Mention} just gnomed you! Your poop bucket had no effect!\n{Constants.Values.GNOME_VID}");
            }
            else
            {
                var items = u2.GetItemList();
                List<int> itemsToRemove = new List<int>();
                int itemCount = 0;
                if (items.Count() >= 5) itemCount = rdm.Next(3, 6);
                else itemCount = items.Count();

                for (int i = 0; i < itemCount; i++)
                {
                    var removal = items.ElementAt(rdm.Next(items.Count()));
                    if (itemsToRemove.Contains(removal.Key))
                    {
                        i--;
                        continue;
                    }
                    itemsToRemove.Add(removal.Key);
                }
                string msg = $"{Context.User.Mention} dumps a bucket of poop on {target.Mention}! The news catches sight of this! You lost:\n";
                foreach (var itemID in itemsToRemove)
                {
                    var itemName = DBFunctions.GetItemName(itemID);
                    var emote = DBFunctions.GetItemEmote(itemID);
                    msg += $"{emote} {itemName},";
                    u2.RemoveItem(itemID);
                }
                msg = msg.Trim(',');
                int poopCount = rdm.Next(3, 10)+1;
                msg += $"\n:poop: {poopCount} poops fall into your inventory! :poop:";
                for (int i = 0; i < poopCount; i++) u2.GiveItem("poop");
                await ReplyAsync(msg);

                DBFunctions.AddNews("MYSTERIOUS MAN DUMPS FECAL MATTER ON INNOCENT BYSTANDER!",
                                    $"On {Var.CurrentDate().ToString("dddd, MMMM dd")} at {Var.CurrentDate().ToString("h:mm tt")}, local civilian {target.Username} was " +
                                    $"minding their own business, when suddenly a crazy individual with a bucket came out of no where and dumped feces all over them! " +
                                    $"After an investigation authorities say the substance contained a mix of urine and fecal matter. \"I've never felt so disgusted in my " +
                                    $"life.\" says {target.Username}. The police say that an investigation is underway to find this mysterious bucket wielding maniac.");
            }
        }

        [Command("telescope")]
        public async Task Telescope([Remainder] string param = "")
        {
            if (Check(Context, "telescope", false)) return;

            if (param == "")
            {
                if (DateTime.Now.Hour < 5 || DateTime.Now.Hour > 20)
                {
                    await ReplyAsync("The stars look beautiful tonight. What else can you see?");
                }
                else await ReplyAsync("Seems it's too bright out to use the that right now.");
            }
            else
            {
                param = param.ToLower();
                var users = await Context.Guild.GetUsersAsync();
                var names = users.Select(x => x.Username.ToLower());
                var items = new User(Context.User.Id).GetItemList();

                if (param == "maxwell")
                    await ReplyAsync("Who?");
                else if (names.Contains(param))
                    await ReplyAsync("They're too close! You can barely even make out any of their features.");
                else if (items.Where(x => DBFunctions.GetItemName(x.Key).ToLower() == param).Count() > 0)
                    if (param == "alien")
                    {
                        await ReplyAsync("The alien looks at directly back at you. It's eyes begin to glow. :alien:");
                        await Context.User.SendMessageAsync(":alien: Suddenly, the alien raises it's hand and it's thoughts begin to flow into your mind," +
                            " but only using one word for each:\n" +
                            "*SCOPE. POWER. DEATH. VOID. ASCEND.*");
                    }
                    else
                        await ReplyAsync($"Seeing your {param} up close really gives you a new perspective on it!");
                else
                    await ReplyAsync("You look through your telescope and see...\nA black squirrel...\nA grey cat...\nAnd a red herring!\n" +
                        $"No sign of any {param}.");
                

            }
        }

        [Command("super_scope"), Alias("superscope")]
        public async Task SuperScope([Remainder] string param = "")
        {
            if (Check(Context, "super scope", false)) return;

            var arrivalDate = new DateTime(2020, 1,30);

            await ReplyAsync($"The voids expansion increases. It will arrive in {(int)((arrivalDate-DateTime.Now).TotalHours)} hours.");

        }

    }


}
