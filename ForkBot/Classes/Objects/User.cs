using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.IO;

namespace ForkBot
{
    public class User
    {
        public ulong ID { get; set; }

        public User(ulong ID = 0)
        {
            this.ID = ID;
            Load();
        }

        User Load()
        {
            var userData = File.ReadAllLines($@"Users\{ID}.user");
            var items = GetDataA("items");
            return this;
        }

        public string GetData(string data)
        {
            string userPath = $@"Users\{ID}.user";
            var uData = File.ReadAllLines(userPath);
            foreach(string d in uData)
            {
                if (d.StartsWith(data))
                {
                    return d.Replace(data + ":", "");
                }
            }

            var uDataS = File.ReadAllText(userPath);
            uDataS = uDataS.Replace("items", data + ":0" + "\nitems");
            Save(uDataS);
            return "0";
        }
        public bool SetData(string data, string value)
        {
            string userPath = $@"Users\{ID}.user";
            var uData = File.ReadAllLines(userPath);
            for (int i = 0; i < uData.Count(); i++)
            {
                if (uData[i].StartsWith(data))
                {
                    uData[i] = uData[i].Substring(0, uData[i].IndexOf(':') + 1) + value;
                    Save(uData);
                    return true;
                }
            }
            return false;
        }

        private void Save(string data)
        {
            File.WriteAllText($@"Users\{ID}.user", data);
        }
        private void Save(string[] data)
        {
            File.WriteAllLines($@"Users\{ID}.user", data);
        }

        public string[] GetDataA(string data)
        {
            var uData = File.ReadAllLines($@"Users\{ID}.user");
            List<string> results = new List<string>();
            bool adding = false;
            foreach (string d in uData)
            {
                if (d.StartsWith(data)) adding = true;
                else if (adding) results.Add(d);
            }
            return results.ToArray();
        }

        public void GiveItem(string item)
        {
            var uData = File.ReadAllText($@"Users\{ID}.user");
            uData = uData.Replace("items{", "items{\r\n\t" + item);
            Save(uData);
        }
        public void RemoveItem(string item)
        {
            var uData = File.ReadAllText($@"Users\{ID}.user");
            uData.Replace("items{", "items{\n" + item);
            Save(uData);
        }
        public string[] GetItemList()
        {
            var uData = File.ReadAllLines($@"Users\{ID}.user");
            bool items = false;
            List<string> itemList = new List<string>();
            for (int i = 0; i < uData.Count(); i++)
            {
                if (uData[i].Contains("}")) break;
                else if (items) itemList.Add(uData[i].Replace("\t",""));
                else if (uData[i].Contains("items{")) items = true;
                
            }
            return itemList.ToArray();
        }

        public void GiveCoins(int amount)
        {
            SetData("coins", Convert.ToString(Convert.ToInt32(GetData("coins")) + amount));
        }
        
    }
}
