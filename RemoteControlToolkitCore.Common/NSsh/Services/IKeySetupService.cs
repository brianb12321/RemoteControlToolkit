using RemoteControlToolkitCore.Common.NSsh.Types;

namespace RemoteControlToolkitCore.Common.NSsh.Services
{
    public interface IKeySetupService
    {
        void EnsureSetup();

        IPublicKeyPair GetServerKeyPair(PublicKeyAlgorithm publicKeyAlgorithm);
    }
}
