using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NiyaCRM.Core.Referentials;

/// <summary>
/// Represents a country in the CRM system.
/// Pure domain entity with minimal business logic.
/// </summary>
[Table("countries")]
public class Country
{

    /// <summary>
    /// Gets or sets the name of the country.
    /// </summary>
    [Required]
    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string CountryName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the two-letter country code (ISO 3166-1 alpha-2).
    /// </summary>
    [Key]
    [Required]
    [StringLength(2)]
    [Column(TypeName = "varchar(2)")]
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the three-letter country code (ISO 3166-1 alpha-3).
    /// </summary>
    [Required]
    [StringLength(3)]
    [Column(TypeName = "varchar(3)")]
    public string CountryCodeAlpha3 { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the country is active.
    /// </summary>
    [Required]
    [StringLength(1)]
    [Column(TypeName = "varchar(1)")]
    public string IsActive { get; set; } = "Y";

    /// <summary>
    /// Initializes a new instance of the <see cref="Country"/> class.
    /// Default constructor for Entity Framework and general use.
    /// </summary>
    public Country() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Country"/> class.
    /// Constructor for creating new country instances with all required properties.
    /// </summary>
    /// <param name="countryName">The country name.</param>
    /// <param name="countryCode">The two-letter country code.</param>
    /// <param name="countryCodeAlpha3">The three-letter country code.</param>
    /// <param name="isActive">Whether the country is active.</param>
    public Country(
        string countryName, 
        string countryCode, 
        string countryCodeAlpha3, 
        string isActive)
    {
        CountryName = countryName;
        CountryCode = countryCode;
        CountryCodeAlpha3 = countryCodeAlpha3;
        IsActive = isActive;
    }
}
