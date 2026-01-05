using Opc.Ua.Export;
using System.IO;
using System.Text.Json;

namespace OpcUaServer.utils
{
    internal static class DataSchemaLoader
    {
        internal static OpcUaModel LoadDataSchemaFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("File not specified.");

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Schema file not found", filePath);

            var data = File.ReadAllText(filePath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var model = JsonSerializer.Deserialize<OpcUaModel>(data, options);

            if (model == null)
                throw new InvalidOperationException($"Failed to deserialize model from data schema provided from\n{filePath}");

            return model;

        }
    }
}
