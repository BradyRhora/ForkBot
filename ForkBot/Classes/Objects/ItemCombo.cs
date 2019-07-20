using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForkBot
{
    class ItemCombo
    {
        public static ItemCombo[] ItemCombos = new ItemCombo[] { new ItemCombo("man","baby","meat_on_bone"),
                                                          new ItemCombo("woman","baby","baby_bottle"),
                                                          new ItemCombo("skull","baby","weed"),
                                                          new ItemCombo("baby_bottle","milk"),
                                                          new ItemCombo("older_woman","woman","wine_glass"),
                                                          new ItemCombo("pregnant_woman","man","woman"),
                                                          new ItemCombo("baby_symbol","pregnant_woman","watch"),
                                                          new ItemCombo("older_man","man","beer"),
                                                          new ItemCombo("gift","box","ribbon"),
                                                          new ItemCombo("stopwatch","iphone","watch"),
                                                          new ItemCombo("tiger","cat","milk","meat_on_bone"),
                                                          new ItemCombo("unlock","key2","lock"),
                                                          new ItemCombo("spy","gun","man"),
                                                          new ItemCombo("special:oldbm","unlock","spy"),
                                                          new ItemCombo("package","lock","box")};
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
                var ingredients = c.Items.ToList();
                foreach (string item in items)
                {
                    if (ingredients.Contains(item))
                    {
                        ingredients.Remove(item);
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
