namespace OXDesk.Core.Extension;

public class AuditedEntity : ICreationAudited, IUpdationAudited
{
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
}
