using System;
using System.ComponentModel.DataAnnotations;
using OXDesk.Core.Common.Redaction;


namespace OXDesk.Core.Auth.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [PersonalData]
        public required string Email { get; set; }
        
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [SensitiveData]
        public required string Password { get; set; }
        
        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [SensitiveData]
        public required string ConfirmPassword { get; set; }
        
        [Required(ErrorMessage = "First name is required")]
        public required string FirstName { get; set; }
        
        [Required(ErrorMessage = "Last name is required")]
        public required string LastName { get; set; }
        
        public Guid? TenantId { get; set; }
        
        public string? Role { get; set; }
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [PersonalData]
        public required string Email { get; set; }
        
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [SensitiveData]
        public required string Password { get; set; }
        
        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class ApiLoginDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [PersonalData]
        public required string Email { get; set; }
        
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [SensitiveData]
        public required string Password { get; set; }
    }

    public class AuthResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public int? UserId { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
