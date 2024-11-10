using Yazlab_2.Models.EntityBase;

namespace Yazlab_2.Models.ViewModel
{
   
        public class EventMessagesViewModel
        {
            public int EventId { get; set; }
            public string EventName { get; set; }
            public List<Mesaj> Messages { get; set; }
        }
    }

