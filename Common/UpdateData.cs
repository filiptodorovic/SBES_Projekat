using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [Serializable]
    public class UpdateData
    {
        int id;
        DateTime timestamp;
        string action;
        string sid; 

        public UpdateData() { }

        public UpdateData(int id, DateTime timestamp, string action, string sid)
        {
            this.Id = id;
            this.Timestamp = timestamp;
            this.Action = action;
            this.Sid = sid;
        }

        public int Id { get => id; set => id = value; }
        public DateTime Timestamp { get => timestamp; set => timestamp = value; }
        public string Action { get => action; set => action = value; }
        public string Sid { get => sid; set => sid = value; }
    }
}
