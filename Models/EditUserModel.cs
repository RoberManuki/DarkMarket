using System;
using System.ComponentModel.DataAnnotations;

namespace DarkMarket.Models.Admin
{
    public class EditUserModel
    {
        [Display(Name = "Nome Completo")]
        public string? FullName { get; set; }

        [Display(Name = "Data de Nascimento")]
        public DateTime? BirthDate { get; set; }
    }
}