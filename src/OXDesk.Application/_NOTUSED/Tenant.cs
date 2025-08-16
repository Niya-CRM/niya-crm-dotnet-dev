using System.Text.Json;

namespace OXDesk.Application.MultiTenancy;

public class Tenant
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string Identifier { get; private set; } = default!;
    public string? ConnectionString { get; private set; }
    public string Host { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public JsonDocument? Settings { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }

    private Tenant() { }

    public static Tenant Create(string name, string identifier, string host, string? connectionString = null)
    {
        return new Tenant
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Identifier = identifier,
            Host = host,
            ConnectionString = connectionString,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateSettings(JsonDocument settings)
    {
        Settings = settings;
        LastModifiedAt = DateTime.UtcNow;
    }
}
