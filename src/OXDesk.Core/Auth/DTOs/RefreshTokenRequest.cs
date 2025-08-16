using System.ComponentModel.DataAnnotations;

namespace OXDesk.Core.Auth.DTOs
{
    public class RefreshTokenRequest
    {
        [Required]
        public required string RefreshToken { get; set; }
    }
}
