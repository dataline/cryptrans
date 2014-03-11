using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureFileTransfer.Security
{
    public static class KeyProvider
    {
        static RSA _rsa = null;
        static Task rsaGenerationTask = null;

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
