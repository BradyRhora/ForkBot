using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ForkBot
{
    public class XMLToolbox
    {
        /// <summary>
        /// Creates a new XML File in the specified path with the specified elements.
        /// </summary>
        /// <param name="path">The path to save the XML file.</param>
        /// <param name="elements">The elements to put in the XML file.</param>
        /// <returns></returns>
        public static XmlDocument NewXML(string path, params string[] elements)
        {
            XmlWriter writer = XmlWriter.Create(path);
            writer.WriteStartDocument();
            foreach(string element in elements)
            {
                writer.WriteStartElement(element);
                writer.WriteEndElement();
            }
            writer.WriteEndDocument();
            writer.Close();
            XmlDocument xml = new XmlDocument();
            xml.Load(path);
            return xml;
        }

        /// <summary>
        /// Gets the first occurence of the specified element and returns as type T.
        /// </summary>
        /// <typeparam name="T">The type to return as.</typeparam>
        /// <param name="xml">The XML Document.</param>
        /// <param name="ElementName">The Element to search for.</param>
        /// <returns></returns>
        public static T GetData<T>(XmlDocument xml, string elementName)
        {
            var element = xml.GetElementsByTagName(elementName)[0];
            return (T)Convert.ChangeType(element.InnerText, typeof(T));
        }
        
        public static XmlDocument Open(string path)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(path);
            return xml;
        }
    }
}
