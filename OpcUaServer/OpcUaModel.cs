namespace OpcUaServer
{
    internal class OpcUaModel
    {
        public List<OpcUaObject> Objects { get; set; } = new();
    }

    internal class OpcUaObject
    {   
        public string Name { get; set; } = string.Empty;
        public List<OpcUaVariable> Variables { get; set; } = new();
    }

    internal record OpcUaVariable
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public object? Value { get; set; }
    }
}
