namespace OXDesk.Core.Extension;

public interface ICreationAudited
{
    DateTime CreatedAt { get; set; }
    Guid CreatedBy { get; set; }
}