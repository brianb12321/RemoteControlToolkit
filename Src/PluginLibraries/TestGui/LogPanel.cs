using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGUI.Controls;
using ConsoleGUI.Data;
using ConsoleGUI.UserDefined;

namespace TestGui
{
    internal class LogPanel : SimpleControl
    {
        private readonly VerticalStackPanel _stackPanel;

        public LogPanel()
        {
            _stackPanel = new VerticalStackPanel();

            Content = _stackPanel;
        }

        public void Add(string message)
        {
            _stackPanel.Add(new WrapPanel
            {
                Content = new HorizontalStackPanel
                {
                    Children = new[]
                    {
                        new TextBlock {Text = $"[{DateTime.Now.ToLongTimeString()}] ", Color = new Color(200, 20, 20)},
                        new TextBlock {Text = message}
                    }
                }
            });
        }
    }
}
