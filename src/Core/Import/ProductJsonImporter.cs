using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ImportResult<ProductDto> Load(string path)
    {
        var items = new List<ProductDto>();
        var errors = new List<string>();

        try
        {
            string json = File.ReadAllText(path);
            var deserialized = JsonSerializer.Deserialize<List<ProductDto>>(json, Options) ?? [];

            // Валідація завантажених об'єктів
            for (int i = 0; i < deserialized.Count; i++)
            {
                var item = deserialized[i];
                if (string.IsNullOrWhiteSpace(item.Sku) || string.IsNullOrWhiteSpace(item.Name))
                {
                    errors.Add($"елемент #{i + 1}: SKU або назва порожні");
                }
                else if (item.Quantity < 0)
                {
                    errors.Add($"елемент #{i + 1}: кількість '{item.Quantity}' не є невід'ємним числом");
                }
                else
                {
                    items.Add(item);
                }
            }
        }
        catch (JsonException ex)
        {
            errors.Add($"Помилка структури JSON: {ex.Message}");
        }

        return new ImportResult<ProductDto>(items, errors);
    }
}