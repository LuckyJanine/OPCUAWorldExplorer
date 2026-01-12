namespace OpcUaServer
{
    internal class OpcUaObject
    {   
        public required uint Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public List<OpcUaVariable> Variables { get; set; } = new();
    }

    internal record OpcUaVariable
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public object? Value { get; set; }
    }
}
