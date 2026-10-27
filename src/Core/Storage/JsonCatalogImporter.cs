using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

// Читає товари з JSON-файлу асинхронно і з підтримкою скасування
public sealed class JsonCatalogImporter : ICatalogImporter
{
    // Назви полів у файлі можна писати і з великої, і з малої літери
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public async Task<IReadOnlyList<Product>> LoadAsync(string path, CancellationToken ct = default)
    {
        // Якщо токен уже скасовано, файл навіть не відкриваємо
        ct.ThrowIfCancellationRequested();

        // useAsync: true = справжнє асинхронне читання файлу, без обгортання в окремий потік
        await using var stream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

        var dtos = await JsonSerializer.DeserializeAsync<List<ProductDto>>(stream, Options, ct)
            ?? throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку.");

        var result = new List<Product>(dtos.Count);
        for (var i = 0; i < dtos.Count; i++)
        {
            ct.ThrowIfCancellationRequested();   // у циклі періодично перевіряємо скасування

            try
            {
                result.Add(Product.FromDto(dtos[i]));
            }
            catch (ArgumentException ex)
            {
                // Погані дані: доменна помилка з номером запису, початковий виняток зберігаємо
                throw new DomainException($"Запис {i + 1}: {ex.Message}", ex);
            }
        }

        return result;
    }
}