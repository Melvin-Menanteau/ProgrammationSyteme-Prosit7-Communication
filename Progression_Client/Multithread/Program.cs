using System.Net.Sockets;
using System.Text;

namespace Multithread
{
    class Program
    {
        public enum StateEnum
        {
            STOPPED,
            RUNNING,
            PAUSED
        }

        private static StreamWriter writer;
        private static int progress = 0;
        private static int steps = 10;
        private static StateEnum state = StateEnum.STOPPED;
        private static object Lock = new();

        public static void Main(string[] args)
        {
            using TcpClient client = new TcpClient("localhost", 8080);
            using NetworkStream stream = client.GetStream();
            writer = new StreamWriter(stream, encoding: Encoding.ASCII) { AutoFlush = true };

            int id = 1;

            Task.Run(() =>
            {
                StreamReader reader = new StreamReader(stream, encoding: Encoding.ASCII);

                Console.WriteLine("Connected to server");

                while (true)
                {
                    string message = reader.ReadLine();

                    if (message != null && Enum.TryParse(message, out StateEnum s))
                    {
                        Console.WriteLine(message);
                        state = s;
                    }
                }
            });

            Thread simulationThread;

            while (true)
            {
                if (state == StateEnum.RUNNING)
                {
                    simulationThread = new(() => SimulationUsine(id));
                    simulationThread.Start();
                    simulationThread.Join();
                    id++;
                    Thread.Sleep(2_000);
                }
            }
        }

        static void SimulationUsine(int idObject)
        {
            progress = 0;

            // UT1
            Console.WriteLine($"Objet {idObject}");
            Console.WriteLine("Entrée dans l'UT1");
            Thread[] UT1 = new Thread[3];

            for (int i = 0; i < UT1.Length; i++)
            {
                UT1[i] = new Thread(() => Machine(i));
                UT1[i].Start();
            }

            for (int i = 0; i < UT1.Length; i++)
            {
                UT1[i].Join();
            }

            Console.WriteLine("Toutes les machines de l'UT1 ont fini de travailler");

            // UT2
            Console.WriteLine("Entrée dans l'UT2");
            Thread UT2 = new Thread(() => Machine(4));

            UT2.Start();

            UT2.Join();

            Console.WriteLine("La machine de l'UT2 a fini de travailler");

            // UT3
            Console.WriteLine("Entrée dans l'UT3");
            Thread[] UT3 = new Thread[2];

            for (int i = 0; i < UT3.Length; i++)
            {
                UT3[i] = new Thread(() => SousUT(i));
                UT3[i].Start();
            }

            for (int i = 0; i < UT3.Length; i++)
            {
                UT3[i].Join();
            }

            Console.WriteLine("Toutes les machines de l'UT3 ont fini de travailler");
        }

        static void Machine(int id)
        {
            if (state != StateEnum.RUNNING)
            {
                return;
            }

            Thread.Sleep(RandomTime());
            Console.WriteLine($"La machine {id} a fin de travailler");
            IncrementProgress();
        }

        static int RandomTime()
        {
            return new Random().Next(100, 5_000);
        }

        static void SousUT(int id)
        {
            Thread[] SUT = new Thread[3];

            for (int i = 0; i < SUT.Length; i++)
            {
                SUT[i] = new Thread(() => Machine(i));
                SUT[i].Start();
            }

            for (int i = 0; i < SUT.Length; i++)
            {
                SUT[i].Join();
            }
        }

        static void IncrementProgress()
        {
            lock (Lock)
            {
                progress++;
                try
                {
                    writer.WriteLine((progress / (float)steps));
                } catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}