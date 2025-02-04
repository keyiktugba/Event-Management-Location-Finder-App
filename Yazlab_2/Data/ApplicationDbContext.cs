using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Yazlab_2.Models.EntityBase;

namespace Yazlab_2.Data
{
    public class ApplicationDbContext : IdentityDbContext<User> 
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Etkinlik> Etkinlikler { get; set; }
        public DbSet<Katilimci> Katilimcilar { get; set; }
        public DbSet<Mesaj> Mesajlar { get; set; }
        public DbSet<Puan> Puanlar { get; set; }
        public DbSet<Kategori> Kategoriler { get; set; }
        public DbSet<Interest> Ilgiler { get; set; }
        public DbSet<Notification> Notifications { get; set; } 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 
            modelBuilder.Entity<Kategori>().HasData(
new Kategori { CategoryID = 1, CategoryName = "Futbol" },
new Kategori { CategoryID = 2, CategoryName = "Voleybol" },
new Kategori { CategoryID = 3, CategoryName = "Basketbol" },
new Kategori { CategoryID = 4, CategoryName = "Yemek" },
new Kategori { CategoryID = 5, CategoryName = "Konser" },
new Kategori { CategoryID = 6, CategoryName = "Sinema" },
new Kategori { CategoryID = 7, CategoryName = "Kitap" },
new Kategori { CategoryID = 8, CategoryName = "Tiyatro" },
new Kategori { CategoryID = 9, CategoryName = "Doğa Yürüyüşü" }
);                       
            modelBuilder.Entity<Katilimci>()
                .HasKey(k => new { k.KullaniciID, k.EtkinlikID });

            modelBuilder.Entity<Katilimci>()
                .HasOne(k => k.Kullanici)
                .WithMany()
                .HasForeignKey(k => k.KullaniciID)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<Katilimci>()
                .HasOne(k => k.Etkinlik)
                .WithMany()
                .HasForeignKey(k => k.EtkinlikID)
                .OnDelete(DeleteBehavior.Cascade); 


            modelBuilder.Entity<Mesaj>()
                .HasOne(m => m.Gonderici)
                .WithMany()
                .HasForeignKey(m => m.GondericiID)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Mesaj>()
             .HasOne(m => m.Etkinlik)
             .WithMany()
             .HasForeignKey(m => m.EtkinlikID)
             .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Puan>()
                .HasKey(p => new { p.KullaniciID, p.KazanilanTarih }); 

            modelBuilder.Entity<Puan>()
                .HasOne(p => p.Kullanici)
                .WithMany()
                .HasForeignKey(p => p.KullaniciID)
                .OnDelete(DeleteBehavior.Cascade);

      
            modelBuilder.Entity<Interest>()
                .HasKey(i => new { i.ID, i.CategoryID });

            modelBuilder.Entity<Interest>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.ID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Interest>()
                .HasOne(i => i.Category)
                .WithMany()
                .HasForeignKey(i => i.CategoryID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
