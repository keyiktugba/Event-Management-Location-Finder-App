using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Yazlab_2.Models.EntityBase
{
    public class Interest
    {
        [Key]
        public int InterestID { get; set; }

        [Key, Column(Order = 0)]
        [ForeignKey("User")]
        public string ID { get; set; }
        public virtual User User { get; set; }

        [Key, Column(Order = 1)]
        [ForeignKey("Category")]
        public int CategoryID { get; set; }
        public virtual Kategori Category { get; set; }
    }
}
