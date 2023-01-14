using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace DataBase
{
    public class DataBaseCRUD
    {
        static string filePath = "..\\..\\..\\DataBase\\DataBase.json";

        public static bool AddEntry(DataBaseEntry entry)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                List<DataBaseEntry> entries = JsonConvert.DeserializeObject<List<DataBaseEntry>>(json);
                if(!entries.Contains(entry))
                {
                    entries.Add(entry);
                    json = JsonConvert.SerializeObject(entries, Formatting.Indented);
                    File.WriteAllText(filePath, json);
                    return true;
                }
                else
                {
                    Console.WriteLine("There is already entry with same uniqueId");
                    return false;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return false;
            }
        }

        public static bool DeleteEntryById(int uniqueId)
        {
            bool returnValue = false;
            try
            {
                string json = File.ReadAllText(filePath);
                List<DataBaseEntry> entries = JsonConvert.DeserializeObject<List<DataBaseEntry>>(json);
                DataBaseEntry entry = entries.Find(f => f.UniqueId == uniqueId);
                if (entry != null)
                {
                    returnValue = entries.Remove(entry);
                    json = JsonConvert.SerializeObject(entries, Formatting.Indented);
                    File.WriteAllText(filePath, json);
                    return returnValue;
                }
                return returnValue;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return false;
            }
        }

        public static bool DeleteEntryBySId(string sId)
        {
            bool returnValue = false;
            try
            {
                string json = File.ReadAllText(filePath);
                List<DataBaseEntry> entries = JsonConvert.DeserializeObject<List<DataBaseEntry>>(json);
                DataBaseEntry entry = entries.Find(f => f.SId == sId);
                if (entry != null)
                {
                    returnValue = entries.Remove(entry);
                    json = JsonConvert.SerializeObject(entries, Formatting.Indented);
                    File.WriteAllText(filePath, json);
                    return returnValue;
                }
                return returnValue;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return false;
            }
        }

        public static DataBaseEntry FindEntryById(int uniqueId)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                List<DataBaseEntry> entries = JsonConvert.DeserializeObject<List<DataBaseEntry>>(json);
                return entries.Find(f => f.UniqueId == uniqueId);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return null;
            }
        }

        public static DataBaseEntry FindEntryBySId(string sId)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                List<DataBaseEntry> entries = JsonConvert.DeserializeObject<List<DataBaseEntry>>(json);
                return entries.Find(f => f.SId == sId);
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return null;
            }
        }

        public static bool ModifyEntry(int uniqueId, DataBaseEntry entry)
        {
            bool returnValue = false;
            try
            {
                string json = File.ReadAllText(filePath);
                List<DataBaseEntry> entries = JsonConvert.DeserializeObject<List<DataBaseEntry>>(json);
                foreach(DataBaseEntry e in entries)
                {
                    if(e.UniqueId == uniqueId)
                    {
                        returnValue = e.ModifyEntryObject(entry);
                    }
                }
                json = JsonConvert.SerializeObject(entries, Formatting.Indented);
                File.WriteAllText(filePath, json);
                return returnValue;

            }
            catch(Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return false;
            }
        }

        public static List<DataBaseEntry> ReadAllEntries()
        {
            List<DataBaseEntry> entries = new List<DataBaseEntry>();
            try
            {
                string json = File.ReadAllText(filePath);
                entries = JsonConvert.DeserializeObject<List<DataBaseEntry>>(json);
                return entries;
            } 
            catch(Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return null;
            }
        }
    }
}
