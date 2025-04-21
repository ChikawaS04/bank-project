using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;

namespace BankApp.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        [Display(Name = "Full Address")]
        public string ClientFullAddress =>
            $"{ClientStreetAddress}, {ClientCityAddress}, {ClientCountryAddress}, {ClientPostalCode}";

        [Display(Name = "First Name")]
        [Required]
        [StringLength(60)]
        [RegularExpression("^[a-zA-Z'-]*$")]
        public string ClientFirstName { get; set; }

        [Display(Name = "Middle Name")]
        [StringLength(50)]
        [RegularExpression("^[a-zA-Z'-]*$")]
        public string? ClientMiddleName { get; set; }

        [Display(Name = "Last Name")]
        [Required]
        [StringLength(100)]
        [RegularExpression("^[a-zA-Z'-]*$")]
        public string ClientLastName { get; set; }

        [Display(Name = "Phone")]
        [Required]
        [RegularExpression(@"^\d{10}$")]
        [DataType(DataType.PhoneNumber)]
        public string ClientPhone { get; set; }

        [Display(Name = "Email")]
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
        public string? ClientEmail { get; set; }

        [Display(Name = "Street Address")]
        [Required]
        [StringLength(100)]
        public string ClientStreetAddress { get; set; }

        [Display(Name = "Postal Code")]
        [Required]
        [RegularExpression("^[A-Za-z][0-9][A-Za-z] [0-9][A-Za-z][0-9]$")]
        public string ClientPostalCode { get; set; }

        [Display(Name = "City")]
        [Required]
        public string ClientCityAddress { get; set; }

        [Display(Name = "Country")]
        [Required]
        [StringLength(100)]
        [RegularExpression("^[a-zA-Z'-]*$")]
        public string ClientCountryAddress { get; set; }

        public ICollection<Account> Accounts { get; set; } = new List<Account>();
    }
}
