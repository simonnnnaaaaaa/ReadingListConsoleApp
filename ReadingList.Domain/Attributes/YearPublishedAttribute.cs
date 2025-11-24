using System.ComponentModel.DataAnnotations;

namespace ReadingList.Domain
{
    public class YearPublishedAttribute : ValidationAttribute
    {
        public int MinYear { get; set; } = 1500;

        public YearPublishedAttribute()
        {
            ErrorMessage = $"Year must be between 1500 and the current year.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success; 

            if (value is int year)
            {
                int currentYear = DateTime.Now.Year;
                if (year < MinYear || year > currentYear)
                {
                    return new ValidationResult(ErrorMessage);
                }
                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid year value.");
        }
    }
}
