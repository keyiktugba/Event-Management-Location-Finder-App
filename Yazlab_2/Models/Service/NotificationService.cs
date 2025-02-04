using Microsoft.EntityFrameworkCore;
using Yazlab_2.Data;
using Yazlab_2.Models.EntityBase;
public class NotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddNotificationAsync(string userId, string message)
    {
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            DateCreated = DateTime.Now,
            IsRead = false
        };

        _context.Notifications.Add(notification);  
        await _context.SaveChangesAsync();

    }

    public async Task<List<Notification>> GetUserNotificationsAsync(string userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.DateCreated)
            .ToListAsync();
    }
    public async Task AddAdminNotificationAsync(string message)
    {
        
        var adminUser = await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == "Admins");

        if (adminUser == null)
        {
            throw new Exception("Admin kullanıcısı bulunamadı.");
        }

        var notification = new Notification
        {
            UserId = adminUser.Id, 
            Message = message,
            DateCreated = DateTime.Now,
            IsRead = false
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }
    public async Task MarkNotificationAsReadAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }


    public void AddPoints(string kullaniciId, int points)
    {
     
        var yeniPuan = new Puan
        {
            KullaniciID = kullaniciId,
            Puanlar = points,
            KazanilanTarih = DateTime.Now
        };

        _context.Puanlar.Add(yeniPuan);
        _context.SaveChanges();

        var notificationService = new NotificationService(_context);

        var notificationMessage = $"{points} puan kazandınız! Yeni toplam puanlarınızı kontrol edin.";

        var notification = new Notification
        {
            UserId = kullaniciId,
            Message = notificationMessage,
            DateCreated = DateTime.Now,
            IsRead = false
        };

        _context.Notifications.Add(notification);
        _context.SaveChanges();

        notification.IsRead = true;
        _context.SaveChanges();
    }
    public async Task<int> GetUnreadNotificationCountAsync(string userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync();
    }


    public async Task NotifyEventCreatorAsync(int eventId, string message)
    {
        var eventDetails = _context.Etkinlikler
            .Where(e => e.ID == eventId)
            .Select(e => new { e.User.Id, e.EtkinlikAdi })
            .FirstOrDefault();

        if (eventDetails != null)
        {
            var notificationMessage = $"'{eventDetails.EtkinlikAdi} adlı etkinlik admin tarafından cevaplandı. Detaylar: {message}";

        
            var notification = new Notification
            {
                UserId = eventDetails.Id, 
                Message = notificationMessage,
                DateCreated = DateTime.Now,
                IsRead = false
            };

          
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }

    
    public async Task NotifyParticipantsAsync(int eventId, string senderId, string message)
    {
        
        var participants = _context.Katilimcilar
            .Where(k => k.EtkinlikID == eventId)
            .Select(k => k.KullaniciID)
            .ToList();

       
        var senderName = _context.Users
            .Where(u => u.Id == senderId)
            .Select(u => u.UserName)
            .FirstOrDefault();

        var eventName = _context.Etkinlikler
            .Where(e => e.ID == eventId)
            .Select(e => e.EtkinlikAdi)
            .FirstOrDefault();

        if (!string.IsNullOrEmpty(senderName) && !string.IsNullOrEmpty(eventName))
        {
           
            var notificationMessage = $"'{eventName}' etkinliğinde {senderName} adlı kullanıcı yeni bir mesaj gönderdi: {message}";

   
            var notifications = participants.Select(participantId => new Notification
            {
                UserId = participantId,
                Message = notificationMessage,
                DateCreated = DateTime.Now,
                IsRead = false
            }).ToList();

            
            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();
        }

    }


}