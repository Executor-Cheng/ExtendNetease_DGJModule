using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;

namespace ExtendNetease_DGJModule.Crypto
{
    public static class RsaHelper
    {
        private static byte[] OidSequence { get; } = new byte[15] { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };

        private static unsafe byte[] GetBytes(byte** keyPtr)
        {
            int count = 0;
            byte temp = 0;
            byte* localKeyPtr = *keyPtr;
            if (*localKeyPtr++ != 0x02)
            {
                throw new ArgumentException("Invalid data found in buffer.", nameof(keyPtr));
            }
            switch (temp = *localKeyPtr++)
            {
                case 0x81:
                    {
                        count = *localKeyPtr++;
                        break;
                    }
                case 0x82:
                    {
                        count = IPAddress.NetworkToHostOrder(*(short*)localKeyPtr);
                        localKeyPtr += 2;
                        break;
                    }
                default:
                    {
                        count = temp;
                        break;
                    }
            }
            while (*localKeyPtr == 0)
            {
                localKeyPtr++;
                count--;
            }
            byte[] buffer = new byte[count];
            fixed (byte* bufferPtr = buffer)
            {
                Buffer.MemoryCopy(localKeyPtr, bufferPtr, count, count);
                localKeyPtr += count;
            }
            *keyPtr = localKeyPtr;
            return buffer;
        }

        private static unsafe void CheckHeader(byte** keyPtr, byte toCheck)
        {
            byte* localPtr = *keyPtr;
            if (*localPtr++ == toCheck)
            {
                int add = *localPtr++ - 0x81;
                if ((uint)add <= 1)
                {
                    localPtr += add + 1;
                    *keyPtr = localPtr;
                    return;
                }
            }
            throw new ArgumentException("Invalid data found in privateKey.", nameof(keyPtr));
        }

        public static unsafe RSACryptoServiceProvider DecodePublicKey(string publicKey)
        {
            fixed (byte* _keyBuffer = Convert.FromBase64String(publicKey))
            {
                byte* keyBuffer = _keyBuffer;
                CheckHeader(&keyBuffer, 0x30);
                byte[] oid = new byte[15];
                fixed (byte* oidPtr = oid)
                {
                    Buffer.MemoryCopy(keyBuffer, oidPtr, 15, 15);
                }
                if (OidSequence.SequenceEqual(oid))
                {
                    keyBuffer += 15;
                    CheckHeader(&keyBuffer, 0x03);
                    if (*keyBuffer++ == 0)
                    {
                        CheckHeader(&keyBuffer, 0x30);
                        RSAParameters rsaParams = new RSAParameters()
                        {
                            Modulus = GetBytes(&keyBuffer),
                            Exponent = GetBytes(&keyBuffer)
                        };
                        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                        rsa.ImportParameters(rsaParams);
                        return rsa;
                    }
                }
            }
            throw new ArgumentException("Invalid data found in publicKey.", nameof(publicKey));
        }
    }
}
