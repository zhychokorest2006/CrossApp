namespace Core;

// Порушення бізнес-правила (наприклад, видача більша за залишок).
// Некоректні аргументи методів і далі лишаються ArgumentException
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public DomainException(string message, Exception inner) : base(message, inner) { }
}