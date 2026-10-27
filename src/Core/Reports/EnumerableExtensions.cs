namespace Core.Reports;

public static class EnumerableExtensions
{
    // Перші n елементів за найбільшим числовим ключем.
    // tieBreaker — другий ключ, щоб порядок однакових елементів не змінювався між запусками
    public static IEnumerable<T> TopBy<T>(
        this IEnumerable<T> source, Func<T, int> key, int n, Func<T, string> tieBreaker)
    {
        return source
            .OrderByDescending(key)
            .ThenBy(tieBreaker, StringComparer.Ordinal)
            .Take(n);
    }
}