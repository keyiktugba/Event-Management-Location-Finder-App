using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Yazlab_2.Models.EntityBase
{
    public class User : IdentityUser
    {
       

       // [Required]
     //   public string Username { get; set; }

       // [Required]
       // [DataType(DataType.Password)]
      //  public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

       // [Required]
       // [DataType(DataType.EmailAddress)]
       // public string Email { get; set; }

        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        public string Gender { get; set; }

        public string PhoneNumber { get; set; }

        public string Interests { get; set; }

        public string ProfilePicture { get; set; }

    
    }


}
