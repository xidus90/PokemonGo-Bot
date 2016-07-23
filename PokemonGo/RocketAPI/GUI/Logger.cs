using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
namespace PokemonGo.RocketAPI.GUI
{

    internal static class RichTextBoxExtension
    {/*
        public static string Text(this RichTextBox source)
        {
            return new System.Windows.Documents.TextRange(source.Document.ContentStart, source.Document.ContentEnd).Text;
        }
        public static long Lines(this RichTextBox source)
        {
            long count = 1;
            int position = 0;
            while ((position = source.Text().IndexOf('\n', position)) != -1)
            {
                count++;
                position++;
            }
            return count;
        }*/
    }

    internal class Logger : TextWriter
    {

        private RichTextBox richTextBox;
        private StringBuilder content = new StringBuilder();

        public Logger(RichTextBox richTextBox)
        {
            this.richTextBox = richTextBox;
        }

        public override void Write(char value)
        {
            base.Write(value);

            content.Append(value);

            if (value == '\n')
            {
                if (richTextBox.Dispatcher.CheckAccess())
                {
                    WriteLogMessage();
                }
                else {
                    richTextBox.Dispatcher.Invoke(new Action(() => {
                        WriteLogMessage();
                    }));
                }

                content = new StringBuilder();
            }
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        private void WriteLogMessage()
        {
            richTextBox.AppendText(Regex.Replace(content.ToString(), @"\r\n?|\n", Environment.NewLine));
            /*
            // TODO: Find a better way
            for (int i = 0; richTextBox.Lines() > 10; i--)
                richTextBox.Text().Remove(richTextBox.Text().IndexOf(Environment.NewLine));
                */
            richTextBox.ScrollToEnd();
        }

    }

}
