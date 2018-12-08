/*
 * This solution is for demonstration purposes only. The solution requires you to enter
 * a domain username and password prior to compiling. Ideally, you would be prompted
 * for credentials or pull the credentials from an external data source at runtime.
 * The unattend.txt file included in the ConfigFile folder must be copied to
 * C:\Program Files\Common Files\microsoft shared\Web Server Extensions\16\CONFIG\
 * prior to running this application. Do not rename the file.
 * 
 * Once the farm has been created using the included PowerShell cmdlet, the farm will
 * begin processing the key/value pairs in the unattend.txt file.
 */

using System.Threading;
using System.Security;
using System.Net;
using System.Collections;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Linq;
using System;

namespace FarmLaunch
{
    class Program
    {
        static void Main(string[] args)
        {
            
            SecureString secString = new NetworkCredential("", "passWord").SecurePassword;
            SecureString passPhrase = new SecureString();
            SecureString passWord = new SecureString();
            Array.ForEach("P@ssw0rD!".ToCharArray(), passPhrase.AppendChar);
            PSCredential c = new PSCredential(@"contoso\farmAccount", secString);

            var hTable = new Hashtable
            {
                { "SPIisWebServiceApplicationPool", c },
                { "SPUnattendedDataRefresh", c },
                { "SecureStoreEncryptionKey", c },
                { "SPFarmService", c }
            };

            //This is used by SPFarmInitializationSettings to retrieve credentials.
            Thread.SetData(Thread.GetNamedDataSlot("SPConfig_Secrets"), hTable);

            using (PowerShell instance = PowerShell.Create())
            {
                instance.AddCommand("Add-PSSnapin").AddParameter("Name", "Microsoft.SharePoint.PowerShell");
                instance.AddStatement();
                instance.AddCommand("New-SPConfigurationDatabase").AddParameter("DatabaseName", "Config").AddParameter("AdministrationContentDatabaseName", "Admin").
                    AddParameter("DatabaseServer", "SqlServerName").AddParameter("LocalServerRole", "Custom").AddParameter("Passphrase", passPhrase).
                    AddParameter("FarmCredentials", c);
                instance.AddStatement();

                Collection<PSObject> PSOutput = instance.Invoke();

                if (instance.Streams.Error.Count > 0)
                {
                    Console.WriteLine(instance.Streams.Error.ElementAt(0).Exception.ToString());
                    while (Console.ReadKey().Key != ConsoleKey.Enter) { };
                }
            }
        }
    }
}