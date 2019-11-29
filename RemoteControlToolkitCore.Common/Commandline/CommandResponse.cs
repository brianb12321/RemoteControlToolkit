using System.Runtime.Serialization;

namespace RemoteControlToolkitCore.Common.Commandline
{
    [DataContract]
    public class CommandResponse
    {
        public const int CODE_SUCCESS = 0;
        public const int CODE_FAILURE = 1;
        public const int CODE_THREAD_ABORT = -1;
        [DataMember]
        public int Code { get; set; }
        [IgnoreDataMember]
        public object Data { get; set; }

        public CommandResponse(int code)
        {
            Code = code;
        }
    }
}