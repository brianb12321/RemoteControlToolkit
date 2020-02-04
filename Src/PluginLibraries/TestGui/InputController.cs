using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGUI.Controls;
using ConsoleGUI.Input;
using RemoteControlToolkitCore.Common.ApplicationSystem;

namespace TestGui
{
    class InputController : IInputListener
    {
        private readonly TextBox _textBox;
        private readonly LogPanel _logPanel;
        private readonly RCTProcess _process;

        public InputController(TextBox textBox, LogPanel logPanel, RCTProcess process)
        {
            _textBox = textBox;
            _logPanel = logPanel;
            _process = process;
        }

        public void OnInput(InputEvent inputEvent)
        {
            if (inputEvent.Key.Key != ConsoleKey.Enter) return;
            if (_textBox.Text == "exit")
            {
                _process.Close();
            }
            _logPanel.Add(_textBox.Text);

            _textBox.Text = string.Empty;
            inputEvent.Handled = true;
        }
    }
}
