namespace Yazlab_2.Models.EntityBase
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Mesaj
    {
        [Key]
        public int MesajID { get; set; }

        [ForeignKey("Gonderici")]
        public string GondericiID { get; set; }  // GondericiID'yi string olarak değiştirin
        public User Gonderici { get; set; }

        [ForeignKey("Alici")]
        public string AliciID { get; set; }  // AliciID'yi string olarak değiştirin
        public User Alici { get; set; }

        [Required]
        public string MesajMetni { get; set; }

        public DateTime GonderimZamani { get; set; } = DateTime.Now;
    }
}
