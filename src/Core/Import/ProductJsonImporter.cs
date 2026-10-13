using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // onSkipped (необов'язковий): викликається для кожного пропущеного запису
    public static ImportResult<ProductDto> Load(string path, Action<string>? onSkipped = null)
    {
        var items = new List<ProductDto>();
        var errors = new List<string>();

        void Skip(string message)
        {
            errors.Add(message);
            onSkipped?.Invoke(message);
        }

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
                    Skip($"елемент #{i + 1}: SKU або назва порожні");
                }
                else if (item.Quantity < 0)
                {
                    Skip($"елемент #{i + 1}: кількість '{item.Quantity}' не є невід'ємним числом");
                }
                else
                {
                    items.Add(item);
                }
            }
        }
        catch (JsonException ex)
        {
            Skip($"Помилка структури JSON: {ex.Message}");
        }

        return new ImportResult<ProductDto>(items, errors);
    }
}