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
 */

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using RemoteControlToolkitCore.Common.NSsh.Packets;
using RemoteControlToolkitCore.Common.NSsh.Services;
using RemoteControlToolkitCore.Common.NSsh.Types;
using RemoteControlToolkitCore.Common.NSsh.Utility;

namespace RemoteControlToolkitCore.Common.NSsh.TransportLayer.State
{
    // TODO: enforce only one KEX INIT and KEX DH INIT packet allowed
    public class VersionsExchangedState : AbstractTransportState
    {
        public static readonly IList<PublicKeyAlgorithm> preferredHostKeyAlorithms =
            new List<PublicKeyAlgorithm> { PublicKeyAlgorithm.DSA }; //, PublicKeyAlgorithm.RSA 

        public static readonly IList<EncryptionAlgorithm> preferredEncryptionAlorithms =
            new List<EncryptionAlgorithm>
            {
                EncryptionAlgorithm.Aes128Cbc,
                EncryptionAlgorithm.BlowfishCbc,
                EncryptionAlgorithm.TripleDesCbc
            };

        public static readonly IList<MacAlgorithm> preferredMacAlgorithms =
            new List<MacAlgorithm> { MacAlgorithm.HmacSha1 };

        public static readonly IList<CompressionAlgorithm> preferredCompressionAlgorithms =
            new List<CompressionAlgorithm> { CompressionAlgorithm.None };

        private bool _newKeysPacketRecevied;
        private ICipherFactory _cipherFactory;
        private IKeySetupService _keySetup;
        private ISecureRandom _random;
        private IMacFactory _macFactory;


        public VersionsExchangedState(ICipherFactory cipherFactory, IKeySetupService keySetup, ISecureRandom random, IMacFactory macFactory)
        {
            _cipherFactory = cipherFactory;
            _keySetup = keySetup;
            _random = random;
            _macFactory = macFactory;
        }

        public override void Process(ITransportLayerManager manager)
        {
            sendKexInitPacket(manager);

            while (manager.Parameters == null || manager.E == null)
            {
                ProcessPacket(manager);
            }

            sendKexDHReplyPacket(manager);

            while (!_newKeysPacketRecevied)
            {
                ProcessPacket(manager);
            }

            manager.ChangeState(TransportLayerState.KeysExchanged);
        }

        protected virtual void sendKexInitPacket(ITransportLayerManager manager)
        {
            KexInitPacket kexInit = new KexInitPacket(_random);
            kexInit.KexAlgorithms.Names.Add("diffie-hellman-group1-sha1");

            foreach (PublicKeyAlgorithm algorithm in preferredHostKeyAlorithms)
            {
                kexInit.ServerHostKeyAlgorithms.Names.Add(PublicKeyAlgorithmHelper.ToString(algorithm));
            }

            foreach (EncryptionAlgorithm algorithm in preferredEncryptionAlorithms)
            {
                kexInit.EncryptionAlgorithmsClientToServer.Names.Add(EncryptionAlgorithmHelper.ToString(algorithm));
                kexInit.EncryptionAlgorithmsServerToClient.Names.Add(EncryptionAlgorithmHelper.ToString(algorithm));
            }

            foreach (MacAlgorithm algorithm in preferredMacAlgorithms)
            {
                kexInit.MacAlgorithmsClientToServer.Names.Add(MacAlgorithmHelper.ToString(algorithm));
                kexInit.MacAlgorithmsServerToClient.Names.Add(MacAlgorithmHelper.ToString(algorithm));
            }

            foreach (CompressionAlgorithm algorithm in preferredCompressionAlgorithms)
            {
                kexInit.CompressionAlgorithmsClientToServer.Names.Add(CompressionAlgorithmHelper.ToString(algorithm));
                kexInit.CompressionAlgorithmsServerToClient.Names.Add(CompressionAlgorithmHelper.ToString(algorithm));
            }

            manager.ServerKexInit = kexInit;
            manager.WritePacket(kexInit);
        }

        private void sendKexDHReplyPacket(ITransportLayerManager manager)
        {
            KexDHReplyPacket kexReply = new KexDHReplyPacket(
                manager.E,
                manager.ClientVersion,
                ConnectedState.VersionString,
                manager.ClientKexInit,
                manager.ServerKexInit,
                new SHA1CryptoServiceProvider(),
                _keySetup.GetServerKeyPair(manager.Parameters.HostKeyVerification),
                manager.Parameters.HostKeyVerification, _random);

            manager.Key = kexReply.K;
            manager.SessionId = kexReply.H;
            manager.Hash = kexReply.H;
            manager.WritePacket(kexReply);
            manager.WritePacket(new NewKeysPacket());
        }

