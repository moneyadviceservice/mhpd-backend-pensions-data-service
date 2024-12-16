using MhpdCommon.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MhpdCommon.Extensions;

public static class ApplicationBuilderExtenions
{
    public static IApplicationBuilder UseClientCertificateValidation(this IApplicationBuilder app, X509Certificate2? certificate)
    {
        return app.Use(async (context, next) =>
        {
            if (certificate != null)
            {
                var tlsFeature = context.Features.Get<ITlsConnectionFeature>();
                var clientCertificate = tlsFeature?.ClientCertificate;

                LogClientCertificate(clientCertificate, certificate);

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
            }

            await next();
        });
    }

    private static void LogClientCertificate(X509Certificate2? requestCertificate, X509Certificate2 clientcertificate)
    {
        var requestDate = DateTime.Now;
        var builder = new StringBuilder($"Validating request certificate against known client certificate...{Environment.NewLine}");
        builder.AppendLine($"Request certificate is {(requestCertificate == null ? "absent" : "present")}");

        if( requestCertificate == null ) return;
        builder.AppendLine($"Thumbrint comparison - request: {requestCertificate.Thumbprint} client: {clientcertificate.Thumbprint}");
        builder.AppendLine($"Issuer comparison - request: {requestCertificate.Issuer} client: {clientcertificate.Issuer}");
        builder.AppendLine($"Issuer Name comparison - request: {requestCertificate.IssuerName} client: {clientcertificate.IssuerName}");
        builder.AppendLine($"Valid from comparison - starts: {requestCertificate.NotBefore} request date: {requestDate}");
        builder.AppendLine($"Valid to comparison - expires: {requestCertificate.NotAfter} request date: {requestDate}");
        Console.WriteLine(builder.ToString());
    }
}
