
using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClienteTresEnRaya
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary> 
    public partial class MainWindow : Window
    {
        TcpClient client;
        NetworkStream serverStream;
        Button currentCell;
        Button[] board = new Button[9];

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ReturnMainWindow()
        {
            Application.Current.Dispatcher.Invoke(
                new Action(() =>
                {
                    InitialWindow.Visibility = Visibility.Visible;
                    Container.Visibility = Visibility.Collapsed;
                })
            );
            
        }

        private void NewGame(object sender, RoutedEventArgs e)
        {
            client = new TcpClient("127.0.0.1", 8888);
            serverStream = client.GetStream();
            Application.Current.Dispatcher.Invoke(
                new Action(() =>
                {
                    InitialWindow.Visibility = Visibility.Collapsed;
                    Container.Visibility = Visibility.Visible;

                    Board.Children.Cast<Button>().ToList().ForEach(button =>
                    {
                        button.Content = string.Empty;
                        button.Background = Brushes.White;
                        button.Foreground = Brushes.Blue;
                    });
                    board = Board.Children.Cast<Button>().ToArray();
                    tbMultiLine.Text = "";
                })
            );

            Thread serverThread = new Thread(new ThreadStart(ClienteThreadStart));
            serverThread.Start();
        }

        public void ClienteThreadStart() 
        {
            byte[] bytesFrom = new byte[256];
            string response;
            string mark;
            string opponentMark;
            string res = "";

            try
            {
                serverStream.Read(bytesFrom, 0, bytesFrom.Length);
                response = Encoding.ASCII.GetString(bytesFrom);
                string[] responseSplit = response.Split(' ');

                WriteLog(response);

                mark = responseSplit[1] == "Cruz" ? "X" : "O";
                opponentMark = responseSplit[1] == "Cruz" ? "O" : "X";

                Application.Current.Dispatcher.Invoke(new Action(() =>
                    Title = "Cliente Tres En Raya - Jugador: " + mark
                ));
                while (true)
                {
                    byte[] bytes = new byte[256];
                    serverStream.Read(bytes, 0, bytes.Length);
                    response = Encoding.ASCII.GetString(bytes);
                    //response = response.Substring(0, response.IndexOf("$"));

                    if (response.StartsWith("VALID_MOVE"))
                    {
                        WriteLog("Movimiento valido");
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            currentCell.Content = mark;
                            currentCell.Foreground = mark == "X" ? Brushes.Blue : Brushes.Red;
                        }
                        ));
                    }
                    else if (response.StartsWith("OPPONENT_MOVED"))
                    {
                        int loc = Int32.Parse(response.Substring(15));
                        Application.Current.Dispatcher.Invoke(new Action(() => {
                            board[loc].Content = opponentMark;
                            board[loc].Foreground = opponentMark == "X" ? Brushes.Blue : Brushes.Red;
                        }
                        ));
                        WriteLog("El oponente movio, es tu turno");
                    }
                    else if (response.StartsWith("VICTORY"))
                    {
                        res = "Has Ganado";
                        WriteLog(res);
                        break;
                    }
                    else if (response.StartsWith("DEFEAT"))
                    {
                        res = "Has perdido";
                        WriteLog(res);
                        break;
                    }
                    else if (response.StartsWith("TIE"))
                    {
                        res = "Has empatado";
                        WriteLog(res);
                        break;
                    }
                    else if (response.StartsWith("MSG"))
                    {
                        WriteLog(response.Substring(4));
                    }
                }
                byte[] bytesTo = new byte[256];
                bytesTo = Encoding.ASCII.GetBytes("QUIT");
                serverStream.Write(bytesTo, 0, bytesTo.Length);
            }
            catch (SocketException ex)
            {
                WriteLog($"SocketException: {ex}");
            }
            finally
            {
                client.Close();
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MessageBoxResult result = MessageBox.Show(this, $"{res}\n¿Quieres jugar de nuevo?", "Cliente Tres en Raya", MessageBoxButton.YesNoCancel);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            NewGame(null, null);
                            break;
                        case MessageBoxResult.No:
                            ReturnMainWindow();
                            break;
                        case MessageBoxResult.Cancel:
                            ReturnMainWindow();
                            break;
                    }
                }
                ));
                
            }
        }

        public void Move(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var column = Grid.GetColumn(button);
            var row = Grid.GetRow(button);
            var index = column + (row * 3);
            
            currentCell = board[index];
            byte[] bytesTo = Encoding.ASCII.GetBytes($"MOVE {index}");
            serverStream.Write(bytesTo, 0, bytesTo.Length);
        }

        public void WriteLog(string content)
        {
            Application.Current.Dispatcher.Invoke(
                new Action(() =>
                {
                    tbMultiLine.Text += $"\n{content}";
                    tbMultiLine.ScrollToEnd();
                })
            );
        }
    }
}
