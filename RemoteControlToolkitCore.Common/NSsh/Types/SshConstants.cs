﻿using RemoteControlToolkitCore.Common.NSsh.Utility;
using BigInteger = Mono.Math.BigInteger;

namespace RemoteControlToolkitCore.Common.NSsh.Types
{
    public static class SshConstants
    {
        public static readonly BigInteger DHPrime = BigIntegerHelper.ParseHex(
            "FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD1" +
            "29024E088A67CC74020BBEA63B139B22514A08798E3404DD" +
            "EF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245" +
            "E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7ED" +
            "EE386BFB5A899FA5AE9F24117C4B1FE649286651ECE65381" +
            "FFFFFFFFFFFFFFFF");
    }
}
