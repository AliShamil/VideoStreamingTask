using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace Client
{
    public partial class MainWindow : Window
    {
        private UdpClient client;
        private IPEndPoint connectEP;

        public MainWindow()
        {
            InitializeComponent();
            client = new UdpClient();
            connectEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 45678);
        }

        private async void BtnSendRequest_Click(object sender, RoutedEventArgs e)
        {
            var receivedBuffer = new byte[ushort.MaxValue - 29];
            await client.SendAsync(receivedBuffer, receivedBuffer.Length, connectEP);

                var list = new List<byte>();
                var len = 0;
              
            while (true)
            {
                do
                {
                    var result = await client.ReceiveAsync();
                    receivedBuffer = result.Buffer;
                    len = receivedBuffer.Length;
                    list.AddRange(receivedBuffer.Take(len));

                } while (len == ushort.MaxValue - 29);

                try
                {
                    var image = ByteArrayToImage(list.ToArray());
                    ImageBox.Source = image;
                    list.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }


            }
        }

        private BitmapImage ByteArrayToImage(byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length == 0) return null;
            var image = new BitmapImage();
            using (var m = new MemoryStream(byteArray))
            {
                m.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = m;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }
    }
}
