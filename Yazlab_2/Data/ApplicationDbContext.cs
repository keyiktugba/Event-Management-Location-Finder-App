/*using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Yazlab_2.Models.EntityBase;

namespace Yazlab_2.Data
{
    public class ApplicationDbContext : IdentityDbContext<User> // IdentityDbContext kullanılarak kullanıcı yönetimi sağlanır
    {
        // Constructor
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> users { get; set; }

        // DbSet'ler: Tablolar için DbSet'leri burada tanımlıyoruz
        public DbSet<Etkinlik> Etkinlikler { get; set; }
        public DbSet<Katilimci> Katilimcilar { get; set; }
        public DbSet<Mesaj> Mesajlar { get; set; }
        public DbSet<Puan> Puans { get; set; }

        // Fluent API ile ilişkiler ve yapılandırmalar (isteğe bağlı)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Identity özelliklerini ekler

            

            // Katilimci tablosu için birleşik anahtar ilişkisi
            modelBuilder.Entity<Katilimci>()
                .HasKey(k => new { k.KullaniciID, k.EtkinlikID });

            modelBuilder.Entity<Katilimci>()
                .HasOne(k => k.Kullanici)
                .WithMany()
                .HasForeignKey(k => k.KullaniciID)
                .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silindiğinde ilişkili katılımı da siler

            modelBuilder.Entity<Katilimci>()
                .HasOne(k => k.Etkinlik)
                .WithMany()
                .HasForeignKey(k => k.EtkinlikID)
                .OnDelete(DeleteBehavior.Cascade);

            // Mesaj tablosunda yabancı anahtar ilişkisi
            modelBuilder.Entity<Mesaj>()
                .HasOne(m => m.Gonderici)
                .WithMany()
                .HasForeignKey(m => m.GondericiID)
                .OnDelete(DeleteBehavior.Restrict); // Gönderici silindiğinde mesaj silinmesin

            modelBuilder.Entity<Mesaj>()
                .HasOne(m => m.Etkinlik)
                .WithMany()
                .HasForeignKey(m => m.EtkinlikID)
                .OnDelete(DeleteBehavior.Restrict); // Alıcı silindiğinde mesaj silinmesin

            // Puan tablosunda yabancı anahtar ilişkisi
            modelBuilder.Entity<Puan>()
                .HasKey(p => new { p.KullaniciID, p.KazanilanTarih }); // Bir kullanıcı bir tarihte birden fazla puan kazanabilir

            modelBuilder.Entity<Puan>()
                .HasOne(p => p.Kullanici)
                .WithMany()
                .HasForeignKey(p => p.KullaniciID)
                .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silindiğinde puanları da siler
        }
    }
}
*/using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Yazlab_2.Models.EntityBase;

namespace Yazlab_2.Data
{
    public class ApplicationDbContext : IdentityDbContext<User> // IdentityDbContext ile kullanıcı yönetimi sağlar
    {
        // Constructor
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet'ler: Tablolar için DbSet'leri burada tanımlıyoruz
        public DbSet<User> Users { get; set; }
        public DbSet<Etkinlik> Etkinlikler { get; set; }
        public DbSet<Katilimci> Katilimcilar { get; set; }
        public DbSet<Mesaj> Mesajlar { get; set; }
        public DbSet<Puan> Puanlar { get; set; }
        public DbSet<Kategori> Kategoriler { get; set; }
        public DbSet<Interest> Ilgiler { get; set; }

        // Fluent API ile ilişkiler ve yapılandırmalar
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Identity özelliklerini ekler
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
);                         // Katilimci tablosu için birleşik anahtar ve ilişkiler
            modelBuilder.Entity<Katilimci>()
                .HasKey(k => new { k.KullaniciID, k.EtkinlikID });

            modelBuilder.Entity<Katilimci>()
                .HasOne(k => k.Kullanici)
                .WithMany()
                .HasForeignKey(k => k.KullaniciID)
                .OnDelete(DeleteBehavior.Restrict); // Cascade yerine Restrict kullanıldı

            modelBuilder.Entity<Katilimci>()
                .HasOne(k => k.Etkinlik)
                .WithMany()
                .HasForeignKey(k => k.EtkinlikID)
                .OnDelete(DeleteBehavior.Restrict); // Cascade yerine Restrict kullanıldı


            // Mesaj tablosu ilişkileri
            modelBuilder.Entity<Mesaj>()
                .HasOne(m => m.Gonderici)
                .WithMany()
                .HasForeignKey(m => m.GondericiID)
                .OnDelete(DeleteBehavior.Restrict); // Gönderici silindiğinde mesaj silinmez

            modelBuilder.Entity<Mesaj>()
                .HasOne(m => m.Etkinlik)
                .WithMany()
                .HasForeignKey(m => m.EtkinlikID)
                .OnDelete(DeleteBehavior.Restrict); // Etkinlik silindiğinde mesaj silinmez

            // Puan tablosu için birleşik anahtar ve ilişkiler
            modelBuilder.Entity<Puan>()
                .HasKey(p => new { p.KullaniciID, p.KazanilanTarih }); // Bir kullanıcı bir tarihte birden fazla puan kazanabilir

            modelBuilder.Entity<Puan>()
                .HasOne(p => p.Kullanici)
                .WithMany()
                .HasForeignKey(p => p.KullaniciID)
                .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silindiğinde ilişkili puanları da siler

            // Interest tablosu için birleşik anahtar ve ilişkiler
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
