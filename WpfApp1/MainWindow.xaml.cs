using System;
using NAudio.Wave;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int Port = 12345; // Choose a port number

        private void StartAudioPlayback()
        {
            // Create a new UDP socket
            UdpClient udpClient = new UdpClient(Port);

            // Begin playing audio
            WaveOutEvent waveOut = new WaveOutEvent();
            BufferedWaveProvider waveProvider = new BufferedWaveProvider(new WaveFormat(44100, 1)); // Adjust the sample rate and channel count if needed
            waveOut.Init(waveProvider);
            waveOut.Play();

            // Continuously receive and play audio data
            ThreadPool.QueueUserWorkItem(_ =>
            {
                while (true)
                {
                    IPEndPoint remoteEndPoint = null;
                    byte[] audioData = udpClient.Receive(ref remoteEndPoint);
                    waveProvider.AddSamples(audioData, 0, audioData.Length);
                }
            });
        }

        private void StartAudioTransmission()
        {
            // Create a new UDP socket
            UdpClient udpClient = new UdpClient();

            // Get the local IP address
            IPAddress ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);

            // Begin capturing audio from the microphone
            WaveInEvent waveIn = new WaveInEvent();
            waveIn.DataAvailable += (s, e) =>
            {
                // Send the audio data over the UDP socket
                udpClient.Send(e.Buffer, e.BytesRecorded, new IPEndPoint(ipAddress, Port));
            };

            // Set up the wave format for the audio capture
            waveIn.WaveFormat = new WaveFormat(44100, 1); // Adjust the sample rate and channel count if needed

            // Start capturing audio
            waveIn.StartRecording();
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StartAudioTransmission();
            StartAudioPlayback();
        }
    }
}
