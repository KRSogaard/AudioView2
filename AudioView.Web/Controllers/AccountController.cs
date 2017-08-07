using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AudioView.Common.Data;
using AudioView.Common.Services;
using AudioView.Web.Filters;
using AudioView.Web.Models;
using AudioView.Web.Tools;

namespace AudioView.Web.Controllers
{
    public class AccountController : Controller
    {
        public const string SessionName = "AudioViewUserId";
        private IUserService userService;

        public AccountController()
        {
            userService = new UserService();
        }

        [AuthFilter]
        public async Task<ActionResult> Index()
        {
            var currentUserName = HttpContext.Session[SessionName];
            if (currentUserName == null)
            {
                return new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "controller", "Account" },
                    { "action", "LogIn" }
                });
            }
            var currentUser = await userService.GetUser((string)currentUserName);
            if (currentUser == null)
            {
                return new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "controller", "Account" },
                    { "action", "LogIn" }
                });
            }
            if (currentUser.Expires != null)
            {
                FlashHelper.Add("A limited account dose not have access to manage user.", FlashType.Error);
                return new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "controller", "Home" },
                    { "action", "Index" }
                });
            }

            var users = await userService.GetUsers();
            return View(users);
        }

        public ActionResult LogIn()
        {
            return View(new LogOnModel());
        }

        [HttpPost]
        public async Task<ActionResult> LogIn(LogOnModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userService.Validate(model.UserName, model.Password);
                if (user != null)
                {
                    if (user.Expires != null && user.Expires <= DateTime.Now)
                    {
                        FlashHelper.Add("Account has expired.", FlashType.Error);
                        return new RedirectToRouteResult(new RouteValueDictionary
                        {
                            { "controller", "Home" },
                            { "action", "Index" }
                        });
                    }

                    HttpContext.Session.Add(SessionName, user.UserName);
                    return new RedirectToRouteResult(new RouteValueDictionary
                    {
                        { "controller", "Home" },
                        { "action", "Index" }
                    });
                }
                FlashHelper.Add("Invalid login information.", FlashType.Error);
            }
            return View(model);
        }

        [AuthFilter]
        public ActionResult Create()
        {
            return View(new CreateModel());
        }

        [HttpPost]
        [AuthFilter]
        public async Task<ActionResult> Create(CreateModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userService.GetUser(model.UserName);
                if (user != null)
                {
                    FlashHelper.Add(string.Format("User with the username {0} aready exists.", model.UserName), FlashType.Error);
                    return new RedirectToRouteResult(new RouteValueDictionary(){
                        { "controller", "Account" },
                        { "action", "Index" }
                    });
                }
                
                var userObject = new User()
                {
                    Id = Guid.NewGuid(),
                    UserName = model.UserName
                };
                if (model.Expires != null)
                {
                    DateTime? expires = null;
                    DateTime tryDate;
                    if (DateTime.TryParseExact(model.Expires.Trim() + " 00:00:00", "dd/MM/yyyy hh:mm:ss",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal, out tryDate))
                    {
                        expires = tryDate;
                    }
                    else
                    {
                        FlashHelper.Add(
                            string.Format("\"{0}\" was not reconiced as a date of the format dd/mm/yyyy.", model.Expires),
                            FlashType.Error);
                        return new RedirectToRouteResult(new RouteValueDictionary(){
                            { "controller", "Account" },
                            { "action", "Index" }
                        });
                    }
                    userObject.Expires = expires;
                }
                await userService.CreateUser(userObject, model.Password);

                FlashHelper.Add(string.Format("User with the username {0} have been created.", model.UserName), FlashType.Success);
                return new RedirectToRouteResult(new RouteValueDictionary(){
                        { "controller", "Account" },
                        { "action", "Index" }
                    });
            }
            return View(model);
        }

        [AuthFilter]
        public async Task<ActionResult> Edit(string username)
        {
            var user = await userService.GetUser(username);
            if (user == null)
            {
                FlashHelper.Add(string.Format("{0} did not exist.", username), FlashType.Notice);
                return new RedirectToRouteResult(new RouteValueDictionary(){
                        { "controller", "Account" },
                        { "action", "Index" }
                    });
            }
            var model = new EditModel()
            {
                UserName = user.UserName,
                Expires = user.Expires?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? ""
            };
            return View(model);
        }

        [HttpPost]
        [AuthFilter]
        public async Task<ActionResult> Edit(string username, EditModel model)
        {
            if (ModelState.IsValid)
            {
                DateTime? expires = null;
                if (model.Expires != null)
                {
                    DateTime tryDate;
                    if (DateTime.TryParseExact(model.Expires.Trim() + " 00:00:00", "dd/MM/yyyy hh:mm:ss",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal, out tryDate))
                    {
                        expires = tryDate;
                    }
                    else
                    {
                        FlashHelper.Add(
                            string.Format("\"{0}\" was not reconiced as a date of the format dd/mm/yyyy.", model.Expires),
                            FlashType.Error);
                        return new RedirectToRouteResult(new RouteValueDictionary(){
                            { "controller", "Account" },
                            { "action", "Index" }
                        });
                    }
                }

                if (!String.IsNullOrWhiteSpace(model.Password) && !String.IsNullOrWhiteSpace(model.PasswordRepeat) &&
                    model.Password == model.PasswordRepeat)
                {
                    await userService.UpdatePassword(username, model.Password);
                }
                await userService.UpdateExpires(username, expires);
                FlashHelper.Add(string.Format("{0}'s password have been changed.", model.UserName), FlashType.Success);
                return new RedirectToRouteResult(new RouteValueDictionary(){
                        { "controller", "Account" },
                        { "action", "Index" }
                    });
            }
            return View(model);
        }

        [AuthFilter]
        public async Task<ActionResult> Delete(string username)
        {
            var user = await userService.GetUser(username);
            if (user == null)
            {
                FlashHelper.Add(string.Format("{0} did not exist.", username), FlashType.Notice);
                return new RedirectToRouteResult(new RouteValueDictionary(){
                        { "controller", "Account" },
                        { "action", "Index" }
                    });
            }

            return View("AreYouSureModel", new AreYouSureModel()
            {
                Message = string.Format("Are you sure you want to delete {0}?", username),
                RedirectAction = "Index",
                Redirect = new RouteValueDictionary()
            });
        }

        [HttpPost]
        [AuthFilter]
        public async Task<ActionResult> Delete(string username, AreYouSureModel model)
        {
            await userService.DeleteUser(username);
            FlashHelper.Add(string.Format("{0} have been deleted.", username), FlashType.Notice);
            return new RedirectToRouteResult(new RouteValueDictionary(){
                        { "controller", "Account" },
                        { "action", "Index" }
                    });
        }
    }
}