using Opc.Ua;
using Opc.Ua.Server;

namespace OpcUaServer
{
    internal class OpcUaDemoServer : StandardServer
    {
        protected override MasterNodeManager CreateMasterNodeManager(
            IServerInternal server,
            ApplicationConfiguration configuration)
        {
            try
            {
                var customNodeManagers = new List<INodeManager>
                {
                    new DemoNodeManager(server, configuration, new string[] { DemoNodeManager.NamespaceUri })
                };

                Console.WriteLine("Master NodeManager Created with the following Registerd NodeManager(s):");

                foreach (var nodeManager in customNodeManagers)
                {
                    Console.WriteLine($"\t{nodeManager.GetType().FullName}");
                }

                return new MasterNodeManager(server, configuration, null, customNodeManagers.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error when creating MasterNodeManager: " + ex.Message);
                throw;
            }

        }

    }
}
