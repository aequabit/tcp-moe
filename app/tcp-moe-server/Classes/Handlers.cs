using System;

namespace tcp_moe_server.Classes
{
    public class Handlers : Enums
    {
        private static Pack pack = new Pack();

        public static void Incoming(ServerClient client, byte[] data)
        {
            /**
             * Get the user data.
             */
            UserData user = (UserData)client.UserState;

            /**
             * If the handshake is done, encrypt the data.
             */
            if (user.HandshakeDone)
                data = user.decrypt(data);

            /**
             * Deserialize the packet.
             */
            object[] unpacked = pack.Deserialize(data);

            /**
             * Break if the packet is empty.
             */
            if (unpacked.Length == 0)
            {
                Helper.Log("[client] {0}: received empty packet", client.EndPoint.ToString());
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
                    Helper.Log("[client] {0}: received handshake", client.EndPoint.ToString());
                    Handshake(client, (byte[])unpacked[1]);
                    break;

                case PacketHeader.Authentication:
                    Helper.Log("[client] {0}: received auth", client.EndPoint.ToString());
                    Authentication(client, (string)unpacked[1], (string)unpacked[2], (string)unpacked[3]);
                    break;

                case PacketHeader.Products:
                    Helper.Log("[client] {0}: received products", client.EndPoint.ToString());
                    Products(client);
                    break;

                case PacketHeader.Load:
                    Helper.Log("[client] {0}: received load", client.EndPoint.ToString());
                    Load(client, (string)unpacked[1]);
                    break;

                default:
                    Helper.Log("[client] unknown packet header: {0}", (int)header);
                    break;
            }
        }

        public static void Handshake(ServerClient client, byte[] data)
        {
            /**
             * Decrypt the client's keys.
             */
            data = Program.Decrypt(data);

            /**
             * Unpack the keys.
             */
            object[] keys = pack.Deserialize(data);

            /**
             * Cast the keys.
             */
            byte[] key = (byte[])keys[0];
            byte[] iv = (byte[])keys[1];

            /**
             * Setup RSA.
             */
            ((UserData)client.UserState).prepareRsa(key, iv);
        }

        public static void Authentication(ServerClient client, string username, string password, string hwid)
        {
            /**
             * Call the API.
             */
            new Http(
                "/authentication",
                String.Format("?username={0}&password={1}&hwid={2}", username, password, hwid),
                (response) =>
            {
                Helper.Log("[client] {0}: auth: successful", client.EndPoint.ToString(), username, password, hwid);
                switch (response)
                {
                    case "unknown_user":
                        Senders.Authentication(client, AuthResponse.UnknownUser);
                        break;

                    case "invalid_password":
                        Senders.Authentication(client, AuthResponse.InvalidPassword);
                        break;

                    case "invalid_hwid":
                        Senders.Authentication(client, AuthResponse.InvalidHwid);
                        break;

                    case "user_unverified":
                        Senders.Authentication(client, AuthResponse.Unverified);
                        break;

                    case "user_banned":
                        Senders.Authentication(client, AuthResponse.Banned);
                        break;

                    case "success":
                        /**
                         * Get the user's rank.
                         */
                        new Http("/rank", "?username=" + username, (string rank) =>
                          {
                              /**
                               * Setup the user data.
                               */
                              UserData user = (UserData)client.UserState;
                              user.Username = username;
                              user.Password = password;
                              user.Hwid = hwid;
                              user.Rank = rank;
                              user.Authenticated = true;

                              Senders.Authentication(client, AuthResponse.Success, user.Username, user.Rank);
                          });
                        break;

                    default:
                        break;
                }
            });
        }

        public static void Products(ServerClient client)
        {
            UserData user = (UserData)client.UserState;

            if (user.Authenticated)
            {
                /**
                 * Get the user's products from the API.
                 */
                new Http(
                "/products",
                String.Format("?username={0}", user.Username),
                (response) =>
                {
                    Helper.Log("[client] {0}: products - {1} - successful", client.EndPoint.ToString(), user.Username);

                    if (response == "invalid_usage" || response == "unknown_user")
                    {
                        return;
                    }

                    Senders.Products(client, response);
                });
            }
        }

        public static void Load(ServerClient client, string product)
        {
            UserData user = (UserData)client.UserState;

            if (user.Authenticated)
            {
                /**
                 * Get the .dll from the API.
                 */
                new Http(
                "/load",
                String.Format("?username={0}&product={1}", user.Username, product),
                (response) =>
                {
                    Helper.Log("[client] {0}: load - {1} / {2} - successful", client.EndPoint.ToString(), user.Username, product);

                    if (response == "invalid_usage" || response == "unknown_product")
                    {
                        return;
                    }

                    if (response == "no_access")
                    {
                        Senders.Error(client, "You don't have access to this product.");
                        return;
                    }

                    if (response == "dll_not_present")
                    {
                        Senders.Error(client, "Product is currently not available.");
                        return;
                    }

                    Senders.Load(client, Convert.FromBase64String(response));
                });
            }
        }
    }
}
