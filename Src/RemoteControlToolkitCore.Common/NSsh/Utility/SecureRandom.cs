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
 */

using System;
using System.Security.Cryptography;

namespace RemoteControlToolkitCore.Common.NSsh.Utility
{
    public class SecureRandom : ISecureRandom
    {
        public RandomNumberGenerator random = RandomNumberGenerator.Create();

        // TODO: Statistically test this
        [CoverageExclude("Hard to test if this is actually random.")]
        public int GetInt32()
        {
            byte[] data = new byte[4];
            random.GetBytes(data);

            int result = data[0];
            result |= data[1] << 8;
            result |= data[2] << 16;
            result |= data[3] << 24;

            return result;
        }

        // TODO: Statistically test this
        [CoverageExclude("Hard to test if this is actually random.")]
        public double GetDouble()
        {
            byte[] data = new byte[8];
            random.GetBytes(data);

            double value = BitConverter.ToDouble(data, 0);
            return Math.IEEERemainder(1, value);
        }

        // TODO: Statistically test this
        [CoverageExclude("Hard to test if this is actually random.")]
        public void GetBytes(byte[] data)
        {
            random.GetBytes(data);
        }
    }
}
