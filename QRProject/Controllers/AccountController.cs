
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using QRProject.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using QRProject.Core;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using QRProject.helper;
using System.Text.RegularExpressions;
using QRProject.meli;
using System.Drawing;
using System.Drawing.Imaging;
using QRProject.zarinpal;
using QRCoder;
using PagedList.Mvc;
using PagedList;
using System.Globalization;





namespace QRProject.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        [RemoveSpaceFilter]

        //notfound ba meghdare "n" va "MissedPage" az tarafe ForgetPass va controller home miad
        //dg nemidunam felan az kojaha dg miad shayad az jaye dg ham biad!
        public ActionResult Login(string returnUrl, string notFound)

        {
            //if (Session["forgetPass"] != null)
            //{
            //    return RedirectToAction("smspage", "Home");
            //}

            if (notFound == "TimeOut")
            {
                ViewBag.notfound = "لطفا دقایقی دیگر تلاش کنید";
            }

            if (notFound == "n")
            {
                ViewBag.notfound = "نام کاربری وارد شده معتبر نیست!";
            }
         
            if (notFound == "missedPage")
            {
                ViewBag.notfound = "اطلاعات صفحه شما تکمیل نشده است. لطفا دوباره تلاش کنید ";
            }

            if (notFound == "s")
            {
                ViewBag.notfound = "در روند تغییر رمز مشکل امنیتی پیش آمده! لطفا دوباره تلاش کنید";
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            //logout
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            var response = Request["g-recaptcha-response"];
            string secretKey = "6LeRqhYaAAAAAIQmhOILS4TZcfaGqS0DRavW0dmO";
            //vase local:
            //6LeRqhYaAAAAAIQmhOILS4TZcfaGqS0DRavW0dmO

            //vase site:
            //6Lf3e8oUAAAAAP3U_1DViKoNfML9UTauUF4tU7kO

            var client = new WebClient();

            //baraye local adres url ra taghir bede be adrese site
            var result1 = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secretKey, response));
            var obj = JObject.Parse(result1);
            var status = (bool)obj.SelectToken("success");

            //if (status != true)
            //{
            //    return View(model);
            //}

            if (model.Email == null)
            {
                Random RndItem = new Random();
                model.Email = "QR" + Guid.NewGuid().ToString() + "@gmail.com";
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.PhoneNumber, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.USRN, Email = model.USRN };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }


                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");

            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Admins", "Account");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion





        /// <summary>

        ///////// QRproject Actions 

        /// </summary>







        //action vaset bedune view
        //agar login shod dar ebteda miad in safhe
        [Authorize]

        public ActionResult Admins(string e)
        {
            var userId = User.Identity.GetUserId();
            ApplicationDbContext cntxt = new ApplicationDbContext();
            UserManager<ApplicationUser> _usermgr = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(cntxt));
            if (e != null)
            {
                ViewBag.e = "شناسه قبلا ثبت شده است";
            }
            //agar super admin budd ebteda in safhe ra biavar
            if (_usermgr.IsInRole(userId, "SuperAdmin"))
            {
                var subjoblist = cntxt.Subjobs.ToList();
                ViewBag.subjoblist = subjoblist;
                return View("SuperAdminClient");
            }
            //agar user mamuli bud in safhe ra biavar
            if (_usermgr.IsInRole(userId, "User"))
            {
                var Page = cntxt.Client.Where(s => s.UserId == userId).FirstOrDefault();
                if (Page == null)
                {//safhe nadare hanuz

                    return View("UserClientWithOutPage");
                }
                //safhe dare
                ApplicationDbContext db = new ApplicationDbContext();
                var userid = db.Client.Where(d => d.UserId == userId).FirstOrDefault().Id;
                return RedirectToAction("Show", "Home", new { id = userid });

            }
            return HttpNotFound();

        }


        //register tavasote khode karbar
        [AllowAnonymous]
        public ActionResult Reg(string error)
        {
            if (error != null)
            {
                if (error=="D")
                {
                    ViewBag.error = "کاربر گرامی این شماره مجاز به ثبت ویترین نیست. توجه داشته باشید با هر شماره موبایل، تنها دو ویترین میتوانید ایجاد کنید. ";
                }
                else
                {
                    ViewBag.error = "متاسفانه مشکلی در سرور پیش آمده! لطفا مجددا سعی نمایید. توجه داشته باشید که نام کاربری و دامنه شما، قبلا در سیستم ثبت نشده باشد.";

                }

            }

            if (Session["Register"] != null)
            {
                return RedirectToAction("smsPage", "Account");
            }
            else
            {
                ApplicationDbContext db = new ApplicationDbContext();
                ViewBag.listOfJobs = db.Subjobs.ToList();
                return View();
            }
        }



        [AllowAnonymous]
        [HttpPost]
        public ActionResult mapload(string jobid, string clientId /*agar por bud yani az safheye shopcart request dade shode kolan doja felan load mishe map yeki moghe sabtena va yeki ham moghe kharid*/)
        {

            //.........................................................................agar az ghesmate kharid vared e in action beshe


            //inghet e code makhsoose sabade kharide va dare gheymate ersal ro be sabade kalaye kharidar ezafe mikone
            //bayad bid aval session kharidar nesbate be in furushgah ro peyda kone
            //dovom inke meghdar hazineye ersal ro bar asase mogheaite kharidar nesbate be furushande beriz tuye session(boronShahri ya DarunShahri)in mablagh ro khode kharidar moghe sabte nam ijad karde
            if (clientId != null)
            {
                #region AddDeliveryPrice

                //in flag baraye in hast ke gheymat ro pas bede be view.bag
                string DPrice = "";
                ApplicationDbContext db = new ApplicationDbContext();
                //bia client ro peyda kon
                var ClientItem = db.Client.Where(r => r.Id == clientId).FirstOrDefault();
                //shahresho bekesh birun 
                //bebin mosavie ba shahre furushande agar mosavi bud haine darun shahri mishe hazineye ersal
                //dar gheire in surat hazineye ersal mishe borun shahri

                if (jobid == ClientItem.City)
                {//yani shahre furushande ba kharidar yekie
                 //bia session sabadekharid ro begir va hazineye ersal ro por kon
                 //cast kon be jense dakhele session yani listi az ShopCartItem
                 //bebin in liste dorostesh ine ke tu yek field zakhire beshe vali eybi nadare tu yeki az aza ham berizi mablagh ro kafie va unvar giresh miaram
                    List<ShopCartItem> cart = Session[clientId] as List<ShopCartItem>;

                    //faghat shayad furushande in field ro khali gozashte, mikhaste rayegan bashe masalan darun shahrish tebghe ghanune sabte nam ke goftim khali bezari rayegan darnazar gerefte mishe
                    //va chon string ye meghdare sefr behesh midam ke un var jam baste mishe error nade
                    if (ClientItem.DarunshahriPrice == null || ClientItem.DarunshahriPrice == "")
                    {
                        cart.FirstOrDefault().DeliveryPrice = "0";
                        DPrice = "0";
                    }
                    else
                    {
                        cart.FirstOrDefault().DeliveryPrice = ClientItem.DarunshahriPrice;
                        DPrice = ClientItem.DarunshahriPrice;
                    }
                    //dobare meghdar ro bargardun be session
                    Session[clientId] = cart;
                }


                else
                //agar inja biad yani shahrashun motefavete va hazineye borunshahri hesab mishe 
                {
                    List<ShopCartItem> cart = Session[clientId] as List<ShopCartItem>;

                    //faghat shayad furushande in field ro khali gozashte, mikhaste rayegan bashe masalan darun shahrish tebghe ghanune sabte nam ke goftim khali bezari rayegan darnazar gerefte mishe
                    //va chon string ye meghdare sefr behesh midam ke un var jam baste mishe error nade
                    if (ClientItem.BorunshahriPrice == null || ClientItem.BorunshahriPrice == "")
                    {
                        cart.FirstOrDefault().DeliveryPrice = "0";
                        DPrice = "0";
                    }
                    else
                    {
                        cart.FirstOrDefault().DeliveryPrice = ClientItem.BorunshahriPrice;
                        DPrice = ClientItem.BorunshahriPrice;
                    }
                    //dobare meghdar ro bargardun be session
                    Session[clientId] = cart;
                }

                ViewBag.ViewPrice = DPrice;

                #endregion
            }


            //.........................................................................agar az ghesmate sabte nam vared action beshe


            if (jobid == "قم")
            {
                ViewBag.tt = "[34.6412,50.8751]";
                ViewBag.lat = "34.6412";
                ViewBag.lng = "50.8751";
            }
            if (jobid == "کرج")
            {
                ViewBag.tt = "[35.82059705201462,50.98274230957031]";
                ViewBag.lat = "35.82059705201462";
                ViewBag.lng = "50.98274230957031";
            }
            if (jobid == "قزوین")
            {
                ViewBag.tt = "[36.2854,50.0047]";
                ViewBag.lat = "36.2854";
                ViewBag.lng = "50.0047";
            }
            if (jobid == "مشهد")
            {
                ViewBag.tt = "[36.2928,59.6048]";
                ViewBag.lat = "36.2928";
                ViewBag.lng = "59.6048 ";
            }
            if (jobid == "اصفهان")
            {
                ViewBag.tt = "[32.6591,51.6838]";
                ViewBag.lat = "32.6591";
                ViewBag.lng = "51.6838 ";
            }
            if (jobid == "تهران")
            {
                ViewBag.tt = "[35.7160,51.4055]";
                ViewBag.lat = "35.7160";
                ViewBag.lng = "51.4055 ";
            }
            if (jobid == "زنجان")
            {
                ViewBag.tt = "[36.6888,48.4949]";
                ViewBag.lat = "36.6888";
                ViewBag.lng = "48.4949";
            }
            if (jobid == "رشت")
            {
                ViewBag.tt = "[37.2790,49.5834]";
                ViewBag.lat = "37.2790";
                ViewBag.lng = "49.5834";
            }
            if (jobid == "اردبیل")
            {
                ViewBag.tt = "[38.2458,48.2976]";
                ViewBag.lat = "38.2458";
                ViewBag.lng = "48.2976";
            }
            if (jobid == "تبریز")
            {
                ViewBag.tt = "[38.07944,46.2974]";
                ViewBag.lat = "38.07944";
                ViewBag.lng = "46.2974";
            }

            if (jobid == "ارومیه")
            {
                ViewBag.tt = "[37.5471,45.0697]";
                ViewBag.lat = "37.5471";
                ViewBag.lng = "45.0697";
            }
            if (jobid == "گرگان")
            {
                ViewBag.tt = "[36.8385,54.4344]";
                ViewBag.lat = "36.8385";
                ViewBag.lng = "54.4344";
            }
            if (jobid == "اراک")
            {
                ViewBag.tt = "[34.0946,49.6978]";
                ViewBag.lat = "34.0946";
                ViewBag.lng = "49.6978";
            }
            if (jobid == "کاشان")
            {
                ViewBag.tt = "[33.9879,51.4449]";
                ViewBag.lat = "33.9879";
                ViewBag.lng = "51.4449";
            }
            if (jobid == "یزد")
            {
                ViewBag.tt = "[31.8870,54.3628]";
                ViewBag.lat = "31.8870";
                ViewBag.lng = "54.3628";
            }
            if (jobid == "اهواز")
            {
                ViewBag.tt = "[31.3278,48.6884]";
                ViewBag.lat = "31.3278";
                ViewBag.lng = "48.6884";
            }
            if (jobid == "شیراز")
            {
                ViewBag.tt = "[29.6073,52.5232]";
                ViewBag.lat = "29.6073";
                ViewBag.lng = "52.5232";
            }
            if (jobid == "بندرعباس")
            {
                ViewBag.tt = "[27.1874,56.2739]";
                ViewBag.lat = "27.1874";
                ViewBag.lng = "56.2739";
            }
            if (jobid == "کرمان")
            {
                ViewBag.tt = "[30.2884,57.0668]";
                ViewBag.lat = "30.2884";
                ViewBag.lng = "57.0668";
            }
            if (jobid == "بجنورد")
            {
                ViewBag.tt = "[37.4886,57.3313]";
                ViewBag.lat = "37.4886";
                ViewBag.lng = "57.3313";
            }
            return PartialView("_map");
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        //این اکشن شماره کاربر را میگیرد ودر صورت صحت اطلاعات نسبت به قوانین
        //مثلا همه ورودی ها پر باشه و ... پیامک کد را ارسال میکند و در سشن ذخیره میکند
        public ActionResult setPhone(string subJob, string city, string PhoneNumber, string urlName, string userName, string password, string LocationLatitude, string LocationLongitude, string DarunshahriPrice, string BorunshahriPrice, string FurushHuzuri)
        {


            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            var response = Request["g-recaptcha-response"];
            string secretKey = "6Lf3e8oUAAAAAP3U_1DViKoNfML9UTauUF4tU7kO";
            //vase local:
            //6Ld5oq8UAAAAAMnUhEnMXRq - aHvWDsLNnQtnE4BD

            //vase site:
            //6Lf3e8oUAAAAAP3U_1DViKoNfML9UTauUF4tU7kO

            var client = new WebClient();

            //baraye local adres url ra taghir bede be adrese site
            var result1 = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secretKey, response));
            var obj = JObject.Parse(result1);
            var status = (bool)obj.SelectToken("success");

            if (status != true)
            {
                return RedirectToAction("index", "home");
            }


            ApplicationDbContext db = new ApplicationDbContext();
            //bia chek kon karbar bishtar az dota account nakhad besaze injuri ta hududi jelogiri mishe
            //az zabte addres haye khub tavasote soodjuyan
            //pas aval bia ye k list az account hayi ke in shomare dare bekesh birun agar dota dasht 
            //sevomiro nasaz
            var ListOfClient = db.Client.Where(r => r.mobile == PhoneNumber).ToList();
            if (ListOfClient.Count>=2)
            {//yani taraf mikhad bish az 2 account ba yek shomare besaze
                return RedirectToAction("Reg", "account", new { error = "D" });
            }


            //Start--agar username ya url tekrari behar tarfandi vared kard 
            //username ke khode username
            //urlid ham id client ke hardo nabayad tekrari bashan
            var isUserName = db.Users.Where(s => s.UserName == userName).FirstOrDefault();
            var isUrl = db.Client.Where(s => s.Id == urlName).FirstOrDefault();
            if (isUserName != null || isUrl != null)
            {
                return RedirectToAction("Reg", "account", new { error = "B" });
            }
            //End---


            registerModel item = new registerModel();
            item.LocationLatitude = LocationLatitude;
            item.LocationLongitude = LocationLongitude;
            item.password = password;
            item.urlName = urlName;
            item.userName = userName;
            item.city = city;


            item.BorunshahriPrice = BorunshahriPrice;
            item.DarunshahriPrice = DarunshahriPrice;

            item.FurushHuzuri = FurushHuzuri;
            //این قسمت واسه این که کسی شیطونی نکنه خارج از لیست عدد بزنه یادت باشه اگر قیمت ارسال ها گرونتر شد
            //و در ویو تغییر ایجاد کردی این ایف هارو هم تغییر بدی
            //in vase BorunshahriPrice
            if (BorunshahriPrice != "")
            {
                //pas meghdar entekhab karde
                if (Convert.ToInt32(BorunshahriPrice) > 61000 || Convert.ToInt32(BorunshahriPrice) < 14000 /*kasi sheytanat nakone hazinaro manfi rad kone bekhad site ro zaye kone*/)
                {
                    return RedirectToAction("reg", "account", new { error = "A" });
                }
            }
            //in vase BorunshahriPrice
            if (DarunshahriPrice != "")
            {
                //pas meghdar entekhab karde
                if (Convert.ToInt32(DarunshahriPrice) > 51000 || Convert.ToInt32(DarunshahriPrice) < 9000 /*kasi sheytanat nakone hazinaro manfi rad kone bekhad site ro zaye kone*/)
                {
                    return RedirectToAction("reg", "account", new { error = "A" });
                }
            }





            item.JobName = db.Subjobs.Where(r => r.SubjobName == subJob).FirstOrDefault().Id;

            if (subJob != null && password != null && city != null && PhoneNumber != null && urlName != null && userName != null && LocationLatitude != null && LocationLongitude != null)
            {

                //کاربر کارکتر های زیادی وارد نکند
                if (subJob.Length < 50 && password.Length < 40 && city.Length < 40 && PhoneNumber.Length < 30 && urlName.Length < 40 && userName.Length < 40 && LocationLongitude.Length < 22 && LocationLatitude.Length < 22)
                {

                    try
                    {
                        Random rnd = new Random();
                        item.smsCode = rnd.Next(1000, 10000).ToString();
                        item.mobile = PhoneNumber;

                        //......In baraye ersale payamake etela resani kharid baraye furushande ast avali shomarei ke bad az
                        //kharid sms bere ke pishfarz hamun shomareye sabte name, badan mitune avaz kone va laghv ham agar khast
                        //payamak barash nare un ro null mikone dar halate pishfarz 1 ast yani payamak bere vasash
                        item.SupportNumber = PhoneNumber;
                        item.LaghvPayamak = "1";
                        //......

                        item.Expiretime = DateTime.Now.AddMinutes(5);
                        item.LocationLatitude = LocationLatitude;
                        item.LocationLongitude = LocationLongitude;
                        meli.Send sender = new Send();
                        sender.SendByBaseNumber2("9017037365", "Aa30081370@", item.smsCode, PhoneNumber, Convert.ToInt32(48743));
                        item.smsCount = 1;
                        Session["Register"] = item;
                        return RedirectToAction("smsPage");

                    }
                    catch (Exception)
                    {
                        Session.Clear();
                        //moshkeli pish amade va ghesmate try error dade
                        return RedirectToAction("reg", "account", new { error = "A" });
                    }

                }

                return RedirectToAction("Reg");
            }

            else
            {
                return RedirectToAction("Reg");
            }


        }

        [RemoveSpaceFilter]
        [AllowAnonymous]
        //  بعد از اکشن بالا ست فون وارد این قسمت میشه که کد ارسال شده برای کاربر 
        // را بگیره و از ویو این مستقیم وارد اکشن  کلاینت رجیستر میشه
        //این اکشن صرفا واسط هست و هیچ مقایسه ای اینجا انجام نمیشه

        public ActionResult smsPage(string A)
        {


            if (Session["Register"] != null)
            {
                //bia cast kon bebin zamanesh ok hast 5daghigh
                registerModel item = new registerModel();
                item = (registerModel)Session["Register"];
                ViewBag.tell = item.mobile;
                if (item.Expiretime >= DateTime.Now)
                {
                    if (A == "error")
                    {
                        ViewBag.error = "کد وارد شده صحیح نمیباشد";

                    }
                    return View();
                }
                else
                {
                    Session.Clear();
                    return RedirectToAction("Reg");
                }

            }
            else
            {
                return RedirectToAction("Reg");
            }

        }



        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        //کد ارسالی از اکشن اس ام اس پیج وارد این اکشن میشه و در صورا صحت کد ومقایسه آن
        // اطلاعات فرد که در سشن ذخیره بود در دیتابیس ذخیره میشه و رجیستر میشه
        //بعد از رجیستر شدن در اینجا پیج سمپل هم برای کاربر ساخته میشه  یک راست به صفحه لاگین هدایت میشه
        public async Task<ActionResult> ClientRegister(string RegCode)
        {
            if (Session["Register"] != null)
            {


                //bia cast kon bebin zamanesh ok hast 5daghigh
                registerModel pointer = new registerModel();
                pointer = (registerModel)Session["Register"];
                if (pointer.Expiretime <= DateTime.Now)
                {
                    Session.Clear();
                    return RedirectToAction("reg");
                }



                //check kon bebin doros ast code ya na
                if (pointer.smsCode == RegCode)
                {

                    ApplicationDbContext cntxt = new Models.ApplicationDbContext();
                    var _rolemgr = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(cntxt));
                    UserManager<ApplicationUser> _usermgr = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(cntxt));
                    var d = cntxt.Users.ToList();

                    //cast sesion be class asli
                    registerModel item = new Models.registerModel();
                    item = (registerModel)Session["Register"];

                    var email = Guid.NewGuid().ToString() + "@gmail.com";
                    var user = new ApplicationUser { UserName = item.userName, Email = email };
                    user.SubjobId = item.JobName;
                    var result = await UserManager.CreateAsync(user, item.password);



                    //اگر کاربر ذخیره و رجیستر شد
                    if (result.Succeeded)
                    {
                        //sesion ra pak kon
                        Session.Clear();





                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        _usermgr.AddToRole(user.Id, "User");

                        //System.Threading.Thread.Sleep(300);

                        Client clientItem = new Client();

                        if (cntxt.Subjobs.Where(da => da.Id == item.JobName).FirstOrDefault().SubjobName == "آرایشی و بهداشتی")
                        {

                            SampleContent sample = cntxt.SampleContent.Where(y => y.JobName == "آرایشی و بهداشتی").FirstOrDefault();

                            clientItem.Address = sample.Address;
                            clientItem.Content = sample.Content;
                            clientItem.Email = sample.Email;
                            clientItem.Fisrtcolor = sample.FirstColor;
                            clientItem.Seccondcolor = sample.SeccondColor;
                            clientItem.Title = sample.Title;
                            clientItem.SaleTitle = sample.SaleTitle;
                            clientItem.PhonNumber = sample.PhoneNumber;
                            clientItem.Lat = item.LocationLatitude;
                            clientItem.Lng = item.LocationLongitude;
                            clientItem.mobile = item.mobile;
                            clientItem.Id = item.urlName;
                            clientItem.UserId = user.Id;
                            clientItem.smsCount = 50;

                            clientItem.DarunshahriPrice = item.DarunshahriPrice;
                            clientItem.BorunshahriPrice = item.BorunshahriPrice;
                            clientItem.City = item.city;

                            clientItem.FurushHuzuri = item.FurushHuzuri;


                            clientItem.SupportNumber = item.SupportNumber;
                            clientItem.LaghvPayamak = item.LaghvPayamak;


                            //invase Site
                            var rnn = Guid.NewGuid().ToString();
                            string sourceDir = Server.MapPath(sample.PhotoAddress);
                            string backupDir = @"c:\sites\baladchee\Contents\Pic\" + rnn + TypHelper(sample.PhotoAddress);
                            System.IO.File.Copy(sourceDir, backupDir, true);
                            clientItem.PhotoAddress = "~/Contents/Pic/" + rnn + TypHelper(sample.PhotoAddress);

                            //invase local
                            //clientItem.PhotoAddress = sample.PhotoAddress;


                            clientItem.BigContent = sample.BigContent;
                            cntxt.Client.Add(clientItem);



                            //یک صفحه پرداخت ویترین بساز که تست تایمش تا یک هفته بعد باشه
                            #region CreatePardakhtVitrin
                            PardakhtVitrin pardakhtVitrin = new PardakhtVitrin();
                            pardakhtVitrin.ClienId = item.urlName;
                            pardakhtVitrin.start = DateTime.Now;
                            //in baraye mohlate testesh hast
                            pardakhtVitrin.testTime = DateTime.Now.AddDays(7);

                            //dar ebteda 0 ast chon pardakhti anjam nadade babate vitrin
                            pardakhtVitrin.pardakht = 0;
                            cntxt.PardakhtVitrin.Add(pardakhtVitrin);

                            #endregion

                        }

                        if (cntxt.Subjobs.Where(da => da.Id == item.JobName).FirstOrDefault().SubjobName == "آموزشی")
                        {

                            SampleContent sample = cntxt.SampleContent.Where(y => y.JobName == "آموزشی").FirstOrDefault();

                            clientItem.Address = sample.Address;
                            clientItem.Content = sample.Content;
                            clientItem.Email = sample.Email;
                            clientItem.Fisrtcolor = sample.FirstColor;
                            clientItem.Seccondcolor = sample.SeccondColor;
                            clientItem.Title = sample.Title;
                            clientItem.SaleTitle = sample.SaleTitle;
                            clientItem.PhonNumber = sample.PhoneNumber;
                            clientItem.Lat = item.LocationLatitude;
                            clientItem.Lng = item.LocationLongitude;
                            clientItem.mobile = item.mobile;
                            clientItem.Id = item.urlName;
                            clientItem.UserId = user.Id;
                            clientItem.smsCount = 50;

                            clientItem.DarunshahriPrice = item.DarunshahriPrice;
                            clientItem.BorunshahriPrice = item.BorunshahriPrice;
                            clientItem.City = item.city;

                            clientItem.FurushHuzuri = item.FurushHuzuri;


                            clientItem.SupportNumber = item.SupportNumber;
                            clientItem.LaghvPayamak = item.LaghvPayamak;

                            var rnn = Guid.NewGuid().ToString();
                            string sourceDir = Server.MapPath(sample.PhotoAddress);
                            string backupDir = @"c:\sites\baladchee\Contents\Pic\" + rnn + TypHelper(sample.PhotoAddress);
                            System.IO.File.Copy(sourceDir, backupDir, true);
                            clientItem.PhotoAddress = "~/Contents/Pic/" + rnn + TypHelper(sample.PhotoAddress);

                            //invase local
                            //clientItem.PhotoAddress = sample.PhotoAddress;

                            clientItem.BigContent = sample.BigContent;
                            cntxt.Client.Add(clientItem);



                            //یک صفحه پرداخت ویترین بساز که تست تایمش تا یک هفته بعد باشه
                            #region CreatePardakhtVitrin
                            PardakhtVitrin pardakhtVitrin = new PardakhtVitrin();
                            pardakhtVitrin.ClienId = item.urlName;
                            pardakhtVitrin.start = DateTime.Now;
                            //in baraye mohlate testesh hast
                            pardakhtVitrin.testTime = DateTime.Now.AddDays(7);

                            //dar ebteda 0 ast chon pardakhti anjam nadade babate vitrin
                            pardakhtVitrin.pardakht = 0;
                            cntxt.PardakhtVitrin.Add(pardakhtVitrin);

                            #endregion

                        }


                        if (cntxt.Subjobs.Where(da => da.Id == item.JobName).FirstOrDefault().SubjobName == "پوشاک")
                        {

                            SampleContent sample = cntxt.SampleContent.Where(y => y.JobName == "پوشاک").FirstOrDefault();

                            clientItem.Address = sample.Address;
                            clientItem.Content = sample.Content;
                            clientItem.Email = sample.Email;
                            clientItem.Fisrtcolor = sample.FirstColor;
                            clientItem.Seccondcolor = sample.SeccondColor;
                            clientItem.Title = sample.Title;
                            clientItem.SaleTitle = sample.SaleTitle;
                            clientItem.PhonNumber = sample.PhoneNumber;
                            clientItem.Lat = item.LocationLatitude;
                            clientItem.Lng = item.LocationLongitude;
                            clientItem.mobile = item.mobile;
                            clientItem.Id = item.urlName;
                            clientItem.UserId = user.Id;
                            clientItem.smsCount = 50;

                            clientItem.DarunshahriPrice = item.DarunshahriPrice;
                            clientItem.BorunshahriPrice = item.BorunshahriPrice;
                            clientItem.City = item.city;

                            clientItem.FurushHuzuri = item.FurushHuzuri;


                            clientItem.SupportNumber = item.SupportNumber;
                            clientItem.LaghvPayamak = item.LaghvPayamak;

                            var rnn = Guid.NewGuid().ToString();
                            string sourceDir = Server.MapPath(sample.PhotoAddress);
                            string backupDir = @"c:\sites\baladchee\Contents\Pic\" + rnn + TypHelper(sample.PhotoAddress);
                            System.IO.File.Copy(sourceDir, backupDir, true);
                            clientItem.PhotoAddress = "~/Contents/Pic/" + rnn + TypHelper(sample.PhotoAddress);

                            //invase local
                            //clientItem.PhotoAddress = sample.PhotoAddress;

                            clientItem.BigContent = sample.BigContent;
                            cntxt.Client.Add(clientItem);



                            //یک صفحه پرداخت ویترین بساز که تست تایمش تا یک هفته بعد باشه
                            #region CreatePardakhtVitrin
                            PardakhtVitrin pardakhtVitrin = new PardakhtVitrin();
                            pardakhtVitrin.ClienId = item.urlName;
                            pardakhtVitrin.start = DateTime.Now;
                            //in baraye mohlate testesh hast
                            pardakhtVitrin.testTime = DateTime.Now.AddDays(7);
                            //dar ebteda 0 ast chon pardakhti anjam nadade babate vitrin
                            pardakhtVitrin.pardakht = 0;
                            cntxt.PardakhtVitrin.Add(pardakhtVitrin);

                            #endregion

                        }



                        if (cntxt.Subjobs.Where(da => da.Id == item.JobName).FirstOrDefault().SubjobName == "فروشگاه و سوپرمارکت")
                        {

                            SampleContent sample = cntxt.SampleContent.Where(y => y.JobName == "فروشگاه و سوپرمارکت").FirstOrDefault();

                            clientItem.Address = sample.Address;
                            clientItem.Content = sample.Content;
                            clientItem.Email = sample.Email;
                            clientItem.Fisrtcolor = sample.FirstColor;
                            clientItem.Seccondcolor = sample.SeccondColor;
                            clientItem.Title = sample.Title;
                            clientItem.SaleTitle = sample.SaleTitle;
                            clientItem.PhonNumber = sample.PhoneNumber;
                            clientItem.Lat = item.LocationLatitude;
                            clientItem.Lng = item.LocationLongitude;
                            clientItem.mobile = item.mobile;
                            clientItem.Id = item.urlName;
                            clientItem.UserId = user.Id;
                            clientItem.smsCount = 50;

                            clientItem.DarunshahriPrice = item.DarunshahriPrice;
                            clientItem.BorunshahriPrice = item.BorunshahriPrice;
                            clientItem.City = item.city;

                            clientItem.FurushHuzuri = item.FurushHuzuri;


                            clientItem.SupportNumber = item.SupportNumber;
                            clientItem.LaghvPayamak = item.LaghvPayamak;

                            var rnn = Guid.NewGuid().ToString();
                            string sourceDir = Server.MapPath(sample.PhotoAddress);
                            string backupDir = @"c:\sites\baladchee\Contents\Pic\" + rnn + TypHelper(sample.PhotoAddress);
                            System.IO.File.Copy(sourceDir, backupDir, true);
                            clientItem.PhotoAddress = "~/Contents/Pic/" + rnn + TypHelper(sample.PhotoAddress);

                            //invase local
                            //clientItem.PhotoAddress = sample.PhotoAddress;

                            clientItem.BigContent = sample.BigContent;
                            cntxt.Client.Add(clientItem);


                            //یک صفحه پرداخت ویترین بساز که تست تایمش تا یک هفته بعد باشه
                            #region CreatePardakhtVitrin
                            PardakhtVitrin pardakhtVitrin = new PardakhtVitrin();
                            pardakhtVitrin.ClienId = item.urlName;
                            pardakhtVitrin.start = DateTime.Now;
                            //in baraye mohlate testesh hast
                            pardakhtVitrin.testTime = DateTime.Now.AddDays(7);
                            //dar ebteda 0 ast chon pardakhti anjam nadade babate vitrin
                            pardakhtVitrin.pardakht = 0;
                            cntxt.PardakhtVitrin.Add(pardakhtVitrin);

                            #endregion


                        }



                        //توضیحات مربوطه خوانده شود
                        #region رستوران و فسد فود

                        //فعلا با تصمیم گروهی قرار شد این قسمت حذف بشه ولی در آینده شاید باز فعالش کنیم به خاطر همین کامنت میکنم این قسمت رو
                        //و در قسمت Identitymodel هم کامنتش میکنم



                        //if (cntxt.Subjobs.Where(da => da.Id == item.JobName).FirstOrDefault().SubjobName == "رستوران و فست فود")
                        //{

                        //    SampleContent sample = cntxt.SampleContent.Where(y => y.JobName == "رستوران و فست فود").FirstOrDefault();

                        //    clientItem.Address = sample.Address;
                        //    clientItem.Content = sample.Content;
                        //    clientItem.Email = sample.Email;
                        //    clientItem.Fisrtcolor = sample.FirstColor;
                        //    clientItem.Seccondcolor = sample.SeccondColor;
                        //    clientItem.Title = sample.Title;
                        //    clientItem.SaleTitle = sample.SaleTitle;
                        //    clientItem.PhonNumber = sample.PhoneNumber;
                        //    clientItem.Lat = item.LocationLatitude;
                        //    clientItem.Lng = item.LocationLongitude;
                        //    clientItem.mobile = item.mobile;
                        //    clientItem.Id = item.urlName;
                        //    clientItem.UserId = user.Id;
                        //    clientItem.smsCount = 50;

                        //    var rnn = Guid.NewGuid().ToString();
                        //    string sourceDir = Server.MapPath(sample.PhotoAddress);
                        //    string backupDir = @"c:\sites\baladchee\Contents\Pic\" + rnn + TypHelper(sample.PhotoAddress);
                        //    System.IO.File.Copy(sourceDir, backupDir, true);
                        //    clientItem.PhotoAddress = "~/Contents/Pic/" + rnn + TypHelper(sample.PhotoAddress);

                        //    //invase local
                        //    //clientItem.PhotoAddress = sample.PhotoAddress;

                        //    clientItem.BigContent = sample.BigContent;
                        //    cntxt.Client.Add(clientItem);


                        //    //یک صفحه پرداخت ویترین بساز که تست تایمش تا یک هفته بعد باشه
                        //    #region CreatePardakhtVitrin
                        //    PardakhtVitrin pardakhtVitrin = new PardakhtVitrin();
                        //    pardakhtVitrin.ClienId = item.urlName;
                        //    pardakhtVitrin.start = DateTime.Now;
                        //    //in baraye mohlate testesh hast
                        //    pardakhtVitrin.testTime = DateTime.Now.AddDays(7);
                        //    //dar ebteda 0 ast chon pardakhti anjam nadade babate vitrin
                        //    pardakhtVitrin.pardakht = 0;
                        //    cntxt.PardakhtVitrin.Add(pardakhtVitrin);

                        //    #endregion

                        //}


                        #endregion



                        if (cntxt.Subjobs.Where(da => da.Id == item.JobName).FirstOrDefault().SubjobName == "پزشکی و درمانی")
                        {

                            SampleContent sample = cntxt.SampleContent.Where(y => y.JobName == "پزشکی و درمانی").FirstOrDefault();

                            clientItem.Address = sample.Address;
                            clientItem.Content = sample.Content;
                            clientItem.Email = sample.Email;
                            clientItem.Fisrtcolor = sample.FirstColor;
                            clientItem.Seccondcolor = sample.SeccondColor;
                            clientItem.Title = sample.Title;
                            clientItem.SaleTitle = sample.SaleTitle;
                            clientItem.Lat = item.LocationLatitude;
                            clientItem.Lng = item.LocationLongitude;
                            clientItem.mobile = item.mobile;
                            clientItem.Id = item.urlName;
                            clientItem.PhonNumber = sample.PhoneNumber;
                            clientItem.UserId = user.Id;
                            clientItem.smsCount = 50;

                            clientItem.DarunshahriPrice = item.DarunshahriPrice;
                            clientItem.BorunshahriPrice = item.BorunshahriPrice;
                            clientItem.City = item.city;

                            clientItem.FurushHuzuri = item.FurushHuzuri;

                            clientItem.SupportNumber = item.SupportNumber;
                            clientItem.LaghvPayamak = item.LaghvPayamak;

                            var rnn = Guid.NewGuid().ToString();
                            string sourceDir = Server.MapPath(sample.PhotoAddress);
                            string backupDir = @"c:\sites\baladchee\Contents\Pic\" + rnn + TypHelper(sample.PhotoAddress);
                            System.IO.File.Copy(sourceDir, backupDir, true);
                            clientItem.PhotoAddress = "~/Contents/Pic/" + rnn + TypHelper(sample.PhotoAddress);

                            //invase local
                            //clientItem.PhotoAddress = sample.PhotoAddress;

                            clientItem.BigContent = sample.BigContent;
                            cntxt.Client.Add(clientItem);


                            //یک صفحه پرداخت ویترین بساز که تست تایمش تا یک هفته بعد باشه
                            #region CreatePardakhtVitrin
                            PardakhtVitrin pardakhtVitrin = new PardakhtVitrin();
                            pardakhtVitrin.ClienId = item.urlName;
                            pardakhtVitrin.start = DateTime.Now;
                            //in baraye mohlate testesh hast
                            pardakhtVitrin.testTime = DateTime.Now.AddDays(7);
                            //dar ebteda 0 ast chon pardakhti anjam nadade babate vitrin
                            pardakhtVitrin.pardakht = 0;
                            cntxt.PardakhtVitrin.Add(pardakhtVitrin);

                            #endregion

                        }






                        if (cntxt.Subjobs.Where(da => da.Id == item.JobName).FirstOrDefault().SubjobName == "صنایع دستی")
                        {

                            SampleContent sample = cntxt.SampleContent.Where(y => y.JobName == "صنایع دستی").FirstOrDefault();

                            clientItem.Address = sample.Address;
                            clientItem.Content = sample.Content;
                            clientItem.Email = sample.Email;
                            clientItem.Fisrtcolor = sample.FirstColor;
                            clientItem.Seccondcolor = sample.SeccondColor;
                            clientItem.Title = sample.Title;
                            clientItem.SaleTitle = sample.SaleTitle;
                            clientItem.Lat = item.LocationLatitude;
                            clientItem.Lng = item.LocationLongitude;
                            clientItem.mobile = item.mobile;
                            clientItem.Id = item.urlName;
                            clientItem.PhonNumber = sample.PhoneNumber;
                            clientItem.UserId = user.Id;
                            clientItem.smsCount = 50;

                            clientItem.DarunshahriPrice = item.DarunshahriPrice;
                            clientItem.BorunshahriPrice = item.BorunshahriPrice;
                            clientItem.City = item.city;

                            clientItem.FurushHuzuri = item.FurushHuzuri;


                            clientItem.SupportNumber = item.SupportNumber;
                            clientItem.LaghvPayamak = item.LaghvPayamak;

                            var rnn = Guid.NewGuid().ToString();
                            string sourceDir = Server.MapPath(sample.PhotoAddress);
                            string backupDir = @"c:\sites\baladchee\Contents\Pic\" + rnn + TypHelper(sample.PhotoAddress);
                            System.IO.File.Copy(sourceDir, backupDir, true);
                            clientItem.PhotoAddress = "~/Contents/Pic/" + rnn + TypHelper(sample.PhotoAddress);

                            //invase local
                            //clientItem.PhotoAddress = sample.PhotoAddress;

                            clientItem.BigContent = sample.BigContent;
                            cntxt.Client.Add(clientItem);

                            //یک صفحه پرداخت ویترین بساز که تست تایمش تا یک هفته بعد باشه
                            #region CreatePardakhtVitrin
                            PardakhtVitrin pardakhtVitrin = new PardakhtVitrin();
                            pardakhtVitrin.ClienId = item.urlName;
                            pardakhtVitrin.start = DateTime.Now;
                            //in baraye mohlate testesh hast
                            pardakhtVitrin.testTime = DateTime.Now.AddDays(7);

                            //dar ebteda 0 ast chon pardakhti anjam nadade babate vitrin
                            pardakhtVitrin.pardakht = 0;
                            cntxt.PardakhtVitrin.Add(pardakhtVitrin);

                            #endregion

                        }




                        if (cntxt.Subjobs.Where(da => da.Id == item.JobName).FirstOrDefault().SubjobName == "فرهنگی و هنری")
                        {

                            SampleContent sample = cntxt.SampleContent.Where(y => y.JobName == "فرهنگی و هنری").FirstOrDefault();

                            clientItem.Address = sample.Address;
                            clientItem.Content = sample.Content;
                            clientItem.Email = sample.Email;
                            clientItem.Fisrtcolor = sample.FirstColor;
                            clientItem.Seccondcolor = sample.SeccondColor;
                            clientItem.Title = sample.Title;
                            clientItem.SaleTitle = sample.SaleTitle;
                            clientItem.Lat = item.LocationLatitude;
                            clientItem.Lng = item.LocationLongitude;
                            clientItem.mobile = item.mobile;
                            clientItem.Id = item.urlName;
                            clientItem.PhonNumber = sample.PhoneNumber;
                            clientItem.UserId = user.Id;
                            clientItem.smsCount = 50;

                            clientItem.DarunshahriPrice = item.DarunshahriPrice;
                            clientItem.BorunshahriPrice = item.BorunshahriPrice;
                            clientItem.City = item.city;

                            clientItem.FurushHuzuri = item.FurushHuzuri;

                            var rnn = Guid.NewGuid().ToString();
                            string sourceDir = Server.MapPath(sample.PhotoAddress);
                            string backupDir = @"c:\sites\baladchee\Contents\Pic\" + rnn + TypHelper(sample.PhotoAddress);
                            System.IO.File.Copy(sourceDir, backupDir, true);
                            clientItem.PhotoAddress = "~/Contents/Pic/" + rnn + TypHelper(sample.PhotoAddress);

                            //invase local
                            //clientItem.PhotoAddress = sample.PhotoAddress;

                            clientItem.BigContent = sample.BigContent;
                            cntxt.Client.Add(clientItem);

                            //یک صفحه پرداخت ویترین بساز که تست تایمش تا یک هفته بعد باشه
                            #region CreatePardakhtVitrin
                            PardakhtVitrin pardakhtVitrin = new PardakhtVitrin();
                            pardakhtVitrin.ClienId = item.urlName;
                            pardakhtVitrin.start = DateTime.Now;
                            //in baraye mohlate testesh hast
                            pardakhtVitrin.testTime = DateTime.Now.AddDays(7);

                            //dar ebteda 0 ast chon pardakhti anjam nadade babate vitrin
                            pardakhtVitrin.pardakht = 0;
                            cntxt.PardakhtVitrin.Add(pardakhtVitrin);

                            #endregion

                        }




                        if (cntxt.Subjobs.Where(da => da.Id == item.JobName).FirstOrDefault().SubjobName == "تجهیزات ساختمانی و ایمنی")
                        {

                            SampleContent sample = cntxt.SampleContent.Where(y => y.JobName == "تجهیزات ساختمانی و ایمنی").FirstOrDefault();

                            clientItem.Address = sample.Address;
                            clientItem.Content = sample.Content;
                            clientItem.Email = sample.Email;
                            clientItem.Fisrtcolor = sample.FirstColor;
                            clientItem.Seccondcolor = sample.SeccondColor;
                            clientItem.Title = sample.Title;
                            clientItem.SaleTitle = sample.SaleTitle;
                            clientItem.Lat = item.LocationLatitude;
                            clientItem.Lng = item.LocationLongitude;
                            clientItem.mobile = item.mobile;
                            clientItem.Id = item.urlName;
                            clientItem.PhonNumber = sample.PhoneNumber;
                            clientItem.UserId = user.Id;
                            clientItem.smsCount = 50;

                            clientItem.DarunshahriPrice = item.DarunshahriPrice;
                            clientItem.BorunshahriPrice = item.BorunshahriPrice;
                            clientItem.City = item.city;

                            clientItem.FurushHuzuri = item.FurushHuzuri;


                            clientItem.SupportNumber = item.SupportNumber;
                            clientItem.LaghvPayamak = item.LaghvPayamak;

                            var rnn = Guid.NewGuid().ToString();
                            string sourceDir = Server.MapPath(sample.PhotoAddress);
                            string backupDir = @"c:\sites\baladchee\Contents\Pic\" + rnn + TypHelper(sample.PhotoAddress);
                            System.IO.File.Copy(sourceDir, backupDir, true);
                            clientItem.PhotoAddress = "~/Contents/Pic/" + rnn + TypHelper(sample.PhotoAddress);

                            //invase local
                            //clientItem.PhotoAddress = sample.PhotoAddress;

                            clientItem.BigContent = sample.BigContent;
                            cntxt.Client.Add(clientItem);

                            //یک صفحه پرداخت ویترین بساز که تست تایمش تا یک هفته بعد باشه
                            #region CreatePardakhtVitrin
                            PardakhtVitrin pardakhtVitrin = new PardakhtVitrin();
                            pardakhtVitrin.ClienId = item.urlName;
                            pardakhtVitrin.start = DateTime.Now;
                            //in baraye mohlate testesh hast
                            pardakhtVitrin.testTime = DateTime.Now.AddDays(7);

                            //dar ebteda 0 ast chon pardakhti anjam nadade babate vitrin
                            pardakhtVitrin.pardakht = 0;
                            cntxt.PardakhtVitrin.Add(pardakhtVitrin);

                            #endregion

                        }




                        if (cntxt.Subjobs.Where(da => da.Id == item.JobName).FirstOrDefault().SubjobName == "سایر مشاغل")
                        {

                            SampleContent sample = cntxt.SampleContent.Where(y => y.JobName == "سایر مشاغل").FirstOrDefault();

                            clientItem.Address = sample.Address;
                            clientItem.Content = sample.Content;
                            clientItem.Email = sample.Email;
                            clientItem.Fisrtcolor = sample.FirstColor;
                            clientItem.Seccondcolor = sample.SeccondColor;
                            clientItem.Title = sample.Title;
                            clientItem.SaleTitle = sample.SaleTitle;
                            clientItem.Lat = item.LocationLatitude;
                            clientItem.Lng = item.LocationLongitude;
                            clientItem.mobile = item.mobile;
                            clientItem.Id = item.urlName;
                            clientItem.PhonNumber = sample.PhoneNumber;
                            clientItem.UserId = user.Id;
                            clientItem.smsCount = 50;

                            clientItem.DarunshahriPrice = item.DarunshahriPrice;
                            clientItem.BorunshahriPrice = item.BorunshahriPrice;
                            clientItem.City = item.city;

                            clientItem.FurushHuzuri = item.FurushHuzuri;


                            clientItem.SupportNumber = item.SupportNumber;
                            clientItem.LaghvPayamak = item.LaghvPayamak;

                            var rnn = Guid.NewGuid().ToString();
                            string sourceDir = Server.MapPath(sample.PhotoAddress);
                            string backupDir = @"c:\sites\baladchee\Contents\Pic\" + rnn + TypHelper(sample.PhotoAddress);
                            System.IO.File.Copy(sourceDir, backupDir, true);
                            clientItem.PhotoAddress = "~/Contents/Pic/" + rnn + TypHelper(sample.PhotoAddress);

                            //invase local
                            //clientItem.PhotoAddress = sample.PhotoAddress;

                            clientItem.BigContent = sample.BigContent;
                            cntxt.Client.Add(clientItem);

                            //یک صفحه پرداخت ویترین بساز که تست تایمش تا یک هفته بعد باشه
                            #region CreatePardakhtVitrin
                            PardakhtVitrin pardakhtVitrin = new PardakhtVitrin();
                            pardakhtVitrin.ClienId = item.urlName;
                            pardakhtVitrin.start = DateTime.Now;
                            //in baraye mohlate testesh hast
                            pardakhtVitrin.testTime = DateTime.Now.AddDays(7);

                            //dar ebteda 0 ast chon pardakhti anjam nadade babate vitrin
                            pardakhtVitrin.pardakht = 0;
                            cntxt.PardakhtVitrin.Add(pardakhtVitrin);

                            #endregion

                        }



                        #region CreatVitrinQr

                        QRCodeGenerator qr = new QRCodeGenerator();
                        QRCodeData data = qr.CreateQrCode("http://baladchee.com/" + item.urlName, QRCodeGenerator.ECCLevel.Q);
                        QRCode code = new QRCode(data);

                        using (Bitmap bitMap = code.GetGraphic(10))
                        {
                            var GUID = Guid.NewGuid().ToString();
                            using (MemoryStream ms = new MemoryStream())
                            {
                                bitMap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                byte[] byteImage = ms.ToArray();
                                System.Drawing.Image img = System.Drawing.Image.FromStream(ms);

                                img.Save(Server.MapPath("~/Contents/Qrs/") + GUID + ".Jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);
                            }
                            clientItem.QrImageAddres = "~/Contents/Qrs/" + GUID + ".Jpeg";

                        }


                        #endregion

                        #region CreatLocationQr
                        QRCodeGenerator Locationqr = new QRCodeGenerator();
                        QRCodeData Locationdata = Locationqr.CreateQrCode("geo:" + item.LocationLatitude + "," + item.LocationLongitude, QRCodeGenerator.ECCLevel.Q);
                        QRCode Locationcode = new QRCode(Locationdata);

                        using (Bitmap bitMap1 = Locationcode.GetGraphic(10))
                        {
                            var GUID2 = Guid.NewGuid().ToString();
                            using (MemoryStream ms1 = new MemoryStream())
                            {
                                bitMap1.Save(ms1, System.Drawing.Imaging.ImageFormat.Png);
                                byte[] byteImage = ms1.ToArray();
                                System.Drawing.Image img1 = System.Drawing.Image.FromStream(ms1);

                                img1.Save(Server.MapPath("~/Contents/Qrs/") + GUID2 + ".Jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);
                            }
                            clientItem.QrLocation = "~/Contents/Qrs/" + GUID2 + ".Jpeg";
                            cntxt.SaveChanges();
                        }


                        #endregion




                        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                        return RedirectToAction("Login", "Account");
                    }

                    return RedirectToAction("Admins", "Account");
                }
                //ramz ro eshtebah neveshte
                else
                {
                    return RedirectToAction("smsPage", "Account", new { A = "error" });
                }
            }
            return RedirectToAction("Reg", "Account");

        }



        [AllowAnonymous]
        [HttpPost]
        public JsonResult testuserName(string userName)
        {
            var db = new ApplicationDbContext();
            var item = db.Users.Where(t => t.UserName == userName).FirstOrDefault();
            if (item != null)
            {
                return Json(new { success = true, responseText = "نام کاربری قبلا ثبت شده است" }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { success = true, responseText = "✓" }, JsonRequestBehavior.AllowGet);

            }

        }


        [AllowAnonymous]
        [HttpPost]
        public JsonResult testUrl(string url)
        {
            var db = new ApplicationDbContext();
            var item = db.Client.Where(t => t.Id == url).FirstOrDefault();
            if (item != null)
            {
                return Json(new { success = true, responseText = "آدرس قبلا ثبت شده است" }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { success = true, responseText = "✓" }, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MyRegister(RegisterViewModel model, string subjobId)
        {
            ApplicationDbContext cntxt = new Models.ApplicationDbContext();
            var _rolemgr = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(cntxt));
            UserManager<ApplicationUser> _usermgr = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(cntxt));
            var d = cntxt.Users.ToList();
            foreach (var item in d)
            {
                if (item.UserName == model.USRN)
                {
                    var subjoblist = cntxt.Subjobs.ToList();
                    ViewBag.subjoblist = subjoblist;
                    ViewBag.tekrari = "نام کاربری قبلا ثبت شده است";
                    return View("SuperAdminClient", model);
                }
            }
            if (ModelState.IsValid)
            {
                var email = Guid.NewGuid().ToString() + "@gmail.com";
                var user = new ApplicationUser { UserName = model.USRN, Email = email };
                user.SubjobId = subjobId;
                var result = await UserManager.CreateAsync(user, model.Password);


                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    _usermgr.AddToRole(user.Id, "User");

                    return RedirectToAction("Admins", "Account");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            ViewBag.tekrari = "مشکلی پیش آمده لطفا دوباره تلاش کنید";
            var subjoblist2 = cntxt.Subjobs.ToList();
            ViewBag.subjoblist = subjoblist2;
            return View("SuperAdminClient", model);
        }




        static string TypHelper(string obj)
        {
            string TypeOfFile = "";
            string selfFile = obj;

            for (int i = obj.Length - 1; i >= 0; i--)
            {
                if (obj.Substring(i, 1) != ".")
                {

                    TypeOfFile = obj.Substring(i, 1) + TypeOfFile;


                }
                if (obj.Substring(i, 1) == ".")
                {
                    return "." + TypeOfFile;
                }
            }
            return obj;
        }



        //add Page
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddPage(Client model, HttpPostedFileBase RootAddres)
        {



            if (ModelState.IsValid)
            {
                ApplicationDbContext db = new ApplicationDbContext();

                var ThereIs = db.Client.Where(s => s.Id == model.Id).FirstOrDefault();

                if (ThereIs != null)
                {
                    return RedirectToAction("Admins", "Account", new { e = "there is" });
                }

                if (RootAddres != null && RootAddres.ContentLength > 0)
                    try
                    {
                        string ValuePath = Path.GetFileName(Guid.NewGuid().ToString()) + TypHelper(RootAddres.FileName);
                        string ValueSaverSaver = Path.Combine(Server.MapPath("~/Contents/Pic"), ValuePath);
                        RootAddres.SaveAs(ValueSaverSaver);

                        var item = new Client();


                        item.PhotoAddress = "~/Contents/Pic/" + ValuePath;
                        item.Id = model.Id;
                        item.mobile = model.PhonNumber;
                        item.Content = model.Content;
                        item.Title = model.Title;
                        item.ownerName = model.ownerName;
                        item.UserId = User.Identity.GetUserId();
                        ApplicationDbContext dbg = new ApplicationDbContext();







                        #region CreatQr

                        QRCodeGenerator qr = new QRCodeGenerator();
                        QRCodeData data = qr.CreateQrCode("http://baladchee.com/" + item.Id, QRCodeGenerator.ECCLevel.Q);
                        QRCode code = new QRCode(data);

                        using (Bitmap bitMap = code.GetGraphic(10))
                        {
                            var GUID = Guid.NewGuid().ToString();
                            using (MemoryStream ms = new MemoryStream())
                            {
                                bitMap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                byte[] byteImage = ms.ToArray();
                                System.Drawing.Image img = System.Drawing.Image.FromStream(ms);

                                img.Save(Server.MapPath("~/Contents/Qrs/") + GUID + ".Jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);
                            }
                            item.QrImageAddres = "~/Contents/Qrs/" + GUID + ".Jpeg";

                        }

                        #endregion




                        #region CreatLocationQr

                        //pishfarz adresesh tehrane
                        QRCodeGenerator Locationqr = new QRCodeGenerator();
                        QRCodeData Locationdata = Locationqr.CreateQrCode("geo:" + "35.7160" + "," + "51.4055", QRCodeGenerator.ECCLevel.Q);
                        QRCode Locationcode = new QRCode(Locationdata);

                        using (Bitmap bitMap = Locationcode.GetGraphic(10))
                        {
                            var GUID2 = Guid.NewGuid().ToString();
                            using (MemoryStream ms = new MemoryStream())
                            {
                                bitMap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                byte[] byteImage = ms.ToArray();
                                System.Drawing.Image img = System.Drawing.Image.FromStream(ms);

                                img.Save(Server.MapPath("~/Contents/Qrs/") + GUID2 + ".Jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);
                            }
                            item.QrLocation = "~/Contents/Qrs/" + GUID2 + ".Jpeg";
                            dbg.Client.Add(item);
                            dbg.SaveChanges();
                        }
                        #endregion




                    }
                    catch (Exception)
                    {

                        return View("NotFound");
                    }
            }
            else
            {
                //ViewBag.Message = "شما فایلی را مشخص نکرده اید";
                return View("NotFound");
            }

            //var g = db.Client.Where(d => d.UserId == User.Identity.GetUserId()).FirstOrDefault();
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Admins", "Account");
            //return RedirectToAction("Show","Home",new {id=model.Id});
        }



        //dashboard


        [Authorize]
        [RemoveSpaceFilter]
        public ActionResult dashboard(string ClientId)
        {
            ViewBag.clientid = ClientId;
            ApplicationDbContext db = new ApplicationDbContext();
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                //ViewBag.Data = "12,7,14,45,29";
                //ViewBag.ObjectName = "'test1','test3','test4','test5','test6'";
                #region statistic


                //ببین کلا باید بازدید های هر 30 روز رو نشون بده 
                //چون حجم دیتا بیس خیلی زیاد میره بالا پس بیش از 30 روز قبل رو پاک کن
                //1 ebteda tarikhe 30 ruze pish ro dar biar
                var OneMonthAgo = DateTime.Now.AddDays(-30);

                //boro tu jadval va bazdid haye in furush gah ro peyda kon
                foreach (var item in db.statistic)
                {
                    //un hayi ke tarikhe createshun az tarikhe yek mahe pish kuchiktar bud paak kon
                    //intori dg table statistic sangin nemishe
                    if (item.ClienId == ClientId && item.CreateDate < OneMonthAgo)
                    {
                        //pak kon baraye bish az yek mahe pishe va bedard nemikhore
                        //ma nahayatan amare yek mah ro mikhaym
                        db.statistic.Remove(item);
                    }
                }
                db.SaveChanges();

                //hala az miun baghimandeye amar ke baraye yek mah be invar ast
                //ebteda liste marbute estekhraj shavad
                //sepas groupby bar asase tarikh shavad va 30 ruze akhir ro neshun bede
                var mylist = db.statistic.Where(t => t.ClienId == ClientId).OrderBy(w => w.CreateDate).ToList();
                //hala az in list 30 taye akhar ro be ma bede
                var list = mylist.GroupBy(r => r.CreateDate.Value.Date).Select(g => new { name = g.Key, count = g.Count() }).ToList().OrderByDescending(r => r.name).Take(30);

                //sakhtar ro bar asase js amar dorost mikone
                string names = "";
                string count = "";
                int sum = 0;
                foreach (var item in list)
                {
                    names = "'" + Replace.ToShamsi(item.name) + "'" + "," + names;
                }
                foreach (var item in list)
                {
                    count = item.count.ToString() + "," + count;
                    sum = sum + Convert.ToInt32(item.count);
                }

                ViewBag.Data = count;
                ViewBag.ObjectName = names;
                ViewBag.max = list.Max(i => i.count) + 1;
                ViewBag.sum = sum;
                #endregion

                #region isSeenOrders
                //check for seen orders 
                //false is order
                //true is not order
                var check = db.Sefareshat.Where(o => o.isSeen == false && o.clientId == ClientId).FirstOrDefault();
                if (check != null)
                {
                    ViewBag.isSeen = "false";
                }
                #endregion


                return View();
            }
            return HttpNotFound();
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult AddColor(string Firstcolor, string Seccondcolor, string ClientId)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                var searchItem = db.Client.Where(t => t.Id == ClientId).FirstOrDefault();
                searchItem.Fisrtcolor = Firstcolor;
                searchItem.Seccondcolor = Seccondcolor;
                db.SaveChanges();
            }
            return RedirectToAction("Show", "Home", new { id = ClientId });
        }

        [Authorize]
        [RemoveSpaceFilter]
        public ActionResult Opinion(string ClientId)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {

                #region isSeenOrders
                //check for seen orders 
                //false is order
                //true is not order
                var check = db.Sefareshat.Where(o => o.isSeen == false && o.clientId == ClientId).FirstOrDefault();
                if (check != null)
                {
                    ViewBag.isSeen = "false";
                }
                #endregion

                ViewBag.clientid = ClientId;
                ViewBag.opinionList = db.Opinion.Where(i => i.ClienId == ClientId).ToList();
                return View();
            }
            return HttpNotFound();
        }


        [Authorize]
        [RemoveSpaceFilter]
        public ActionResult Orders(string ClientId, int? page)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {

                //in viewbag baraye in hast ke bad az inke vared riz factor shod dobare bere be safheye ghablie factor ha
                ViewBag.pageCount = page;

                #region isSeenOrders
                //all of false change to true 
                //ghermezi hoshdar bere chon dg baz shod va sefaresh ro did
                var check = db.Sefareshat.Where(o => o.isSeen == false && o.clientId == ClientId).ToList();
                foreach (var item in check)
                {
                    item.isSeen = true;
                }
                db.SaveChanges();
                #endregion

                ViewBag.clientid = ClientId;


                #region Amare Tarakoneshhaye Emruz
                //aval ebtedaye in mah ro be shamsi be dast miarim
                PersianCalendar persianDa = new PersianCalendar();
                //yek int baraye furush haye emruz
                int TodayListSale = 0;
                //baraye bedast avardane ebtedaye ruz bayad time alan ro menhaye time alan konim ta ebtedaye ruz bedast biad
                //bad az un meghdar ta time alan mishe bazeye furushe emruzemun
                //in time ba saat server tanzim mishe pas yani bayad server saatesh bayad dorost bashe

                var FirstTime = persianDa.GetHour(DateTime.Now) - persianDa.GetHour(DateTime.Now);
                foreach (var item in db.Sefareshat)
                {
                    //factor haye infurushande ke pardakhte nahayi shodan
                   if (item.clientId==ClientId&&item.isFinaly==true)
                    {
                        //beyne bazeye zamani ebtedaye ruz ta allan bud
                        if (persianDa.GetHour(item.CreateDate)>= FirstTime&& persianDa.GetYear(item.CreateDate)== persianDa.GetYear(DateTime.Now)&& persianDa.GetDayOfYear(item.CreateDate)== persianDa.GetDayOfYear(DateTime.Now))
                        {
                            TodayListSale += item.Sum;
                        }

                    }
                }
                //in viewbag baraye namayeshe furush emruz
                ViewBag.TodayListSale = TodayListSale;
                #endregion

                #region Amare Tarakoneshhaye In Mah
                //aval ebtedaye in mah ro be shamsi be dast miarim
                PersianCalendar persian = new PersianCalendar();
                int ThisMonthSaleList = 0;

                //ebtedaye in mah
                var Emruz = persian.GetDayOfMonth(DateTime.Now);
                //ebtedaye in mah yani alan menhaye alan:)
                //pas
                var EbtedayeMah = Emruz - Emruz;
                
                foreach (var item in db.Sefareshat)
                {
                    if (item.clientId==ClientId&&item.isFinaly==true)
                    {
                        if (persian.GetDayOfMonth(item.CreateDate)>=EbtedayeMah&& persian.GetDayOfMonth(item.CreateDate)<=Emruz && persian.GetYear(item.CreateDate) == persian.GetYear(DateTime.Now) && persian.GetMonth(item.CreateDate) == persian.GetMonth(DateTime.Now))
                        {
                            ThisMonthSaleList += item.Sum;
                        }
                    }

                }

                ViewBag.ThisMonthListSale = ThisMonthSaleList;

                #endregion


                return View(db.Sefareshat.Where(i => i.clientId == ClientId && i.isFinaly == true).OrderByDescending(e => e.CreateDate).ToList().OrderByDescending(r=>r.CreateDate).ToPagedList(page ?? 1,10));
            }
            return HttpNotFound();
        }



        [Authorize]
        [RemoveSpaceFilter]
        public ActionResult DetailsOrders(string orderid, string ClientId,int? page)
        {
            //faghat baraye khode saheb page va super admin namayesh bede
            ApplicationDbContext db = new Models.ApplicationDbContext();
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                //yek list az modelemun besaz baraye namayesh factor be furushande
                List<FactorVM> FirstList = new List<FactorVM>();
                //ebteda yek list az riz sefareshat be ma bede(tamamie mahsulate kharidari shode az vitrin ha anja hastand)
                var ThisFactorList = db.RizSefareshat.Where(e => e.SefareshatId == orderid).ToList();
                //boro dakhele list estekhraj shode
                //ebteda product va moreoroduct budanesh ro moshakhas kon
                foreach (var item in ThisFactorList)
                {
                    //yek nemune az factor besaz
                    FactorVM PItem = new FactorVM();
                    //pas mahsul product ast va gheire tekrari ast
                    //ghesmate dovome if baraye in hast ke masalan nanevise 2 ta kif enghadr tu do radif
                    //jeloye recorde tekrari tu jadval ro migire
                    if (item.ProductId != null && FirstList.Where(p => p.ProductId == item.ProductId).FirstOrDefault() == null)
                    {
                        PItem.ImageAddress = Replace.returnProductPicAddress(item.ProductId);
                        PItem.ProductId = item.ProductId;
                        PItem.OrderId = item.SefareshatId;
                        PItem.Price = Convert.ToInt32(Replace.returnProductPrice(item.ProductId));
                        PItem.Title = Replace.returnProductName(item.ProductId);
                        PItem.Count = ThisFactorList.Where(r => r.ProductId == item.ProductId).ToList().Count();
                        PItem.Sum = Convert.ToInt32(PItem.Count) * (Convert.ToInt32(Replace.returnProductPrice(item.ProductId)));

                        FirstList.Add(PItem);

                    }

                    //pas mahsul moreproduct ast
                    if (item.MoreProducstId != null && FirstList.Where(p => p.MoreProductId == item.MoreProducstId).FirstOrDefault() == null)
                    {
                        PItem.ImageAddress = Replace.returnMoreProductPicAddress(item.MoreProducstId);
                        PItem.MoreProductId = item.MoreProducstId;
                        PItem.OrderId = item.SefareshatId;
                        PItem.Price = Convert.ToInt32(Replace.returnMoreProductPrice(item.MoreProducstId));
                        PItem.Title = Replace.returnMoreProductName(item.MoreProducstId);
                        PItem.Count = ThisFactorList.Where(r => r.MoreProducstId == item.MoreProducstId).ToList().Count();
                        PItem.Sum = Convert.ToInt32(PItem.Count) * (Convert.ToInt32(Replace.returnMoreProductPrice(item.MoreProducstId)));

                        FirstList.Add(PItem);

                    }
                }
                //کد پیگیری سیستمی
                ViewBag.tracking_code = db.Sefareshat.Where(r => r.clientId == ClientId && r.Id == orderid).FirstOrDefault().Tracking_Code;
                ViewBag.Client = ClientId;

                #region اطلاعات خریدار

                //bia factore asli ro bekesh birun ke azash id moshtari ro estekhraj kon
                var moshtariId = db.Sefareshat.Where(e => e.Id == orderid && e.clientId == ClientId).FirstOrDefault().moshtariId;
                //hala bia etelaate un moshtari ro bekesh birun az table moshtari
                moshtari MoshrtariItem = new moshtari();
                MoshrtariItem = db.moshtari.Where(r => r.Id == moshtariId).FirstOrDefault();
                ViewBag.moshtari = MoshrtariItem;
                ViewBag.tozihateMoshtari = db.Sefareshat.Where(e => e.Id == orderid && e.clientId == ClientId).FirstOrDefault().Tozihat;

                //location moshtari
                if (MoshrtariItem.LocationLatitude != null && MoshrtariItem.LocationLongitude != null)
                {
                    ViewBag.tt = "[" + MoshrtariItem.LocationLatitude + "," + MoshrtariItem.LocationLongitude + "]";
                }


                #endregion


                //in baraye in hast ke bad az inke vared riz factor shod dobare bere be safheye ghablie factor ha
                ViewBag.pageCount = page;


                return View(FirstList);
            }
            return HttpNotFound();

        }




        [Authorize]
        [RemoveSpaceFilter]
        public ActionResult PaymentsList(string ClientId)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                #region isSeenOrders
                //check for seen orders 
                //false is order
                //true is not order
                var check = db.Sefareshat.Where(o => o.isSeen == false && o.clientId == ClientId).FirstOrDefault();
                if (check != null)
                {
                    ViewBag.isSeen = "false";
                }
                #endregion

                ViewBag.clientid = ClientId;
                ViewBag.PaymentsList = db.factors.Where(i => i.ClienId == ClientId).OrderByDescending(s => s.CreateDate).ToList();
                return View();
            }
            return HttpNotFound();
        }



        [Authorize]
        public ActionResult DeleteOpinion(string ClientId, string opinionId)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            //اگر فرد لاگین در صفحه خودش است برای امنیت هست این شرط که در تمام اکشن های /
            //کاربران نوشته شده تا کسی سواستفاده نکنه و اطلاعات فرد رجیستر دیگه رو دخل و تصرف نداشته باشه

            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                var searchItem = db.Opinion.Where(e => e.Id == opinionId).FirstOrDefault();
                db.Opinion.Remove(searchItem);
                db.SaveChanges();
                ViewBag.clientid = ClientId;
                return RedirectToAction("Opinion", "Account", new { ClientId = ClientId });
            }
            return HttpNotFound();
        }


        //in action dg estefade nenishe
        //badan pakesh kon ba test ke motmaen shodi
        [Authorize]
        public ActionResult DeleteOrders(string ClientId, string orderid)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            //اگر فرد لاگین در صفحه خودش است، برای امنیت هست این شرط که در تمام اکشن های /
            //کاربران نوشته شده تا کسی سواستفاده نکنه و اطلاعات فرد رجیستر شده،اطلاعات فرد رجیستر شده دیگه رو دخل و تصرف نداشته باشه

            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                var searchItem = db.Sefareshat.Where(e => e.Id == orderid).FirstOrDefault();
                db.Sefareshat.Remove(searchItem);
                db.SaveChanges();
                ViewBag.clientid = ClientId;

                //halabayad pdf factoresham pak beshe
                //inbaraye local hast
                if (searchItem.FactorAddres != null)
                {
                    if (System.IO.File.Exists(Server.MapPath(searchItem.FactorAddres)))
                    {
                        System.IO.File.Delete(Server.MapPath(searchItem.FactorAddres));
                    }

                }
                //


                return RedirectToAction("Orders", "Account", new { ClientId = ClientId });
            }
            return HttpNotFound();
        }


        [Authorize]
        [RemoveSpaceFilter]
        public ActionResult ClientClub(string ClientId)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                #region isSeenOrders
                //check for seen orders 
                //false is order
                //true is not order
                var check = db.Sefareshat.Where(o => o.isSeen == false && o.clientId == ClientId).FirstOrDefault();
                if (check != null)
                {
                    ViewBag.isSeen = "false";
                }
                #endregion

                ViewBag.clientid = ClientId;
                ViewBag.ClubList = db.CustomerClub.Where(i => i.ClienId == ClientId).ToList();
                return View();
            }
            return HttpNotFound();
        }

        [Authorize]
        public ActionResult DeleteClub(string ClientId, string opinionId)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                var searchItem = db.CustomerClub.Where(e => e.Id == opinionId).FirstOrDefault();
                db.CustomerClub.Remove(searchItem);
                db.SaveChanges();
                ViewBag.clientid = ClientId;
                return RedirectToAction("ClientClub", "Account", new { ClientId = ClientId });
            }
            return HttpNotFound();
        }

        [Authorize]
        public ActionResult Contacts(string ClientId, string error)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                #region isSeenOrders
                //check for seen orders 
                //false is order
                //true is not order
                var check = db.Sefareshat.Where(o => o.isSeen == false && o.clientId == ClientId).FirstOrDefault();
                if (check != null)
                {
                    ViewBag.isSeen = "false";
                }
                #endregion

                ViewBag.clientid = ClientId;
                ViewBag.Contacts = db.Contacts.Where(i => i.ClienId == ClientId).ToList();
                if (error != null)
                {
                    ViewBag.phone = "لطفا تنها شماره موبایل وارد نمایید";
                }
                return View();
            }
            return HttpNotFound();
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [RemoveSpaceFilter]
        public ActionResult AddContact(string ClientId, string Description, string PhoneNumber)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {


                var r = new Regex(@"^[0-9]{11}$");
                if (!r.IsMatch(PhoneNumber))
                {

                    return RedirectToAction("Contacts", "Account", new { ClientId = ClientId, error = "1" });
                }

                if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
                {
                    Contacts item = new Contacts();
                    item.PhoneNumber = PhoneNumber;
                    item.Description = Description;
                    item.ClienId = ClientId;
                    db.Contacts.Add(item);
                    db.SaveChanges();

                    ViewBag.clientid = ClientId;


                }
                return RedirectToAction("Contacts", "Account", new { ClientId = ClientId });
            }
            return HttpNotFound();
        }


        [Authorize]
        public ActionResult DeleteContacts(string ClientId, string ContactId)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                var searchItem = db.Contacts.Where(e => e.Id == ContactId).FirstOrDefault();
                db.Contacts.Remove(searchItem);
                db.SaveChanges();
                ViewBag.clientid = ClientId;
                return RedirectToAction("Contacts", "Account", new { ClientId = ClientId });
            }
            return HttpNotFound();
        }

        [Authorize]
        public ActionResult SmsSend(string ClientId, string Sent, string e)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();

            //liste payamak
            List<baladcheeProduct> ListOf = new List<baladcheeProduct>();
            //نوشتم مخالف 20 که شارژ و تمدید سالانه بلدچی رو توی خرید اس ام اس ها
            //نشون نده چون با شارژ و تمدید بلدچی 20 تا پیامک هدیه میدم
            //چون جدول جفتشوت یکی اینطوی فعلا تشخیص میده
            foreach (var item in db.baladcheeProduct.Where(s => s.SmsCount != 20))
            {
                ListOf.Add(item);
            }
            ViewBag.list = ListOf;
            if (e == "A")
            {
                ViewBag.e1 = "هیچ شماره ای در سیستم شما ثبت نیست";
            }
            if (e == "B")
            {
                ViewBag.e1 = "تعداد کارکتر های وارد شده مجاز نیست";
            }

            if (e == "C")
            {
                ViewBag.e1 = "لطفا ابتدا بلدچی خود را تمدید فرمایید، برای این کار به صفحه ویترین خود رفته و گزینه پیام را در نوار بالا بزنید";
            }

            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {

                #region isSeenOrders
                //check for seen orders 
                //false is order
                //true is not order
                var check = db.Sefareshat.Where(o => o.isSeen == false && o.clientId == ClientId).FirstOrDefault();
                if (check != null)
                {
                    ViewBag.isSeen = "false";
                }
                #endregion

                ViewBag.ClientSmsCount = db.Client.Where(s => s.Id == ClientId).FirstOrDefault().smsCount;
                ViewBag.ClubList = db.CustomerClub.Where(s => s.ClienId == ClientId).Count();
                ViewBag.ContactList = db.Contacts.Where(s => s.ClienId == ClientId).Count();
                ViewBag.sum = db.Contacts.Where(s => s.ClienId == ClientId).Count() + db.CustomerClub.Where(s => s.ClienId == ClientId).Count();
                ViewBag.clientid = ClientId;

                //ersal shode ast
                if (Sent == "ok")
                {
                    ViewBag.sent = "sent";
                }
                //adame mojudi
                if (Sent == "Inventory")
                {
                    ViewBag.sent = "Inventory";
                }
                return View();
            }
            return HttpNotFound();
        }


        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult SendEvent(string ClientId, string Content, string checkContact, string checkClub)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            ViewBag.clientid = ClientId;
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {


                #region isSeenOrders
                //check for seen orders 
                //false is order
                //true is not order
                var check = db.Sefareshat.Where(o => o.isSeen == false && o.clientId == ClientId).FirstOrDefault();
                if (check != null)
                {
                    ViewBag.isSeen = "false";
                }
                #endregion


                if (Content.Contains("کوس") || Content.Contains("کیر") || Content.Contains("خایه") || Content.Contains("کون") || Content.Contains("جنده") || Content.Contains("میکشم") || Content.Contains("گاییدم") || Content.Contains("سکس"))
                {
                    return HttpNotFound();
                }

                //agar hich shomarei nadasht va ersal anjam dad
                if (db.Contacts.Where(u => u.ClienId == ClientId).ToList().Count < 1 && db.CustomerClub.Where(u => u.ClienId == ClientId).ToList().Count < 1)
                {


                    return RedirectToAction("SmsSend", "Account", new { ClientId = ClientId, e = "A" });
                }

                if (Content.Length > 71 || Content.Length < 3)
                {
                    //ersal nashavad chon saghf ersale payamak 70 karekter ast
                    //va kamtar az 4 ta bud
                    //mitavanad dobar ersal dashte bashad agar tul matnash bishtar ast


                    return RedirectToAction("SmsSend", "Account", new { ClientId = ClientId, e = "B" });
                }
                //agar hardo tik ra vared konid
                //ham bekhad bayare bashgahe moshtarianesh va ham mokhtabash ersal dashte bashe
                if (checkClub != null && checkContact != null)
                {
                    var ClientItem = db.Client.Where(s => s.Id == ClientId).FirstOrDefault();
                    var ClubList = db.CustomerClub.Where(s => s.ClienId == ClientId).ToList();
                    var ContactList = db.Contacts.Where(s => s.ClienId == ClientId).ToList();

                    //agar mojudi dasht nesbat be tedad ersal
                    if (Convert.ToInt32(ClientItem.smsCount) >= ClubList.Count() + ContactList.Count)
                    {
                        //agar aslan shomarei vojud dasht
                        if (ClubList.Count + ContactList.Count > 0)
                        {



                            List<string> listOfComplete = new List<string>();
                            //majmooe hardo
                            //clublist
                            for (int i = 0; i < ClubList.Count; i++)
                            {
                                listOfComplete.Add(Replace.withOutZiro(ClubList[i].PhoneNumber));
                            }
                            //contact
                            for (int i = 0; i < ContactList.Count; i++)
                            {
                                listOfComplete.Add(Replace.withOutZiro(ContactList[i].PhoneNumber));
                            }

                            var forCountFirst = listOfComplete.Count / 100;
                            var forCountSecod = listOfComplete.Count % 100;

                            //halate aval :
                            //agar baghimande nadasht yani faghat mazrabi az 100 bud
                            //100 200 300 400 ...1000 va...
                            if (forCountSecod == 0)
                            {


                                for (int i = 1; i <= forCountFirst; i++)
                                {
                                    string[] p = new string[0];
                                    meli.Send sender = new Send();


                                    for (int j = ((i * 100) - 100); j < i * 100; j++)
                                    {
                                        Array.Resize(ref p, p.Length + 1);
                                        p[p.Length - 1] = listOfComplete[j];
                                        ClientItem.smsCount = ClientItem.smsCount - 1;

                                    }

                                    sender.SendSimpleSMS("9017037365", "Aa30081370@", p, "50001060009775", Content, false);
                                    System.Threading.Thread.Sleep(5000);


                                }
                                SMSBox sms = new Core.SMSBox();
                                sms.Content = Content;
                                sms.ClienId = ClientId;
                                db.SMSBox.Add(sms);

                                db.SaveChanges();
                                return RedirectToAction("SmsSend", "Account", new { ClientId = ClientId, Sent = "ok" });

                            }



                            //halate dovom :
                            //agar baghimande dasht va khareje ghesmat nadasht yani tedad kochaktar az 100 ast
                            //28 67 92 11 ...ta ghabl az 100
                            if (forCountFirst == 0)
                            {

                                string[] p = new string[0];
                                meli.Send sender = new Send();


                                for (int j = 0; j < 100; j++)
                                {
                                    if (listOfComplete.Count > j)
                                    {
                                        Array.Resize(ref p, p.Length + 1);
                                        p[p.Length - 1] = listOfComplete[j];
                                        ClientItem.smsCount = ClientItem.smsCount - 1;

                                    }
                                    else
                                    {
                                        break;
                                    }

                                }

                                SMSBox sms = new Core.SMSBox();
                                sms.Content = Content;
                                sms.ClienId = ClientId;
                                db.SMSBox.Add(sms);
                                db.SaveChanges();
                                sender.SendSimpleSMS("9017037365", "Aa30081370@", p, "50001060009775", Content, false);
                                System.Threading.Thread.Sleep(1000);

                                return RedirectToAction("SmsSend", "Account", new { ClientId = ClientId, Sent = "ok" });
                            }


                            //halate sevom
                            //por halat tarin ast inke tedad ham bishtar az 100 ast 
                            //124 345 87876 ...
                            if (forCountFirst != 0 && forCountSecod != 0)
                            {

                                for (int i = 1; i <= forCountFirst; i++)
                                {
                                    string[] p = new string[0];
                                    meli.Send sender1 = new Send();


                                    for (int j = ((i * 100) - 100); j < i * 100; j++)
                                    {
                                        Array.Resize(ref p, p.Length + 1);
                                        p[p.Length - 1] = listOfComplete[j];
                                        ClientItem.smsCount = ClientItem.smsCount - 1;

                                    }
                                    sender1.SendSimpleSMS("9017037365", "Aa30081370@", p, "50001060009775", Content, false);
                                    System.Threading.Thread.Sleep(5000);
                                }



                                string[] pn = new string[0];
                                meli.Send sender = new Send();

                                for (int j = (forCountFirst * 100); j < listOfComplete.Count; j++)
                                {

                                    Array.Resize(ref pn, pn.Length + 1);
                                    pn[pn.Length - 1] = listOfComplete[j];
                                    ClientItem.smsCount = ClientItem.smsCount - 1;



                                }
                                SMSBox sms = new Core.SMSBox();
                                sms.Content = Content;
                                sms.ClienId = ClientId;
                                db.SMSBox.Add(sms);
                                db.SaveChanges();
                                sender.SendSimpleSMS("9017037365", "Aa30081370@", pn, "50001060009775", Content, false);
                                System.Threading.Thread.Sleep(1000);

                                return RedirectToAction("SmsSend", "Account", new { ClientId = ClientId, Sent = "ok" });
                            }


                        }
                    }
                    ViewBag.e1 = "تعداد موجودی پیامک شما نسبت به شماره های ارسالی کم میباشد";
                    return RedirectToAction("SmsSend", "Account", new { ClientId = ClientId, sent = "Inventory" });
                }




                ////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////////////


                if (checkContact != null && checkClub == null)
                {
                    var ClientItem = db.Client.Where(s => s.Id == ClientId).FirstOrDefault();
                    var ContactList = db.Contacts.Where(s => s.ClienId == ClientId).ToList();

                    //agar mojudi dasht nesbat be tedad ersal
                    if (Convert.ToInt32(ClientItem.smsCount) >= ContactList.Count)
                    {
                        //agar aslan shomarei vojud dasht
                        if (ContactList.Count > 0)
                        {



                            List<string> listOfComplete = new List<string>();

                            //contact
                            for (int i = 0; i < ContactList.Count; i++)
                            {
                                listOfComplete.Add(Replace.withOutZiro(ContactList[i].PhoneNumber));
                            }

                            var forCountFirst = listOfComplete.Count / 100;
                            var forCountSecod = listOfComplete.Count % 100;

                            //halate aval :
                            //agar baghimande nadasht yani faghat mazrabi az 100 bud
                            //100 200 300 400 ...1000 va...
                            if (forCountSecod == 0)
                            {


                                for (int i = 1; i <= forCountFirst; i++)
                                {
                                    string[] p = new string[0];
                                    meli.Send sender = new Send();


                                    for (int j = ((i * 100) - 100); j < i * 100; j++)
                                    {
                                        Array.Resize(ref p, p.Length + 1);
                                        p[p.Length - 1] = listOfComplete[j];
                                        ClientItem.smsCount = ClientItem.smsCount - 1;

                                    }
                                    sender.SendSimpleSMS("9017037365", "Aa30081370@", p, "50001060009775", Content, false);
                                    System.Threading.Thread.Sleep(5000);
                                }
                                SMSBox sms = new Core.SMSBox();
                                sms.Content = Content;
                                sms.ClienId = ClientId;
                                db.SMSBox.Add(sms);
                                db.SaveChanges();
                                return RedirectToAction("SmsSend", "Account", new { ClientId = ClientId, Sent = "ok" });

                            }



                            //halate dovom :
                            //agar baghimande dasht va khareje ghesmat nadasht yani tedad kochaktar az 100 ast
                            //28 67 92 11 ...ta ghabl az 100
                            if (forCountFirst == 0)
                            {

                                string[] p = new string[0];
                                meli.Send sender = new Send();


                                for (int j = 0; j < 100; j++)
                                {
                                    if (listOfComplete.Count > j)
                                    {
                                        Array.Resize(ref p, p.Length + 1);
                                        p[p.Length - 1] = listOfComplete[j];
                                        ClientItem.smsCount = ClientItem.smsCount - 1;

                                    }
                                    else
                                    {
                                        break;
                                    }

                                }

                                SMSBox sms = new Core.SMSBox();
                                sms.Content = Content;
                                sms.ClienId = ClientId;
                                db.SMSBox.Add(sms);
                                db.SaveChanges();
                                sender.SendSimpleSMS("9017037365", "Aa30081370@", p, "50001060009775", Content, false);
                                System.Threading.Thread.Sleep(1000);

                                return RedirectToAction("SmsSend", "Account", new { ClientId = ClientId, Sent = "ok" });
                            }


                            //halate sevom
                            //por halat tarin ast inke tedad ham bishtar az 100 ast 
                            //124 345 87876 ...
                            if (forCountFirst != 0 && forCountSecod != 0)
                            {

                                for (int i = 1; i <= forCountFirst; i++)
                                {
                                    string[] p = new string[0];
                                    meli.Send sender1 = new Send();


                                    for (int j = ((i * 100) - 100); j < i * 100; j++)
                                    {
                                        Array.Resize(ref p, p.Length + 1);
                                        p[p.Length - 1] = listOfComplete[j];
                                        ClientItem.smsCount = ClientItem.smsCount - 1;

                                    }
                                    sender1.SendSimpleSMS("9017037365", "Aa30081370@", p, "50001060009775", Content, false);
                                    System.Threading.Thread.Sleep(5000);
                                }



                                string[] pn = new string[0];
                                meli.Send sender = new Send();

                                for (int j = (forCountFirst * 100); j < listOfComplete.Count; j++)
                                {

                                    Array.Resize(ref pn, pn.Length + 1);
                                    pn[pn.Length - 1] = listOfComplete[j];
                                    ClientItem.smsCount = ClientItem.smsCount - 1;



                                }
                                SMSBox sms = new Core.SMSBox();
                                sms.Content = Content;
                                sms.ClienId = ClientId;
                                db.SMSBox.Add(sms);
                                db.SaveChanges();
                                sender.SendSimpleSMS("9017037365", "Aa30081370@", pn, "50001060009775", Content, false);
                                System.Threading.Thread.Sleep(1000);

                                return RedirectToAction("SmsSend", "Account", new { ClientId = ClientId, Sent = "ok" });
                            }


                        }
                    }
                    ViewBag.e1 = "تعداد موجودی پیامک شما نسبت به شماره های ارسالی کم میباشد";

                    return RedirectToAction("SmsSend", "Account", new { ClientId = ClientId, sent = "Inventory" });
                }



                ////////////////////////////////////////////////////////////////////////////////////
                ///////////////////////////////////////////////////////////////////////////////////
                //////////////////////////////////////////////////////////////////////////////////



                if (checkContact == null && checkClub != null)
                {
                    var ClientItem = db.Client.Where(s => s.Id == ClientId).FirstOrDefault();
                    var ClubList = db.CustomerClub.Where(s => s.ClienId == ClientId).ToList();


                    //agar mojudi dasht nesbat be tedad ersal
                    if (Convert.ToInt32(ClientItem.smsCount) >= ClubList.Count())
                    {
                        //agar aslan shomarei vojud dasht
                        if (ClubList.Count > 0)
                        {



                            List<string> listOfComplete = new List<string>();
                            //majmooe hardo
                            //clublist
                            for (int i = 0; i < ClubList.Count; i++)
                            {
                                listOfComplete.Add(Replace.withOutZiro(ClubList[i].PhoneNumber));
                            }


                            var forCountFirst = listOfComplete.Count / 100;
                            var forCountSecod = listOfComplete.Count % 100;

                            //halate aval :
                            //agar baghimande nadasht yani faghat mazrabi az 100 bud
                            //100 200 300 400 ...1000 va...
                            if (forCountSecod == 0)
                            {


                                for (int i = 1; i <= forCountFirst; i++)
                                {
                                    string[] p = new string[0];
                                    meli.Send sender = new Send();


                                    for (int j = ((i * 100) - 100); j < i * 100; j++)
                                    {
                                        Array.Resize(ref p, p.Length + 1);
                                        p[p.Length - 1] = listOfComplete[j];
                                        ClientItem.smsCount = ClientItem.smsCount - 1;

                                    }
                                    sender.SendSimpleSMS("9017037365", "Aa30081370@", p, "50001060009775", Content, false);
                                    System.Threading.Thread.Sleep(5000);
                                }

                                SMSBox sms = new Core.SMSBox();
                                sms.Content = Content;
                                sms.ClienId = ClientId;
                                db.SMSBox.Add(sms);
                                db.SaveChanges();
                                return RedirectToAction("SmsSend", "Account", new { ClientId = ClientId, Sent = "ok" });

                            }



                            //halate dovom :
                            //agar baghimande dasht va khareje ghesmat nadasht yani tedad kochaktar az 100 ast
                            //28 67 92 11 ...ta ghabl az 100
                            if (forCountFirst == 0)
                            {

                                string[] p = new string[0];
                                meli.Send sender = new Send();


                                for (int j = 0; j < 100; j++)
                                {
                                    if (listOfComplete.Count > j)
                                    {
                                        Array.Resize(ref p, p.Length + 1);
                                        p[p.Length - 1] = listOfComplete[j];
                                        ClientItem.smsCount = ClientItem.smsCount - 1;

                                    }
                                    else
                                    {
                                        break;
                                    }

                                }

                                SMSBox sms = new Core.SMSBox();
                                sms.Content = Content;
                                sms.ClienId = ClientId;
                                db.SMSBox.Add(sms);
                                db.SaveChanges();
                                sender.SendSimpleSMS("9017037365", "Aa30081370@", p, "50001060009775", Content, false);
                                System.Threading.Thread.Sleep(1000);

                                return RedirectToAction("SmsSend", "Account", new { ClientId = ClientId, Sent = "ok" });
                            }


                            //halate sevom
                            //por halat tarin ast inke tedad ham bishtar az 100 ast 
                            //124 345 87876 ...
                            if (forCountFirst != 0 && forCountSecod != 0)
                            {

                                for (int i = 1; i <= forCountFirst; i++)
                                {
                                    string[] p = new string[0];
                                    meli.Send sender1 = new Send();


                                    for (int j = ((i * 100) - 100); j < i * 100; j++)
                                    {
                                        Array.Resize(ref p, p.Length + 1);
                                        p[p.Length - 1] = listOfComplete[j];
                                        ClientItem.smsCount = ClientItem.smsCount - 1;

                                    }
                                    sender1.SendSimpleSMS("9017037365", "Aa30081370@", p, "50001060009775", Content, false);
                                    System.Threading.Thread.Sleep(5000);
                                }



                                string[] pn = new string[0];
                                meli.Send sender = new Send();

                                for (int j = (forCountFirst * 100); j < listOfComplete.Count; j++)
                                {

                                    Array.Resize(ref pn, pn.Length + 1);
                                    pn[pn.Length - 1] = listOfComplete[j];
                                    ClientItem.smsCount = ClientItem.smsCount - 1;



                                }
                                SMSBox sms = new Core.SMSBox();
                                sms.Content = Content;
                                sms.ClienId = ClientId;
                                db.SMSBox.Add(sms);
                                db.SaveChanges();
                                sender.SendSimpleSMS("9017037365", "Aa30081370@", pn, "50001060009775", Content, false);
                                System.Threading.Thread.Sleep(1000);

                                return RedirectToAction("SmsSend", "Account", new { ClientId = ClientId, Sent = "ok" });
                            }


                        }
                    }
                    ViewBag.e1 = "تعداد موجودی پیامک شما نسبت به شماره های ارسالی کم میباشد";

                    return RedirectToAction("SmsSend", "Account", new { ClientId = ClientId, sent = "Inventory" });
                }



                ViewBag.clientid = ClientId;
                return RedirectToAction("SmsSend", "Account", new { ClientId = ClientId });

            }
            return HttpNotFound();
        }


        [Authorize]
        [RemoveSpaceFilter]
        public ActionResult SentBox(string ClientId)
        {

            ApplicationDbContext db = new Models.ApplicationDbContext();
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {

                #region isSeenOrders
                //check for seen orders 
                //false is order
                //true is not order
                var check = db.Sefareshat.Where(o => o.isSeen == false && o.clientId == ClientId).FirstOrDefault();
                if (check != null)
                {
                    ViewBag.isSeen = "false";
                }
                #endregion

                ViewBag.clientid = ClientId;
                //agar mokhalef 1 bud yani karbar pakesh nakarde bud
                ViewBag.SMSBox = db.SMSBox.Where(i => i.ClienId == ClientId && i.isShow == null).ToList();
                return View();
            }
            return HttpNotFound(); ;
        }




        [Authorize]
        public ActionResult DeleteSmsBox(string ClientId, string smsSentId)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                var searchItem = db.SMSBox.Where(e => e.Id == smsSentId).FirstOrDefault();
                searchItem.isShow = "1";
                db.SaveChanges();
                ViewBag.clientid = ClientId;
                return RedirectToAction("SentBox", "Account", new { ClientId = ClientId });
            }
            return HttpNotFound();
        }

        [Authorize]
        public ActionResult SaleCode(string ClientId, string ms)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {

                #region isSeenOrders
                //check for seen orders 
                //false is order
                //true is not order
                var check = db.Sefareshat.Where(o => o.isSeen == false && o.clientId == ClientId).FirstOrDefault();
                if (check != null)
                {
                    ViewBag.isSeen = "false";
                }
                #endregion

                var item = db.Client.Where(u => u.Id == ClientId).FirstOrDefault();
                ViewBag.code = item.SaleCode;
                ViewBag.percent = item.Percent;
                if (ms != null)
                {
                    ViewBag.ms = "کد تخفیف با موفقیت ثبت شد";
                }
                ViewBag.clientid = ClientId;
                return View();
            }
            return HttpNotFound();
        }





        [Authorize]
        public ActionResult unavailable(string ClientId)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {

                #region isSeenOrders
                //check for seen orders 
                //false is order
                //true is not order
                var check = db.Sefareshat.Where(o => o.isSeen == false && o.clientId == ClientId).FirstOrDefault();
                if (check != null)
                {
                    ViewBag.isSeen = "false";
                }
                #endregion


                //bia kalahaye namoojood ro behem neshun bede
                List<KalaNamojudViewModel> ListeNamujud = new List<KalaNamojudViewModel>();
                var AdameMojudiMore = db.MoreProducts.Where(e => e.ClienId == ClientId && e.Display != "notsee" && e.count < 1).ToList();
                var AdameMojudiProduct = db.Product.Where(e => e.ClienId == ClientId && e.Display != "notsee" && e.count < 1).ToList();
                foreach (var AdameMojudiMoreItem in AdameMojudiMore)
                {
                    KalaNamojudViewModel Item = new KalaNamojudViewModel();
                    Item.ClientId = AdameMojudiMoreItem.ClienId;
                    Item.PicAddress = AdameMojudiMoreItem.pPicAddres;
                    Item.KalaName = AdameMojudiMoreItem.pTitle;
                    Item.MoreProduct = "More";
                    ListeNamujud.Add(Item);

                }
                foreach (var AdameMojudiProductItem in AdameMojudiProduct)
                {
                    KalaNamojudViewModel Item = new KalaNamojudViewModel();
                    Item.ClientId = AdameMojudiProductItem.ClienId;
                    Item.PicAddress = AdameMojudiProductItem.pPicAddres;
                    Item.KalaName = AdameMojudiProductItem.pTitle;
                    Item.Product = "Product";
                    ListeNamujud.Add(Item);

                }

                ViewBag.ListeNamujud = ListeNamujud;


                ViewBag.clientid = ClientId;
                return View();
            }
            return HttpNotFound();
        }








        [Authorize]
        [ValidateAntiForgeryToken]
        [RemoveSpaceFilter]
        public ActionResult AddSaleCode(string ClientId, string SaleCode, string Percent)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {

                //agar khali bud percent bia be 0 tabdil kon ke unvar be moshkel nakhorim sare emale code
                if (Percent == null || Percent == "")
                {
                    Percent = "0";
                }


                if (SaleCode.Length <= 6 && Convert.ToInt32(Percent) >= 0 && Convert.ToInt32(Percent) <= 99)
                {
                    var item = db.Client.Where(u => u.Id == ClientId).FirstOrDefault();
                    item.SaleCode = SaleCode;
                    item.Percent = Percent;
                    db.SaveChanges();
                    ViewBag.clientid = ClientId;
                    //A yani code takhfifesh sabt shod
                    return RedirectToAction("SaleCode", "Account", new { ClientId = ClientId, ms = "A" });
                }
            }
            return HttpNotFound();
        }
        [Authorize]
        public ActionResult DeleteSaleCode(string ClientId)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {

                var item = db.Client.Where(u => u.Id == ClientId).FirstOrDefault();
                item.SaleCode = "";
                item.Percent = "";
                db.SaveChanges();
                ViewBag.clientid = ClientId;
                //A yani code takhfifesh sabt shod
                return RedirectToAction("SaleCode", "Account", new { ClientId = ClientId });

            }
            return HttpNotFound();
        }




        [Authorize]
        public ActionResult BaladcheeQR(string ClientId)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            ViewBag.clientid = ClientId;
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {

                #region isSeenOrders
                //check for seen orders 
                //false is order
                //true is not order
                var check = db.Sefareshat.Where(o => o.isSeen == false && o.clientId == ClientId).FirstOrDefault();
                if (check != null)
                {
                    ViewBag.isSeen = "false";
                }
                #endregion


                return View();
            }
            return HttpNotFound();
        }



        [Authorize]
        public ActionResult ManagementApk(string ClientId)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            ViewBag.clientid = ClientId;
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                #region isSeenOrders
                //check for seen orders 
                //false is order
                //true is not order
                var check = db.Sefareshat.Where(o => o.isSeen == false && o.clientId == ClientId).FirstOrDefault();
                if (check != null)
                {
                    ViewBag.isSeen = "false";
                }
                #endregion

                return View();
            }
            return HttpNotFound();
        }



        [Authorize]
        [ValidateAntiForgeryToken]

        public FileResult ManagementApkDownload(string ClientId)
        {

            ApplicationDbContext db = new Models.ApplicationDbContext();
            ViewBag.clientid = ClientId;
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {

                byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/apks/baladcheeManagement.apk"));
                var response = new FileContentResult(fileBytes, "application/octet-stream");
                response.FileDownloadName = "baladcheeM.apk";
                return response;
            }

            byte[] fileBytes2 = System.IO.File.ReadAllBytes("");
            var response2 = new FileContentResult(fileBytes2, "application/octet-stream");
            response2.FileDownloadName = "none.png";
            return response2;

        }



        [Authorize]
        [ValidateAntiForgeryToken]

        public FileResult QrDownload(string ClientId)
        {

            ApplicationDbContext db = new Models.ApplicationDbContext();
            ViewBag.clientid = ClientId;
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                var file = db.Client.Where(e => e.Id == ClientId).FirstOrDefault().QrImageAddres;
                byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath(file));
                var response = new FileContentResult(fileBytes, "application/octet-stream");
                response.FileDownloadName = "baladcheeQR.png";
                return response;
            }

            byte[] fileBytes2 = System.IO.File.ReadAllBytes("");
            var response2 = new FileContentResult(fileBytes2, "application/octet-stream");
            response2.FileDownloadName = "none.png";
            return response2;

        }



        public FileResult RahyabQrDownload(string ClientId)
        {

            ApplicationDbContext db = new Models.ApplicationDbContext();
            ViewBag.clientid = ClientId;
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                var file = db.Client.Where(e => e.Id == ClientId).FirstOrDefault().QrLocation;
                byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath(file));
                var response = new FileContentResult(fileBytes, "application/octet-stream");
                response.FileDownloadName = "RahyabBaladcheeQR.png";
                return response;
            }

            byte[] fileBytes2 = System.IO.File.ReadAllBytes("");
            var response2 = new FileContentResult(fileBytes2, "application/octet-stream");
            response2.FileDownloadName = "none.png";
            return response2;

        }


        //کلا برای پرداخت در ام وی سی دوتا اکشن لازم است یکی همین پیمنت
        //و دومی پایینیش که وریفای نام داره که نشون میده پرداخت نهایی شده یا نه
        // حالا من از دوجا به این رکوئست میدم یکی شارژ اس ام اس ها و دیگری شارژ سالانه بلدچی
        //حالا برای اینکه تفاوت ها و کار های مربوط به هرکدوم رو تشخیص بدم
        //یه فلگ به نام "فرام ویترین" گذاشتم که یعنی وقتی پر بود ، طرف داره بلدچی رو تمدید میکنه در غیر این
        //صورت داره پیامک شارژ میکنه که اونم از ویوی شو میاد 
        public ActionResult Payment(string bProductId, string ClientId, string FromVitrin)
        {



            if (bProductId == null || bProductId == "")
            {
                ViewBag.clientid = ClientId;
                return View();
            }

            ApplicationDbContext db = new Models.ApplicationDbContext();
            var bproduct = db.baladcheeProduct.Where(u => u.Id == bProductId).FirstOrDefault();

            #region isSeenOrders
            //check for seen orders 
            //false is order
            //true is not order
            var check = db.Sefareshat.Where(o => o.isSeen == false && o.clientId == ClientId).FirstOrDefault();
            if (check != null)
            {
                ViewBag.isSeen = "false";
            }
            #endregion

            factor factor = new factor();
            factor.Price = bproduct.Price;
            factor.IsFinaly = false;
            factor.Description = bproduct.Name;
            factor.ClienId = ClientId;
            //اگر برای تمدید بلدچی بود
            //بیست تا پیامک بهش هدیه بده و همین امر باعث میشه من بفهمم
            //برای تمدید ویترین بوده
            if (FromVitrin != null)
            {
                factor.smsSaleCount = "20";

            }

            //پس الس میشه برای شارژ پیامک
            else
            {
                //بیا ببین هنوز تو مرحله تستی ویترین یا اصلا مهلت تستش هم تموم شده یا اصلا زمان تمدیدش رسده؟
                //اگر برای اولین بار هست که وارد شده یعنی هزینه ای بابت شارژ ویترین نداده
                //شارژ نکن پیامک رو، چون باید حداقل یک بار ویترین خودش رو شارژ کنه و بعد بتونه پیامک هاش رو شارژ کنه
                //شاید یه سوالی پیش بیاد اونم این هست که میتونه بار اول ویترین رو شارژ کنه و بعد یک عالمه پیامک شارژ کنه
                //که سال دیگه هم ویترین رو شارژ نکنه ولی پیامک بفرسته!از نظر من اگر هوشش رسید اشکالی نداره و اگر هم 
                //نرسید فکرش ، پس ویترین رو شارژ میکنه و به موجودی ما هم اضافه میشه!:)
                //اینم یه چنلج باحال

                #region pardakht
                //bebin pardakht haro anjam dade ya na (safhe vitrin ro faghat)   

                //bia pardakht nashodeye hazine vitrin ro peyda kon dar ebteda
                var pointer = db.PardakhtVitrin.Where(r => r.ClienId == ClientId && r.pardakht == 0).FirstOrDefault();
                if (pointer != null)
                {
                    //bebin zamane test be payan reside?
                    //yani taze sabte nam karde va avalin pardakht ro anjam nadade pas zamane testeshe
                    if (pointer.testTime != null && pointer.End == null && pointer.testTime > DateTime.Now)
                    {


                        return RedirectToAction("SmsSend", "Account", new { ClientId = ClientId, e = "C" });

                    }

                    //zamane tamdid reside az 5 ruze ghabl etela bede
                    else if (pointer.testTime == null && pointer.End.Value.Date == DateTime.Now.AddDays(5).Date || pointer.testTime == null && pointer.End.Value.Date == DateTime.Now.AddDays(4).Date || pointer.testTime == null && pointer.End.Value.Date == DateTime.Now.AddDays(3).Date || pointer.testTime == null && pointer.End.Value.Date == DateTime.Now.AddDays(2).Date || pointer.testTime == null && pointer.End.Value.Date == DateTime.Now.AddDays(1).Date || pointer.testTime == null && pointer.End.Value.Date == DateTime.Now.Date)
                    {


                        return RedirectToAction("SmsSend", "Account", new { ClientId = ClientId, e = "C" });

                    }

                    //kolan pardakhtharo anjam nadade va safhe baste shode
                    //hala ta avalin bare sabte nameshe or badi pardakhte salanashe
                    else if (pointer.testTime < DateTime.Now && pointer.End == null || pointer.testTime == null && pointer.End < DateTime.Now)
                    {

                        return RedirectToAction("SmsSend", "Account", new { ClientId = ClientId, e = "C" });

                    }

                }
                #endregion



                factor.smsSaleCount = Convert.ToString(bproduct.SmsCount);
            }

            Random rnd = new Random();
            db.factors.Add(factor);

            db.SaveChanges();

            System.Net.ServicePointManager.Expect100Continue = false;
            zarinpal.PaymentGatewayImplementationService zp = new zarinpal.PaymentGatewayImplementationService();
            string Authority;

            //server
            int Status = zp.PaymentRequest("e1dbe04a-4a9a-11ea-b2ac-000c295eb8fc", int.Parse(bproduct.Price), bproduct.Name, "alirezaketabi70@gmail.com", "09059092414", "http://www.baladchee.com/Account/Verify/" + factor.Id, out Authority);


            //local
            //int Status = zp.PaymentRequest("e1dbe04a-4a9a-11ea-b2ac-000c295eb8fc", int.Parse(bproduct.Price), bproduct.Name, "alirezaketabi70@gmail.com", "09059092414", "http://localhost:22904/Account/Verify/" + factor.Id, out Authority);



            if (Status > 0)
            {
                Response.Redirect("https://www.zarinpal.com/pg/StartPay/" + Authority);
            }
            else
            {
                ViewBag.error = "error: " + Status;

            }
            ViewBag.clientid = ClientId;
            return View();
        }





        [AllowAnonymous]
        public ActionResult Verify(string id)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();



            var factor = db.factors.Where(d => d.Id == id).FirstOrDefault();
            var client = db.Client.Where(e => e.Id == factor.ClienId).FirstOrDefault();

            #region isSeenOrders
            //check for seen orders 
            //false is order
            //true is not order
            var check = db.Sefareshat.Where(o => o.isSeen == false && o.clientId == client.Id).FirstOrDefault();
            if (check != null)
            {
                ViewBag.isSeen = "false";
            }
            #endregion

            if (Request.QueryString["Status"] != "" && Request.QueryString["Status"] != null && Request.QueryString["Authority"] != "" && Request.QueryString["Authority"] != null)
            {
                if (Request.QueryString["Status"].ToString().Equals("OK"))
                {
                    int Amount = Convert.ToInt32(factor.Price);
                    long RefID;
                    System.Net.ServicePointManager.Expect100Continue = false;
                    zarinpal.PaymentGatewayImplementationService zp = new zarinpal.PaymentGatewayImplementationService();

                    int Status = zp.PaymentVerification("e1dbe04a-4a9a-11ea-b2ac-000c295eb8fc", Request.QueryString["Authority"].ToString(), Amount, out RefID);

                    //agar status barabar ba 100 bud yani moafaghiat amiz bude
                    if (Status > 0)
                    {
                        ViewBag.isSuccess = true;
                        ViewBag.refId = RefID;
                        factor.IsFinaly = true;
                        if (client.smsCount == null)
                        {
                            client.smsCount = 0;
                        }

                        client.smsCount = client.smsCount + Convert.ToInt32(factor.smsSaleCount);


                        //agar baraye sharj salane bud dar
                        //pardakhtvitrin sabt kon hala az koja befahme
                        //az 20 ta payamak akhe man baraye sharj baladchee
                        //20ta payamak tasmim gereftam bedam
                        if (factor.smsSaleCount == "20")
                        {   //bedehi aval ro pardakht kard
                            //pardakht 0 ast yani pardakht nashode
                            PardakhtVitrin PardakhtVitrinItem = new PardakhtVitrin();
                            PardakhtVitrinItem = db.PardakhtVitrin.Where(s => s.ClienId == client.Id && s.pardakht == 0).FirstOrDefault();
                            PardakhtVitrinItem.pardakht = 1;

                            //yek sal tamdid kon
                            PardakhtVitrin pointer = new PardakhtVitrin();
                            pointer.start = DateTime.Now;
                            pointer.End = DateTime.Now.AddYears(1);
                            pointer.ClienId = client.Id;
                            pointer.pardakht = 0;

                            db.PardakhtVitrin.Add(pointer);
                        }




                        db.SaveChanges();

                    }
                    else
                    {
                        ViewBag.Status = Status;
                    }

                }
                else
                {
                    Response.Write("Error! Authority: " + Request.QueryString["Authority"].ToString() + " Status: " + Request.QueryString["Status"].ToString());
                }
            }
            else
            {
                Response.Write("Invalid Input");
            }
            ViewBag.clientid = client.Id;
            return View();
        }




        [Authorize]
        [RemoveSpaceFilter]
        public ActionResult ReceivedSms(string ClientId, string Laghv)
        {

            ApplicationDbContext db = new Models.ApplicationDbContext();
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                //in baraye payamie ke az ChangeReceivedSms miad
                if (Laghv == "B")
                {
                    ViewBag.l = "تغییرات اعمال گردید.";
                }
                if (Laghv == "A")
                {
                    ViewBag.l = "پیامک فروش لغو گردید.";
                }


                #region isSeenOrders
                //check for seen orders 
                //false is order
                //true is not order
                var check = db.Sefareshat.Where(o => o.isSeen == false && o.clientId == ClientId).FirstOrDefault();
                if (check != null)
                {
                    ViewBag.isSeen = "false";
                }
                #endregion

                ViewBag.clientid = ClientId;

                ViewBag.isLaghv = db.Client.Where(r => r.Id == ClientId).FirstOrDefault().LaghvPayamak;
                ViewBag.supportPhone = db.Client.Where(r => r.Id == ClientId).FirstOrDefault().SupportNumber;




                return View();
            }
            return HttpNotFound(); ;
        }



        [Authorize]
        [RemoveSpaceFilter]
        public ActionResult ChangeReceivedSms(string PhoneNumber, string LaghvPayamak, string ClientId)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {

                if (LaghvPayamak == "1" && PhoneNumber != null && PhoneNumber != "")
                {
                    var myClient = db.Client.Where(e => e.Id == ClientId).FirstOrDefault();
                    myClient.LaghvPayamak = "1";
                    myClient.SupportNumber = PhoneNumber;
                    db.SaveChanges();

                    return RedirectToAction("ReceivedSms", "Account", new { ClientId = ClientId, Laghv = "B" });

                }
                //faghat null bud mige faghat bayad null bashe
                if (LaghvPayamak != "1" && LaghvPayamak == null && LaghvPayamak != "")
                {
                    var myClient = db.Client.Where(e => e.Id == ClientId).FirstOrDefault();
                    myClient.LaghvPayamak = null;
                    db.SaveChanges();

                    return RedirectToAction("ReceivedSms", "Account", new { ClientId = ClientId, Laghv = "A" });
                }

            }
            return HttpNotFound();
        }


    }

}
