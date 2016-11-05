using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Client
{
    class DiffieHellman
    {
        public BigInteger p { get; set; }
        public BigInteger g { get; set; }
        public BigInteger b { get; set; }
        public BigInteger A { get; set; }
        public BigInteger B { get; set; }
        public BigInteger test { get; set; }
        public DiffieHellman(int p, int g)
        {
            this.p = p;
            this.g = g;
            this.b = generateRandomNumber();
            this.B = BigInteger.ModPow(this.g, this.b, this.p);
        }
        public BigInteger generateRandomNumber()
        {
            return 15;
        }
        public BigInteger generateSecret()
        {
            return 15;
        }
    }
}
