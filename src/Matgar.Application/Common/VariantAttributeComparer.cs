using System.Text.Json;

namespace Matgar.Application.Common
{
    // بنحول AttributesJson لأي نص متطابقه (canonical) عشان نقارن
    // التركيبات بشكل مستقل عن ترتيب المفاتيح والفراغات. فمثلًا:
    // {"Size":"M","Color":"Red"} == {"Color":"Red","Size":"M"}
    internal static class VariantAttributeComparer
    {
        public static string Canonical(string attributesJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(attributesJson) ? "{}" : attributesJson);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    return "{}";

                var pairs = new List<string>(doc.RootElement.EnumerateObject().Count());
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    var value = prop.Value.ValueKind == JsonValueKind.String
                        ? prop.Value.GetString() ?? ""
                        : prop.Value.GetRawText();
                    pairs.Add($"{prop.Name}={value}");
                }

                pairs.Sort(StringComparer.Ordinal);
                return string.Join("|", pairs);
            }
            catch
            {
                return attributesJson?.Trim() ?? "";
            }
        }
    }
}