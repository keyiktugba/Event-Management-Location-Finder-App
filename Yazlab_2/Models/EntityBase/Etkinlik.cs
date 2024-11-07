namespace Yazlab_2.Models.EntityBase
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Etkinlik
    {
        public int ID { get; set; }
        public int CategoryID { get; set; }
        public string UserID { get; set; }

        [Required]
        public string EtkinlikAdi { get; set; }

        public string Aciklama { get; set; }

        [Required]
        public DateTime Tarih { get; set; }

        [Required]
        public TimeSpan Saat { get; set; }

        public int EtkinlikSuresi { get; set; }

        [Required]
        public string Konum { get; set; }

        [ForeignKey("CategoryID")]
        public Kategori Category { get; set; }

        [ForeignKey("UserID")]
        public User User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }


}
