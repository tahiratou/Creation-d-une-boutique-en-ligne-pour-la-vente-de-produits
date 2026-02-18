using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BoutiqueEnLigne.Attributes
{
    public class RoleAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            var userRole = context.HttpContext.Session.GetString("UserRole");

            if (userId == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (!_roles.Contains(userRole))
            {
                context.HttpContext.Response.StatusCode = 403;
                context.Result = new ContentResult
                {
                    Content = "Accès refusé. Vous n'avez pas les permissions nécessaires."
                };
            }
        }
    }
}