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
        public BigInteger B;
        public void SetB(BigInteger B)
        {
            this.B = B;
            this.Key = computeKey();
        }
        public BigInteger Key { get; set; }
        public DiffieHellman(BigInteger p, BigInteger g)
        {
            this.p = p;
            this.g = g;
            Random rnd = new Random();
            this.b = rnd.Next(1000, Int32.MaxValue);
            this.A = BigInteger.ModPow(this.g, this.b, this.p);
        }
        public BigInteger computeKey()
        {
            return BigInteger.ModPow(this.B, this.b, this.p);
        }
        public BigInteger generateSecret()
        {
            return 15;
        }

    }
}
