using Microsoft.Extensions.Compliance.Classification;

namespace OXDesk.Core.Common.Redaction
{
    /// <summary>
    /// Defines data classifications used for redaction within OXDesk.
    /// </summary>
    public static class RedactionTaxonomy
    {
        /// <summary>
        /// The taxonomy name for OXDesk redaction classifications.
        /// </summary>
        public static string TaxonomyName => typeof(RedactionTaxonomy).FullName!;

        /// <summary>
        /// Classification for personal data.
        /// </summary>
        public static DataClassification PersonalData => new(TaxonomyName, nameof(PersonalData));

        /// <summary>
        /// Classification for sensitive data.
        /// </summary>
        public static DataClassification SensitiveData => new(TaxonomyName, nameof(SensitiveData));
    }

    
    public class SensitiveDataAttribute : DataClassificationAttribute
    {
        public SensitiveDataAttribute() : base(RedactionTaxonomy.SensitiveData) { }
    }

    public class PersonalDataAttribute : DataClassificationAttribute
    {
        public PersonalDataAttribute() : base(RedactionTaxonomy.PersonalData) { }
    }
}
