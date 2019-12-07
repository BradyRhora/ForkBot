using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForkBot
{
    public partial class Raid
    {
        public class Class
        {

            static Class Archer = new Class("Archer", "The sharpshooting master, attacking from a distance, never missing their target.", "🏹", power: 7);
            static Class Cleric = new Class("Cleric", "The magical healer, protecting and buffing their allies.", "💉", magic_power: 6);
            static Class Mage = new Class("Mage", "The amazing spellcaster, blasting their enemies with elements and more.", "📘", magic_power: 8);
            static Class Paladin = new Class("Paladin", "The religious warrior, smiting their enemies and blessing their allies.", "🔨", magic_power: 7, power: 6);
            static Class Rogue = new Class("Rogue", "The stealthy thief, moving quickly and quietly, their enemies won't see them coming.", "🗡", speed: 8);
            static Class Warrior = new Class("Warrior", "The mighty fighter, using a variety of tools and weapons to surpass their foes.", "⚔", power: 8);
            static Class[] classes = { Archer, Cleric, Mage, Paladin, Rogue, Warrior };

            public static Class[] Classes() { return classes; }
            public static string startMessage()
            {
                string msg = ":man_mage: Welcome... To The Dungeon of Efrüg! A mysterious dungeon that shifts its rooms with each entry, full of deadly monsters and fearsome foes!\n" +
                             "First you must choose your class, then you may enter the dungeon and duel various beasts, before taking on... ***The Boss!***\n" +
                             "To start, use the command `;r choose [class]`! You may choose between the following classes:\n\n";

                foreach (Class c in classes)
                {
                    msg += $"{c.Emote} The {c.Name}, {c.Description}\n";
                }



                return msg;
            }

            public string Name;
            public string Description;
            public string Emote;
            public int Speed, Power, Magic_Power;
            public Class(string Name, string Description, string Emote, int speed = 5, int power = 5, int magic_power = 5)
            {
                this.Name = Name;
                this.Description = Description;
                this.Emote = Emote;
                Speed = speed;
                Power = power;
                Magic_Power = magic_power;
            }
            public static Class GetClass(string className)
            {
                return classes.Where(x => x.Name.ToLower() == className.ToLower()).FirstOrDefault();
            }
        }
        
        public class Action : IShoppable
        {
            public static Action Attack = new Action("attack", "Use your fists or currently equipped weapon to attack an enemy in range.", Type.Attack);
            public static Action Move = new Action("move", "Move somewhere else on the board.", Type.Movement);
            public static Action Pass = new Action("pass", "End your turn without taking an action.", Type.Pass);
            public static Action Equip = new Action("equip", "Equip an item from your inventory to use as a weapon", Type.Equip);
            public static Action[] Actions = new Action[] { Attack, Move, Pass, Equip };
            public string Name { get; set; }
            public string Description { get; set; }
            public Type type;

            //shop vars
            public int Price { get; set; }
            public bool ForSale { get; set; }
            public string Emote { get; set; }


            public Action(string name, string description, Type type)
            {
                Name = name;
                Description = description;
                this.type = type;
            }

            public override bool Equals(object obj)
            {
                return (obj is Action) && ((Action)(obj)).Name == Name;
            }

            public override int GetHashCode()
            {
                var hashCode = -176021468;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Description);
                hashCode = hashCode * -1521134295 + type.GetHashCode();
                return hashCode;
            }

            public enum Type
            {
                Attack,
                Movement,
                Spell,
                Pass,
                Equip,
                Skill
            }
        }
        public class Spell : Action
        {
            static Spell Lightning_Bolt = new Spell("lightning bolt", "⚡", "Fires a jolt of lightning that can zap through multiple enemies.");
            static Spell Magic_missile = new Spell("magic missile", "☄", "Multiple beams of light launch at various enemies.");
            static Spell Fire_Bolt = new Spell("fire bolt", "🔥", "Launches a burning flame at the enemy, possibly setting it aflame.");
            static Spell Tornado = new Spell("tornado", "🌪", "Creates a powerful cyclone of wind that sucks in enemies and deals damage over time.");
            static Spell Summon_Familiar = new Spell("summon familiar", "🐺", "Summons a creature to aid you in battle!");
            static Spell Heal = new Spell("heal", "❤️", "Restores an ally's heath.");
            static Spell Flame_Wall = new Spell("flame wall", "🔥", "Creates a wall of fire across the room that lasts for several turns.");

            public static Spell[] Spells = { Lightning_Bolt, Magic_missile, Fire_Bolt, Tornado, Summon_Familiar, Heal, Flame_Wall };
            string EffectEmote;
            public Spell(string name, string emote, string description) : base(name,description, Type.Spell)
            {
                ForSale = true;
                Emote = "📖";
                EffectEmote = emote;
            }
        }

        public class Skill : Action
        {
            public static Skill[] Skills = { };


            public Skill(string name, string description, string emote) :base(name,description,Type.Skill)
            {
                ForSale = true;
                Emote = emote;
            }
        }

        public interface IShoppable
        {
            bool ForSale { get; set; }
            int Price { get; set; }
            string Description { get; set; }
            string Name { get; set; }

            string Emote { get; set; }
        }
    }
}
