using MhpdCommon.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace MhpdCommon.Extensions;

public static class ApplicationBuilderExtenions
{
    public static IApplicationBuilder UseClientCertificateValidation(this IApplicationBuilder app, X509Certificate2 certificate)
    {
        return app.Use(async (context, next) =>
        {
            var tlsFeature = context.Features.Get<ITlsConnectionFeature>();
            var clientCertificate = tlsFeature?.ClientCertificate;
            if (clientCertificate == null ||
            !clientCertificate.Thumbprint.Equals(certificate.Thumbprint, StringComparison.OrdinalIgnoreCase) ||
            !clientCertificate.Issuer.Equals(certificate.Issuer, StringComparison.OrdinalIgnoreCase) ||
            clientCertificate.NotBefore > DateTime.Now ||
            clientCertificate.NotAfter < DateTime.Now)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync(SecurityConstants.Mtls.InvalidCertificateMessage);
                return;
            }
            await next();
        });
    }
}
