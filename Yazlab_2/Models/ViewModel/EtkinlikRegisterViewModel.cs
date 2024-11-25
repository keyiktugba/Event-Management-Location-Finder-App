using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Yazlab_2.Models.ViewModel
{
    public class EtkinlikRegisterViewModel
    {
        public int EtkinlikID { get; set; }
        [Required]
        public string EtkinlikAdi { get; set; }

        public string Aciklama { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan Saat { get; set; }

        public int EtkinlikSuresi { get; set; }

        [Required]
        public string Konum { get; set; }

        [Required]
        public int CategoryID { get; set; }

        public string UserID { get; set; }

    
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
