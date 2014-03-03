using SecureFileTransfer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Features
{
    public static class ExceptionExtensions
    {
        public static void Handle(this Exception ex, bool doNotThrow = false)
        {
#if DEBUG
            if (!doNotThrow)
                throw ex;
            else
#endif
            Console.WriteLine("ERROR: " + ex.ToString() + "\n" + ex.Message + "\nStack Trace:\n" + ex.StackTrace);
        }

        public static int GetDescriptionResource(this InvalidHandshakeException ex)
        {
            switch (ex.Phase)
            {
                case SecureFileTransfer.Network.InvalidHandshakeException.HandshakePhase.VersionExchange:
                    return Resource.String.ErrHandshakeWrongVersion;

                case SecureFileTransfer.Network.InvalidHandshakeException.HandshakePhase.FlagExchange:
                    return Resource.String.ErrHandshakeFeatureReq;

                case SecureFileTransfer.Network.InvalidHandshakeException.HandshakePhase.Authentication:
                    return Resource.String.ErrHandshakeInvalidPassword;

                case SecureFileTransfer.Network.InvalidHandshakeException.HandshakePhase.RSAExchange:
                case SecureFileTransfer.Network.InvalidHandshakeException.HandshakePhase.AESExchange:
                case SecureFileTransfer.Network.InvalidHandshakeException.HandshakePhase.EncryptionChannelTest:
                    return Resource.String.ErrHandshakeEncryptionFailed;

                case SecureFileTransfer.Network.InvalidHandshakeException.HandshakePhase.End:
                case SecureFileTransfer.Network.InvalidHandshakeException.HandshakePhase.InitialHello:
                default:
                    return Resource.String.ErrHandshakeInvalidResponse;
            }
        }
    }
}
