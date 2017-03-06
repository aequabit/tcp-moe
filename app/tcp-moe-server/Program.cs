using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using tcp_moe_server.Classes;

namespace tcp_moe_server
{
    class Program
    {
        private static Pack pack;
        private static ServerListener server;

        /**
         * Declare public key and RSA properties.
         */
        private static byte[] publicKey;
        private static RSACryptoServiceProvider rsa;

        static void Main(string[] args)
        {
            Console.Title = "0 clients | tcp-moe - Server";

            /**
             * Initialize the serializer.
             */
            pack = new Pack();

            /**
             * Setup RSA.
             */
            rsa = new RSACryptoServiceProvider(2048);
            publicKey = rsa.ExportCspBlob(false);

            /**
             * Initialize the server.
             */
            server = new ServerListener();

            /**
             * Register events.
             */
            server.ClientExceptionThrown += Events.Server_ClientExceptionThrown;
            server.ClientReadPacket += Events.Server_ClientReadPacket;
            server.ClientStateChanged += Events.Server_ClientStateChanged;
            server.ClientWritePacket += Events.Server_ClientWritePacket;
            server.ExceptionThrown += Events.Server_ExceptionThrown;
            server.StateChanged += Events.Server_StateChanged;

            /**
             * Start listening.
             */
            server.Listen(Config.port);

            /**
             * Block the application.
             */
            while (true) { } // TODO: make this more elegant
        }

        public static byte[] Decrypt(byte[] data)
        {
            return rsa.Decrypt(data, true);
        }

        public static byte[] GetPublicKey()
        {
            return publicKey;
        }

        public static void Send(ServerClient client, byte[] data)
        {
            /**
             * Get the user data.
             */
            UserData user = ((UserData)client.UserState);

            /**
             * If the handshake is done, encrypt the data.
             */
            if (user.HandshakeDone)
                data = user.encrypt(data);

            client.Send(data);
        }
    }
}
