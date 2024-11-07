namespace Yazlab_2.Models.EntityBase
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Puan
    {
        [Key, Column(Order = 0)]
        [ForeignKey("Kullanici")]
        public string KullaniciID { get; set; }
        public User Kullanici { get; set; }

        public int Puanlar { get; set; }

        [Key, Column(Order = 1)]
        public DateTime KazanilanTarih { get; set; } = DateTime.Now;
    }

}
