using System;
using System.Collections.Generic;
namespace Core.Domain;

public sealed class Warehouse
{
    private const int MaxIssuePerCustomer = 3;
    private readonly Dictionary<string, int> _issuesPerCustomer = new();

    public void IssueToCustomer(Product product, string customerId, int amount)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("CustomerId обов'язковий", nameof(customerId));

        int alreadyIssued = _issuesPerCustomer.GetValueOrDefault(customerId, 0);
        if (alreadyIssued >= MaxIssuePerCustomer)
        {
            throw new InvalidOperationException(
                $"Клієнт {customerId} вже отримав {alreadyIssued} видач — ліміт {MaxIssuePerCustomer} вичерпано");
        }

        product.Issue(amount);
        _issuesPerCustomer[customerId] = alreadyIssued + 1;
    }
}