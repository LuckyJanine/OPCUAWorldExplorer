using System.Text.Json;

namespace OpcUaServer.utils
{
    internal static class DataSchemaLoader
    {
        /// <summary>
        /// Load Top-level Json for OpcUa organizer Nodes
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>Dictionary string as Keys for OpcUa organizer Nodes, JsonElement as value</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        internal static IEnumerable<KeyValuePair<string, List<OpcUaObject>>> LoadDataSchemaFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("File not specified.");

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Schema file not found", filePath);

            var data = File.ReadAllText(filePath);

            var dict = new Dictionary<string, List<OpcUaObject>>();

            if (string.IsNullOrWhiteSpace(data))
            {
                return dict;
            }

            using var doc = JsonDocument.Parse(data); // the entire Json tree in memory

            JsonElement root = doc.RootElement;

            foreach (JsonProperty prop in root.EnumerateObject()) // Top level Json Keys
            {
                if (!dict.ContainsKey(prop.Name))
                {
                    var models = DeserializeJsonToOpcUaObjects(prop.Value);
                    dict.TryAdd(prop.Name, models);
                }
            }
            
            return dict;
        }

        private static List<OpcUaObject> DeserializeJsonToOpcUaObjects(JsonElement json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                var uaObjs = JsonSerializer.Deserialize<List<OpcUaObject>>(json, options);
                return uaObjs;
            }
            catch (JsonException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return new List<OpcUaObject>();
        }
    }
}
