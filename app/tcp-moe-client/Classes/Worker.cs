/**
 * Part of the tcp-moe project.
 * Property of aequabit.
 * Distributed under the Apache 2.0 License.
 */

using System;
using System.Windows.Forms;

namespace tcp_moe_client.Classes
{
    public class Worker
    {
        private ApplicationContext context;

        private UserClient client;

        public static Worker instance;

        public Forms.Authentication auth;
        public Forms.Loader loader;

        public bool running = false;

        public Worker()
        {
            running = true;

            /**
             * Make the worker instance globally available.
             */
            instance = this;

            /**
             * Setup the context.
             */
            context = new ApplicationContext();

            /**
             * Initialize the client.
             */
            client = new UserClient();

            /**
             * Register events.
             */
            client.ExceptionThrown += Events.Client_ExceptionThrown;
            client.ReadPacket += Events.Client_ReadPacket;
            client.StateChanged += Events.Client_StateChanged;
            client.WritePacket += Events.Client_WritePacket;;

            /**
             * Connect to the server.
             */
            client.Connect(Config.host, Config.port);

            /**
             * Show the authentication form.
             */
            auth = new Forms.Authentication(this);
            auth.Show();

#if DEBUG
            /**
             * Show the debugging form.
             */
            new Forms.Debug().Show();
#endif
        }

        public void KillAuth()
        {
            auth.Kill();
        }

        public void MakeLoader()
        {
            /**
             * Show the loader.
             */
            loader = new Forms.Loader(this);
            auth.Invoke((MethodInvoker)delegate
            {
                loader.Show();
            });
        }

        public bool ClientConnected()
        {
            return client.Connected;
        }

        public void Shutdown()
        {
            running = false;

            /**
             * Disconnect from the server gracefully.
             */
            client.Disconnect();

            /**
             * Terminate the main thread.
             */
            context.ExitThread();
        }


        public void Send(byte[] data)
        {
            /**
             * If the handshake is done, encrypt the data.
             */
            if (Session.HandshakeDone)
                data = Session.encrypt(data);

            client.Send(data);
        }

        public ApplicationContext Satisfy()
        {
            return context;
        }
    }
}
