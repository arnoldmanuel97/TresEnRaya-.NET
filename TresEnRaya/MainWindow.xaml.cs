
using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ServerTresEnRaya
{
    
    public partial class MainWindow : Window
    {
        TcpListener server;

        public MainWindow()
        {
            InitializeComponent();
            btn_apagar.IsEnabled = false;
        }

        public void StartServer(object sender, RoutedEventArgs e) 
        {
            btn_encender.IsEnabled = false;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            server = new TcpListener(localAddr, 8888);
            server.Start();

            //thread para no bloquear la interfaz
            Thread serverThread = new Thread(new ThreadStart(ServerThreadStart));
            serverThread.Start();
            btn_apagar.IsEnabled = true;
        }

        public void StopServer(object sender, RoutedEventArgs e) 
        {
            writeLog("Apagando servidor");
            server.Stop();
            btn_encender.IsEnabled = true;
            btn_apagar.IsEnabled = false;
            Application.Current.Dispatcher.Invoke(new Action(() => txtbLogs.Text = ""));
        }

        public void ServerThreadStart() 
        {
            try
            {
                writeLog("Servidor Encendido");
                while (true)
                {
                    Game game = new Game();
                    Player playerX = new Player(server.AcceptTcpClient(), MarkType.Cruz, game);
                    writeLog("Jugador X Conectado");
                    writeLog("Emparejando Jugador X");
                    Player playerO = new Player(server.AcceptTcpClient(), MarkType.Circulo, game);
                    playerX.SetOpponent(playerO);
                    playerO.SetOpponent(playerX);
                    game.currentPlayer = playerX;
                    playerX.Start();
                    playerO.Start();
                }
            }
            catch (SocketException ex)
            {
                writeLog("Error: " + ex.Message);
                server.Stop();
            }
        }

        public void writeLog(string content) 
        {

            Application.Current.Dispatcher.Invoke(
                new Action(() => 
                {
                    txtbLogs.Text += $"\n{content}";
                    txtbLogs.ScrollToEnd();
                })
            );
        }
    }   
}
