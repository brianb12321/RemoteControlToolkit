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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RemoteControlToolkitCore.Common.NSsh.Types
{
    /// <summary>
    /// Represents a SSH name list.
    /// </summary>
    public class NameList : IByteData
    {
        /// <summary>
        /// Creates a new empty name list.
        /// </summary>
        public NameList() { }

        /// <summary>
        /// Creates a new name list from the list of names.
        /// </summary>
        /// <remarks>
        /// Each item in the list <b>must</b> be US-ASCII only. Other characters
        /// will be transformed during encoding.
        /// </remarks>
        /// <param name="names">The names to create the list from.</param>
        public NameList(IList<string> names)
        {
            _names = names;
        }

        /// <summary>
        /// Creates a new name list from the binary reader.
        /// </summary>
        /// <param name="reader">The reader to get the list data from.</param>
        public NameList(BinaryReader reader)
        {
            uint length = reader.ReadUInt32BE();
            byte[] data = reader.ReadBytes((int)length);

            string nameList = Encoding.ASCII.GetString(data);
            _names = new List<string>(nameList.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// The names associated with this name list.
        /// </summary>
        private IList<string> _names = new List<string>();

        /// <summary>
        /// Gets a list of names from this name list.
        /// </summary>
        public IList<string> Names
        {
            get
            {
                return _names;
            }
        }

        public byte[] ToByteArray()
        {
            StringBuilder nameList = new StringBuilder();

            for (int i = 0; _names != null && i < _names.Count; i++)
            {
                string name = _names[i];

                if (name.IndexOf(',') != -1)
                {
                    throw new ArgumentException("Name list contains ',' character.");
                }

                nameList.Append(name);

                if ((i + 1) != _names.Count)
                {
                    nameList.Append(",");
                }
            }

            byte[] data = Encoding.ASCII.GetBytes(nameList.ToString());
            uint length = (uint)data.Length;

            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(memoryStream);

            writer.WriteBE(length);
            writer.Write(data);

            return memoryStream.ToArray();
        }
    }
}
