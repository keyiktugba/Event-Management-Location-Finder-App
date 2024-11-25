using Yazlab_2.Models.EntityBase;

namespace Yazlab_2.Models.ViewModel
{
    public class ProfileViewModel
    {
        public User User { get; set; }    // Kullanıcı bilgileri
        public List<Etkinlik> Events { get; set; } // Kullanıcıya ait etkinlikler
        public List<Interest> Interests { get; set; } // Kullanıcıya ait ilgi alanları
        public List<int> SelectedInterests { get; set; }  // Seçilen ilgi alanlarının ID'leri
        public List<Kategori> Categories { get; set; }
        public IFormFile ProfilePicture { get; set; } // Profil fotoğrafı
    }

}
