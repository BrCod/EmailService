using System.Text.Json;

namespace EmailService.Shared.Serialization
{
    public sealed class JsonSerializerOptionsFactory
    {
        public JsonSerializerOptions Create()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            return options;
        }
    }
}
