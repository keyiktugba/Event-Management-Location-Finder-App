using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Yazlab_2.Models.EntityBase;

public class NotificationController : Controller
{
    private readonly NotificationService _notificationService;

    public NotificationController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // Kullanıcıya bildirimleri göster
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var notifications = await _notificationService.GetUserNotificationsAsync(userId);
        return View(notifications);
    }

    // Bildirimi "okunmuş" olarak işaretle
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        await _notificationService.MarkNotificationAsReadAsync(notificationId);
        return RedirectToAction("Index");
    }

    // Yeni bir bildirim ekle (örneğin admin veya sistem tarafından)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateNotification(string userId, string message)
    {
        await _notificationService.AddNotificationAsync(userId, message);
        return RedirectToAction("Index");
    }
}
