namespace Yazlab_2.Models.ViewModel
    {
        public class AdminDashboardViewModel
        {
            public int TotalUsers { get; set; }
            public int TotalEvents { get; set; }
            public int WeeklyEvents { get; set; }
            public Dictionary<int, int> AgeDistribution { get; set; } // Yaş grubu dağılımı
            public int MaleUsers { get; set; }
            public int FemaleUsers { get; set; }
        public List<string> WeeklyEventDates { get; set; } // Haftalık etkinlik günleri
        public List<int> WeeklyEventCounts { get; set; } // Her gün için etkinlik sayısı
    }
    }



