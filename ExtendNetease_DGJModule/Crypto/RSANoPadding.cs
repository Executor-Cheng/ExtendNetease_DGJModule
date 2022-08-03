using System;
using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;

namespace ExtendNetease_DGJModule.Crypto
{
    public sealed class RSANoPadding : RSA
    {
        private RSAParameters _parameters;

        public override RSAParameters ExportParameters(bool includePrivateParameters)
        {
            throw new NotSupportedException();
        }

        public override void ImportParameters(RSAParameters parameters)
        {
            _parameters = parameters;
        }

        public override byte[] Encrypt(byte[] data, RSAEncryptionPadding padding)
        {
            if (_parameters.Modulus == null ||
                _parameters.Exponent == null)
            {
                throw new InvalidOperationException();
            }
            byte[] mb = new byte[_parameters.Modulus.Length + 1],
                   eb = new byte[_parameters.Exponent.Length + 1],
                   db = new byte[data.Length + 1];
            Array.Copy(_parameters.Modulus, 0, mb, 1, _parameters.Modulus.Length);
            Array.Copy(_parameters.Exponent, 0, eb, 1, _parameters.Exponent.Length);
            Array.Copy(data, 0, db, 1, data.Length);
            Array.Reverse(mb);
            Array.Reverse(eb);
            Array.Reverse(db);
            BigInteger m = new BigInteger(mb),
                       e = new BigInteger(eb),
                       d = new BigInteger(db),
                       result = BigInteger.ModPow(d, e, m);
            byte[] buffer = result.ToByteArray();
            if (buffer[buffer.Length - 1] == 0)
            {
                Array.Resize(ref buffer, buffer.Length - 1);
            }
            Array.Reverse(buffer);
            return buffer;
        }

        public override byte[] Decrypt(byte[] data, RSAEncryptionPadding padding)
        {
            throw new NotSupportedException();
        }
    }
}
