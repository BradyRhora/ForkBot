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
                string msg = "🧙 Welcome... To The Dungeon of Efrüg! A mysterious dungeon that shifts its rooms with each entry, full of deadly monsters and fearsome foes!\n" +
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
        public class Item
        {
            readonly static Item[] Items =
            {
                new Item("dagger", "🗡", 50, "A short, deadly blade that can be coated in poison."),
                new Item("key", "🔑", -1, "An item found in dungeons used to open doors and chests."),
                new Item("ring", "💍", 150, "A valuable item that can sold in shops or enchanted."),
                new Item("bow and arrow", "🏹", 50, "A well crafted piece of wood with a string attached, used to launch arrows at enemies to damage them from a distance."),
                new Item("pill", "💊", 25, "A drug with various effects."),
                new Item("syringe", "💉", 65, "A needle filled with healing liquids to regain health."),
                new Item("shield", "🛡", 45, "A sturdy piece of metal that can be used to block incoming attacks."),
                new Item("gem", "💎", 200, "A large valuable gem that can be sold or used as an arcane focus to increase a spells power."),
                new Item("apple", "🍎", 10, "A red fruit that provides minor healing."),
                new Item("banana", "🍌", 12, "A long yellow fruit that provides minor healing."),
                new Item("potato", "🥔", 15, "A vegetable that can be cooked in various ways and provides minor healing."),
                new Item("meat", "🍖", 20, "Meat from some sort of animal that can be cooked and provides more than minor healing."),
                new Item("cake", "🍰", 25, "A baked good, that's usually eaten during celebrations. Provides minor healing for all party members."),
                new Item("ale", "🍺", 10, "A cheap drink that provides minor healing, but may have unwanted side effects."),
                new Item("guitar", "🎸", 50, "A musical instrument, usually with six strings that play different notes."),
                new Item("saxophone", "🎷", 50, "A brass musical instrument."),
                new Item("drum", "🥁", 50, "A musical instrument that usually requires sticks to play beats."),
                new Item("candle", "🕯", 50, "A chunk of wax with a wick in the middle that slowly burns to create minor light.")
            };

            string Name { get; }
            string Description { get; }
            int Value { get; }
            string Emote { get; }
            string[] Tags;

            public Item(string name, string emote, int value, string description, params string[] tags)
            {
                Name = name;
                Emote = emote;
                Value = value;
                Description = description;
                Tags = tags;
            }

        }
        public class Spell
        {
            static Spell Lightning_Bolt = new Spell("lightning bolt", "⚡", "Fires a jolt of lightning that can zap through multiple enemies.");
            static Spell Magic_missile = new Spell("magic missile", "☄", "Multiple beams of light launch at various enemies.");
            static Spell Fire_Bolt = new Spell("fire bolt", "🔥", "Launches a burning flame at the enemy, possibly setting it aflame.");
            static Spell Tornado = new Spell("tornado", "🌪", "Creates a powerful cyclone of wind that sucks in enemies and deals damage over time.");
            static Spell Summon_Familiar = new Spell("summon familiar", "🐺", "Summons a creature to aid you in battle!");
            static Spell[] Spells = { Lightning_Bolt, Magic_missile, Fire_Bolt, Tornado, Summon_Familiar };

            string Name { get; }
            string Emote { get; }
            string Description { get; }
            public Spell(string name, string emote, string description)
            {
                Name = name;
                Emote = emote;
                Description = description;

            }
        }
        public class Action
        {
            public static Action Weapon = new Action("weapon", "Use your fists or currently eqipped weapon to attack an enemy in range.", Type.Attack);
            public static Action Move = new Action("move", "Move somewhere else on the board.", Type.Movement);
            public static Action Spell = new Action("spell", "Cast a spell.", Type.Spell);
            public static Action Pass = new Action("pass", "End your turn without taking an action.", Type.Pass);
            public static Action[] Actions = new Action[] { Weapon, Move, Spell, Pass };
            public string Name;
            public string Description;
            Type type;

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
                Pass
            }
        }
    }
}
