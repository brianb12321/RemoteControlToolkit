using System.Linq;
using System.Text;

namespace RemoteControlToolkitCore.Common.ApplicationSystem
{
    public class CommandRequest
    {
        public ICommandElement[] Arguments { get; }

        public CommandRequest(ICommandElement[] args)
        {
            Arguments = args;
        }

        public string GetArguments()
        {
            if (Arguments.Count() > 1)
            {
                StringBuilder sb = new StringBuilder();
                foreach (ICommandElement element in Arguments.Skip(1))
                {
                    sb.Append(element.ToString() + " ");
                }

                sb.Length--;
                return sb.ToString();
            }
            else return string.Empty;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (ICommandElement element in Arguments)
            {
                sb.Append(element.ToString() + " ");
            }

            sb.Length--;
            return sb.ToString();
        }
    }
}