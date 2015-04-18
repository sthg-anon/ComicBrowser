using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace ComicBrowser
{
    class FileOpenHistory : IDisposable
    {
        public event EventHandler OnFileSelect;

        private const string FILE_NAME = "history.xml";
        private const int HISTORY_LENGTH = 10;

        private readonly LinkedList<string> history = new LinkedList<string>();

        private int width = 20;
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
                        history.AddLast(fileName);
                        adjustWidth(fileName);
                    }
                }
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

        public void Remove(string item)
        {
            history.Remove(item);
        }

        public void OpenFile(string file)
        {
            if (history.Count > 0 && history.Last.Equals(file))
            {
                return;
            }
            else if (history.Contains(file))
            {
                history.Remove(file);
            }
            else
            {
                adjustWidth(file);
            }

            history.AddFirst(file);
            if(history.Count > HISTORY_LENGTH)
            {
                history.RemoveLast();
            }
            
            using (FileStream fs = new FileStream(FILE_NAME, FileMode.Create, FileAccess.Write))
            {
                XmlTextWriter writer = new XmlTextWriter(fs, System.Text.Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("history");

                foreach(string openedFile in history)
                {
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
                entries[index].Click += OnFileSelect;
                index++;
            }

            return entries;
        }

        public void Dispose()
        {
            graphics.Dispose();
        }
    }
}
