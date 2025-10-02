namespace OXDesk.Core.Extension;

public interface IUpdationAudited
{
    DateTime UpdatedAt { get; }
    Guid UpdatedBy { get; }
}
