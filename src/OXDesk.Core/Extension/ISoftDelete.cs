namespace OXDesk.Core.Extension;

public interface ISoftDelete
{
    DateTime? DeletedAt { get; set; }
    Guid? DeletedBy { get; set; }
}