using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class Base64
    {
        public static string Encode(string text)
        {
            return Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(text));
        }

        public static string Decode(string text)
        {
            return ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(text));
        }
    }

    public enum Encryption
    {
        none,
        cezar,
        xor
    }
}
