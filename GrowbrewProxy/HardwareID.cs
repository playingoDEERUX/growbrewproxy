using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace GrowbrewProxy
{
    public class HardwareID
    {
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++) sb.Append(hashBytes[i].ToString("X2"));
                return sb.ToString();
            }
        }

        public static string GetHwid()
        {
            string hStrMd = "MD_NONE";

            string cpuID = identifier("win32_processor", "processorID");
            hStrMd = CreateMD5(cpuID);

            return hStrMd;
        }

        public static string identifier(string wmiClass, string wmiProperty)
        {
            string result = "";
            ManagementClass mc =
                new ManagementClass(wmiClass);
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
                //Only get the first one
                if (result == "")
                    try
                    {
                        result = mo[wmiProperty].ToString();
                        break;
                    }
                    catch
                    {
                    }

            return result;
        }
    }
}