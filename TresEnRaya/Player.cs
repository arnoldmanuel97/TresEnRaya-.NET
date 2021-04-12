using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ServerTresEnRaya
{
    class Player
    {
        public Player opponent { get; set; }
        TcpClient socket;
        public MarkType mark;
        Game board;
        NetworkStream networkStream;

        public Player(TcpClient socket, MarkType mark, Game board)
        {
            this.socket = socket;
            this.mark = mark;
            this.board = board;
            networkStream = socket.GetStream();

            byte[] bytesTo = Encoding.ASCII.GetBytes($"Bienvenido {mark} ");
            networkStream.Write(bytesTo, 0, bytesTo.Length);
            if (this.mark == MarkType.Cruz)
            {
                byte[] bytesTo2 = Encoding.ASCII.GetBytes("\nEsperando a que el oponente se conecte");
                networkStream.Write(bytesTo2, 0, bytesTo2.Length);
            }
        }

        public void SetOpponent(Player opponent)
        {
            this.opponent = opponent;
        }

        public void Start()
        {
            Thread handler = new Thread(new ThreadStart(Run));
            handler.Start();
        }

        public void Run() 
        {
            byte[] bytesTo;
            string command;
            if (mark == MarkType.Cruz)
            {
                bytesTo = Encoding.ASCII.GetBytes("MSG Te toca");
                networkStream.Write(bytesTo, 0, bytesTo.Length);
            }
            while (true)
            {
                try
                {
                    //Lee el comando enviado por el cliente
                    byte[] bytesFrom = new byte[256];
                    networkStream.Read(bytesFrom, 0, bytesFrom.Length);
                    command = Encoding.ASCII.GetString(bytesFrom);
                    if (command.StartsWith("MOVE"))
                    {
                        //Comprobamos si es valido
                        int location = Int32.Parse(command.Substring(5));
                        if (board.ValidMove(location, this))
                        {
                            bytesTo = Encoding.ASCII.GetBytes("VALID_MOVE");
                            networkStream.Write(bytesTo, 0, bytesTo.Length);

                            bytesTo = board.HasWinner() ? Encoding.ASCII.GetBytes("VICTORY") 
                                : board.boardFilledUp() ? Encoding.ASCII.GetBytes("TIE") : null;
                            if (bytesTo != null)
                            {
                                networkStream.Write(bytesTo, 0, bytesTo.Length);
                            }
                        }
                        else
                        {
                            bytesTo = Encoding.ASCII.GetBytes("INVALID_MOVE");
                            networkStream.Write(bytesTo, 0, bytesTo.Length);
                        }
                    }
                    else if (command.StartsWith("QUIT"))
                    {
                        networkStream.Close();
                        return;
                    }   
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }            
        }

        public void OtherPlayerMoved(int location)
        {
            byte[] bytesTo;
            bytesTo = Encoding.ASCII.GetBytes("OPPONENT_MOVED " + location);
            networkStream.Write(bytesTo, 0, bytesTo.Length);
            bytesTo = board.HasWinner() ? Encoding.ASCII.GetBytes("DEFEAT")
                    : board.boardFilledUp() ? Encoding.ASCII.GetBytes("TIE") : null;
            if (bytesTo != null)
            {
                networkStream.Write(bytesTo, 0, bytesTo.Length);
            }
        }

    }
}
