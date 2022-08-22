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
        static Random rdm = new Random();
        int spinCounter = 0;
        bool cashedOut = false;

        static Slot[][] Slots = new Slot[3][];
        public SlotMachine(IUser user, int bet)
        {
            if (Slots[0] == null) SetSlots();
            gambler = user;
            this.bet = bet;
        }

        public void SetSlots()
        {
            for (int i = 0; i < 3; i++)
            {
                Slots[i] = ShuffleSlots();
            }
        }
        
        public static Slot[] ShuffleSlots()
        {
            List<int> added = new List<int>();
            List<Slot> slotReturn = new List<Slot>();
            for (int i = 0; i < slotOptions.Count(); i++)
            {
                int index;
                do 
                    index = rdm.Next(slotOptions.Count());
                while (added.Contains(index));
                slotReturn.Add(slotOptions[index]);
                added.Add(index);
            }
            return slotReturn.ToArray();
        }

        static Slot BlankSlot = new Slot("black_large_square", 0);
        static Slot[] slotOptions =
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

        public async Task<string> Spin()
        {
            var slot = Slots[0];
            for (int i = 0; i < 3; i++) spins[i] = rdm.Next(Slots[i].Count());

            int winnings = Convert.ToInt32(GetWinnings());
            Properties.Settings.Default.jackpot -= winnings;
            if (Properties.Settings.Default.jackpot < 0) Properties.Settings.Default.jackpot = 0;
            Properties.Settings.Default.Save();
            await User.Get(GetGambler()).GiveCoinsAsync(winnings);

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
                           $"{Slots[0][Prev(spins[0],0)]} : {Slots[1][Prev(spins[1],1)]} : {Slots[2][Prev(spins[2],2)]}\n" +
                           $"{Slots[0][spins[0]]} : {Slots[1][spins[1]]} : {Slots[2][spins[2]]} :arrow_left:\n" +
                           $"{Slots[0][Next(spins[0],0)]} : {Slots[1][Next(spins[1],1)]} : {Slots[2][Next(spins[2],2)]}";
            return board;
        }
        public int SpinCount() { return spinCounter; }

        public double GetWinnings()
        {
            for (int i = 0; i < slotOptions.Count(); i++)
            {
                if (SlotCount("gem") == 3)
                {
                    var jackpot = Properties.Settings.Default.jackpot;
                    Properties.Settings.Default.jackpot = 0;
                    Properties.Settings.Default.Save();
                    DBFunctions.AddNews($"{gambler.Username.ToUpper()} TAKES THE JACKPOT!", $"{Var.CurrentDateFormatted()} {gambler.Username} scored big time, and got those lucky three gems in a row " +
                        $"from the Slots! They went home with a whopping {jackpot} coins and a big smile! Just don't spend it all in one place!");
                    return jackpot;
                }
                else if (Slots[0][spins[0]].ToString() == slotOptions[i].ToString() && Slots[1][spins[1]].ToString() == slotOptions[i].ToString() && Slots[2][spins[2]].ToString() == slotOptions[i].ToString()) return slotOptions[i].GetValue() * bet;
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

        int Prev(int index, int slotIndex)
        {
            if (index - 1 < 0) return Slots[slotIndex].Count() - 1;
            else return index - 1;
        }
        int Next(int index, int slotIndex)
        {
            if (index + 1 > Slots[slotIndex].Count() - 1) return 0;
            else return index + 1;
        }

        int CategoryCount(string category)
        {
            int categoryCount = 0;
            for (int i = 0; i < 3; i++)
            {
                if (Slots[i][spins[i]].GetCategory() == category) categoryCount++;
            }
            return categoryCount;
        }
        int SlotCount(string slot)
        {
            int slotCount = 0;
            for (int i = 0; i < 3; i++)
            {
                if (Slots[i][spins[i]].ToString() == ":" + slot + ":") slotCount++;
            }
            return slotCount;
        }
    }
}