using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace muhaberat_evrak_yonetim.Controllers;

public class BaseController : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Check if user is logged in (except for AccountController actions)
        if (!IsUserLoggedIn() && !IsPublicAction(context))
        {
            context.Result = RedirectToAction("Login", "Account");
            return;
        }

        // Set current user info in ViewBag for all authenticated requests
        if (IsUserLoggedIn())
        {
            ViewBag.CurrentUserId = GetCurrentUserId();
            ViewBag.CurrentUserInfo = GetCurrentUserInfo();
        }

        base.OnActionExecuting(context);
    }

    protected bool IsUserLoggedIn()
    {
        return HttpContext.Session.GetInt32("UserId").HasValue;
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
        return HttpContext.Session.GetString("HasFullAccess") == "true";
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

        // Allow access to Account controller and specific public actions
        if (controllerName?.Equals("Account", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        // Allow access to Error action
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