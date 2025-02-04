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
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var notifications = await _notificationService.GetUserNotificationsAsync(userId);
        return View(notifications);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        await _notificationService.MarkNotificationAsReadAsync(notificationId);
        return RedirectToAction("UserNotifications");
    }
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> UserNotifications()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var notifications = await _notificationService.GetUserNotificationsAsync(userId);
        return View(notifications);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateNotification(string userId, string message)
    {
        await _notificationService.AddNotificationAsync(userId, message);
        return RedirectToAction("Index");
    }
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> UnreadNotificationCount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
        return Json(count); 
    }

}