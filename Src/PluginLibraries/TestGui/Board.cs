﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGUI;
using ConsoleGUI.Controls;
using ConsoleGUI.Data;
using ConsoleGUI.UserDefined;

namespace TestGui
{
    internal class BoardCell : SimpleControl
    {
        private readonly IControl _cell;

        public BoardCell(char content, Color color)
        {
            _cell = new Background
            {
                Color = color,
                Content = new Box
                {
                    HorizontalContentPlacement = Box.HorizontalPlacement.Center,
                    VerticalContentPlacement = Box.VerticalPlacement.Center,
                    Content = new TextBlock {Text = content.ToString()}
                }
            };

            Content = _cell;
        }
    }

    internal class Board : SimpleControl
    {
        private readonly Grid _board;

        public Board()
        {
            _board = new Grid
            {
                Rows = Enumerable.Repeat(new Grid.RowDefinition(3), 10).ToArray(),
                Columns = Enumerable.Repeat(new Grid.ColumnDefinition(5), 10).ToArray()
            };

            for (int i = 1; i < 9; i++)
            {
                var character = (char) ('a' + (i - 1));
                var number = (char) ('0' + (i - 1));
                var darkColor = new Color(50, 50, 50).Mix(Color.White, i % 2 == 1 ? 0f : 0.1f);
                var lightColor = new Color(50, 50, 50).Mix(Color.White, i % 2 == 0 ? 0f : 0.1f);

                _board.AddChild(i, 0, new BoardCell(character, darkColor));
                _board.AddChild(i, 9, new BoardCell(character, lightColor));
                _board.AddChild(0, i, new BoardCell(number, darkColor));
                _board.AddChild(9, i, new BoardCell(number, lightColor));
            }

            string[] pieces = new[]
            {
                "♜♞♝♛♚♝♞♜",
                "♟♟♟♟♟♟♟♟",
                "        ",
                "        ",
                "        ",
                "        ",
                "♙♙♙♙♙♙♙♙",
                "♖♘♗♕♔♗♘♖"
            };

            for (int i = 1; i < 9; i++)
            for (int j = 1; j < 9; j++)
                _board.AddChild(i, j,
                    new BoardCell(pieces[j - 1][i - 1],
                        new Color(139, 69, 19).Mix(Color.White, ((i + j) % 2) == 1 ? 0f : 0.4f)));

            Content = _board;
        }
    }
}