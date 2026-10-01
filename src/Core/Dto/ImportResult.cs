using System.Collections.Generic;

namespace Core.Dto;

public class ImportResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public IReadOnlyList<string> Errors { get; }

    public ImportResult(IReadOnlyList<T> items, IReadOnlyList<string> errors)
    {
        Items = items;
        Errors = errors;
    }
}