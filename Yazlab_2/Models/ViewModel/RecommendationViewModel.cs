using Yazlab_2.Models.EntityBase;

namespace Yazlab_2.Models.ViewModel
{
    public class RecommendationViewModel
    {
        public List<Etkinlik> InterestBased { get; set; }
        public List<Etkinlik> PastEventBased { get; set; }
        public List<Etkinlik> LocationBased { get; set; }
    }

}
