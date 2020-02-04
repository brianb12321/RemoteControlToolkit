using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGUI.Controls;
using ConsoleGUI.Data;
using ConsoleGUI.Space;

namespace TestGui
{
    internal sealed class SimpleDecorator : Decorator
    {
        public override Cell this[Position position] =>
            Math.Abs(position.X - Size.Width / 2) > Size.Width / 2 - 3
                ? Content[position].WithForeground(new Color(255, 0, 0))
                : Content[position];
    }
}