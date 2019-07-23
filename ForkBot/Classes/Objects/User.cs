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
            return this;
        }

        public async Task<string> GetName(IGuild guild)
        {
            return Functions.GetName(await guild.GetUserAsync(ID));
        }

        public string GetFileString()
        {
            return File.ReadAllText($@"Users\{ID}.user");
        }
        public void SetFileString(string fileString)
        {
            File.WriteAllText($@"Users\{ID}.user", fileString);
        }
        public void Archive(bool copy = false)
        {
            if (!copy) File.Move($@"Users\{ID}.user", $@"Users\{ID}.archiveuser");
            else File.Copy($@"Users\{ID}.user", $@"Users\{ID}.archiveuser");
        }

        public string GetData(string data)
        {
            string userPath = $@"Users\{ID}.user";
            var uData = File.ReadAllLines(userPath);
            foreach(string d in uData)
            {
                if (d.StartsWith(data+":"))
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
            GetData(data); // ensure that the data exists

            var uData = File.ReadAllLines(userPath);
            for (int i = 0; i < uData.Count(); i++)
            {
                if (uData[i].Split(':')[0] == data)
                {
                    uData[i] = uData[i].Substring(0, uData[i].IndexOf(':') + 1) + value;
                    Save(uData);
                    break;
                }
            }
        }

        public void AddData(string data, int addition)
        {
            if (GetData("gemtime") != "0")
            {
                var gemTime = Functions.StringToDateTime(GetData("gemtime"));
                if (DateTime.Now < gemTime) addition *= 3;
            }
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
                else if (adding && d.Contains("}")) break;
                else if (adding) results.Add(d.Replace("\t",""));
            }

            if (!adding)
            {
                var list = uData.ToList();
                list.Add($"{data}{{");
                list.Add("}");
                uData = list.ToArray();
                Save(uData);
            }

            return results.ToArray();
        }
        public void AddDataA(string dataA, string data)
        {
            GetDataA(dataA); //ensure data array exists
            var uData = File.ReadAllText($@"Users\{ID}.user");
            uData = uData.Replace($"{dataA}{{", $"{dataA}{{\r\n\t" + data);
            Save(uData);
        }
        public void RemoveDataA(string dataA, string data)
        {
            var items = GetDataA(dataA);
            var list = items.ToList();
            list.Remove(data);
            var uData = File.ReadAllText($@"Users\{ID}.user");
            int index = uData.IndexOf($"{dataA}{{");
            int endIndex = -1;

            for (int i = index; i < uData.Length; i++)
            {
                if (uData[i] == '}') { endIndex = i; break; }
            }

            var uData2 = uData.Substring(endIndex+1);
            uData = uData.Substring(0, index + dataA.Count() + 1);
            foreach (string i in list)
            {
                uData += "\r\n\t" + i;
            }
            uData += "\r\n}" + uData2;
            Save(uData);
        }

        public void GiveItem(string item)
        {
            AddDataA("items", item);
        }
        public void RemoveItem(string item)
        {
            RemoveDataA("items", item);
        }
        public string[] GetItemList()
        {
            return GetDataA("items");
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
