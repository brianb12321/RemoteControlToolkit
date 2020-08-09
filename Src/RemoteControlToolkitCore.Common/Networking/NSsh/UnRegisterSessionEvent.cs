using TinyMessenger;

namespace RemoteControlToolkitCore.Common.Networking.NSsh
{
    public class UnRegisterSessionEvent : ITinyMessage
    {
        public object Sender { get; }
        public ISshSession Session { get; }
        public UnRegisterSessionEvent(object sender, ISshSession session)
        {
            Sender = sender;
            Session = session;
        }
    }
}