using System.Linq;
using System.Text;

namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    public class CommandRequest
    {
        public string[] Arguments { get; }

        public CommandRequest(string[] args)
        {
            Arguments = args;
        }

        public string GetArguments()
        {
            if (Arguments.Count() > 1)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string element in Arguments.Skip(1))
                {
                    sb.Append(element + " ");
                }

                sb.Length--;
                return sb.ToString();
            }
            else return string.Empty;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string element in Arguments)
            {
                sb.Append(element + " ");
            }

            sb.Length--;
            return sb.ToString();
        }
    }
}