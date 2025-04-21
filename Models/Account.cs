using BankApp.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankApp.Models
{
    public class Account
    {
        [Key]
        public int AccountId { get; set; }

        [BindNever]
        [Display(Name = "Account Number")]
        public string? AccountNumber { get; set; }

        [Display(Name = "Account Type")]
        [Required(ErrorMessage = "Account Type is required.")]
        public AccountType AccountType { get; set; }

        [Display(Name = "Balance")]
        [Required(ErrorMessage = "Balance is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Balance must be a positive number.")]
        [DataType(DataType.Currency)]
        public decimal Balance { get; set; }

        [Display(Name = "Opened On")]
        [DataType(DataType.Date)]
        public DateTime OpenDate { get; set; } = DateTime.Now;

        [Display(Name = "Status")]
        [Required]
        public AccountStatus Status { get; set; }

        // Foreign key to Client
        [Required]
        [ForeignKey("Client")]
        public int ClientId { get; set; }
        [ValidateNever]
        public Client Client { get; set; }

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
