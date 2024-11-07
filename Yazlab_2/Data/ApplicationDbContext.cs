using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
                .HasOne(m => m.Alici)
                .WithMany()
                .HasForeignKey(m => m.AliciID)
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
