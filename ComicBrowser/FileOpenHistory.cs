using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace ComicBrowser
{
    class FileOpenHistory
    {
        private const string FILE_NAME = "history.xml";

        private readonly Stack<string> history = new Stack<string>();

        private int width = 100;
        private Graphics graphics;
        private Font font;

        public FileOpenHistory(Graphics graphics, Font font)
        {
            this.graphics = graphics;
            this.font = font;

            if(!File.Exists(FILE_NAME))
            {
                return;
            }

            using (FileStream fs = new FileStream(FILE_NAME, FileMode.Open, FileAccess.Read))
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(fs);

                XmlNodeList nodeList = xml.DocumentElement.SelectNodes("/history");
                if (nodeList == null)
                {
                    return;
                }

                foreach (XmlNode tagNode in nodeList)
                {
                    foreach (var child in tagNode.ChildNodes)
                    {
                        XmlElement element = (XmlElement)child;
                        string fileName = element.InnerXml;
                        history.Push(fileName);
                        adjustWidth(fileName);
                    }
                }
            }

            flipStack();
        }

        private void flipStack()
        {
            Console.WriteLine("Stack length: {0}", history.Count);
            List<string> temp = new List<string>(history);
            Console.WriteLine("temp length: {0}", temp.Count);
            history.Clear();
            
            for (int ii = 0; ii < temp.Count; ii++)
            {
                Console.WriteLine("Adding {0}", temp[ii]);
                history.Push(temp[ii]);
            }
        }

        private void adjustWidth(string text)
        {
            int strWidth = (int)Math.Ceiling(graphics.MeasureString(text, font).Width);
            if (strWidth > width)
            {
                width = strWidth;
            }
        }

        public void OpenFile(string file)
        {
            adjustWidth(file);

            history.Push(file);
            using (FileStream fs = new FileStream(FILE_NAME, FileMode.Create, FileAccess.Write))
            {
                XmlTextWriter writer = new XmlTextWriter(fs, System.Text.Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("history");

                foreach(string openedFile in history)
                {
                    Console.WriteLine("saving file {0}", openedFile);
                    writer.WriteElementString("file", openedFile);
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
            }
        }

        public ToolStripButton[] getEntries()
        {
            ToolStripButton[] entries = new ToolStripButton[history.Count];

            int index = 0;
            foreach(string entry in history)
            {
                entries[index] = new ToolStripButton();
                entries[index].Width = width;
                entries[index].Text = entry;
                index++;
            }

            return entries;
        }
    }
}
