using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Net;
using System.Net.Sockets;
using Common;

namespace Sender
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int remotePort;
        private IPAddress remoteIp;
        private IPEndPoint remoteEP;

        private int packetsCount;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Создаем конечную точку отправки
                remotePort = Convert.ToInt32(txtboxPort.Text);
                remoteIp = IPAddress.Parse(txtboxIP.Text);
                remoteEP = new IPEndPoint(remoteIp, remotePort);

                SendPacket();
                SendData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void SendPacket()
        {
            byte[] packet = new byte[CommonValues.PACKET_LENGTH];

            packetsCount = int.Parse(txtboxCount.Text);

            // Отправляем тестовые пакеты по Udp
            using (Socket socket = new Socket(remoteIp.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
            {
                for (int i = 0; i < packetsCount; i++)
                {
                    socket.SendTo(packet, remoteEP);
                }
                socket.Shutdown(SocketShutdown.Both);
            }
        }

        private void SendData()
        {
            // Отправляе число пакетов по Tcp
            using (Socket socket = new Socket(remoteIp.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(remoteEP);

                socket.Send(BitConverter.GetBytes(packetsCount));

                socket.Shutdown(SocketShutdown.Both);

                MessageBox.Show("Все пакеты успешно отправлены");
            }
        }

    }
}
