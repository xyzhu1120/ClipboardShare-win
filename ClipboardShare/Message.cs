using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardShare
{
    class Message
    {
        public static readonly string TEXT = "TEXT";
        public static readonly string FILE = "FILE";
        public string type{get; set;}
        public string content{get; set;}

        public Message(string mtype, string mcontent)
        {
            type = mtype;
            content = mcontent;
        }

        public Message(string msg)
        {
            int pos = 0;
            int oldpos = 0;
            pos = msg.IndexOf(" ");
            type = msg.Substring(0, pos);
            oldpos = pos;
            pos = msg.IndexOf(" ", pos + 1);
            int len = Int32.Parse(msg.Substring(pos + 1, pos - oldpos - 1));
            content = msg.Substring(pos + 1, len);
        }

        public override string ToString() {
            return type + " " +　content.Length + " " +content;
        }
    }
}
