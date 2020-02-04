using System;

namespace RemoteControlToolkitCore.Common.Commandline
{
    public class PseudoTerminalMode
    {
        /// <summary>
        /// Turns on or off local echoing.
        /// </summary>
        public bool ECHO { get; set; } = true;
        /// <summary>
        /// Determines whether line by line or character by character input is selected.
        /// </summary>

        public bool ICANON { get; set; } = true;
        /// <summary>
        /// Enable or disable output processing such as inserting CR to LF.
        /// </summary>
        public bool OPOST { get; set; } = true;
        /// <summary>
        /// The time for a read operation to be completed.
        /// </summary>
        public TimeSpan VTIME { get; set; } = TimeSpan.Zero;

        public bool SIGINT { get; set; } = true;
        public bool SIGTSTP { get; set; } = true;
        public bool ClearScrollbackOnClear { get; set; } = true;
    }
}