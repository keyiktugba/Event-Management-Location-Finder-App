namespace Yazlab_2.Models.EntityBase
{
    using Microsoft.EntityFrameworkCore;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Katilimci
    {
        [Key, Column(Order = 0)]
        [ForeignKey("Kullanici")]
        public string KullaniciID { get; set; } // int olarak değiştirdik
        public User Kullanici { get; set; }

        [Key, Column(Order = 1)]
        [ForeignKey("Etkinlik")]
        public int EtkinlikID { get; set; }
        public Etkinlik Etkinlik { get; set; }
      

    }
}
