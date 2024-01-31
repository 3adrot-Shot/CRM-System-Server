using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using TableHost.Back;
using System.Collections.ObjectModel;
using Org.BouncyCastle.Tls.Crypto;

namespace TableHost
{
    // Класс модели данных, замените его свойствами на свои
    public class YourDataModel
    {
        public string ExpanderHeader { get; set; }
        public string DataReceived { get; set; }
        public string DataResponse { get; set; }
    }

    public partial class MainWindow : Window
    {
        //private ObservableCollection<ConnectionInfo> _connections;
        private TcpListener? _listener;
        public ObservableCollection<YourDataModel> YourDataCollection { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // Инициализируем коллекцию данных
            YourDataCollection = new ObservableCollection<YourDataModel>();

            // Привязываем коллекцию к ListView
            YourListView.ItemsSource = YourDataCollection;

            // Вызываем метод для добавления примера данных
            AddExampleData();
            //_connections = new ObservableCollection<ConnectionInfo>();
            //ConnectionListView.ItemsSource = _connections;
            StartListening();
        }

        // Метод для добавления примера данных
        private void AddExampleData(string Header = "Example Header", string DataReceived = "Example Header", string DataResponse = "Example Header")
        {
            YourDataModel exampleData = new YourDataModel
            {
                ExpanderHeader = Header,
                DataReceived = DataReceived,
                DataResponse = DataResponse
            };

            YourDataCollection.Add(exampleData);
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

                byte[] lengthBuffer = new byte[4];
                await stream.ReadAsync(lengthBuffer.AsMemory(0, 4));
                int dataLength = BitConverter.ToInt32(lengthBuffer, 0);

                MemoryStream memoryStream = new();

                byte[] buffer = new byte[4096];
                int bytesRead;

                do
                {
                    bytesRead = await stream.ReadAsync(buffer.AsMemory(0, Math.Min(buffer.Length, dataLength - (int)memoryStream.Length)));
                    if (bytesRead > 0)
                    {
                        memoryStream.Write(buffer, 0, bytesRead);
                    }
                } while (memoryStream.Length < dataLength);

                if (memoryStream.Length > 0)
                {
                    byte[] receivedData = memoryStream.ToArray();
                    string jsonData = Encoding.UTF8.GetString(receivedData);
                    string response = ProcessingMessage.EnterResponse(jsonData);
                    PrintC($"======\nRecived date: \"{jsonData}\"\nResponse date: \"{response}\"\n ======");
                    AddExampleData("Header dafault text", jsonData, response);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(BitConverter.GetBytes(responseBytes.Length).AsMemory(0, 4)); // Отправляем длину ответа
                    await stream.WriteAsync(responseBytes);
                }
                else
                {
                    PrintC($"BytesRead = 0");
                    string response = "Null response";
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(BitConverter.GetBytes(responseBytes.Length).AsMemory(0, 4)); // Отправляем длину ответа
                    await stream.WriteAsync(responseBytes);
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
            ConsoleTextBox.AppendText($"\n{text}\n");
        }

        private void ConsoleTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            StartListening();
        }
    }
}
