namespace Core.Events;

// Що саме сталося з товаром
public enum CatalogChangeKind
{
    Added,
    Received,
    Issued,
    Removed
}