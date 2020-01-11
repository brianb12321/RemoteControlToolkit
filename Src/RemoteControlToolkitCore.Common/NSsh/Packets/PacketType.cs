/*
 * Copyright 2008 Luke Quinane
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * This work has been adapted from the Ganymed SSH-2 Java client.
 * 
 */

namespace RemoteControlToolkitCore.Common.NSsh.Packets
{
    public enum PacketType : byte
    {
        Invalid = 0,

        Disconnect = 1,
        Ignore = 2,
        Unimplemented = 3,
        Debug = 4,
        ServiceRequest = 5,
        ServiceAccept = 6,

        KexInit = 20,
        NewKeys = 21,

        KexDHInit = 30,
        KexDHReply = 31,

        UserAuthRequest = 50,
        UserAuthFailure = 51,
        UserAuthSuccess = 52,
        UserAuthBanner = 53,
        UserAuthPublicKeyOk = 60,
        //UserAuthInfoResponse = 61,

        GlobalRequest = 80,
        //REQUEST_SUCCESS = 81,
        GlobalRequestFailure = 82,

        ChannelOpen = 90,
        ChannelOpenConfirmation = 91,
        ChannelOpenFailure = 92,
        ChannelWindowAdjust = 93,
        ChannelData = 94,
        ChannelExtendedData = 95,
        ChannelEof = 96,
        ChannelClose = 97,
        ChannelRequest = 98,
        ChannelSuccess = 99,
        ChannelFailure = 100
    }
}
