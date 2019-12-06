using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

namespace ForkBot
{
    public class Item
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Custom { get; set; }
        public ulong ID { get; set; }
        public bool Shoppable { get; set; }
        public bool Present { get; set; }
        public int Cost { get; set; }
        public string GetEmote()
        {
            if (!Custom) return $":{Name}:";
            else return $"<:{Name}:{ID}>";
        }

        public void Serialize()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Item));
            using (TextWriter writer = new StreamWriter($@"Files\Items\{Name}.xml"))
            {
                serializer.Serialize(writer, this);
            }
        }
    }
}
