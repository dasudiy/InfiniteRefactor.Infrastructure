using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace AirMaster.Infrastructure.DataService.Bindings.Stream.Infrastructure
{
    class CertificateValidator
    {
        internal static bool Validate(X509Certificate2 certificate, X509Certificate2 rootCA = null, X509Chain chain = null)
        {
            if (certificate == null) { return false; }

            if (chain == null)
            {
                chain = new X509Chain();
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                chain.Build(certificate);
            }
            else
            {
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                chain.Build(certificate);
            }

            if (chain.ChainStatus.Any(s => s.Status != X509ChainStatusFlags.NoError)) { return false; }
            if (rootCA != null)
            {
                return chain.ChainElements.Cast<X509ChainElement>().FirstOrDefault(x => x.Certificate.Issuer == x.Certificate.Subject).Certificate.Equals(rootCA);
            }
            else
            {
                return true;
            }
        }

        internal static X509Certificate2 ValidateAndGetRootCA(X509Certificate2 certificate, X509Certificate2 rootCA = null, X509Chain chain = null)
        {
            if (chain == null)
            {
                chain = new X509Chain();
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                chain.Build(certificate);
            }
            else
            {
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                chain.Build(certificate);
            }

            if (chain.ChainStatus.Any(s => s.Status != X509ChainStatusFlags.NoError))
            {
                throw new ArgumentException("Certificate is not valid, " + string.Join(" ,", chain.ChainStatus.Where(s => s.Status != X509ChainStatusFlags.NoError).Select(s => s.StatusInformation)));
            }
            if (rootCA != null)
            {
                if (!chain.ChainElements.Cast<X509ChainElement>().FirstOrDefault(x => x.Certificate.Issuer == x.Certificate.Subject).Certificate.Equals(rootCA))
                {
                    throw new ArgumentException(certificate.Subject + "'s rootCA is not " + rootCA.Subject);
                }
            }

            return chain.ChainElements.Cast<X509ChainElement>().FirstOrDefault(x => x.Certificate.Issuer == x.Certificate.Subject).Certificate;
        }
    }
}
