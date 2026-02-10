using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Syncfusion.EJ2.Navigations;

namespace LancasterCreditCardDiversion.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var loginUrl = "~/Auth/Login";
            base.OnActionExecuting(context);

            var currentPath = context.HttpContext.Request.Path.Value;

            if (HttpContext.Session.IsAvailable)
            {
                if (HttpContext.Session.GetString("Username") == null &&
                (context.HttpContext.Request.Path.Value == null ||
                !context.HttpContext.Request.Path.Value.Contains(loginUrl)))
                {
                    context.Result = new RedirectResult(loginUrl);
                }
                else
                {
                    var currentCaseId = HttpContext.Session.GetString("CurrentCaseId");

                    var caseRequiredActions = new[]
                    {
                        "MergeTemplate", "ListDocuments", "ListRequests", "ListComments", "CaseActivityLog"
                    };

                    var currentAction = context.RouteData.Values["action"]?.ToString();

                    if (caseRequiredActions.Contains(currentAction) && string.IsNullOrEmpty(currentCaseId))
                    {
                        TempData["Message"] = "Please select a case first to access this feature.";
                        TempData["MessageType"] = "error";
                        context.Result = new RedirectToActionResult("Index", "Cases", null);
                        return;
                    }

                    var menuItems = new List<MenuItem>
                    {
                        new MenuItem { Text = "Case Search", Url = Url.Action("AllCasesSearch", "Cases"), IconCss = "e-icons e-search" },
                        new MenuItem { Text = "Cases", Url = Url.Action("Index", "Cases"), IconCss = "e-icons e-agenda-date-range" },
                        new MenuItem { Text = "Conciliation Management", Url = Url.Action("ConciliationManagement", "Cases"), IconCss = "e-icons e-description" },
                        new MenuItem { Text = "Templates", Url = Url.Action("ListTemplates", "LetterTemplates"), IconCss = "e-icons e-list-unordered" },
                        new MenuItem { Text = "Manage Hearing Dates", Url = Url.Action("ListHearingDates", "HearingDates"), IconCss = "e-icons e-agenda-date-range" },
                        new MenuItem { Text = "Users", Url = Url.Action("ListUsers", "Users"), IconCss = "e-icons e-user" },
                        new MenuItem { Text = "Help", Url = Url.Action("Help", "Cases"), IconCss = "e-icons e-circle-info" },
                    };

                    ViewBag.mainMenuItems = menuItems;
                    ViewBag.CurrentPath = currentPath;
                    ViewBag.headerText0 = new TabHeader { Text = "Case" };
                    ViewBag.headerText1 = new TabHeader { Text = "Merge Letters" };
                    ViewBag.headerText2 = new TabHeader { Text = "Case Documents" };
                    ViewBag.headerText3 = new TabHeader { Text = "Eligibility Requests" };
                    ViewBag.headerText4 = new TabHeader { Text = "Case Comments" };
                    ViewBag.headerText5 = new TabHeader { Text = "Activity Log" };
                }
            }
            else
            {
                TempData["SessionExpiredMessage"] = "Your session has expired due to inactivity. Please log in again.";
                context.Result = new RedirectResult(loginUrl);
            }
        }
    }
}