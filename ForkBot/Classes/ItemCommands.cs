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
                msg = "You've been holding this in for a while.. Poop gets everywhere and is a pain to clean up.\n**Hygiene-50**";
                user.AddData("stat.hygiene", -50);
            }
            else
            {
                msg = "This... this is the best poop you've ever had in your life! You feel fantastic!\n**Hygiene+40 Happiness+20**";
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
        
        [Command("eyeglasses")]
        public async Task Eyeglasses()
        {
            if (Check(Context, "eyeglasses")) return;
            await Context.Channel.SendMessageAsync(":eyeglasses: You probably don't use these to see, but they look nice!\n**Fashion+5**");
            Functions.GetUser(Context.User).AddData("stat.fashion", 10);
        }

        [Command("briefcase")]
        public async Task Briefcase()
        {
            if (Check(Context, "briefcase")) return;

            int r = rdm.Next(50)+1;
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

        [Command("gun"), Alias("rob")]
        public async Task Gun() { if (Check(Context, "gun", false)) return; await Context.Channel.SendMessageAsync("Choose someone to rob with `;gun [user]`..."); }

        [Command("gun"), Alias("rob")]
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
                var items = u2.GetItemList();
                if (items.Count() == 0)
                {
                    await Context.Channel.SendMessageAsync($"You try to steal an item from {user.Username}... but they have nothing!"+
                                                           $" You drop your gun and run before the police arrive. {(user as IGuildUser).Mention} picks up the gun!");
                    u2.GiveItem("gun");
                }
                else
                {
                    string item = u2.GetItemList()[rdm.Next(u2.GetItemList().Count())];
                    u1.GiveItem(item);
                    u2.RemoveItem(item);
                    await Context.Channel.SendMessageAsync($":gun: {(user as IGuildUser).Mention}! {(Context.User as IGuildUser).Mention} has stolen your {item} from you!");
                }
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
            if (Check(Context, "unicorn")) return;
            if (choice <= 0 || choice > 5) return;
            var user = Functions.GetUser(Context.User);
            string msg = ":unicorn: Your wish is my command!\n";
            bool repeat;
            do
            {
                repeat = false;
                switch (choice)
                {
                    case 1:
                        int coinAmount = rdm.Next(1000, 3000);
                        user.GiveCoins(coinAmount);
                        msg += $"**+{coinAmount} coins!**";
                        break;
                    case 2:
                        int amount = rdm.Next(50, 200);
                        msg += $"**Fashion+{amount}**";
                        break;
                    case 3:
                        await Context.Channel.SendMessageAsync("You got...");
                        var presents = Functions.GetItemList();
                        for (int i = 0; i < 3; i++)
                        {
                            var sPresentData = presents[rdm.Next(presents.Count())];
                            string sPresentName = sPresentData.Split('|')[0];
                            user.GiveItem(sPresentName);
                            msg += $"A {Func.ToTitleCase(sPresentName)}! {Functions.GetItemEmote(sPresentData)} {sPresentData.Split('|')[1]}\n";
                        }
                        break;
                    case 4:
                        int hAmount = rdm.Next(50, 200);
                        msg += $"**Happiness+{hAmount}";
                        break;
                    case 5:
                        choice = rdm.Next(4) + 1;
                        repeat = true;
                        break;
                }
            } while (repeat);
            await Context.Channel.SendMessageAsync(msg);
        }

        [Command("crystal_ball"),Alias("crystalball","nextpresent")]
        public async Task CrystalBall()
        {
            if (Check(Context,"crystal_ball")) return;
            await Context.Channel.SendMessageAsync("You gaze into the crystal ball and... learn the exact time until the next present!");
            var nextPres = (Var.presentTime + Var.presentWait) - DateTime.Now;
            await Context.User.SendMessageAsync($"The next present will be ready in {nextPres.Hours} hours {nextPres.Minutes} minutes, and {nextPres.Seconds} seconds!");
        }
        
        [Command("eggplant")]
        public async Task Eggplant()
        {
            if (Check(Context, "eggplant")) return;
            await Context.Channel.SendMessageAsync(":eggplant: You eat the eggplant. What were you expecting?\n**Fullness+10**");
            Functions.GetUser(Context.User).AddData("stat.fullness", 10);
        }

        [Command("apple")]
        public async Task Apple()
        {
            if (Check(Context, "apple")) return;
            await Context.Channel.SendMessageAsync(":apple: Keeps the doctor away!\n**Healthiness+5 Fullness+10**");
            Functions.GetUser(Context.User).AddData("stat.fullness", 10);
            Functions.GetUser(Context.User).AddData("stat.healthiness", 5);
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
                Functions.GetUser(Context.User).AddData("stat.fullness", -10);
                Functions.GetUser(Context.User).AddData("stat.happiness", -20);
            }
            else
            {
                msg = ":cooking: Yum! " + eggType + " eggs!\n**Fullness+20**";
                Functions.GetUser(Context.User).AddData("stat.fullness", 20);
            }
            await ReplyAsync(msg);
        }

        [Command("egg")]
        public async Task Egg(IUser user)
        {
            if (Check(Context, "egg")) return;
            await Context.Channel.SendMessageAsync($":egg: You throw your egg at {user.Username}!\n**Their Happiness-10**");
            Functions.GetUser(user).AddData("stat.happiness", -10);
        }
        
        [Command("ramen")]
        public async Task Ramen()
        {
            if (Check(Context, "ramen")) return;
            await Context.Channel.SendMessageAsync(":ramen: Sweet sweet ramen...\n**Fullness+50**");
            Functions.GetUser(Context.User).AddData("stat.fullness", 50);
        }

        [Command("goose")]
        public async Task Goose()
        {
            if (Check(Context, "goose")) return;
            await Context.Channel.SendMessageAsync("<:goose:369992347314028554> A herd of geese fly by... eugh!\n**Happiness-35**");
            Functions.GetUser(Context.User).AddData("stat.happiness", -35);
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
                user.AddData("stat.happiness", 25);
            }
            else if (rand < 20)
            {
                msg = "Youness returns your test... You passed!\n**Happiness+20**";
                user.AddData("stat.happiness", 20);
            }
            else if (rand < 30)
            {
                msg = "Youness returns your test... You failed...\n**Happiness-30**";
                user.AddData("stat.happiness", -30);
            }
            else if (rand < 40)
            {
                msg = "Youness yells at the next class for coming in early. You sense his manliness.\n**Happiness+30**";
                user.AddData("stat.happiness", 30);
            }
            else
            {
                msg = "Youness tells you there *is* one thing you can do to get that A+ you need...\n**Fullness+10(inches) Happiness+5**";
                user.AddData("stat.fullness", 10);
                user.AddData("stat.happiness", 5);
            }

            await Context.Channel.SendMessageAsync($"<:youness:373579959899258880> {msg}");
        }

        [Command("moneybag")]
        public async Task Moneybag()
        {
            if (Check(Context, "moneybag")) return;

            int r = rdm.Next(50) + 1;
            int amount;
            if (r < 20) amount = rdm.Next(100) + 1;
            else amount = rdm.Next(100, 1000) + 1;

            await Context.Channel.SendMessageAsync($":moneybag: Loads of coins come out of the bag!!\n**+{amount} coins**");
            Functions.GetUser(Context.User).GiveCoins(amount);
        }

        [Command("shopping_cart"), Alias("shoppingcart","cart")]
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

            if (rdm.Next(100) < 60)
            {
                if (rdm.Next(100) > 80)
                {
                    int amount;
                    do amount = rdm.Next(500);
                    while (amount > Convert.ToInt32(u2.GetData("coins")));
                    u1.GiveCoins(amount);
                    u2.GiveCoins(-amount);
                    await Context.Channel.SendMessageAsync($":knife: {(user as IGuildUser).Mention}! {(Context.User as IGuildUser).Mention} has stolen {amount} coins from you!");
                }
                else
                {
                    var items = u2.GetItemList();
                    if (items.Count() == 0)
                    {
                        await Context.Channel.SendMessageAsync($"You try to steal an item from {user.Username}... but they have nothing!" +
                                                               $" You drop your knife and run before the police arrive. {(user as IGuildUser).Mention} picks up the knife!");
                        u2.GiveItem("knife");
                    }
                    else
                    {
                        string item = u2.GetItemList()[rdm.Next(u2.GetItemList().Count())];
                        u1.GiveItem(item);
                        u2.RemoveItem(item);
                        await Context.Channel.SendMessageAsync($":knife: {(user as IGuildUser).Mention}! {(Context.User as IGuildUser).Mention} has stolen your {item} from you!");
                    }
                }
            }
            else await Context.Channel.SendMessageAsync($":knife: Your attempt to rob {user.Username} fails! You get nothing.");
        }

        [Command("beer")]
        public async Task Beer()
        {
            if (Check(Context, "beer")) return;
            await Context.Channel.SendMessageAsync(":beer: You drink the beer and feel a little tipsy.\n**Sobriety-5 Happiness+10**");
            Functions.GetUser(Context.User).AddData("stat.sobriety", -5);
            Functions.GetUser(Context.User).AddData("stat.happiness", 10);
        }

        [Command("paintbrush")]
        public async Task Paintbrush()
        {
            if (Check(Context, "paintbrush", false)) return;
            using (ImageFactory fact = new ImageFactory())
            {
                using (WebClient web = new WebClient())
                {
                    bool downloaded = false;
                    while (!downloaded)
                    {
                        try { web.DownloadFile(Context.User.GetAvatarUrl(), @"Files\paintbrush.png"); downloaded = true; }
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
                foreach (int effect in effects) {
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
                            fact.ReplaceColor(colour, System.Drawing.Color.FromArgb(rdm.Next(255) + 1, rdm.Next(255) + 1, rdm.Next(255) + 1),50);
                            break;

                    }
                }
                fact.Save(@"Files\paintbrush_edited.png");
            }
            await Context.Channel.SendFileAsync(@"Files\paintbrush_edited.png");
        }

        [Command("santa")]
        public async Task Santa()
        {
            if (Check(Context, "santa")) return;

            await Context.Channel.SendMessageAsync("You got...");
            string sMessage = ""; var user = Functions.GetUser(Context.User);
            var presents = Functions.GetItemList();
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
        
        [Command("watch")]
        public async Task Watch()
        {
            if (Check(Context, "watch")) return;
            await Context.Channel.SendMessageAsync(":watch: `" + (DateTime.UtcNow-new TimeSpan(4,0,0)).ToLocalTime()+"`");
        }

        [Command("mag"), Alias("burn")]
        public async Task Mag() { if (Check(Context, "gun", false)) return; await Context.Channel.SendMessageAsync("Choose someone to burn with `;mag [user]`..."); }

        [Command("mag"), Alias("burn")]
        public async Task Mag(IUser user)
        {
            if (Check(Context, "mag")) return;
            var u1 = Functions.GetUser(Context.User);
            var u2 = Functions.GetUser(user);

            if (rdm.Next(100) > 80)
            {
                int amount;
                do amount = rdm.Next(500);
                while (amount > Convert.ToInt32(u2.GetData("coins")));
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
                    string item = u2.GetItemList()[rdm.Next(u2.GetItemList().Count())];
                    u2.RemoveItem(item);
                    await Context.Channel.SendMessageAsync($":mag: {(user as IGuildUser).Mention}! {(Context.User as IGuildUser).Mention} has burnt your {item}!");
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
        }

    }
}
