using System.ComponentModel.DataAnnotations;
using MaProgramez.Resources;
using System;

namespace MaProgramez.Repository.Models
{
    public class UserModel
    {
        public string UserId { get; set; }

        [Required(ErrorMessage = @"{0} trebuie completat")]
        [Display(Name = @"Nume utilizator")]
        public string UserName { get; set; }

        [Required(ErrorMessage = @"{0} trebuie completata")]
        [StringLength(100, ErrorMessage = @"{0} trebuie sa contina minim {2} caractere.", MinimumLength = 6)]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = @"Parola")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = @"Confirma parola")]
        [Compare("Password", ErrorMessage = @"Parola si confirmarea parolei nu se potrivesc.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = @"{0} trebuie completat")]
        [Display(Name = @"Prenume")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = @"{0} trebuie completat")]
        [Display(Name = @"Nume")]
        public string LastName { get; set; }

        [Required(ErrorMessage = @"{0} trebuie completat")]
        [EmailAddress(ErrorMessage = @"Adresa de mail incorecta")]
        [Display(Name = @"Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = @"{0} trebuie completat")]
        [Display(Name = @"Telefon mobil")]
        public string Phone { get; set; }
    }
}