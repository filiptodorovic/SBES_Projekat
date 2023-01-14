using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityManager
{
    public class Formatter
    {
        /// <summary>
        /// Returns username based on the Windows Logon Name. 
        /// </summary>
        /// <param name="winLogonName"> Windows logon name can be formatted either as a UPN (<username>@<domain_name>) or a SPN (<domain_name>\<username>) </param>
        /// <returns> username </returns>
        public static string ParseName(string winLogonName)
        {
            string[] parts = new string[] { };

            if (winLogonName.Contains("@"))
            {
                ///UPN format
                parts = winLogonName.Split('@');
                return parts[0];
            }
            else if (winLogonName.Contains("\\"))
            {
                /// SPN format
                parts = winLogonName.Split('\\');
                return parts[1];
            }
            else if (winLogonName.Contains("CN"))
            {
                // sertifikati, name je formiran kao CN=imeKorisnika, OU=rola;
                int startIndex = winLogonName.IndexOf("=") + 1;
                int endIndex = winLogonName.IndexOf(","); //sad umesto ; savljamo , jer nam je dotle username
                string s = winLogonName.Substring(startIndex, endIndex - startIndex);
                return s;
            }
            else
            {
                return winLogonName;
            }
        }

        public static string ParseGroup(string name)
        {
            string group = "";


            group = name.Substring(name.IndexOf("OU=")).Split(' ')[0];
            group = group.Substring(group.IndexOf("=") + 1);
            group = group.Remove(group.Length - 1);


            return group;

        }
    }
}
