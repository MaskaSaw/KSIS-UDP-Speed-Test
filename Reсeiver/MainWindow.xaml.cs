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
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Common;

namespace Receiver
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int localPort = 11000;
        private IPAddress localIp;
        private IPEndPoint localEP;

        private DateTime timeStart;
        private DateTime timeEnd;

        private int packetsCount;
        private int packetIndex;
        private bool isAllPackets;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetLocalEP();
        }

        private void receive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetLocalEP();
                Thread thread = new Thread(Time);
                thread.Start();
                ReceivePackets();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SetLocalEP()
        {
            try
            {
                // Установка конечной точки для принятия пакетов
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                localIp = Dns.GetHostAddresses(Dns.GetHostName()).Where(x => x.AddressFamily == AddressFamily.InterNetwork).Last();
                localPort = int.Parse(txtboxPort.Text);
                localEP = new IPEndPoint(localIp, localPort);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            txtblockIP.Text = localIp.ToString();
        }

        private void ReceivePackets()
        {
            isAllPackets = false;
            packetIndex = 0;

            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);   // Точка отправителя

            byte[] buffer = new byte[CommonValues.PACKET_LENGTH];

            // Создание Udp сокета для принятия пакетов
            using (Socket socket = new Socket(localIp.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Bind(localEP);

                while (!isAllPackets || socket.Available != 0)
                {
                    if (socket.Available != 0)
                    {
                        if (packetIndex == 0)
                        {
                            timeStart = DateTime.Now;   
                        }
                        socket.ReceiveFrom(buffer, ref remoteEP);
                        packetIndex++;
                        timeEnd = DateTime.Now;     
                    }
                }

                // Закрываем соединение
                socket.Shutdown(SocketShutdown.Both);
            }

            // Выводим статистику
            ShowSpecifications();
        }

        private void Time()
        {
            byte[] buffer = new byte[sizeof(int)];

            try
            {
                // По Tcp принимаем число пакетов
                using (Socket listener = new Socket(localIp.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
                {
                    listener.Bind(localEP);
                    listener.Listen(1);
                    using (Socket socket = listener.Accept())
                    {
                        socket.Receive(buffer);
                        packetsCount = BitConverter.ToInt32(buffer, 0);
                        socket.Shutdown(SocketShutdown.Both);
                    }
                }

                isAllPackets = true;    //Все пакеты отправленны
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ShowSpecifications()
        {
            TimeSpan delta = timeEnd.Subtract(timeStart);                               // Время отправки всех пакетов
            double speed = TimeSpan.FromSeconds(1).Ticks * packetsCount / delta.Ticks;   // Скорость

            double percent = (double)packetIndex / packetsCount * 100;   // Процент принятых пакетов
            MessageBox.Show("Все пакеты успешно получены");
            txtblockPercent.Text = "Процент полученных данных: " + Math.Round(percent, 5) + " %";
            txtblockSpeed.Text = "Скорость передачи: " + speed + " пакетов в секуеду";
        }
    }
}
