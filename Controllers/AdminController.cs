using Film_website.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Film_website.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserService _userService;
        private readonly UserActivityService? _activityService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(UserService userService, ILogger<AdminController> logger, UserActivityService? activityService = null)
        {
            _userService = userService;
            _logger = logger;
            _activityService = activityService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Log admin access if activity service is available
                if (_activityService != null && User.Identity?.Name != null)
                {
                    var adminUser = await _userService.GetUserByEmailOrUserNameAsync(User.Identity.Name);
                    if (adminUser != null)
                    {
                        await _activityService.LogAdminAccessAsync(adminUser.Id, "Accessed Admin Index", HttpContext);
                    }
                }

                // Get all users with their roles for user management section
                var usersWithRoles = await _userService.GetAllUsersWithRolesDictionaryAsync();

                ViewBag.Message = "Chào mừng đến trang quản trị";
                ViewBag.TotalUsers = usersWithRoles.Count;

                return View(usersWithRoles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách người dùng");
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi tải danh sách người dùng.";
                ViewBag.Message = "Chào mừng đến trang quản trị";
                return View(new Dictionary<Film_website.Models.User, IList<string>>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserActivities(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("User ID is required");
                }

                // If activity service is not available, return empty result
                if (_activityService == null)
                {
                    return Json(new
                    {
                        user = new { id = userId, name = "Unknown", username = "unknown", email = "unknown" },
                        activities = new object[0],
                        totalCount = 0,
                        message = "Activity tracking is not yet configured"
                    });
                }

                // Log admin action
                if (User.Identity?.Name != null)
                {
                    var adminUser = await _userService.GetUserByEmailOrUserNameAsync(User.Identity.Name);
                    if (adminUser != null)
                    {
                        await _activityService.LogAdminAccessAsync(adminUser.Id, $"Viewed activity log for user {userId}", HttpContext);
                    }
                }

                var activities = await _activityService.GetRecentActivitiesAsync(userId, 50);
                var totalCount = await _activityService.GetTotalUserActivitiesCountAsync(userId);

                // Get user info for display
                var user = await _userService.GetUserByEmailOrUserNameAsync(userId);
                if (user == null)
                {
                    // Try getting by ID directly
                    var allUsers = await _userService.GetAllUsersAsync();
                    user = allUsers.FirstOrDefault(u => u.Id == userId);
                }

                var response = new
                {
                    user = user != null ? new
                    {
                        id = user.Id,
                        name = $"{user.FirstName} {user.LastName}",
                        username = user.DisplayUserName,
                        email = user.Email
                    } : null,
                    activities = activities.Select(a => new {
                        id = a.Id,
                        activityType = a.ActivityType,
                        description = a.Description,
                        createdAt = a.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                        ipAddress = a.IpAddress,
                        location = a.Location ?? "Unknown"
                    }),
                    totalCount = totalCount
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving activities for user {userId}");
                return StatusCode(500, "Error retrieving user activities");
            }
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Translator()
        {
            ViewData["Title"] = "AI Translator - CineHub Admin";
            return View();
        }
    }
}