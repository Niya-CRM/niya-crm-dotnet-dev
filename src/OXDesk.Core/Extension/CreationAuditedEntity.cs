namespace OXDesk.Core.Extension;

public class CreationAuditedEntity : ICreationAudited
{
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
}