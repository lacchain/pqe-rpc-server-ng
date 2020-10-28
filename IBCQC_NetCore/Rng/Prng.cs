using Org.BouncyCastle.Crypto.Prng;

namespace IBCQC_NetCore.Rng
{
    public class Prng

    {

        public byte[]  GetBytes(int byteCount)
        {

            byte[] bytes = new byte[byteCount];
            var randomGenerator = new CryptoApiRandomGenerator();
            randomGenerator.NextBytes(bytes, 0, byteCount);

            return bytes;

        }

    }
}
