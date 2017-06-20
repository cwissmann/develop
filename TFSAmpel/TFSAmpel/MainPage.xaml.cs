// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TFSAmpel
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using System;
    using Windows.UI;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;
    using GHIElectronics.UWP.Shields;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SolidColorBrush greenBrush = new SolidColorBrush(Colors.Green);
        private SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Colors.Gray);
        private SolidColorBrush yellowBrush = new SolidColorBrush(Colors.Yellow);

        private string connectionString = "[CONNECTIONSTRING]";

        private CloudQueueClient queueClient;
        private CloudQueue queue;

        private DispatcherTimer timer;

        private FEZHAT fezhat;

        public MainPage()
        {
            this.InitializeComponent();

            this.Init();
            this.InitAzureConnection();
        }

        private async void Init()
        {
            this.greenLight.Fill = greenBrush;
            this.redLight.Fill = grayBrush;

            this.fezhat = await FEZHAT.CreateAsync();
            this.fezhat.D2.Color = FEZHAT.Color.Green;
            this.fezhat.D3.Color = FEZHAT.Color.Black;

            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromSeconds(5);
            this.timer.Tick += ReadMessageFromQueue;
            this.timer.Start();
        }

        private void InitAzureConnection()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            this.queueClient = storageAccount.CreateCloudQueueClient();
            this.queue = this.queueClient.GetQueueReference("[QUEUE]");
        }

        private async void ReadMessageFromQueue(object sender, object e)
        {
            CloudQueueMessage message = await this.queue.GetMessageAsync();

            try
            {
                if (message.AsString.Contains("failed"))
                {
                    this.greenLight.Fill = grayBrush;
                    this.redLight.Fill = redBrush;

                    this.fezhat.D2.Color = FEZHAT.Color.Black;
                    this.fezhat.D3.Color = FEZHAT.Color.Red;
                }
                else if (message.AsString.Contains("succeeded"))
                {
                    this.greenLight.Fill = greenBrush;
                    this.redLight.Fill = grayBrush;

                    this.fezhat.D2.Color = FEZHAT.Color.Green;
                    this.fezhat.D3.Color = FEZHAT.Color.Black;
                }
                else if (message.AsString.Contains("canceled"))
                {
                    this.greenLight.Fill = yellowBrush;
                    this.redLight.Fill = yellowBrush;

                    this.fezhat.D2.Color = FEZHAT.Color.Yellow;
                    this.fezhat.D3.Color = FEZHAT.Color.Yellow;
                }
                else
                {
                    this.greenLight.Fill = grayBrush;
                    this.redLight.Fill = grayBrush;
                }

                await this.queue.DeleteMessageAsync(message);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
