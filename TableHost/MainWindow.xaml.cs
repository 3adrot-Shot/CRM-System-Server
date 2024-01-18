using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;

namespace TableHost
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<ConnectionInfo> _connections;

        public MainWindow()
        {
            InitializeComponent();
            _connections = new ObservableCollection<ConnectionInfo>();
            ConnectionListView.ItemsSource = _connections;

            StartListening();
        }

        private async void StartListening()
        {
            try
            {
                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
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
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string jsonData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    ClientInfo clientInfo = JsonConvert.DeserializeObject<ClientInfo>(jsonData);

                    // Теперь у тебя есть информация о клиенте в объекте clientInfo
                    // Ты можешь использовать ее для вывода в консоль или для других действий
                    Console.WriteLine($"New connection: {clientInfo.Login}");

                    // Отправляем сообщение обратно клиенту
                    string response = "Welcome, " + clientInfo.Login + "!";
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                }
            }
            catch (Exception ex)
            {
                HandleServerError(ex.Message);
            }
        }

        private async void HandleConnection(TcpClient client)
        {
            try
            {
                // Обработка подключения
                // Тут можешь реализовать авторизацию, обмен данными и т.д.

                // Пример: выводим информацию о подключении в ListView
                var remoteEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _connections.Add(new ConnectionInfo { IPAddress = remoteEndPoint.Address.ToString(), Status = "Connected" });
                });

                await Task.Delay(5000); // Пауза для примера, можешь убрать

                // Закрываем соединение
                client.Close();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _connections.Remove(new ConnectionInfo { IPAddress = remoteEndPoint.Address.ToString() });
                });
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
                ConsoleTextBox.AppendText($"Error: {errorMessage}\n");
            });
        }
    }

    public class ConnectionInfo
    {
        public string IPAddress { get; set; }
        public string Status { get; set; }
    }
}
