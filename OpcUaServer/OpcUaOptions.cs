namespace OpcUaServer
{
    public sealed class OpcUaOptions
    {
        public string[] BaseAddresses { get; set; } = Array.Empty<string>();
        public int Port { get; set; } = 4840;

        public string ApplicationName { get; set; } = string.Empty;
    }
}
