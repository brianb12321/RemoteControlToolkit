using System;
using BigInteger = Mono.Math.BigInteger;

namespace RemoteControlToolkitCore.Common.NSsh.Utility
{
    public static class BigIntegerHelper
    {
        public static BigInteger ParseHex(string number)
        {
            if (number == null)
                throw new ArgumentNullException("number");

            number = number.ToLower();

            int i = 0, len = number.Length;
            char c;
            bool digits_seen = false;
            BigInteger val = new BigInteger(0);
            if (number[i] == '+')
            {
                i++;
            }
            else if (number[i] == '-')
            {
                throw new FormatException("Number is negative.");
            }

            for (; i < len; i++)
            {
                c = number[i];
                if (c == '\0')
                {
                    i = len;
                    continue;
                }
                if (c >= '0' && c <= '9')
                {
                    val = val * 16 + (c - '0');
                    digits_seen = true;
                }
                else if (c >= 'a' && c <= 'f')
                {
                    val = val * 16 + (c - 'a' + 10);
                    digits_seen = true;
                }
                else
                {
                    if (Char.IsWhiteSpace(c))
                    {
                        for (i++; i < len; i++)
                        {
                            if (!Char.IsWhiteSpace(number[i]))
                                throw new FormatException();
                        }
                        break;
                    }
                    else
                        throw new FormatException();
                }
            }
            if (!digits_seen)
                throw new FormatException();
            return val;
        }
    }
}
