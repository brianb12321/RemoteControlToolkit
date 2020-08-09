using RemoteControlToolkitCore.Common.Networking.NSsh.Types;

namespace RemoteControlToolkitCore.Common.Networking.NSsh.Services
{
    public interface IKeySetupService
    {
        void EnsureSetup();

        IPublicKeyPair GetServerKeyPair(PublicKeyAlgorithm publicKeyAlgorithm);
    }
}
