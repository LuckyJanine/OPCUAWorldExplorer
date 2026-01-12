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
            var data = DataSchemaLoader.LoadDataSchemaFromFile("DataSchema.json");

            // *** *** Design choice: *** ***
            // Keep Top-level (Domain-specific) Orgnizing intent in Json
            // So that top-level Json keys become OPCUA Orgnizer Nodes/Folders: FolderType - Pure navigation, no semantics

            //base.CreateAddressSpace(externalReferences);
            //NodeState objectsFolder = FindPredefinedNode(ObjectIds.ObjectsFolder, typeof(NodeState));
            //No need to let CustomNodeManager be aware of Standard Nodes from `Standard Server`

            // Adds a reference from the `Objects` folder
            // Attach to the Objects folder using a proper reference
            // Tell the Object folder that it "has" this child
            //if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out var refs)) // “Does the ObjectsFolder already have a reference list where I can attach my nodes?”
            //{
            //    refs = new List<IReference>();
            //    externalReferences[ObjectIds.ObjectsFolder] = refs;
            //}

            // Keep some boundaries for potential different CustomNodeManagers
            // to avoid flat all Organizer Folders into the single ObjectsFolder (for now, only DemoNodeManager ... 
            // ObjectsFolder -> DemoNodeManagerRootFolder 

            var demoNodeManagerRoot = new FolderState(null) 
            {
                SymbolicName = "DemoModel",
                ReferenceTypeId = ReferenceTypeIds.Organizes,
                TypeDefinitionId = ObjectTypeIds.FolderType,
                NodeId = new NodeId("DemoModel", NamespaceIndex),
                BrowseName = new QualifiedName("DemoModel", NamespaceIndex),
                DisplayName = "DemoModel"
            };

            // ? Q ***** to understand the diff: AddChild() vs AddRefernece()
            //objectsFolder.AddChild(demoNodeManagerRoot);
            //AddPredefinedNode(SystemContext, demoNodeManagerRoot);
            // A ***:
            // AddChild() is correct when:
            // Adding Variables, Methods, or Components to an Object instance
            // Using HasComponent or HasProperty
            // so: 
            // AddChild() is designed for object instances,
            // where the parent node owns the child(composition). 
            //objectsFolder.AddReference(
            //    ReferenceTypeIds.Organizes,
            //    false,
            //    demoNodeManagerRoot.NodeId
            //);


            // DemoNodeManagerRootFolder -> Organizer/Domain Folders 
            foreach (var domain in data)
            {
                //if (domain.Value.ValueKind != JsonValueKind.Array)
                //{
                //    // Not sure to throw or to continue, for real server in this case
                //    throw new InvalidOperationException($"Json model schema mismatch: {domain.Key}");
                //}

                if (domain.Value.Count == 0)
                {
                    throw new Exception($"No object node created for the folder: {domain.Key}");
                }

                var domainFolder = CreateOpcUaOrganizerFolder(demoNodeManagerRoot, domain.Key);

                var componentFolders = CreateOpcUaComponentsFolder(domainFolder, domain.Value);

                if (componentFolders != null && componentFolders.Count > 0)
                {
                    Console.WriteLine($"{componentFolders.Count} Component-Folders created for {domain.Key} Folder.");
                }

            }
        }

        private FolderState CreateOpcUaOrganizerFolder(
            NodeState root,
            string folderName)
        {
            var organizerFolder = new FolderState(root)
            {
                SymbolicName = folderName,
                ReferenceTypeId = ReferenceTypeIds.Organizes,
                TypeDefinitionId = ObjectTypeIds.FolderType,
                NodeId = new NodeId($"{root.NodeId}.{folderName}", NamespaceIndex),
                BrowseName = new QualifiedName(folderName, NamespaceIndex),
                DisplayName = folderName
            };

            //parent.AddChild(organizerFolder);
            //AddPredefinedNode(SystemContext, organizerFolder);

            // ObjectsFolder is a Folder, not a functional object that `owns` components - it is a navigation root
            // Folders organize nodes, they do not own them
            root.AddReference(
                ReferenceTypeIds.Organizes,
                false,
                organizerFolder.NodeId
            );

            return organizerFolder;
        }

        // OpcUa ObjectNode: 
        // The OPC specification defines that every Node can be uniquely identified in the Adress Space
        // via an Identifier (=NodeId).
        // The NodeId is defined either by a GUID (Global Unique Identifier),
        // a numeric expression,
        // an array of bytes or a string value.
        // In general but not necessarily, the NodeId contains the “Namespace”.

        /// <summary>
        /// Create Object/Component Folder (with References) in OpcUa
        /// </summary>
        // Corresponds to the flattened (bottom-level) Json objects
        private IReadOnlyList<FolderState> CreateOpcUaComponentsFolder(
            NodeState parent,
            List<OpcUaObject> models) 
        {
            //foreach (JsonElement obj in value.EnumerateArray())
            //{

            //}

            var componentFolders = new List<FolderState>();

            if (models != null && models.Count > 0)
            {
                foreach (var uaObj in models)
                {
                    var uaObjFolder = new FolderState(parent)
                    {
                        SymbolicName = uaObj.DisplayName,
                        NodeId = new NodeId(uaObj.Id, NamespaceIndex),
                        BrowseName = new QualifiedName(uaObj.DisplayName, NamespaceIndex),
                        DisplayName = uaObj.DisplayName,
                        TypeDefinitionId = ObjectTypeIds.FolderType,
                        ReferenceTypeId = ReferenceTypeIds.HasComponent
                    };

                    // Iterates over each variable defined in the JSON
                    // Each variable will become a Variable node under the parent Object (Component) node
                    // Variable nodes are child nodes of the Object node.
                    foreach (var attri in uaObj.Variables)
                    {
                        var variableNode = new BaseDataVariableState(uaObjFolder) // `New` a Variable node in memory
                        {
                            NodeId = new NodeId($"{uaObj.Id}.{attri.Name}", NamespaceIndex), // Unique identifier (e.g., Movie.Name)
                            BrowseName = new QualifiedName(attri.Name, NamespaceIndex), // Name clients see when browsing the Movie object
                            DisplayName = attri.Name,
                            DataType = JsonToOpcUaNodeTypeMapper.GetDataTypeId(attri.Type),
                            ValueRank = ValueRanks.Scalar, // Scalar vs Array (-1 for Scalar)
                            TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
                            AccessLevel = AccessLevels.CurrentReadOrWrite,
                            UserAccessLevel = AccessLevels.CurrentReadOrWrite,
                            Value = attri.Value, // Default from data schema definition
                            StatusCode = StatusCodes.Good,
                            Timestamp = DateTime.UtcNow // Last update time
                        };

                        uaObjFolder.AddChild(variableNode);
                    }

                    parent.AddReference(
                        ReferenceTypeIds.HasComponent, false, uaObjFolder.NodeId);

                    uaObjFolder.AddReference(
                        ReferenceTypeIds.HasComponent, true, parent.NodeId);

                    // Adds an Organizes reference from Movie → ObjectsFolder. Clients see it when browsing Objects.
                    // Standard UA reference type for “logical grouping/containment/navigation.”
                    // isForward: Forward reference (Movie → Objects). Back-reference is handled automatically.
                    // NodeId of the standard OPC UA Objects folder
                    // ******************
                    // objectNode.AddReference( 
                    //    ReferenceTypeIds.Organizes, 
                    //    true, 
                    //    ObjectIds.ObjectsFolder  
                    // );
                    // ⓘ ***** In OPC UA, “attaching to Objects” is just adding a forward Organizes reference to the standard ObjectsFolder node.
                    // a.k.a “This node appears inside the `Objects` folder of the OPC UA server.”


                    AddPredefinedNode(SystemContext, uaObjFolder);

                    // Registers the node with the NodeManager
                    // The server now knows this node exists and can manage it
                    // Without this, clients cannot see or browse it
                    // ******************
                    // AddPredefinedNode(SystemContext, objectNode);
                    // ⓘ Adding the node here does not yet attach it to the graph — that comes in the next step with a reference.

                    componentFolders.Add( uaObjFolder );
                }
            }

            return componentFolders;
        }

        
    }
}
