// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Runtime.Serialization;

namespace System.Reflection
{
    public class StrongNameKeyPair : IDeserializationCallback, ISerializable
    {
        bool _keyPairExported;
        byte [] _keyPairArray;
        string _keyPairContainer;
        byte [] _publicKey;

        // Build key pair from file.
        public StrongNameKeyPair(FileStream keyPairFile)
        {
            if (keyPairFile == null) {
                throw new ArgumentNullException ("keyPairFile");
            }

            var num = (int) keyPairFile.Length;
            _keyPairArray = new byte[num];
            keyPairFile.Read (_keyPairArray, 0, num);
            _keyPairExported = true;
        }

        // Build key pair from byte array in memory.
        public StrongNameKeyPair(byte[] keyPairArray)
        {
            if (keyPairArray == null) {
                throw new ArgumentNullException ("keyPairArray");
            }

            _keyPairArray = new byte[keyPairArray.Length];
            Array.Copy (keyPairArray, _keyPairArray, keyPairArray.Length);
            _keyPairExported = true;
        }

        protected StrongNameKeyPair(SerializationInfo info, StreamingContext context)
        {
            _keyPairExported = (bool) info.GetValue ("_keyPairExported", typeof(bool));
            _keyPairArray = (byte []) info.GetValue ("_keyPairArray", typeof(byte []));
            _keyPairContainer = (string) info.GetValue ("_keyPairContainer", typeof(string));
            _publicKey = (byte []) info.GetValue ("_publicKey", typeof(byte []));
        }

        public StrongNameKeyPair(string keyPairContainer)
        {
            if (keyPairContainer == null) {
                throw new ArgumentNullException ("keyPairContainer");
            }

            _keyPairContainer = keyPairContainer;
            _keyPairExported = false;
        }

        public byte[] PublicKey
        {
            [SecuritySafeCritical]
            get {
                if (_publicKey == null) {
                    _publicKey = ComputePublicKey ();
                }

                byte [] array = new byte[_publicKey.Length];
                Array.Copy (_publicKey, array, _publicKey.Length);
                return array;
            }
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue ("_keyPairExported", _keyPairExported);
            info.AddValue ("_keyPairArray", _keyPairArray);
            info.AddValue ("_keyPairContainer", _keyPairContainer);
            info.AddValue ("_publicKey", _publicKey);
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
        }

        byte [] ComputePublicKey ()
        {
            using (var rsa = Mixin.CreateRSA (this)) {
                var cspBlob = ToCapiPublicKeyBlob (rsa);
                var publicKey = new byte[12 + cspBlob.Length];
                Buffer.BlockCopy (cspBlob, 0, publicKey, 12, cspBlob.Length);
                // The first 12 bytes are documented at:
                // http://msdn.microsoft.com/library/en-us/cprefadd/html/grfungethashfromfile.asp
                // ALG_ID - Signature
                publicKey [1] = 36;
                // ALG_ID - Hash
                publicKey [4] = 4;
                publicKey [5] = 128;
                // Length of Public Key (in bytes)
                publicKey [8] = (byte) (cspBlob.Length >> 0);
                publicKey [9] = (byte) (cspBlob.Length >> 8);
                publicKey [10] = (byte) (cspBlob.Length >> 16);
                publicKey [11] = (byte) (cspBlob.Length >> 24);
                return publicKey;
            }
        }

        static byte [] ToCapiPublicKeyBlob (RSA rsa)
        {
            var rsap = rsa.ExportParameters (false);
            var blob = new byte[rsap.Modulus.Length + 20];
            blob [0] = 0x06; // PUBLICKEYBLOB (0x06)
            blob [1] = 0x02; // Version (0x02)
            blob [2] = 0x00; // Reserved (word)
            blob [3] = 0x00;
            blob [5] = 0x24; // ALGID
            WriteUInt32LE (blob, 8, 0x31415352); // DWORD magic = RSA1
            WriteUInt32LE (blob, 12, (uint) rsap.Modulus.Length << 3); // DWORD bitlen

            // DWORD public exponent
            blob [18] = rsap.Exponent [0];
            blob [17] = rsap.Exponent [1];
            blob [16] = rsap.Exponent [2];

            // BYTE modulus[rsapubkey.bitlen/8];
            Array.Reverse (rsap.Modulus);
            Buffer.BlockCopy (rsap.Modulus, 0, blob, 20, rsap.Modulus.Length);
            return blob;
        }

        static void WriteUInt32LE (byte [] bytes, int offset, uint value)
        {
            bytes [offset + 3] = (byte) (value >> 24);
            bytes [offset + 2] = (byte) (value >> 16);
            bytes [offset + 1] = (byte) (value >> 8);
            bytes [offset] = (byte) value;
        }
    }
}
