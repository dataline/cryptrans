using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureFileTransfer.Security
{
    /// <summary>
    /// This creates AES and RSA keys.
    /// 
    /// As mobile devices take a long time generating RSA keypairs, only one
    /// keypair is generated and reused until the app is restarted.
    /// The generation of this key can be done in background and should be started
    /// as soon as the app starts.
    /// </summary>
    public static class KeyProvider
    {
        static RSA _rsa = null;
        static Task rsaGenerationTask = null;

        /// <summary>
        /// Starts RSA key generation in background.
        /// </summary>
        public static void StartKeyGeneration()
        {
            if (_rsa == null && rsaGenerationTask == null)
            {
                rsaGenerationTask = Task.Run(() =>
                {
                    RSA r = new RSA();
                    r.Generate();
                    _rsa = r;
                });
            }
        }

        public static async Task<RSA> GetRSAAsync()
        {
            if (_rsa != null)
                return _rsa;
            if (rsaGenerationTask == null)
                StartKeyGeneration();

            await rsaGenerationTask;
            rsaGenerationTask = null;

            return _rsa;
        }

        public static async Task<AES> GetAESAsync()
        {
            return await Task.Run<AES>(() =>
            {
                return GetAES();
            });
        }

        public static RSA GetRSA()
        {
            return GetRSAAsync().Result;
        }

        public static AES GetAES()
        {
            AES a = new AES();
            a.Generate();
            return a;
        }
    }
}
