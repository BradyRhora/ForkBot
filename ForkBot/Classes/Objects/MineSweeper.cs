using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForkBot
{
    public class MineSweeper
    {
        int boardSize, bombCount;
        int[,] board; //0 = unchecked, 1 = bomb, 2 = checked, 3 = flagged
        static Random rdm = new Random();
        public User player;
        Dictionary<string, int> LetterNumMap = new Dictionary<string, int>();
        bool lose = false;
        static string alphabet = " abcdefghijklmnopqrstuvwxyz";

        public MineSweeper(User user, int boardSize = 10, int bombCount = 10)
        {
            if (boardSize > ('z' - 'a')) throw new ArgumentOutOfRangeException("boardSize", $"boardSize must be lower than {'z' - 'a' + 1}.");
            player = user;
            this.boardSize = boardSize;
            this.bombCount = bombCount;
            board = new int[boardSize, boardSize];
            for (int x = 0; x < boardSize; x++) for (int y = 0; y < boardSize; y++) board[x, y] = 0;
            for (int i = 0; i < boardSize; i++) LetterNumMap.Add(Convert.ToString(alphabet[i+1]), i);
            //populate board
            for (int i = 0; i < bombCount; i++)
            {
                int x = rdm.Next(boardSize);
                int y = rdm.Next(boardSize);

                while (board[x, y] == 1)
                {
                    x = rdm.Next(boardSize);
                    y = rdm.Next(boardSize);
                }

                board[x, y] = 1;
            }
        }

        public string Build()
        {
            Dictionary<int, string> nums = new Dictionary<int, string>{ { 0, ":white_circle:" }, { 1, ":one:" }, { 2, ":two:" }, { 3, ":three:" }, { 4, ":four:" }, { 5, ":five:" }, { 6, ":six:" }, { 7, ":seven:" }, { 8, ":eight:" }, { 9, ":nine:" } };
            string msg = "";
            for (int x = 0; x < boardSize; x++)
            {
                msg += ":regional_indicator_" + alphabet[x + 1] + ":";
                for (int y = 0; y < boardSize; y++)
                {
                    if (!lose)
                    {
                        if (board[x, y] == 2) msg += nums[CountBombs(x, y)];
                        else if (board[x, y] == 3) msg += ":flag_white:";
                        else msg += ":black_circle:";
                    }
                    if (lose)
                    {
                        if (board[x, y] == 1) msg += ":bomb:";
                        else msg += nums[CountBombs(x, y)];
                    }
                }
                msg+=('\n');
            }
            msg += ":black_circle:";
            for (int y = 0; y < boardSize; y++) msg += ":regional_indicator_" + alphabet[y + 1] + ":";
            return msg;
        }

        private int CountBombs(int x, int y)
        {
            int bombs = 0;
            for (int i = x - 1; i <= x + 1; i++)
            {
                for (int o = y - 1; o <= y + 1; o++)
                {
                    if (!(i == x && o == y))
                    {
                        if (i >= 0 && i < boardSize && o >= 0 && o < boardSize)
                        {
                            if (board[i, o] == 1) bombs++;
                        }
                    }
                }
            }
            return bombs;
        }

        public bool Flag(string[] coords)
        {
            var tile = board[LetterNumMap[coords[0]], LetterNumMap[coords[1]]];
            if (tile == 0) { board[LetterNumMap[coords[0]], LetterNumMap[coords[1]]] = 3; return true; }
            return false;
        }
        
        public bool Turn(string[] coords)
        {
            int x = LetterNumMap[coords[0]];
            int y = LetterNumMap[coords[1]];
            var tile = board[LetterNumMap[coords[0]], LetterNumMap[coords[1]]];
            if (tile == 0)
            {
                board[LetterNumMap[coords[0]], LetterNumMap[coords[1]]] = 2;

                int[,] c = new int[,] { { x, y - 1 }, { x - 1, y }, { x + 1, y }, { x, y + 1 } };

                for (int i = 0; i < 4; i++)
                {
                    var x2 = c[i, 0];
                    var y2 = c[i, 1];

                    if (x2 > 0 && x2 < boardSize && y2 > 0 && y2 < boardSize)
                    {
                        if (board[x2, y2] == 0) Turn(new string[] { $"{alphabet[x2]}", $"{alphabet[y2]}" });
                    }
                }
                        

                return true;
            }
            else if (tile == 1) { lose = true; return true; }
            return false;
        }
    }
}
