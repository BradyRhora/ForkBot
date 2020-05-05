using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForkBot
{
    class ItemCombo
    {
        public static ItemCombo[] ItemCombos = new ItemCombo[] { new ItemCombo("man","baby","meat"),
                                                          new ItemCombo("woman","baby","baby bottle"),
                                                          new ItemCombo("skull","baby","weed"),
                                                          new ItemCombo("baby bottle","milk"),
                                                          new ItemCombo("old woman","woman","wine"),
                                                          new ItemCombo("pregnant woman","man","woman"),
                                                          new ItemCombo("baby symbol","pregnant woman","watch"),
                                                          new ItemCombo("old man","man","beer"),
                                                          new ItemCombo("gift","box","ribbon"),
                                                          new ItemCombo("stopwatch","iphone","watch"),
                                                          new ItemCombo("tiger","cat","milk","meat"),
                                                          new ItemCombo("unlock","key2","lock"),
                                                          new ItemCombo("spy","gun","man"),
                                                          new ItemCombo("special:oldbm","unlock","spy"),
                                                          new ItemCombo("package","lock","box"),
                                                          new ItemCombo("poop bucket", "poop", "bucket"),
                                                          new ItemCombo("super scope", "telescope", "skull","battery"),
                                                          new ItemCombo("enchanted ring", "full void","ring"),
                                                          new ItemCombo("maxwell","weedswell","venomswell","superwell","santawell","ragewell","mariowell","goldswell","batswell","monkaswell","hollowswell")};
        public string[] Items;
        public string Result;

        public ItemCombo(string result, params string[] items)
        {
            Items = items;
            Result = result;
        }

        public static string CheckCombo(params string[] items)
        {
            int iCount = 0;
            foreach(ItemCombo c in ItemCombos)
            {
                var ingredients = c.Items.Select(x=>DBFunctions.GetItemID(x)).ToList();
                foreach (string item in items)
                {
                    var itemID = DBFunctions.GetItemID(item);
                    if (ingredients.Contains(itemID))
                    {
                        ingredients.Remove(itemID);
                        iCount++;
                    }
                }
                if (iCount == c.Items.Count() && iCount == items.Count()) return c.Result;
                else iCount = 0;
            }
            return null;
        }
    }
}
