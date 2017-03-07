/**
 * Part of the tcp-moe project.
 * Property of aequabit.
 * Distributed under the Apache 2.0 License.
 */

using System.Collections.Generic;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace tcp_moe_client.Classes
{
    public class Handlers : Enums
    {
        private static Pack pack = new Pack();

        public static void Incoming(byte[] data)
        {
            /**
             * If the handshake is done, decrypt the data.
             */
            if (Session.HandshakeDone)
                data = Session.decrypt(data);

            /**
             * Deserialize the packet.
             */
            object[] unpacked = pack.Deserialize(data);

            /**
             * Break if the packet is empty.
             */
            if (unpacked.Length == 0)
            {
                Debug.Log("[client] received empty packet");
                return;
            }

            /**
             * Get the packet header.
             */
            PacketHeader header = (PacketHeader)unpacked[0];

            /**
             * Determine the packet type.
             */
            switch (header)
            {
                case PacketHeader.Handshake:
                    Handshake((byte[])unpacked[1], (int)unpacked[2]);
                    break;

                case PacketHeader.Error:
                    Error((string)unpacked[1]);
                    break;

                case PacketHeader.Authentication:
                    Authentication((AuthResponse)unpacked[1], (string)unpacked[2], (string)unpacked[3]);
                    break;

                case PacketHeader.Products:
                    Products((string)unpacked[1]);
                    break;

                case PacketHeader.Load:
                    Load((byte[])unpacked[1]);
                    break;

                default:
                    Debug.Log("[client] unknown packet header: {0}", (int)header);
                    break;
            }
        }

        public static void Handshake(byte[] publicKey, int version)
        {
            /**
             * Check if the client is up-to-date.
             */
            if (version != Config.version)
            {
                UI.MsgBox.Error("Your client is outdated.", "Client outdated");
                Worker.instance.Shutdown();
                return;
            }

            /**
             * Instanciate RSA and import the server's public key.
             */
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
            rsa.ImportCspBlob(publicKey);

            /**
             * Create Rijndael en- and decryptor.
             */
            RijndaelManaged rnj = new RijndaelManaged();
            Session.encryptor = rnj.CreateEncryptor();
            Session.decryptor = rnj.CreateDecryptor();

            /**
             * Serialize and encrypt the keys using the server's public key.
             */
            byte[] keys = rsa.Encrypt(
                pack.Serialize(rnj.Key, rnj.IV),
                true
            );

            /**
             * Send the handshake packet.
             */
            Senders.Handshake(keys);

            Session.HandshakeDone = true;
        }

        public static void Error(string message)
        {
            UI.MsgBox.Error(message);
        }

        public static void Authentication(AuthResponse response, string username, string rank)
        {
            /**
             * Determine the authentication result.
             */
            switch (response)
            {
                case AuthResponse.Success:
                    Session.Username = username;
                    Session.Rank = rank;
                    Worker.instance.MakeLoader();
                    Worker.instance.KillAuth();
                    break;

                case AuthResponse.UnknownUser:
                    UI.MsgBox.Error("The user specified does not exist.", "Authentication error.");
                    break;

                case AuthResponse.InvalidPassword:
                    UI.MsgBox.Error("The entered password is incorrect.", "Authentication error.");
                    break;

                case AuthResponse.InvalidHwid:
                    UI.MsgBox.Error("Your hardware changed since the last login.", "Authentication error.");
                    break;

                case AuthResponse.Unverified:
                    UI.MsgBox.Error("Your account isn't verified yet.", "Authentication error.");
                    break;

                case AuthResponse.Banned:
                    UI.MsgBox.Error("You have been banned from using tcp-moe services.", "Authentication error.");
                    break;
            }
            if (response != AuthResponse.Success)
                Worker.instance.auth.Unlock();
        }

        public static void Products(string json)
        {
            /**
             * Decode the JSON and add all products to the list.
             */
            JArray products = JArray.Parse(json);
            foreach (JToken product in products)
            {
                Worker.instance.loader.AddProduct(
                    JsonConvert.DeserializeObject<Product>(product.ToString())
                );
            }
        }

        public static void Load(byte[] product)
        {
            /**
             * Start the Injection.
             */
            Injector.Inject(
                Worker.instance.loader.selected.process,
                product
            );
        }
    }
}
