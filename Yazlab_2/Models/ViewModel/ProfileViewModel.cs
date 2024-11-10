using Yazlab_2.Models.EntityBase;

namespace Yazlab_2.Models.ViewModel
{
    public class ProfileViewModel
    {
        public User User { get; set; }    // Kullanıcı bilgileri
        public List<Etkinlik> Events { get; set; } // Kullanıcıya ait etkinlikler
    }

}
