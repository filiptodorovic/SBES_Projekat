using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace DataBase
{
    public class DataBaseEntry
    {
        int uniqueId;
        string sId;
        string username;
        DateTime timeStamp;
        string actionName;

        public int UniqueId { get => uniqueId; set => uniqueId = value; }
        public string SId { get => sId; set => sId = value; }
        public string Username { get => username; set => username = value; }
        public DateTime TimeStamp { get => timeStamp; set => timeStamp = value; }
        public string ActionName { get => actionName; set => actionName = value; }
    
        public DataBaseEntry()
        {
            uniqueId = 0;
            sId = "";
            username = "";
            timeStamp = DateTime.MinValue;
            actionName = "";
        }

        public DataBaseEntry(int uniqueId, string sId, string username, DateTime timeStamp, string actionName)
        {
            UniqueId = uniqueId;
            SId = sId;
            Username = username;
            TimeStamp = timeStamp;
            ActionName = actionName;
        }

        public override string ToString()
        {
           
           return $"UniqueID: {UniqueId}\n" +
                  $"SId: {SId}\n" +
                  $"Username: {Username}\n" +
                  $"Time Stamp: {TimeStamp.ToString()}\n" +
                  $"Action Name: {ActionName}\n";
        }

        //treba nam da bi proverili da li postoji vec neki entitet sa ovim idijem. list.Contains poziva ovu metodu,
        //a ovde samo proveravamo dva objekta na osnovu uniqueId, nece proveravati svako polje posebno
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return (this.uniqueId.Equals(((DataBaseEntry)obj).uniqueId));
        }

        public bool ModifyEntryObject(DataBaseEntry entry)
        {
            try
            {
                this.TimeStamp = entry.TimeStamp;
                this.ActionName = entry.ActionName;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
                return false;
            }
        }
    }
}
