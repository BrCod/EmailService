namespace EmailService.Domain.ValueObjects
{
    public sealed class TemplateData
    {
        private readonly Dictionary<string, string> _values = new();
        public TemplateData() { }
        public TemplateData(IDictionary<string, string> values)
        {
            foreach (var kvp in values) _values[kvp.Key] = kvp.Value;
        }
        public IReadOnlyDictionary<string, string> Values => _values;
        public string? this[string key] => _values.TryGetValue(key, out var v) ? v : null;
    }
}