        public override void KexInitPacket(ITransportLayerManager manager, KexInitPacket packet)
        {
            manager.ClientKexInit = packet;

			if (!CheckAlgorithmSupport(packet.KexAlgorithms))
            {
                throw new InvalidOperationException("No supported kex init algorithm.");
            }

            TransportLayerParameters parameters = new TransportLayerParameters();

            parameters.HostKeyVerification = DecideHostKeyAlgorithm(packet.ServerHostKeyAlgorithms);

            parameters.ClientToServerEncryption = DecideEncryptionAlgorithm(packet.EncryptionAlgorithmsClientToServer);
            parameters.ServerToClientEncryption = DecideEncryptionAlgorithm(packet.EncryptionAlgorithmsServerToClient);

            parameters.ClientToServerMac = DecideMacAlgorithm(packet.MacAlgorithmsClientToServer);
            parameters.ServerToClientMac = DecideMacAlgorithm(packet.MacAlgorithmsServerToClient);

            parameters.ClientToServerCompression = DecideCompressionAlgorithm(packet.CompressionAlgorithmsClientToServer);
            parameters.ServerToClientCompression = DecideCompressionAlgorithm(packet.CompressionAlgorithmsServerToClient);
            
            manager.Parameters = parameters;
        }

        public override void KexDHInitPacket(ITransportLayerManager manager, KexDHInitPacket packet)
        { 	
            manager.E = packet.E;
        }

        public override void NewKeysPacket(ITransportLayerManager manager, NewKeysPacket packet)
        {

            SymmetricAlgorithm transmitCipher = _cipherFactory.CreateCipher(
                manager.Parameters.ServerToClientEncryption,
                manager.Key,
                manager.Hash,
                manager.SessionId,
                'B',
                'D');

            SymmetricAlgorithm receiveCipher = _cipherFactory.CreateCipher(
                manager.Parameters.ClientToServerEncryption,
                manager.Key,
                manager.Hash,
                manager.SessionId,
                'A',
                'C');

            _macFactory.Initialize(
                manager.Parameters.ServerToClientMac,
                manager.Key,
                manager.Hash,
                manager.SessionId);

            lock (manager.CommunicationLock)
            {
                manager.TransmitCipher = transmitCipher;
                manager.ReceiveCipher = receiveCipher;
                manager.TransmitMac = 'F';
                manager.ReceiveMac = 'E';
            }

            _newKeysPacketRecevied = true;
        }

        public bool CheckAlgorithmSupport(NameList kexAlgorithms)
        {
            return kexAlgorithms.Names.Contains("diffie-hellman-group1-sha1");
        }

        public PublicKeyAlgorithm DecideHostKeyAlgorithm(NameList kexAlgorithms)
        {
            foreach (PublicKeyAlgorithm algorithm in preferredHostKeyAlorithms)
            {
                if (kexAlgorithms.Names.Contains(PublicKeyAlgorithmHelper.ToString(algorithm)))
                {
                    return algorithm;
                }
            }

            throw new InvalidOperationException("No supported host key algorithm.");
        }

        private EncryptionAlgorithm DecideEncryptionAlgorithm(NameList encryptionAlgorithms)
        {
            foreach (EncryptionAlgorithm algorithm in preferredEncryptionAlorithms)
            {
                if (encryptionAlgorithms.Names.Contains(EncryptionAlgorithmHelper.ToString(algorithm)))
                {
                    return algorithm;
                }
            }

            throw new InvalidOperationException("No supported encryption algorithm.");
        }

        private MacAlgorithm DecideMacAlgorithm(NameList macAlgorithms)
        {
            foreach (MacAlgorithm algorithm in preferredMacAlgorithms)
            {
                if (macAlgorithms.Names.Contains(MacAlgorithmHelper.ToString(algorithm)))
                {
                    return algorithm;
                }
            }

            throw new InvalidOperationException("No supported mac algorithm.");
        }

        private CompressionAlgorithm DecideCompressionAlgorithm(NameList compressionAlgorithms)
        {
            foreach (CompressionAlgorithm algorithm in preferredCompressionAlgorithms)
            {
                if (compressionAlgorithms.Names.Contains(CompressionAlgorithmHelper.ToString(algorithm)))
                {
                    return algorithm;
                }
            }

            throw new InvalidOperationException("No supported compression algorithm.");
        }
    }
}
