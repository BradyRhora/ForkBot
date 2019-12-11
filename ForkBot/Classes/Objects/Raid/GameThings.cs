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
            static Class Archer = new Class("Archer", "The sharpshooting master, attacking from a distance, never missing their target.", "🏹", power: 8);
            static Class Cleric = new Class("Cleric", "The magical healer, protecting and buffing their allies.", "💉", magic_power: 6, baseActions: new Action[] { Spell.Heal });
            static Class Mage = new Class("Mage", "The amazing spellcaster, blasting their enemies with elements and more.", "📘", magic_power: 8, power:4, baseActions: new Action[] { Spell.Fire_Bolt });
            static Class Paladin = new Class("Paladin", "The religious warrior, smiting their enemies and blessing their allies.", "🔨", magic_power: 7, power: 7);
            static Class Rogue = new Class("Rogue", "The stealthy thief, moving quickly and quietly, their enemies won't see them coming.", "🗡", speed: 8, power:6);
            static Class Warrior = new Class("Warrior", "The mighty fighter, using a variety of tools and weapons to surpass their foes.", "⚔", power: 9, speed:4);
            static Class[] classes = { Archer, Cleric, Mage, Paladin, Rogue, Warrior };

            public static Class[] Classes() { return classes; }
            public static string StartMessage()
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
            public int BaseSpeed, BasePower, BaseMagic_Power;
            public Action[] BaseActions;
            public Class(string Name, string Description, string Emote, int speed = 5, int power = 5, int magic_power = 5, Action[] baseActions = null)
            {
                this.Name = Name;
                this.Description = Description;
                this.Emote = Emote;
                BaseSpeed = speed;
                BasePower = power;
                BaseMagic_Power = magic_power;

                if (baseActions != null)
                    BaseActions = baseActions;
                else
                    BaseActions = new Action[0];

                BaseActions = Spell.Spells;
            }
            public static Class GetClass(string className)
            {
                return classes.Where(x => x.Name.ToLower() == className.ToLower()).FirstOrDefault();
            }
        }
        
        public class Action : IShoppable
        {
            public static Action Attack = new Action("attack", "Use your fists or currently equipped weapon to attack an enemy in range.", Type.Attack, reqDir:true);
            public static Action Move = new Action("move", "Move somewhere else on the board.", Type.Movement, reqDir: true);
            public static Action Pass = new Action("pass", "End your turn without taking an action.", Type.Pass);
            public static Action Equip = new Action("equip", "Equip an item from your inventory to use as a weapon.", Type.Equip);
            public static Action Info = new Action("info", "Give information on the specified action and how to use it.", Type.Info);
            public static Action[] Actions = new Action[] { Attack, Move, Pass, Equip, Info };
            public string Name { get; set; }
            public string Description { get; set; }
            public Type type;
            public int Required_Level;
            public ActionDetails Details;
            public bool RequiresDirection;
            public string EffectEmote;
            //shop vars
            public int Price { get; set; }
            public bool ForSale { get; set; }
            public string Emote { get; set; }


            public Action(string name, string description, Type type, ActionDetails actDet = null, int reqLvl = 1, bool reqDir = false)
            {
                Name = name;
                Description = description;
                this.type = type;
                if (type == Type.Spell)
                    Types = new ItemType[] { ItemType.Magic };
                else if (type == Type.Skill)
                    Types = new ItemType[] { ItemType.Skill };
                Required_Level = reqLvl;
                Details = actDet;
                RequiresDirection = reqDir;
            }

            public static Action GetActionByName(string name)
            {
                var AllActions = Actions.Concat(Spell.Spells).Concat(Skill.Skills);
                foreach (Action a in AllActions)
                    if (a.Name == name) 
                        return a;
                return null;
            }

            public ActionDetails UseAction()
            {
                return Details;
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
                Skill,
                Info
            }

            public ItemType[] Types { get; set; }
        }

        public class Spell : Action
        {
            public static Spell Lightning_Bolt = new Spell("lightning_bolt","⚡","Fires a jolt of lightning that can zap through multiple enemies.",9,new ActionDetails(2,Direction.Cardinal,3,new ActionEffect("Zap","⚡",2,true)));
            public static Spell Magic_missile = new Spell("magic_missile", "☄", "Multiple beams of light launch at various enemies.",8, new ActionDetails(1.5,Direction.Cardinal,10));
            public static Spell Fire_Bolt = new Spell("fire_bolt", "🔥", "Launches a burning flame at the enemy, possibly setting it aflame.", 7, new ActionDetails(2, Direction.Cardinal, 7, new ActionEffect("Burn", "🔥", 1)));
            public static Spell Tornado = new Spell("tornado", "🌪", "Creates a powerful cyclone of wind that sucks in enemies and deals damage over time.", 8, new ActionDetails(0, Direction.All, 1, new ActionEffect("Tornado", "🌪", 3,lifespan:3)));
            public static Spell Summon_Familiar = new Spell("summon_familiar", "🐺", "Summons a creature to aid you in battle!",8, new ActionDetails(0, Direction.All, 1, new ActionEffect("Wolf", "🐺", 0,false,5,Monster.Wolf)));
            public static Spell Heal = new Spell("heal", "❤️", "Restores an ally's heath.",7, new ActionDetails(-2,Direction.Cardinal,5));
            public static Spell Flame_Wall = new Spell("flame_wall", "🔥", "Creates a wall of fire across the room that lasts for several turns.",9, new ActionDetails(1,Direction.Cardinal,10,new ActionEffect("Fire", "🔥", 2,true,4)));

            public static Spell[] Spells = { Lightning_Bolt, Magic_missile, Fire_Bolt, Tornado, Summon_Familiar, Heal, Flame_Wall };
            
            public Spell(string name, string emote, string description, int requiredlvl, ActionDetails actDet = null, bool reqDir = true) : base(name,description, Type.Spell, actDet, requiredlvl, reqDir)
            {
                ForSale = true;
                Emote = "📖";
                EffectEmote = emote;
                Price = 10 * requiredlvl;
            }

            public static Spell GetSpell(string spellName)
            {
                foreach(Spell s in Spells)
                    if (s.Name == spellName) return s;
                return null;
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }
        }
        
        public class ActionDetails
        {
            public double Power;
            public int Range;
            public Direction PossibleDirections;
            public ActionEffect Effect;

            public ActionDetails(double power = 0, Direction posDir = Direction.All, int range = 1, ActionEffect effect = null)
            {
                Power = power;
                Range = range;

                PossibleDirections = posDir;
                Effect = effect;
            }

        }

        public class ActionEffect : Placeable
        {
            public Monster Monster = null; 
            public int Lifespan; //in rounds
            bool Passive;

            string Name;
            string Emote;
            int Power;

            public ActionEffect(string name, string emote, int power, bool passive = true, int lifespan = 1, Monster summon = null)
            {
                Name = name;
                Emote = emote;
                Power = power;
                Passive = passive;
                Lifespan = lifespan;
                Monster = summon;

                if (Monster != null) Passive = false;

            }

            public override string GetEmote()
            {
                return Emote;
            }

            public override int GetMoveDistance()
            {
                return Power;
            }

            public override string GetName()
            {
                return Name;
            }

            public override int RollAttackDamage()
            {
                return rdm.Next(Power) + Power/2;
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


            ItemType[] Types { get; set; }

            
        }

        public enum ItemType
        {
            Magic,
            Food,
            Weapon,
            General,
            Skill
        }
    }
}
