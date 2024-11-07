using System.ComponentModel.DataAnnotations;

namespace Yazlab_2.Models.EntityBase
{
    public class Kategori
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        public string CategoryName { get; set; }
    }
}
