/**
 * Part of the tcp-moe project.
 * Property of aequabit.
 * Distributed under the Apache 2.0 License.
 */

﻿using System;
using System.Security.Cryptography;

namespace tcp_moe_client.Classes
{
    public class Session
    {
        public static string Username;
        public static string Rank;

        public static bool Authenticated = false;
        public static bool HandshakeDone = false;

        public static ICryptoTransform encryptor;
        public static ICryptoTransform decryptor;

        public static byte[] encrypt(byte[] data)
        {
            return encryptor.TransformFinalBlock(data, 0, data.Length);
        }

        public static byte[] decrypt(byte[] data)
        {
            return decryptor.TransformFinalBlock(data, 0, data.Length);
        }
    }
}
