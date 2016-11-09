using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Client
{
    public class XOR
    {
        public static string Code(string input, int key)
        {
            byte pad;
            byte[] buffor = Encoding.ASCII.GetBytes(input);
            byte[] tmp = new byte[input.Length];

            if (BitConverter.IsLittleEndian)
                pad = (byte)key.ToString()[0];
            else
                pad = (byte)key.ToString()[key.ToString().Length - 1];

            for (int i = 0; i < input.Length; i++)
                tmp[i] = (byte)(buffor[i] ^ pad);

            return Encoding.ASCII.GetString(tmp);
        }
    }

    public class CaesarShift
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
