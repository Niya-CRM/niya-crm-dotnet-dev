namespace OXDesk.Core.Extension;

public class SoftDeletedEntity : ISoftDelete
{
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
