using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class UpdateData
    {
        int id;
        DateTime timestamp;
        string action;

        public UpdateData(int id, DateTime timestamp, string action)
        {
            this.Id = id;
            this.Timestamp = timestamp;
            this.Action = action;
        }

        public int Id { get => id; set => id = value; }
        public DateTime Timestamp { get => timestamp; set => timestamp = value; }
        public string Action { get => action; set => action = value; }
    }
}
