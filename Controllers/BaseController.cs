using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace muhaberat_evrak_yonetim.Controllers;

public class BaseController : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {

        if (!IsUserLoggedIn() && !IsPublicAction(context))
        {
            context.Result = RedirectToAction("Login", "Account");
            return;
        }

        if (IsUserLoggedIn())
        {
            ViewBag.CurrentUserId = GetCurrentUserId();
            ViewBag.CurrentUserInfo = GetCurrentUserInfo();
            ViewBag.CurrentUserFullName = HttpContext.Session.GetString("UserFullName");
            ViewBag.CurrentUserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.CurrentDepartmentName = HttpContext.Session.GetString("DepartmentName");
            ViewBag.CurrentRoleName = HttpContext.Session.GetString("RoleName");
        }

        base.OnActionExecuting(context);
    }

    protected bool IsUserLoggedIn()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
            return false;

        // Check session expiry
        var lastActivity = HttpContext.Session.GetString("LastActivity");
        if (!string.IsNullOrEmpty(lastActivity))
        {
            if (DateTime.TryParse(lastActivity, out DateTime lastActiveTime))
            {
                // Session timeout after 30 minutes of inactivity
                if (DateTime.Now.Subtract(lastActiveTime).TotalMinutes > 30)
                {
                    HttpContext.Session.Clear();
                    return false;
                }
            }
        }

        // Update last activity
        HttpContext.Session.SetString("LastActivity", DateTime.Now.ToString());
        return true;
    }

    protected int? GetCurrentUserId()
    {
        return HttpContext.Session.GetInt32("UserId");
    }

    protected int GetCurrentUserIdRequired()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("Kullanıcı oturumu gereklidir.");
        }
        return userId.Value;
    }

    protected string? GetCurrentUserInfo()
    {
        var userName = HttpContext.Session.GetString("UserName");
        var departmentName = HttpContext.Session.GetString("DepartmentName");
        var roleName = HttpContext.Session.GetString("RoleName");
        
        if (!string.IsNullOrEmpty(userName))
        {
            return $"{userName} - {departmentName} ({roleName})";
        }
        return null;
    }

    protected bool HasFullAccess()
    {
        var hasFullAccessStr = HttpContext.Session.GetString("HasFullAccess");
        return bool.TryParse(hasFullAccessStr, out bool hasFullAccess) && hasFullAccess;
    }

    protected string? GetCurrentUserDepartment()
    {
        return HttpContext.Session.GetString("DepartmentName");
    }

    protected int? GetCurrentUserDepartmentId()
    {
        return HttpContext.Session.GetInt32("DepartmentId");
    }

    protected string? GetCurrentUserRole()
    {
        return HttpContext.Session.GetString("RoleName");
    }

    protected int? GetCurrentUserRoleId()
    {
        return HttpContext.Session.GetInt32("RoleId");
    }

    private bool IsPublicAction(ActionExecutingContext context)
    {
        var controllerName = context.RouteData.Values["controller"]?.ToString();
        var actionName = context.RouteData.Values["action"]?.ToString();

        if (controllerName?.Equals("Account", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        if (actionName?.Equals("Error", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        return false;
    }

    protected IActionResult RedirectToUnauthorized()
    {
        return RedirectToAction("AccessDenied", "Account");
    }

    protected void SetSuccessMessage(string message)
    {
        TempData["Success"] = message;
    }

    protected void SetErrorMessage(string message)
    {
        TempData["Error"] = message;
    }
}