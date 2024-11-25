using System.ComponentModel.DataAnnotations.Schema;

namespace Yazlab_2.Models.EntityBase
{
    public class Notification
    {
        public int Id { get; set; }

        // ForeignKey olarak kullanıcıyı ilişkilendiriyoruz
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }  // Kullanıcıyı ilişkilendiriyoruz

        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
