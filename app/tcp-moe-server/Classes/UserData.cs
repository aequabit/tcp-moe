/**
 * Part of the tcp-moe project.
 * Property of aequabit.
 * Distributed under the Apache 2.0 License.
 */

using System;
using System.Security.Cryptography;

namespace tcp_moe_server.Classes
{
    class UserData
    {
        public string Username;
        public string Password;
        public string Hwid;
        public string Rank;
        public string SessionId;

        public bool Authenticated = false;
        public bool HandshakeDone = false;

        private ICryptoTransform encryptor;
        private ICryptoTransform decryptor;

        public void prepareRsa(byte[] key, byte[] iv)
        {
            RijndaelManaged rnj = new RijndaelManaged();
            encryptor = rnj.CreateEncryptor(key, iv);
            decryptor = rnj.CreateDecryptor(key, iv);

            HandshakeDone = true;
        }

        public byte[] encrypt(byte[] data)
        {
            return encryptor.TransformFinalBlock(data, 0, data.Length);
        }

        public byte[] decrypt(byte[] data)
        {
            return decryptor.TransformFinalBlock(data, 0, data.Length);
        }
    }
}
