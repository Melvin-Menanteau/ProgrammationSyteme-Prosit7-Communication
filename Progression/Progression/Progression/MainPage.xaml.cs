using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;

namespace Progression
{
    public enum StateEnum
    {
        STOPPED,
        RUNNING,
        PAUSED
    }

    public partial class MainPage : ContentPage
    {
        private TcpListener listener;
        private TcpClient client;
        private StateEnum state = StateEnum.STOPPED;
        private StreamWriter writer;

        public MainPage()
        {
            InitializeComponent();
            Task.Run(() =>
            {
                listener = new TcpListener(IPAddress.Any, 8080);
                listener.Start();
                ListenForClients();
                Debug.WriteLine("Server started");
            });
        }

        ~MainPage()
        {
            listener.Stop();
            listener.Dispose();
        }

        private async Task ListenForClients()
        {
            while (true)
            {
                client = await listener.AcceptTcpClientAsync();
                writer = new StreamWriter(client.GetStream(), encoding: Encoding.ASCII) { AutoFlush = true };
                Debug.WriteLine("Client connected");
                await HandleClientComm();
            }
        }

        private void NotifyClient()
        {
            writer.WriteLine(state.ToString());
        }

        private async Task HandleClientComm()
        {
            StreamReader reader = new StreamReader(client.GetStream(), encoding: Encoding.ASCII);
            string message;

            while (true)
            {
                try {
                    message = reader.ReadLine();
                    float progress;

                    if (float.TryParse(message, out progress))
                    {
                        Debug.WriteLine(progress);
                        await UpdateProgress(progress);
                    }
                } catch (Exception e)
                {
                    Debug.WriteLine($"ERROR: {e.Message}");
                }
            }
        }

        private async Task UpdateProgress(float progress)
        {
            await pbstatus1.ProgressTo(progress, 500, Easing.Linear);
        }

        public async void Button_Click(object sender, EventArgs e)
        {
            // TODO: Lancement fabrication
            Debug.WriteLine("Button clicked");

            if (state == StateEnum.STOPPED || state == StateEnum.PAUSED)
            {
                state = StateEnum.RUNNING;
                state_button.Text = "Pause";
                NotifyClient();
            } else if (state == StateEnum.RUNNING)
            {
                state = StateEnum.PAUSED;
                state_button.Text = "Resume";
                NotifyClient();
            }
        }
    }

}
