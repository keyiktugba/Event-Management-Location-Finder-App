using Yazlab_2.Models.EntityBase;

namespace Yazlab_2.Models.ViewModel
{
    public class KategoriViewModel
    {
        public Kategori Kategori { get; set; } // Ekleme/Güncelleme için kullanılacak
        public List<Kategori> Kategoriler { get; set; } // Listeleme için kullanılacak
    }

}
