using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class XOR
    {
        public static void Enode()
        {

        }
        public static void Decode()
        {

        }
    }

    public class CesarShift
    {
        public static char Cipher(char ch, int key)
        {
            if (!char.IsLetter(ch))
                return ch;

            char offset = char.IsUpper(ch) ? 'A' : 'a';
            return (char)((((ch + key) - offset) % 26) + offset);
        }
        public static string Encode(string input, int key)
        {
            if (key > 26)
                key = key % 26;

            string output = string.Empty;

            foreach (char ch in input)
                output += Cipher(ch, key);

            return output;
        }
        public static string Decode(string input, int key)
        {
            if (key > 26)
                key = key % 26;

            return Encode(input, 26 - key);
        }
    }
}
