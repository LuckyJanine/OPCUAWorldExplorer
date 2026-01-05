using Opc.Ua;
using Opc.Ua.Server;
using OpcUaServer.utils;

namespace OpcUaServer
{
    internal class DemoNodeManager : CustomNodeManager2
    {
        public const string NamespaceUri = "urn:mini:custom:nodemanager:demo";

        internal DemoNodeManager(
            IServerInternal server, 
            ApplicationConfiguration configuration, 
            params string[] namespaceUris) 
            : base(server, configuration, namespaceUris)
        {
            SystemContext.NodeIdFactory = this;
        }

        public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            var model = DataSchemaLoader.LoadDataSchemaFromFile("DataSchema.json");
        }
    }
}
