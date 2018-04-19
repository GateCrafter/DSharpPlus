using System;
using Newtonsoft.Json.Linq;

namespace DSharpPlus.Lavalink
{
    public class StringEventArgs : System.EventArgs
    {
        public StringEventArgs(string s)
        {
            message = s;
        }
        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
    public class JsonEventArgs : System.EventArgs
    {
        public JsonEventArgs(JObject d)
        {
            data = d;
        }
        private JObject data;

        public JObject Data
        {
            get { return data; }
            set { data = value; }
        }
    }

    public class ExceptionEventArgs : System.EventArgs
    {
        public ExceptionEventArgs(Exception e)
        {
            error = e;
        }
        private Exception error;

        public Exception Error
        {
            get { return error; }
            set { error = value; }
        }
    }
}