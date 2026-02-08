namespace OXDesk.Core.Identity.DTOs;

/// <summary>
/// Request DTO for creating or updating a user signature.
/// All fields are nullable as per requirements.
/// </summary>
public sealed class UpsertUserSignatureRequest
{
    public string? ComplimentaryClose { get; set; }
    public string? FullName { get; set; }
    public string? JobTitle { get; set; }
    public string? Company { get; set; }
    public string? Department { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? Telephone { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
}

/// <summary>
/// Response DTO for a user signature.
/// </summary>
public sealed class UserSignatureResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? ComplimentaryClose { get; set; }
    public string? FullName { get; set; }
    public string? JobTitle { get; set; }
    public string? Company { get; set; }
    public string? Department { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? Telephone { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedByText { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedByText { get; set; }
}
