using Microsoft.Extensions.Configuration;
using Opc.Ua;
using Opc.Ua.Configuration;
using Opc.Ua.Server;
using OpcUaServer;

internal class Program
{
    /// <summary>
    /// Compiler refuses to treat:
    /// static async void Main() as entry point - need to understand/why?
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static async Task Main(string[] args)
    {
        IConfigurationRoot rootConfig = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var options = new OpcUaOptions();
        rootConfig.GetSection("OpcUa").Bind(options);

        var appConfig = new ApplicationConfiguration()
        {
            ApplicationName = options.ApplicationName,
            ApplicationType = ApplicationType.Server,
            ApplicationUri = $"urn:{System.Net.Dns.GetHostName()}:{options.ApplicationName}",
            ProductUri = $"urn:{options.ApplicationName}",
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
                BaseAddresses = new StringCollection(options.BaseAddresses),
                ServerCapabilities = new StringCollection { "DA" },
                SecurityPolicies = new ServerSecurityPolicyCollection
                {
                    new ServerSecurityPolicy
                    {
                        SecurityMode = MessageSecurityMode.None,
                        SecurityPolicyUri = SecurityPolicies.None
                    }
                },
                DiagnosticsEnabled = true
            },
            TransportQuotas = new TransportQuotas
            {
                OperationTimeout = 5000, // 5000 ms, 5 seconds
                MaxStringLength = 100, // 100 chars
                MaxByteStringLength = 100, // 100 bytes
                MaxMessageSize = 1024, // 1kb max
                MaxArrayLength = 10 // 10 element max
            }
        };
       
        appConfig.ValidateAsync(ApplicationType.Server).GetAwaiter().GetResult();

        //ITelemetryContext telemetry = DefaultTelemetryContext.Create();

        // [Obsolete("Use ApplicationInstance(ITelemetryContext) instead.")]
        // Can't find implementation for ITelemetryContext at the moment 
        // "Copyright (c) 2005-2025 The OPC Foundation, Inc. All rights reserved."
        // have to go with this Obsolete Constructor
        var appInstance = new ApplicationInstance();
        appInstance.ApplicationConfiguration = appConfig;

        bool valid = await appInstance.CheckApplicationInstanceCertificatesAsync(false, 0);
        // await application.CheckApplicationInstanceCertificateAsync(false, 2048);

        if (!valid)
        {
            throw new Exception("Certificates Invalid.");
        }

        var demoServer = new OpcUaDemoServer();
        //var server = new StandardServer();

        try
        {
            await appInstance.StartAsync(demoServer);

            Console.WriteLine("Running OPC UA DemoServer...");

            foreach (var endpoint in options.BaseAddresses)
            {
                Console.WriteLine($" - {endpoint}");
            }

        }
        catch (ServiceResultException ex)
        {
            Console.WriteLine(ex.Message);
            if (ex.InnerException != null || ex.InnerResult != null)
            {
                Console.WriteLine(ex.InnerException);
                Console.WriteLine(ex.InnerResult.AdditionalInfo);
            }
        }
        catch (Exception ex) 
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Console.WriteLine();
            Console.WriteLine("Press Ctrl+C to exit...");
        }

        var shutdownEvent = new TaskCompletionSource<bool>();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true; // prevents the default behavior - not to terminate the process immediately
            // need a chance to do a graceful shutdown
            shutdownEvent.SetResult(true);
        };
        await shutdownEvent.Task;

        await appInstance.StopAsync();

    }
}