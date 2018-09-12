using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Discord;

namespace ForkBot
{
    class SlotMachine
    {
        IUser gambler;
        int bet;
        int[] spins = new int[3];
        Random rdm = new Random();
        int spinCounter = 0;
        bool cashedOut = false;

        public SlotMachine(IUser user, int bet)
        {
            gambler = user;
            this.bet = bet;
        }

        static Slot BlankSlot = new Slot("black_large_square", 0);
        Slot[] slots =
        {
            BlankSlot,
            new Slot("apple",2,"fruit"),
            new Slot("grapes",2.5,"fruit"),
            new Slot("cherries",5,"fruit"),
            BlankSlot,
            new Slot("bell",10),
            new Slot("seven",20),
            new Slot("moneybag",50),
            new Slot("gem",0)
        };

        public string Spin()
        {
            for (int i = 0; i < 3; i++) spins[i] = rdm.Next(slots.Count());

            int winnings = Convert.ToInt32(GetWinnings());
            Properties.Settings.Default.jackpot -= winnings;
            if (Properties.Settings.Default.jackpot < 0) Properties.Settings.Default.jackpot = 0;
            Properties.Settings.Default.Save();
            Functions.GetUser(GetGambler()).GiveCoins(winnings);

            string winMSG = ":poop: YOU LOSE";
            if (winnings != 0) winMSG = ":star: YOU WIN!";

            string resultMSG = "\n===========\n" +
                     winMSG + "\n" +
                     "===========";

            if (winnings != 0) resultMSG += $"\nYou got {winnings} coins!";
            else resultMSG += $"\nYou lost {GetBet()} coins.";

            return resultMSG;
        }
        public string Generate()
        {
            string board = ":slot_machine: **SLOTS** :slot_machine:\n" +
                           "===========\n" +
                           ":gem: " + Properties.Settings.Default.jackpot + "\n" +
                           "===========\n" +
                           $"{slots[Prev(spins[0])]} : {slots[Prev(spins[1])]} : {slots[Prev(spins[2])]}\n" +
                           $"{slots[spins[0]]} : {slots[spins[1]]} : {slots[spins[2]]} :arrow_left:\n" +
                           $"{slots[Next(spins[0])]} : {slots[Next(spins[1])]} : {slots[Next(spins[2])]}";
            return board;
        }
        public int SpinCount() { return spinCounter; }

        public double GetWinnings()
        {
            for (int i = 0; i < slots.Count(); i++)
            {
                if (SlotCount("gem") == 3)
                {
                    var jackpot = Properties.Settings.Default.jackpot;
                    Properties.Settings.Default.jackpot = 0;
                    Properties.Settings.Default.Save();
                    return jackpot;
                }
                else if (spins[0] == i && spins[1] == i && spins[2] == i) return slots[i].GetValue() * bet;
                else if (SlotCount("seven") == 2) return 5 * bet;
                else if (CategoryCount("fruit") == 3) return 1.5 * bet;
                else if (CategoryCount("fruit") == 2) return 1.2 * bet;
                else if (SlotCount("cherries") == 2) return 2 * bet;
            }
            return 0;
        }
        public IUser GetGambler() { return gambler; }
        public int GetBet() { return bet; }
        public bool CashedOut() { return cashedOut; }

        int Prev(int index)
        {
            if (index - 1 < 0) return slots.Count() - 1;
            else return index - 1;
        }
        int Next(int index)
        {
            if (index + 1 > slots.Count() - 1) return 0;
            else return index + 1;
        }

        int CategoryCount(string category)
        {
            int categoryCount = 0;
            for (int i = 0; i < 3; i++)
            {
                if (slots[spins[i]].GetCategory() == category) categoryCount++;
            }
            return categoryCount;
        }
        int SlotCount(string slot)
        {
            int slotCount = 0;
            for (int i = 0; i < 3; i++)
            {
                if (slots[spins[i]].ToString() == ":" + slot + ":") slotCount++;
            }
            return slotCount;
        }
    }
}