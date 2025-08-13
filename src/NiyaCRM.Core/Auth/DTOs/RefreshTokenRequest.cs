using System.ComponentModel.DataAnnotations;

namespace NiyaCRM.Core.Auth.DTOs
{
    public class RefreshTokenRequest
    {
        [Required]
        public required string RefreshToken { get; set; }
    }
}
