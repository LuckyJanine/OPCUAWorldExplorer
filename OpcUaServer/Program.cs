using Microsoft.Extensions.Configuration;
using Opc.Ua;
using Opc.Ua.Server;
using OpcUaServer;
using System.Security.Cryptography.X509Certificates;

internal class Program
{
    /// <summary>
    /// Compiler refuses to treat:
    /// static async void Main() as entry point - need to understand/why?
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static void Main(string[] args)
    {
        IConfigurationRoot rootConfig = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var options = new OpcUaOptions();
        rootConfig.GetSection("OpcUa").Bind(options);

        var config = new ApplicationConfiguration()
        {
            ApplicationName = options.ApplicationName,
            ApplicationType = ApplicationType.Server,
            SecurityConfiguration = new SecurityConfiguration
            {
                ApplicationCertificate = new CertificateIdentifier
                {
                    StoreType = "Directory",
                    StorePath = "CertStorage/Own",
                    SubjectName = $"CN={options.ApplicationName}"
                },
                TrustedIssuerCertificates = new CertificateTrustList
                {
                    StoreType = "Directory",
                    StorePath = "CertStorage/Issuer"
                },
                TrustedPeerCertificates = new CertificateTrustList
                {
                    StoreType = "Directory",
                    StorePath = "CertStorage/Trusted"
                },
                RejectedCertificateStore = new CertificateTrustList
                {
                    StoreType = "Directory",
                    StorePath = "CertStorage/Rejected"
                },
                AutoAcceptUntrustedCertificates = true,
                AddAppCertToTrustedStore = true
            },
            ServerConfiguration = new ServerConfiguration
            {
                BaseAddresses = new StringCollection(options.BaseAddresses)
            }
        };
       
        config.ValidateAsync(ApplicationType.Server).GetAwaiter().GetResult();

        Console.WriteLine("Running OPC UA Server.");
        Console.WriteLine("Configured endpoints:");

        foreach (var endpoint in options.BaseAddresses)
        {
            Console.WriteLine($" - {endpoint}");
        }

        var server = new StandardServer();

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();

    }
}