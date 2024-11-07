namespace Yazlab_2.Models.EntityBase
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Etkinlik
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string EtkinlikAdi { get; set; }

        public string Aciklama { get; set; }

        [Required]
        public DateTime Tarih { get; set; }

        [Required]
        public TimeSpan Saat { get; set; }

        public int EtkinlikSuresi { get; set; } // dakika cinsinden

        [Required]
        public string Konum { get; set; }

        public string Kategori { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
