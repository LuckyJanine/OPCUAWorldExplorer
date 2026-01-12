using Opc.Ua;

namespace OpcUaServer.utils
{
    internal static class JsonToOpcUaNodeTypeMapper
    {
        /// <summary>
        /// a Static mapping method takes deserialized Json object Data Type as the input 
        /// </summary>
        /// <param name="type"></param>
        /// <returns>OpcUa Data Type</returns>
        /// <exception cref="NotSupportedException"></exception>
        public static NodeId GetDataTypeId(string type) =>
            type switch
            {
                "String" => DataTypeIds.String,
                "UInt16" => DataTypeIds.UInt16,
                //"Int32" => DataTypeIds.Int32,
                //"Int16" => DataTypeIds.Int16,
                //"Int64" => DataTypeIds.Int64,
                //"UInt32" => DataTypeIds.UInt32,
                //"Boolean" => DataTypeIds.Boolean,
                //"Double" => DataTypeIds.Double,
                //"Float" => DataTypeIds.Float,
                _ => throw new NotSupportedException($"Unsupported data type: {type}")
            };
    }
}
