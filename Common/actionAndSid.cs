using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [Serializable]
    public class ActionAndSid
    {
        string aciton;
        string sid;

        public ActionAndSid() { }

        public ActionAndSid(string aciton, string sid)
        {
            this.Aciton = aciton;
            this.Sid = sid;
        }

        public string Aciton { get => aciton; set => aciton = value; }
        public string Sid { get => sid; set => sid = value; }
    }
}
