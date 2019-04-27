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

        public async Task<string> GetName(IGuild guild)
        {
            return Functions.GetName(await guild.GetUserAsync(ID));
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
        public void SetData(string data, string value)
        {
            string userPath = $@"Users\{ID}.user";
            var uData = File.ReadAllLines(userPath);
            GetData(data); // ensure that the data exists
            for (int i = 0; i < uData.Count(); i++)
            {
                if (uData[i].Split(':')[0] == data)
                {
                    uData[i] = uData[i].Substring(0, uData[i].IndexOf(':') + 1) + value;
                    Save(uData);
                }
            }
        }

        public void AddData(string data, int addition)
        {
            int newData = Convert.ToInt32(GetData(data)) + addition;
            SetData(data, Convert.ToString(newData));
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
            var items = GetItemList();
            var list = items.ToList();
            list.Remove(item);
            var uData = File.ReadAllText($@"Users\{ID}.user");
            int index = uData.IndexOf("{");
            uData = uData.Substring(0, index+1);
            foreach(string i in list)
            {
                uData += "\r\n\t" + i;
            }
            uData += "\r\n}";
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
            SetData("coins", Convert.ToString(GetCoins() + amount));
        }
        public int GetCoins() { return Convert.ToInt32(GetData("coins")); }
        public string[] GetStats()
        {
            return File.ReadAllLines($@"Users\{ID}.user").Where(x => x.StartsWith("stat.")).ToArray();
        }
    }
}
