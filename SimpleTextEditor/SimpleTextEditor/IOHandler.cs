using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SimpleTextEditor
{
    public class IOHandler
    {
        private StreamWriter writer;
        private StreamReader reader;
        public string Text { set;  get; } = "";

        public string ReadTextFile(string path)
        {
            try
            {
                reader = new StreamReader(path);
                Text = reader.ReadToEnd();
                reader.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine("Read File Exception: " + e.Message);
            }
            return Text;
        }

        public void WriteTextFile(string path, string text)
        {
            try
            {
                writer = new StreamWriter(path, true);
                writer.Write(text);
                writer.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine("Write File Exception: " + e.Message);
            }
        }
    }
}
