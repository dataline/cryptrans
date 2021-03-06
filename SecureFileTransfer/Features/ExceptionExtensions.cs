﻿using SecureFileTransfer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureFileTransfer.Features
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Handle this exception.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="doNotThrow">If this is true, an exception is never re-thrown even when in Debug mode.</param>
        public static void Handle(this Exception ex, bool doNotThrow = false)
        {
            Console.WriteLine("Error handling routine entered.");
#if DEBUG
            if (!doNotThrow)
                throw ex;
            else
                Console.WriteLine("ERROR: " + ex.ToString() + "\n" + ex.Message + "\nStack Trace:\n" + ex.StackTrace);
#else
            Console.WriteLine("Handled exception: " + ex.ToString());
#endif
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
