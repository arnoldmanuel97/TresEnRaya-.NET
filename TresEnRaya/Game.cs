using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerTresEnRaya
{
    class Game
    {
        public MarkType[] board = new MarkType[9];
        public Player currentPlayer;

        public bool HasWinner()
        {
            // Check for horizontal wins
            if (board[0] != MarkType.Libre && (board[1] & board[2]) == board[0])
                return true;
            if (board[3] != MarkType.Libre && (board[3] & board[4] & board[5]) == board[3])
                return true;
            if (board[6] != MarkType.Libre && (board[6] & board[7] & board[8]) == board[6])
                return true;

            // Check for vertical wins
            if (board[0] != MarkType.Libre && (board[0] & board[3] & board[6]) == board[0])
                return true;
            if (board[1] != MarkType.Libre && (board[1] & board[4] & board[7]) == board[1])
                return true;
            if (board[2] != MarkType.Libre && (board[2] & board[5] & board[8]) == board[2])
                return true;

            // Check for diagonal wins
            if (board[0] != MarkType.Libre && (board[0] & board[4] & board[8]) == board[0])
                return true;
            if (board[2] != MarkType.Libre && (board[4] & board[6]) == board[2])
                return true;

            return false;
        }

        public bool boardFilledUp()
        {
            // Comprobamos si todas las fichas estan llenas
            return !board.Any(f => f == MarkType.Libre);
        }
        
        public bool ValidMove(int location, Player player)
        {
            //si es tu turno & la celda esta libre, el movimiento es valido
            if (player == currentPlayer && board[location] == MarkType.Libre)
            {
                board[location] = currentPlayer.mark;
                currentPlayer = currentPlayer.opponent;
                currentPlayer.OtherPlayerMoved(location);
                return true;
            }
            return false;
        }

    }
}
