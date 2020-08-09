/*
 * Copyright 2009 Aaron Clauson (aaron@sipsorcery.com)
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

using System.Security.Principal;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Services
{
    /// <summary>
    /// A minimalist authentication service provider that accepts any username and
    /// password combination. It should only ever be used for testing.
    /// </summary>
    public class BasicAuthenticationService : IPasswordAuthenticationService {

        public IIdentity CreateIdentity(string userName, string password) {
            return new GenericIdentity(userName);
        }
    }
}
