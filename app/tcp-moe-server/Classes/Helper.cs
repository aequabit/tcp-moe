using System;
using System.Text;
using System.Security.Cryptography;

namespace tcp_moe_server.Classes
{
    public class Helper
    {
        public static void Log(string format, params object[] args)
        {
            string txt = String.Format("[{0}] {1}", DateTime.Now.ToString("dd.MM.yyyy, HH:mm:ss"), String.Format(format, args));
            Console.WriteLine(txt);
        }

        public static string Base64EncodeUrl(string data)
        {
            return data.Replace("+", "-").Replace("/", "_").Replace("=", "~");
        }

        public static string Base64Encode(string data)
        {
            return Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(data)
                );
        }

        public static string Base64Decode(string base64)
        {
            return Encoding.UTF8.GetString(
                    Convert.FromBase64String(
                        base64
                    )
                );
        }

        public static string SHA256(string input)
        {
            using (SHA256 hasher = System.Security.Cryptography.SHA256.Create())
            {
                byte[] dbytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sBuilder = new StringBuilder();

                for (int n = 0; n <= dbytes.Length - 1; n++)
                {
                    sBuilder.Append(dbytes[n].ToString("X2"));
                }

                return sBuilder.ToString().ToLower();
            }
        }
    }
}
