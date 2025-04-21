using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using BankApp.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BankApp.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [BindNever]
        [Display(Name = "Transaction Number")]
        public string? TransactionNumber { get; set; }

        [Display(Name = "Transaction Date")]
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Display(Name = "Amount")]
        [Required(ErrorMessage = "Transaction amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Transaction Type is required.")]
        [Display(Name = "Transaction Type")]
        public TransactionType TransactionType { get; set; }

        [Display(Name = "Description")]
        [StringLength(200)]
        public string? Description { get; set; }

        // Foreign key to Account
        [Required]
        public int AccountId { get; set; }

        [ValidateNever]
        public Account Account { get; set; }
    }
}
