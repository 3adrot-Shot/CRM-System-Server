using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TableHost.Back;

namespace TableHost
{
    public class ClientInfo
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public partial class MainWindow : Window
    {
        private ObservableCollection<ConnectionInfo> _connections;
        private TcpListener _listener;

        public MainWindow()
        {
            InitializeComponent();
            _connections = new ObservableCollection<ConnectionInfo>();
            ConnectionListView.ItemsSource = _connections;
            StartListening();
        }

        private void StartListening()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, 8081);
                _listener.Start();
                ConsoleTextBox.AppendText($"Started server...");
                WaitForClients();
            }
            catch (Exception ex)
            {
                HandleServerError(ex.Message);
            }
        }

        private async void WaitForClients()
        {
            try
            {
                while (true)
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    HandleClient(client);
                }
            }
            catch (Exception ex)
            {
                HandleServerError(ex.Message);
            }
        }

        private async void HandleClient(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[4096];
                StringBuilder sb = new StringBuilder(); // Используем StringBuilder для сбора всех данных
                int bytesRead;

                do
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string jsonData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        sb.Append(jsonData);
                    }
                } while (stream.DataAvailable);

                if (sb.Length > 0)
                {
                    string jsonData = sb.ToString();
                    string response = ProcessingMessage.EnterRespons(jsonData);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                }
                else
                {
                    PrintC($"\nBytesRead = 0\n");
                    string response = "Null response";
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                }

                // Не закрываем соединение здесь
            }
            catch (Exception ex)
            {
                HandleServerError(ex.Message);
            }
        }


        private void HandleServerError(string errorMessage)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                PrintC($"Error: {errorMessage}\n");
            });
        }
        
        public void PrintC(string text)
        {
            ConsoleTextBox.AppendText($"Error: {text}\n");
        }
    }

    public class ConnectionInfo
    {
        public string IPAddress { get; set; }
        public string Status { get; set; }
    }
}
