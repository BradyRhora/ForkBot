using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForkBot
{
    class Raid
    {
        public static string startMessage = "🧙 Welcome... To The Dungeon of Efrüg!\nFirst you must choose your class, then you may enter the dungeon and duel various beasts, before taking on... ***The Boss!***\n"+
                                     "To start, use the command `;r choose [class]`! Choose between the following classes:";

        Class[] classes = { new Class("Archer","","bow_and_arrow"), new Class("Cleric","","syringe"), new Class("Mage","","blue_book"), new Class("Paladin","","hammer"), new Class("Rogue", "", "dagger"), new Class("Warrior","","crossed_swords")};




        class Class
        {
            public string Name;
            public string Description;
            public string Emote;

            public Class(string Name, string Description, string Emote)
            {
                this.Name = Name;
                this.Description = Description;
            }
        }

    }
}
