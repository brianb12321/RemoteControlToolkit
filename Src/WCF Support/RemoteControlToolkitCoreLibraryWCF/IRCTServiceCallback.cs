using System.ServiceModel;

namespace RemoteControlToolkitCoreLibraryWCF
{
    [ServiceContract]
    public interface IRCTServiceCallback
    {
        [OperationContract]
        void SetTitle(string title);
        [OperationContract]
        void Print(string message);
        [OperationContract]
        string ReadLine();
        [OperationContract]
        string ReadToEnd();
        [OperationContract]
        void ClearScreen();
        char Read();
        [OperationContract]
        int Read(byte[] data, int offset, int count);
        [OperationContract]
        (uint columns, uint rows) GetTerminalDimensions();

        [OperationContract]
        (string row, string column) GetCursorPosition();
        [OperationContract]

        void UpdateCursorPosition(int col, int row);
    }
}