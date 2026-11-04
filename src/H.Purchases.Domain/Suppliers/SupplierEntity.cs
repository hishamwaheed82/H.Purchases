using System;
using H.Platform.Common;
using Volo.Abp;

namespace H.Purchases.Suppliers;

/// <summary>
/// Supplier master — the external party from whom goods/services are purchased.
/// </summary>
public class Supplier : HTrackedAggregateRoot<Guid>
{
    /// <summary>Short, unique business code (e.g. "SUP001").</summary>
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>Primary contact phone number (optional).</summary>
    public string? Phone { get; set; }

    /// <summary>Primary contact email address (optional).</summary>
    public string? Email { get; set; }

    public bool IsActive { get; set; } = true;

    protected Supplier() { }

    public Supplier(Guid id) : base(id) { }

    public void EnsureValid()
    {
        Code = Check.NotNullOrWhiteSpace(Code, nameof(Code), maxLength: 32);
        Name = Check.NotNullOrWhiteSpace(Name, nameof(Name), maxLength: 256);
    }
}
