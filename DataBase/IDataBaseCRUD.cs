using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace DataBase
{
    public interface IDataBaseCRUD
    {
        bool AddEntry(string filePath, DataBaseEntry entry);
        
        bool DeleteEntryById(string filePath, int uniqueId);
        
        bool DeleteEntryBySId(string filePath, string sId);

        List<DataBaseEntry> ReadAllEntries(string filePath);

        DataBaseEntry FindEntryById(string filePath, int uniqueId);
        
        DataBaseEntry FindEntryBySId(string filePath, string sId);

        bool ModifyEntry(string filePath, int uniqueId, DataBaseEntry entry);



    }
}
