using System.ServiceModel;

namespace RemoteControlToolkitCoreLibraryWCF
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IRCTServiceCallback))]
    public interface IRCTService
    {
        [OperationContract(IsInitiating = true)]
        void StartShell();

        [OperationContract]
        void SendControlC();
    }
}