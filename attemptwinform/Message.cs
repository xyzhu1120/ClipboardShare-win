using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardShare
{
    class Message
    {
        public static readonly int TEXT = 1;
        public int type{get; set;}
        public string content{get; set;}

        public Message(int mtype, string mcontent)
        {
            type = mtype;
            content = mcontent;
        }

        public override string ToString() {
            return type + " " +content;
        }
    }
}
