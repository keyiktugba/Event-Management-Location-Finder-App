using System.ComponentModel.DataAnnotations;

namespace Yazlab_2.Models.ViewModel
{
    public class UserLoginViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
