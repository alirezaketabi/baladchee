using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using QRProject.Core;
using QRProject.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using QRProject.meli;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using PagedList.Mvc;
using PagedList;
using QRProject.helper;
using QRProject.FactorViewModel;

namespace QRProject.Controllers
{

    public class HomeController : Controller
    {


        private ApplicationUserManager _userManager;
        private ApplicationSignInManager _signInManager;
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




        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult TestAdd(string editor2)
        {
            return View();
        }

        [RemoveSpaceFilter]
        public ActionResult Index()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            return View();
        }

        [RemoveSpaceFilter]
        public ActionResult Workshop()
        {

            return View();

        }


        //آدرس آموزش روت اتریبیوت تو یوتویوب 
        //https://www.youtube.com/watch?v=FzqJxpnTzMM

        [AllowAnonymous]
        [RemoveSpaceFilter]
        [Route("{id}")]
        public ActionResult show(string id, string error, string order)
        {
            #region Comment
            //az action Admins dakhele AccountController miad inja
            //در این اکشن چند مورد بررسی میشود در واقع این اکشن نهایی ترین اکشن قبل از نمایش ویترین به
            //کاربر است، در ابتدا نظرات زیاد بررسی میشود که کسی اتک نزد به دیتا بیس بیش از 4 تا نظر نده کسی
            //بعد تعداد پروداکت ها یا خدمات و...خلاصه 20 تا بیشتر نتونه کالا و خدمات وارد کنه
            //یه بررسی کوچیکی هم میکنه ببینه اینی که اومده صفحه صاحب پیج هست اصلا!؟ که داشبوردو بهش نمایش بده
            //بعد آمار رو نشون میده به صاحب پیج
            //بررسی میکنه ببینه پرداخت ویترین انجام شده یا نه و مهلت به پایان رسیده یا نه
            //بعد پرداخت نکرده و مهلت تموم شده بیا صفحه رو فقط به خودش نشون بده نه به کاربرا و کیوآر اسکن کن های صفحش

            #endregion

            ViewBag.clientid = id;
            var k = User.Identity.GetUserId();
            ApplicationDbContext db = new ApplicationDbContext();
            //az samte nazarat miad(erroe) vase inke fard bish az 4bar nazar dade va baraye 
            //data base dare moshkel ijad mikone
            //meghdar bede va dar load safhe behesh begu
            if (error == "true")
            {
                ViewBag.error = "true";
            }
            //hajme video ziad bud
            if (error == "s")
            {
                ViewBag.error = "s";
            }
            //nazar bamoafaghiat sabte shode
            if (error == "NotTrue")
            {
                ViewBag.error = "NotTrue";
            }
            //agar bish az 3 ta product vared karde(az samte AddProduct miad) 
            if (error == "moreProduct")
            {
                ViewBag.error = "moreProduct";
            }

            if (order == "true")
            {
                ViewBag.order = "true";
            }

            try
            {
                var myid = db.Client.Where(i => i.Id == id).FirstOrDefault().UserId;
                string flag = "";
                bool result = User.IsInRole("SuperAdmin");
                //agar farde login karde super admin bud
                if (result)
                {
                    flag = "yes";
                }
                else
                {


                    //agar agahi baraye in shakhsi ast ke login shode
                    if (myid == k)
                    {
                        flag = "yes";
                    }
                    else
                    {
                        flag = "no";
                    }

                }
                ViewBag.flag = flag;

                //var userId = User.Identity.GetUserId();
                Client SelectPage = db.Client.Where(e => e.Id == id).FirstOrDefault();
                if (SelectPage != null && id != null)
                {
                    //liste product ha ro beriz tu ViewBag.listOfProducts agar display!=notsee bud
                    var listOfProducts = db.Product.Where(r => r.ClienId == SelectPage.Id&&r.Display!="notsee").ToList();
                    ViewBag.listOfProducts = listOfProducts;




                    #region sabte amar
                    statistic item = new Core.statistic();
                    item.ClienId = id;
                    db.statistic.Add(item);
                    db.SaveChanges();
                    #endregion




                    #region pardakht
                    //bebin pardakht haro anjam dade ya na (safhe vitrin ro faghat)   

                    //bia pardakht nashodeye hazine vitrin ro peyda kon dar ebteda
                    var pointer = db.PardakhtVitrin.Where(r => r.ClienId == id && r.pardakht == 0).FirstOrDefault();
                    if (pointer != null)
                    {
                        //bebin zamane test be payan reside?
                        //yani taze sabte nam karde va avalin pardakht ro anjam nadade pas zamane testeshe


                        if (pointer.testTime != null && pointer.End == null && pointer.testTime > DateTime.Now)
                        { //behesh payam bede pardakht kone
                          //pishnahad ta 30 darsad takhfif


                            ViewBag.Note = "pardakht nashode";
                        }

                        //zamane tamdid reside az 5 ruze ghabl etela bede
                        else if (pointer.testTime == null && pointer.End.Value.Date == DateTime.Now.AddDays(5).Date || pointer.testTime == null && pointer.End.Value.Date == DateTime.Now.AddDays(4).Date || pointer.testTime == null && pointer.End.Value.Date == DateTime.Now.AddDays(3).Date || pointer.testTime == null && pointer.End.Value.Date == DateTime.Now.AddDays(2).Date || pointer.testTime == null && pointer.End.Value.Date == DateTime.Now.AddDays(1).Date || pointer.testTime == null && pointer.End.Value.Date == DateTime.Now.Date)
                        {

                            //behesh payam bede pardakht kone
                            //pishnahad ta 30 darsad takhfif
                            ViewBag.Note = "pardakht nashode";
                        }

                        //kolan pardakhtharo anjam nadade va safhe baste shode
                        //hala ta avalin bare sabte nameshe or badi pardakhte salanashe
                        else if (pointer.testTime < DateTime.Now && pointer.End == null || pointer.testTime == null && pointer.End < DateTime.Now)
                        {
                            //صفحه را نمایش نده تا مبلغ را پرداخت کند 
                            //va takhfif ra digar kamtar kon masalan 15 darsad
                            //darhale hazer 30 darsad bud
                            ViewBag.EndOfOpportunity = "end";
                        }

                    }
                    #endregion


                    //bia bebin mahsoolato khadamate bishtar ham sabt karde 
                    //ke dokmeye un ro baraye karbaranesh faal koni
                    var moreProducts = db.MoreProducts.ToList().Where(D => D.ClienId == id).Count();
                    if (moreProducts > 0)
                    {
                        ViewBag.moreProducts = "True";
                    }


                    //bia bebin mahsulat o khadamate (tu hamin safhe asli hamun eslide haye mahsulat) chizi sabt karde 
                    //ke un ghesmat ro baraye karbaranesh neshun bedi
                    var Products = db.Product.ToList().Where(D => D.ClienId == id).Count();
                    if (Products > 0)
                    {
                        ViewBag.Products = "True";
                    }


                    return View(SelectPage);

                }

                return View("NotFound");
            }
            catch (Exception)
            {

                return View("NotFound");
            }



        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult EditBigContent(string BigContent, string Id, string BigContents)
        {
            Product item = new Core.Product();
            ApplicationDbContext db = new ApplicationDbContext();
            //in if vase ine ke faghat khode fard betune agahi khodesho edit kone
            //super admin ham mitune edit kone

            if (db.Client.Where(s => s.Id == Id).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                if (BigContent != null && BigContent.Length < 20000)
                {   //jelo giri az vared shodan script va hack shodan
                    if (BigContent.ToLower().Contains("script"))
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    var searchItem = db.Client.Where(r => r.Id.ToString() == Id).FirstOrDefault();
                    searchItem.BigContent = BigContent;
               
                    db.SaveChanges();

                    return RedirectToAction("Show", "Home", new { id = Id });
                }
                if (BigContents != null)
                {

                    var searchItem = db.Client.Where(r => r.Id.ToString() == Id).FirstOrDefault();
                    searchItem.BigContent = BigContents;
             
                    db.SaveChanges();

                    return RedirectToAction("Show", "Home", new { id = Id });
                }
            }
            return RedirectToAction("Index", "Home");


        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize]

        public ActionResult AddClientVideo(string myClientId, HttpPostedFileBase RootAddresVideo)
        {
            Client item = new Core.Client();
            ApplicationDbContext db = new ApplicationDbContext();
            //in if vase ine ke faghat khode fard betune agahi khodesho edit kone
            //super admin ham mitune edit kone
            if (db.Client.Where(s => s.Id == myClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                var selectedItem = db.Client.Where(r => r.Id.ToString() == myClientId).FirstOrDefault();

                //maghadir khali nabashe va ta hodude 8 mg bishtar nabashe
                if (RootAddresVideo != null && RootAddresVideo.ContentLength > 0 && RootAddresVideo.ContentLength < 8596284)
                {
                    try
                    {


                        //agar file film bud paak kon
                        //local
                        //if (System.IO.File.Exists(Server.MapPath(selectedItem.VideoAddress)))
                        //{
                        //  System.IO.File.Delete((Server.MapPath(selectedItem.VideoAddress)));
                        //}

                        //.............................................................................//

                        //in vase vaghti ke site oon balayi vase local
                        if (selectedItem.VideoAddress != null)
                        {
                            if (System.IO.File.Exists(@"C:\sites\baladchee\videoTest\" + nameHelper(selectedItem.VideoAddress)))
                            {
                                System.IO.File.Delete(@"C:\sites\baladchee\videoTest\" + nameHelper(selectedItem.VideoAddress));
                            }

                        }

                        string ValuePath = Path.GetFileName(Guid.NewGuid().ToString()) + TypHelper(RootAddresVideo.FileName);
                        string ValueSaverSaver = Path.Combine(Server.MapPath("~/videoTest"), ValuePath);
                        selectedItem.VideoAddress = "~/videoTest/" + ValuePath;
                        var file = Request.Files[0];
                        file.SaveAs(ValueSaverSaver);
                        db.SaveChanges();

                        //frshordesazi video

                    }
                    catch
                    {
                        return RedirectToAction("Show", "Home", new { id = myClientId });
                    }
                }
                else
                {
                    //hajme video mojaz nist
                    return RedirectToAction("Show", "Home", new { id = myClientId, error = "s" });

                }

            }
            return RedirectToAction("Show", "Home", new { id = myClientId });

        }





        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteClientVideo(string myClientId)
        {
            //agar file film bud paak kon
            //local
            //if (System.IO.File.Exists(Server.MapPath(selectedItem.VideoAddress)))
            //{
            //  System.IO.File.Delete((Server.MapPath(selectedItem.VideoAddress)));
            //}

            //.............................................................................//

            //in vase vaghti ke site oon balayi vase local
            Client item = new Core.Client();
            ApplicationDbContext db = new ApplicationDbContext();
            //in if vase ine ke faghat khode fard betune agahi khodesho edit kone
            //super admin ham mitune edit kone
            if (db.Client.Where(s => s.Id == myClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                var selectedItem = db.Client.Where(r => r.Id.ToString() == myClientId).FirstOrDefault();

                if (selectedItem.VideoAddress != null)
                {
                    if (System.IO.File.Exists(@"C:\sites\baladchee\videoTest\" + nameHelper(selectedItem.VideoAddress)))
                    {
                        System.IO.File.Delete(@"C:\sites\baladchee\videoTest\" + nameHelper(selectedItem.VideoAddress));
                    }



                }
                selectedItem.VideoAddress = null;

            }

            db.SaveChanges();
            return RedirectToAction("Show", "Home", new { id = myClientId });
        }


        #region TypHelper
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
            //var hh = "test";
            return obj;
        }

        #endregion




        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize]
        public ActionResult EditPicTitle(string Id, string Content, string Title, HttpPostedFileBase RootAddres)
        {
            Product item = new Core.Product();
            ApplicationDbContext db = new ApplicationDbContext();
            //in if vase ine ke faghat khode fard betune agahi khodesho edit kone
            //super admin ham mitune edit kone
            if (db.Client.Where(s => s.Id == Id).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                var selectedItem = db.Client.Where(r => r.Id.ToString() == Id).FirstOrDefault();
                //agar content kheili ziad bud begu
                if (Content.Length > 500)
                {
                    return RedirectToAction("Show", "Home", new { id = Id }); ;
                }
                //agar Title kheili ziad bud begu
                if (Title.Length > 70)
                {
                    return RedirectToAction("Show", "Home", new { id = Id });
                }

                if (ModelState.IsValid)
                {


                    if (RootAddres != null && RootAddres.ContentLength > 0)
                        try
                        {
                            //agar file aks bud paak kon
                            //local
                            //if (System.IO.File.Exists(Server.MapPath(selectedItem.PhotoAddress)))
                            //{
                            //    System.IO.File.Delete((Server.MapPath(selectedItem.PhotoAddress)));
                            //}

                            //.............................................................................//

                            //in vase vaghti ke site oon balayi vase local
                            if (System.IO.File.Exists(@"C:\sites\baladchee\Contents\Pic\" + nameHelper(selectedItem.PhotoAddress)))
                            {
                                System.IO.File.Delete(@"C:\sites\baladchee\Contents\Pic\" + nameHelper(selectedItem.PhotoAddress));
                            }

                            string ValuePath = Path.GetFileName(Guid.NewGuid().ToString()) + TypHelper(RootAddres.FileName);
                            string ValueSaverSaver = Path.Combine(Server.MapPath("~/Contents/Pic"), ValuePath);
                            WebImage img = new WebImage(RootAddres.InputStream);

                            if (img.Width > 660)
                            {
                                img.Resize(660, img.Height);
                            }

                            if (img.Height > 460)
                            {
                                img.Resize(img.Width, 460);
                            }

                            img.Save(ValueSaverSaver);
                            selectedItem.PhotoAddress = "~/Contents/Pic/" + ValuePath;
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {

                            return View("NotFound");
                        }

                    //agar aks ra avaz nakard va faghat Content Ra Taghir dad
                    if (Content != null)
                    {

                        selectedItem.Content = Content;

                    }

                    //agar aks ra avaz nakard va faghat Content Ra Taghir dad
                    if (Title != null)
                    {

                        selectedItem.Title = Title;

                    }
                    db.SaveChanges();
                }

                else
                {
                    //ViewBag.Message = "شما فایلی را مشخص نکرده اید";
                    return View("NotFound");
                }

            }
            return RedirectToAction("Show", "Home", new { id = Id });
        }


        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult AddProduct(string Id, string pContent, HttpPostedFileBase RootAddresProduct, string pTitle, string Takhfif, string GhabeliatGheymat, string GheymatGhabli, string GheymatFeli, string SefareshMahsool, string ProductSocialMediaLink, string anbarGardani)
        {
            Product item = new Core.Product();
            ApplicationDbContext db = new ApplicationDbContext();
            //in if vase ine ke faghat khode fard betune agahi khodesho edit kone
            //super admin ham mitune edit kone

            if (db.Client.Where(s => s.Id == Id).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {

                if (ModelState.IsValid)
                {
                    //bedune dashtane code zarin pal be in ghesmat umadan emkan pazir nist
                    //vali baz baraye etminan if ro mizaram
                    if (!Replace.hasZarinCode(Id))
                    {
                        return View("NotFound");
                    }
                    //tedade agahi ha nabayad bish az 3 adad bashad
                    List<Product> ClientProducts = new List<Core.Product>();
                    ClientProducts = db.Product.Where(t => t.ClienId == Id && t.Display != "notsee").ToList();
                    if (ClientProducts.Count() <= 2)
                    {

                        if (RootAddresProduct != null && RootAddresProduct.ContentLength > 0)
                            try
                            {


                                string ValuePath = Path.GetFileName(Guid.NewGuid().ToString()) + TypHelper(RootAddresProduct.FileName);
                                string ValueSaverSaver = Path.Combine(Server.MapPath("~/Contents/Pic"), ValuePath);
                                WebImage img = new WebImage(RootAddresProduct.InputStream);

                                if (img.Width > 660)
                                {
                                    img.Resize(660, img.Height);
                                }

                                if (img.Height > 460)
                                {
                                    img.Resize(img.Width, 460);
                                }
                                img.Save(ValueSaverSaver);
                                item.pPicAddres = "~/Contents/Pic/" + ValuePath;
                                item.ClienId = Id;
                                if (pContent.Length < 3500)
                                {
                                    item.pContent = pContent;
                                }
                                if (pTitle.Length < 100)
                                {
                                    item.pTitle = pTitle;
                                }
                                if (Takhfif != null)
                                {
                                    item.Takhfif = "تخفیف دارد";
                                }
                                //agar ghabeliate sefaresh darad bayad gheymat ham dashte bashad
                                if (GhabeliatGheymat != null && GheymatFeli != null)
                                {
                                    item.NamayesheGheymat = "قابلیت نمایش قیمت دارد";
                                    item.GheymatFeli = GheymatFeli;
                                }
                                //gheymate bedune takhfif
                                if (GheymatGhabli != null&&Takhfif!=null)
                                {
                                    item.GheymatGhabli = GheymatGhabli;
                                }
                                //ghabeliate sefaresh darad
                                if (SefareshMahsool != null && GhabeliatGheymat != null)
                                {
                                    item.SefareshMahsool = "قابلیت سفارش دارد";
                                }


                                //bia bebin tu linki ke dade instagram hast. ye mogheili linke jaye dg nade
                                //felan ba in siasat hala age ye ruzi insta nabud ja be ja mikonam
                                //pas:
                                if (ProductSocialMediaLink.Contains("www.instagram.com") && ProductSocialMediaLink.Length < 120)
                                {
                                    item.ProductSocialMediaLink = ProductSocialMediaLink;
                                }



                                //anbar gardani
                                try
                                {
                                    if (anbarGardani != null && anbarGardani != "")
                                    {
                                        var myCount = Convert.ToInt32(anbarGardani);
                                        if (myCount < 1000 && myCount > 0)
                                        {
                                            item.count = myCount;
                                        }
                                    }

                                }
                                catch (Exception)
                                {

                                    return View("NotFound");
                                }
                                //end anbarGardani


                                item.pCreateDate = DateTime.Now;
                                db.Product.Add(item);
                                db.SaveChanges();



                            }
                            catch (Exception)
                            {

                                return View("NotFound");
                            }


                    }
                    //Agar product ha bish az 3 ta shod digar save nakon
                    else
                    {
                        return RedirectToAction("Show", "Home", new { id = Id, error = "moreProduct" });
                    }
                }




                else
                {
                    //ViewBag.Message = "شما فایلی را مشخص نکرده اید";
                    return View("NotFound");
                }

            }

            return RedirectToAction("Show", "Home", new { id = Id });
        }



        static string nameHelper(string obj)
        {
            string TypeOfFile = "";
            string selfFile = obj;

            for (int i = obj.Length - 1; i >= 0; i--)
            {
                if (obj.Substring(i, 1) != "/")
                {

                    TypeOfFile = obj.Substring(i, 1) + TypeOfFile;


                }
                if (obj.Substring(i, 1) == "/")
                {
                    return "/" + TypeOfFile;
                }
            }
            return obj;
        }









        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult EditProduct(HttpPostedFileBase pRootAddres, string Id, string pId, string pTitle, string pContent,/*az inja be bad*/ string GhabeliateGheymatEdit, string EditGheymatFeli, string GhabeliateTakhfifEdit, string EditGheymatGhabli, string EditGhabeliateSefaresh, string EditProductSocialMediaLink, string anbarGardaniEdit)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            //in if vase ine ke faghat khode fard betune agahi khodesho edit kone
            if (db.Client.Where(s => s.Id == Id).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                var selectedItem = db.Client.Where(r => r.Id.ToString() == Id).FirstOrDefault();
                //agar pcontent kheili ziad bud begu
                if (pContent.Length > 3500)
                {

                    return RedirectToAction("Show", "Home", new { id = Id });
                }
                //agar ptitile kheili ziad bud begu
                if (pTitle.Length > 300)
                {
                    return RedirectToAction("Show", "Home", new { id = Id });
                }

                Product item = db.Product.Where(r => r.pId == pId).FirstOrDefault();
                if (item != null)
                {
                    if (pRootAddres != null && pRootAddres.ContentLength > 0)
                        try
                        {
                            //agar file akse mahsool bud paak kon                            
                            //ta fazaye ezafi dar server nagirad

                            if (System.IO.File.Exists((Server.MapPath(item.pPicAddres))))
                            {
                                System.IO.File.Delete((Server.MapPath(item.pPicAddres)));
                            }



                            string ValuePath = Path.GetFileName(Guid.NewGuid().ToString()) + TypHelper(pRootAddres.FileName);
                            string ValueSaverSaver = Path.Combine(Server.MapPath("~/Contents/Pic"), ValuePath);
                            WebImage img = new WebImage(pRootAddres.InputStream);

                            if (img.Width > 660)
                            {
                                img.Resize(660, img.Height);
                            }

                            if (img.Height > 460)
                            {
                                img.Resize(img.Width, 460);
                            }
                            img.Save(ValueSaverSaver);
                            item.pPicAddres = "~/Contents/Pic/" + ValuePath;
                            db.SaveChanges();



                        }
                        catch (Exception)
                        {

                            return View("NotFound");
                        }

                    if (pContent != null || pContent != "")
                    {
                        item.pContent = pContent;


                    }
                    if (pTitle != null || pTitle != "")
                    {
                        item.pTitle = pTitle;

                    }


                    //AzinjaEmal mikonim code JadidRa
                    //agar ghabeliate sefaresh darad bayad gheymat ham dashte bashad
                    if (GhabeliateGheymatEdit != null && EditGheymatFeli != null && EditGheymatFeli != "")
                    {
                        item.NamayesheGheymat = "قابلیت نمایش قیمت دارد";
                        item.GheymatFeli = EditGheymatFeli;



                    }
                    else
                    {
                        item.NamayesheGheymat = null;
                        item.GheymatFeli = null;
                    }

                    //ghabeliate Takhfif
                    //gheymate bedune takhfif
                    if (GhabeliateTakhfifEdit != null && EditGheymatGhabli != null && EditGheymatGhabli != "" && GhabeliateGheymatEdit != null && EditGheymatFeli != null && EditGheymatFeli != "")
                    {
                        item.Takhfif = "تخفیف دارد";
                        item.GheymatGhabli = EditGheymatGhabli;
                    }
                    else
                    {
                        item.Takhfif = null;
                        item.GheymatGhabli = null;
                    }

                    //ijade Sefaresh

                    if (EditGhabeliateSefaresh != null && EditGheymatFeli != null && EditGheymatFeli != "" && GhabeliateGheymatEdit != null)
                    {
                        item.SefareshMahsool = "قابلیت سفارش دارد";
                    }
                    else
                    {
                        item.SefareshMahsool = null;
                    }
                    //ta inja

                    //for instaLink
                    if (EditProductSocialMediaLink!=null&& EditProductSocialMediaLink !="")
                    {
                        if (EditProductSocialMediaLink.Contains("instagram.com"))
                        {
                            item.ProductSocialMediaLink = EditProductSocialMediaLink;
                        }
                    }
                    else
                    {
                        item.ProductSocialMediaLink = null;
                    }


                    if (anbarGardaniEdit != null)
                    {
                        try
                        {
                            item.count = Convert.ToInt32(anbarGardaniEdit);
                        }
                        catch (Exception)
                        {


                        }
                    }




                    db.SaveChanges();
                }
            }
            return RedirectToAction("Show", "Home", new { id = Id });
        }


        //ghara ast hich sanadi paak nashavad faghat az dide furushande paak shavad
        //display = notsee (gharar dad) moghe namayesh chek mishavad dar action show
        [Authorize]
        public ActionResult DeleteProduct(string pId, string userId)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            if (User.Identity.GetUserId() == userId || User.IsInRole("SuperAdmin"))
            {
                var item = db.Product.Where(d => d.pId == pId).FirstOrDefault();
                item.Display = "notsee";
                db.SaveChanges();

                //agar file akse mahsool bud paak kon
                //ta fazaye ezafi dar server nagirad
                //if (System.IO.File.Exists(Server.MapPath(item.pPicAddres)))
                //{
                //    System.IO.File.Delete((Server.MapPath(item.pPicAddres)));
                //}
            }
            var tt = db.Client.Where(t => t.UserId == userId).FirstOrDefault().Id;
            return RedirectToAction("Show", "Home", new { id = tt });
        }


        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Nazarat(string nazar, string Id)
        {
            //var response = Request["g-recaptcha-response"];
            //string secretKey = "6Lf3e8oUAAAAAP3U_1DViKoNfML9UTauUF4tU7kO";
            var client = new WebClient();
            //var result1 = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secretKey, response));
            //var obj = JObject.Parse(result1);
            //var status = (bool)obj.SelectToken("success");
            // if (status != true)
            //{
            //  return HttpNotFound();
            // }
            if (nazar.ToLower().Contains("script"))
            {
                return RedirectToAction("Show", "Home", new { id = Id });
            }
            ApplicationDbContext db = new ApplicationDbContext();
            bool isOk = false;
            //faghat 4bar mitune ba in ip nazar bezare
            //agar bare aval ast count opinion 0 ast pas bare aval faghat
            //pas bare aval isOk ra true kon
            if (db.Opinion.Count() == 0)
            {
                isOk = true;
            }
            List<Opinion> opinionList = new List<Opinion>();
            foreach (var opinion in db.Opinion)
            {

                if (opinion.Ip == Request.UserHostAddress && opinion.ClienId == Id)
                {
                    opinionList.Add(opinion);
                }

            }
            if (opinionList.Count() < 5)
            {
                isOk = true;
            }
            //agar sharayete bala ra dasht pas mitune nazar bezare
            //kamtar az 5 ta nazar gozashte bud yani
            if (isOk)
            {
                if (nazar.Length < 800 && nazar != null)
                {

                    Opinion item = new Opinion();
                    item.Content = nazar;
                    item.ClienId = Id;
                    item.Ip = Request.UserHostAddress;
                    item.Count = item.Count + 1;
                    item.Ip = Request.UserHostAddress;
                    db.Opinion.Add(item);
                    db.SaveChanges();

                }
                else
                {
                    return RedirectToAction("Show", "Home", new { id = Id, error = "true" });
                }
            }
            else
            {
                return RedirectToAction("Show", "Home", new { id = Id, error = "true" });
            }

            return RedirectToAction("Show", "Home", new { id = Id, error = "NotTrue" });
        }



        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditContact(string PhonNumber, string Addres, string Email, string ClientId, string Instagram, string Telegram)
        {

            ApplicationDbContext db = new Models.ApplicationDbContext();

            //in if vase ine ke faghat khode fard betune agahi khodesho edit kone
            //super admin ham mitune edit kone

            if (db.Client.Where(s => s.Id == ClientId).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                var searchItem = db.Client.Where(t => t.Id.ToString() == ClientId).FirstOrDefault();

                if (Email.Length < 70)
                {
                    searchItem.Email = Email;
                }

                if (Addres.Length < 150)
                {
                    searchItem.Address = Addres;
                }


                if (PhonNumber.Length < 40)
                {
                    searchItem.PhonNumber = PhonNumber;
                }

                if (Instagram.Length < 40)
                {
                    searchItem.Instagram = Instagram;
                }
                if (Telegram.Length < 40)
                {
                    searchItem.Telegram = Telegram;
                }


                db.SaveChanges();
            }

            return RedirectToAction("Show", "Home", new { id = ClientId });
        }


        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult EditSaleTitle(string SaleTitle, string Id)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();

            //in if vase ine ke faghat khode fard betune agahi khodesho edit kone
            //super admin ham mitune edit kone

            if (db.Client.Where(s => s.Id == Id).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                if (SaleTitle.Length < 100)
                {
                    var editItem = db.Client.Where(s => s.Id == Id).FirstOrDefault();
                    editItem.SaleTitle = SaleTitle;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Show", "Home", new { id = Id });
        }


        [HttpPost]
        public JsonResult AddCustomerClub(string PhoneNumber, string ClientId)
        {



            ApplicationDbContext db = new Models.ApplicationDbContext();
            //yek listi az bashgahe moshtarian in client be ma bede
            var customerClubList = db.CustomerClub.Where(t => t.ClienId == ClientId).ToList();

            var r = new Regex(@"^[0-9]{11}$");
            if (!r.IsMatch(PhoneNumber))
            {
                return Json(new { success = true, responseText = "لطفا فقط شماره موبایل وارد فرمایید" }, JsonRequestBehavior.AllowGet);

            }

            foreach (var customer in customerClubList)
            {
                if (customer.PhoneNumber == PhoneNumber)
                {
                    return Json(new { success = true, responseText = "شما قبلا در باشگاه مشتریان عضو شده اید" }, JsonRequestBehavior.AllowGet);

                }
            }

            foreach (var customer in customerClubList)
            {
                if (customer.Ip == Request.UserHostAddress)
                {
                    return Json(new { success = true, responseText = "با هر سیستم تنها یک بار میتوانید ثبت شماره انجام دهید" }, JsonRequestBehavior.AllowGet);

                }
            }
            CustomerClub item = new CustomerClub();
            item.ClienId = ClientId;
            item.PhoneNumber = PhoneNumber;
            item.Ip = Request.UserHostAddress;
            db.CustomerClub.Add(item);
            db.SaveChanges();


            //بنظرم نیازی نیست دیگه پیامک بفرسته چون بصورت جی اس کد تخفیف توی صفحه میاد
            // بعد از درج شماره،اینطوری هزینه هم کمتر و اینکه سیایت این کد هم بر اساس کم شده پیامک از فروشنده هست
            //که بنظر جالب نیست ، فعلا کامت میکنم تا بعد ببینیم چه تصمیمی گرفته میشه

            #region sendSms

            //var CustomClient = db.Client.Where(u => u.Id == ClientId).FirstOrDefault();
            //if (CustomClient.smsCount > 0 && CustomClient.SaleCode != null)
            //{
            //    if (CustomClient.SaleCode != "")
            //    {


            //        meli.Send sender = new Send();
            //        string MatnePayam = " کد تخفیف " + CustomClient.Title + " " + CustomClient.SaleCode;
            //        sender.SendSimpleSMS2("9017037365", "Aa30081370@", PhoneNumber, "50001060009775", MatnePayam, true);
            //        CustomClient.smsCount = CustomClient.smsCount - 1;
            //        db.SaveChanges();
            //    }
            //}

            #endregion

            return Json(new { success = true, responseText = "از اینکه به باشگاه مشتریان ما پیوستید، سپاسگذاریم.در آینده، از تمامی تخفیف ها و رویداد های این فروشگاه با خبر خواهید شد!" }, JsonRequestBehavior.AllowGet);

        }


        [ValidateAntiForgeryToken]
        public ActionResult AddApplayingFor(string ApplayingForPhonNumber, string ApplayingForAddres, string ApplayingForName)
        {

            ApplicationDbContext db = new Models.ApplicationDbContext();
            ApplyingFor item = new ApplyingFor();
            item.PhoneNumber = ApplayingForPhonNumber;
            item.Addres = ApplayingForAddres;
            item.Name = ApplayingForName;
            db.ApplyingFor.Add(item);
            db.SaveChanges();
            return RedirectToAction("Index", "Home");

        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ForgetPass(string userName)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var item = db.Users.Where(s => s.UserName == userName).FirstOrDefault();
            if (item != null)
            {

                var ClientMobile = db.Client.Where(i => i.UserId == item.Id).FirstOrDefault();
                if (ClientMobile == null)
                {
                    //yani farayand sabte name in karbar ba moshkel ruberu shode bude
                    //va aslan client vasash sakhte nashode vali user sakhte shode
                    //pas ba ghanune man in shakhs emkane daryafte ramz ro nadare va agar yadesh nist bayad yekbar dg sabtenam kone

                    return RedirectToAction("Login", "Account", new { notFound = "missedPage" });
                }
                //check kon barasase Expiretime e forgetModel
                //agar zire 10 min bud pas bezanesh
                if (Session["forgetPass"]!=null)
                {
                    forgetModel myForgetModel = new forgetModel();
                    myForgetModel = (forgetModel)Session["forgetPass"] ;
                    if (myForgetModel.Expiretime>=DateTime.Now)
                    {
                        return RedirectToAction("Login", "Account", new { notFound = "TimeOut" });
                    }
                }
                Random rnd = new Random();
                forgetModel sample = new Models.forgetModel();
                sample.smsCode = rnd.Next(1000, 10000).ToString();
                sample.userName = userName;
                sample.Expiretime = DateTime.Now.AddMinutes(10);
                Session["forgetPass"] = sample;

                var customClientMobile = ClientMobile.mobile;
                meli.Send sender = new Send();
                //این کد را دریافت کن و بعد منتظر رمز جدید باش که واسش اونم پیامک میشه
                //وقتی این رو درست وارد کنه
                sender.SendByBaseNumber2("9017037365", "Aa30081370@", sample.smsCode, customClientMobile, Convert.ToInt32(48743));
                return RedirectToAction("smspage");

            }

            return RedirectToAction("Login", "Account", new { notFound = "n" });


        }

        //بعد از اکشن بالا میاد اینجا اگر نام کاربری رو درست نوشته بود



        public ActionResult smspage(string notMatch, string SetCode)
        {
            //agar time session tamum shode bud ke tamam
            //boro dobare be login
            if (Session["forgetPass"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ////dargheire in surat cast kon  session ro
            //forgetModel sample = (forgetModel)Session["forgetPass"];
            ////tarikhe enghezaye session gozashte bud
            //if (sample.Expiretime <= DateTime.Now)
            //{
            //    Session.Clear();
            //    return RedirectToAction("Login", "Account");
            //}


            if (notMatch == "a")
            {
                ViewBag.notmatch = "کد وارد شده صحیح نمیباشد";
            }
            if (SetCode != null)
            {
                return RedirectToAction("SetNewPassword", "Home", new { SetCode = SetCode });
            }
            return View();


        }


        public async Task<ActionResult> SetNewPassword(string SetCode)
        {
            if (Session["forgetPass"] != null)
            {

                //bia cast kon bebin zamanesh ok hast 5daghigh
                forgetModel pointer = new forgetModel();
                pointer = (forgetModel)Session["forgetPass"];
                //if (pointer.Expiretime <= DateTime.Now)
                //{
                //    Session.Clear();
                //    return RedirectToAction("Login", "Account");
                //}


                if (pointer.smsCode == SetCode)
                {


                    try
                    {
                        ApplicationDbContext db = new ApplicationDbContext();
                        var userId = db.Users.Where(t => t.UserName == pointer.userName).FirstOrDefault().Id;
                        var client = db.Client.Where(s => s.UserId == userId).FirstOrDefault();
                        var token = await UserManager.GeneratePasswordResetTokenAsync(userId);
                        Random rnd = new Random();
                        var randomPassword = rnd.Next(100000, 1000000);
                        var result = await UserManager.ResetPasswordAsync(userId, token, randomPassword.ToString());



                        meli.Send sender = new Send();
                        //ارسال رمز جدید بعد از وارد کردن تایید کد تایید صلاحیت که قبل از این وارد شده بود 
                        sender.SendByBaseNumber2("9017037365", "Aa30081370@", randomPassword.ToString(), client.mobile, Convert.ToInt32(48744));
                        //Session.Clear();
     
                    }
                    catch (Exception)
                    {
                        Session.Clear();
                        return RedirectToAction("Login", "Account", new { notFound = "s" });
                    }
                }
                else
                {
                    return RedirectToAction("smspage", new { notMatch = "a" });
                }

            }
            return RedirectToAction("Login", "Account");
        }

        //1
        //in action comment shod
        //zira gharar shod kolan dargah vasl shavad va sabade kala tarif shavad
        #region orderProduct

        //[ValidateAntiForgeryToken]
        //[HttpPost]
        //az doja ersale sefaresh darim ya az safhe show tavasot slider ya az sfhaye moreproduct
        //hala az koja befahmim az kojae?
        //fromMoreProduct ==fromMoreProduct bud az moreProduct ast 
        //agar khali bud az safhe show ast
        //public ActionResult orderProduct(string PhoneNumber, string Address, string Tozihat, string pId, string cId, string productTitle, string Postal, string FromMoreProduct)
        //{
        //    //az Safhe show
        //    if (FromMoreProduct == "" || FromMoreProduct == null)
        //    {
        //        String TrackingCode = "";

        //        //id sefaresh ro migiram ke vaghti factor sakht addresesh bere to table sefareshat
        //        //ke vaghti khast sefaresh ro paak kone factoresham pak beshe
        //        String sefareshId = "";
        //        ApplicationDbContext db = new ApplicationDbContext();
        //        //telephon nabayad khali bashad
        //        if (PhoneNumber == null)
        //        {
        //            return RedirectToAction("show", "home", new { id = cId });
        //        }

        //        else
        //        {

        //            Sefareshat item = new Sefareshat();
        //            item.PhoneNumber = PhoneNumber;
        //            item.Address = Address;
        //            item.Tozihat = Tozihat;
        //            item.ProductId = pId;
        //            item.clientId = cId;
        //            item.productTitle = productTitle;
        //            item.isSeen = false;
        //            item.PostalCode = Postal;
        //            item.Tracking_Code = Replace.CreateTarackingCode();
        //            TrackingCode = item.Tracking_Code;
        //            db.Sefareshat.Add(item);

        //            sefareshId = item.Id;

        //            db.SaveChanges();
        //        }
        //        //return RedirectToAction("show", "home", new { id = cId, order = "true" });
        //        #region myfactorModel
        //        factorViewModel myFactorModel = new factorViewModel();
        //        myFactorModel.buyerPhone = PhoneNumber;
        //        myFactorModel.ClintNumber = Replace.retuenClientNumber(cId);
        //        myFactorModel.shopName = cId;
        //        myFactorModel.TrackingCode = TrackingCode;
        //        myFactorModel.ProductName = Replace.returnProductName(pId);
        //        myFactorModel.SubmitDate = DateTime.Now;
        //        myFactorModel.ProductPrice = Replace.returnProductPrice(pId);
        //        myFactorModel.SefareshId = sefareshId;
        //        #endregion

        //        //boro be safheye factor
        //        TempData["factor"] = myFactorModel;
        //        //az moreproduct kharid karde bayad ino safhe factor bedune ke dobare hedayatesh kone be in safhe
        //        TempData["FromeMoreProduct"] = false;

        //        return RedirectToAction("factor", "home", new { cid = cId });
        //    }

        //    //az Safhe MoreProduct
        //    else if (FromMoreProduct == "fromMoreProduct")
        //    {
        //        //telephon nabayad khali bashad
        //        String TrackingCode = "";

        //        //id sefaresh ro migiram ke vaghti factor sakht addresesh bere to table sefareshat
        //        //ke vaghti khast sefaresh ro paak kone factoresham pak beshe
        //        String sefareshId = "";
        //        if (PhoneNumber == null)
        //        {
        //            return RedirectToAction("MoreProducts", "home", new { id = cId });
        //        }
        //        else
        //        {
        //            ApplicationDbContext db = new ApplicationDbContext();
        //            Sefareshat item = new Sefareshat();
        //            item.PhoneNumber = PhoneNumber;
        //            item.Address = Address;
        //            item.Tozihat = Tozihat;
        //            item.MoreProducstId = pId;
        //            item.clientId = cId;
        //            item.productTitle = productTitle;
        //            item.Tracking_Code = Replace.CreateTarackingCode();
        //            TrackingCode = item.Tracking_Code;
        //            item.isSeen = false;
        //            item.PostalCode = Postal;
        //            db.Sefareshat.Add(item);

        //            sefareshId = item.Id;

        //            db.SaveChanges();
        //        }
        //        //return RedirectToAction("MoreProducts", "home", new { id = cId, order = "true" });
        //        #region myfactorModel
        //        factorViewModel myFactorModel = new factorViewModel();
        //        myFactorModel.buyerPhone = PhoneNumber;
        //        myFactorModel.ClintNumber = Replace.retuenClientNumber(cId);
        //        myFactorModel.shopName = cId;
        //        myFactorModel.TrackingCode = TrackingCode;
        //        myFactorModel.ProductName = Replace.returnMoreProductName(pId);
        //        myFactorModel.ProductPrice = Replace.returnMoreProductPrice(pId);
        //        myFactorModel.SubmitDate = DateTime.Now;
        //        myFactorModel.SefareshId = sefareshId;
        //        #endregion

        //        //boro be safheye factor
        //        TempData["factor"] = myFactorModel;

        //        //az moreproduct kharid karde bayad ino safhe factor bedune ke dobare hedayatesh kone be in safhe
        //        TempData["FromeMoreProduct"] = true;

        //        return RedirectToAction("factor", "home", new { cid = cId });
        //    }

        //    return RedirectToAction("MoreProducts", "home", new { id = cId, order = "true" });
        //}


        #endregion

        //2
        //inham comment mishavad bana be tasmimi ke bala gofte shod
        #region factor
        //[AllowAnonymous]
        //in yek action vaset ast bad az in karbar be pdf rafte va factor ra daryaft mikonad
        //hala kharid ya az safheye moreProduct bude va ya az khode product 
        //ma bayad bedunim chon bad az kharid baz gasht ro ke mizane bayad montaghel beshe be safhei ke bude 
        //az koja mifahmim az "fromMore" agar por bud az moreumade va agar khali bud az product
        //public ActionResult factor(string cid)
        //{
        //    var isFromeMore = (bool)TempData["FromeMoreProduct"];
        //    if (isFromeMore)
        //    {
        //        ViewBag.isfromMore = "A";
        //    }
        //    ViewBag.cid = cid;
        //    return View();
        //}


        #endregion

        //3
        //inham comment mishavad bana be tasmimi ke bala gofte shod
        #region pdf
        //[AllowAnonymous]
        //public ActionResult pdf(string shopName, string TrackingCode, string buyerPhone, string ClintNumber)
        //{

        //    factorViewModel myFactorModel = new factorViewModel();
        //    myFactorModel = (factorViewModel)TempData["factor"];



        //    //ابتدا فایل را ایجاد میکنیم
        //    iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 10, 10, 10, 10);

        //    string filename = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00")
        //    + DateTime.Now.Hour.ToString("00") + DateTime.Now.Minute.ToString("00") + DateTime.Now.Second.ToString("00") + DateTime.Now.Millisecond.ToString("00") + "-" + "فاکتور سفارش" + ".pdf";
        //    string path = Path.Combine(Server.MapPath("~/Contents/PDF/"), filename);

        //    //Create our file stream and bind the writer to the document and the stream
        //    iTextSharp.text.pdf.PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, new FileStream(path, FileMode.Create));



        //    //ثبت آدرس فاکتور در جدول سفارشات کلاینت
        //    ApplicationDbContext db = new ApplicationDbContext();
        //    var TheOrder = db.Sefareshat.Where(w => w.Id == myFactorModel.SefareshId).FirstOrDefault();
        //    TheOrder.FactorAddres = "~/Contents/PDF/" + filename;
        //    db.SaveChanges();
        //    //پایان کد



        //    //Open the document for writing
        //    document.Open();

        //    //Add a new page
        //    document.NewPage();


        //    iTextSharp.text.pdf.BaseFont bfArialUniCode = iTextSharp.text.pdf.BaseFont.CreateFont(Server.MapPath("~/fonts/tahoma.ttf"), iTextSharp.text.pdf.BaseFont.IDENTITY_H, iTextSharp.text.pdf.BaseFont.EMBEDDED);
        //    //Create a font from the base font
        //    iTextSharp.text.Font font = new iTextSharp.text.Font(bfArialUniCode, 10, 1, iTextSharp.text.BaseColor.BLACK);

        //    //Use a table so that we can set the text direction
        //    iTextSharp.text.pdf.PdfPTable table = new iTextSharp.text.pdf.PdfPTable(1);
        //    //Ensure that wrapping is on, otherwise Right to Left text will not display
        //    table.DefaultCell.NoWrap = false;
        //    //Create a regex expression to detect hebrew or arabic code points
        //    const string regex_match_arabic_hebrew = @"[\u0600-\u06FF,\u0590-\u05FF]+";
        //    if (Regex.IsMatch("م الموافق", regex_match_arabic_hebrew, RegexOptions.IgnoreCase))
        //    {
        //        table.RunDirection = iTextSharp.text.pdf.PdfWriter.RUN_DIRECTION_RTL;
        //    }


        //    //تصویر
        //    iTextSharp.text.Image myImage = iTextSharp.text.Image.GetInstance(Server.MapPath("~/images/PdfLogo.png"));
        //    iTextSharp.text.pdf.PdfPCell cellImg = new iTextSharp.text.pdf.PdfPCell(myImage);
        //    cellImg.Border = 0;
        //    cellImg.FixedHeight = 80;
        //    cellImg.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
        //    cellImg.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
        //    cellImg.PaddingBottom = 5;
        //    table.AddCell(cellImg);

        //    //کد های بدنه
        //    //title foorushga
        //    string namec = Replace.returnClientTitle(myFactorModel.shopName);
        //    iTextSharp.text.pdf.PdfPCell shopName1 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(namec, font));

        //    shopName1.PaddingBottom = 18;
        //    shopName1.PaddingTop = 18;
        //    shopName1.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.White);
        //    shopName1.NoWrap = false;
        //    shopName1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
        //    shopName1.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
        //    table.AddCell(shopName1);




        //    //نام محصول اضافه شود
        //    iTextSharp.text.pdf.PdfPCell orderName = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("نام محصول : " + myFactorModel.ProductName, font));
        //    orderName.Border = 1;
        //    orderName.PaddingBottom = 8;
        //    orderName.PaddingTop = 8;
        //    orderName.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.White);
        //    orderName.NoWrap = false;
        //    orderName.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
        //    orderName.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
        //    table.AddCell(orderName);

        //    //قیمت محصول
        //    iTextSharp.text.pdf.PdfPCell ProductPrice = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(" قیمت واحد محصول(تومان) : " + myFactorModel.ProductPrice, font));
        //    ProductPrice.Border = 1;
        //    ProductPrice.PaddingBottom = 8;
        //    ProductPrice.PaddingTop = 8;
        //    ProductPrice.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.White);
        //    ProductPrice.NoWrap = false;
        //    ProductPrice.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
        //    ProductPrice.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
        //    table.AddCell(ProductPrice);

        //    //نام فروشگاه اضافه شود
        //    iTextSharp.text.pdf.PdfPCell ShopName = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("نام فروشگاه : " + myFactorModel.shopName, font));
        //    ShopName.Border = 1;
        //    ShopName.PaddingBottom = 5;
        //    ShopName.PaddingTop = 5;
        //    ShopName.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.White);
        //    ShopName.NoWrap = false;
        //    ShopName.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
        //    ShopName.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
        //    table.AddCell(ShopName);

        //    //تاریخ ثبت سفارش
        //    string mydate = Replace.ToShamsiForPdf(myFactorModel.SubmitDate);
        //    iTextSharp.text.pdf.PdfPCell date = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("تاریخ ثبت سفارش : " + mydate, font));
        //    date.Border = 1;
        //    date.PaddingBottom = 5;
        //    date.PaddingTop = 5;
        //    date.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.White);
        //    date.NoWrap = false;
        //    date.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
        //    date.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
        //    table.AddCell(date);



        //    //شماره تماس
        //    iTextSharp.text.pdf.PdfPCell Phone = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("شماره تماس فروشنده : " + myFactorModel.ClintNumber, font));
        //    Phone.Border = 1;
        //    Phone.PaddingBottom = 7;
        //    Phone.PaddingTop = 7;
        //    Phone.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.White);
        //    Phone.NoWrap = false;
        //    Phone.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
        //    Phone.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
        //    table.AddCell(Phone);



        //    //شماره پیگیری
        //    iTextSharp.text.pdf.PdfPCell kodePeygiri = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("کد پیگیری سفارش: " + myFactorModel.TrackingCode, font));
        //    kodePeygiri.Border = 1;
        //    kodePeygiri.PaddingBottom = 7;
        //    kodePeygiri.PaddingTop = 7;
        //    kodePeygiri.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.White);
        //    kodePeygiri.NoWrap = false;
        //    kodePeygiri.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
        //    kodePeygiri.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
        //    table.AddCell(kodePeygiri);



        //    //فوتر

        //    iTextSharp.text.pdf.PdfPCell Footer = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("آسودگی خاطر از یک خرید مطمئن", font));
        //    Footer.Border = 0;
        //    Footer.PaddingBottom = 5;
        //    Footer.PaddingTop = 20;
        //    Footer.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.White);
        //    Footer.NoWrap = false;
        //    Footer.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
        //    Footer.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
        //    table.AddCell(Footer);



        //    iTextSharp.text.pdf.PdfPCell FooterEnd = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("www.baladchee.com", font));
        //    FooterEnd.Border = 0;
        //    FooterEnd.PaddingBottom = 5;
        //    FooterEnd.PaddingTop = 20;
        //    FooterEnd.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.White);
        //    FooterEnd.NoWrap = false;
        //    FooterEnd.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
        //    FooterEnd.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
        //    table.AddCell(FooterEnd);

        //    document.Add(table);

        //    document.Close();


        //    byte[] fileBytes = System.IO.File.ReadAllBytes(path);
        //    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, filename);


        //}
        #endregion




        [AllowAnonymous]
        [RemoveSpaceFilter]
        public ActionResult MoreProducts(string id, string search, int? page, string order, string error)
        {
            ApplicationDbContext db = new ApplicationDbContext();


            //bia pardakht nashodeye hazine vitrin ro peyda kon dar ebteda kolan
            var pointer = db.PardakhtVitrin.Where(r => r.ClienId == id && r.pardakht == 0).FirstOrDefault();
            if (pointer.testTime < DateTime.Now && pointer.End == null || pointer.testTime == null && pointer.End < DateTime.Now)
            {
                //صفحه  مور را نمایش نده تا مبلغ را پرداخت کند 
                ViewBag.EndOfOpportunity = "end";
            }


            //baraye back az samte moreproduct
            ViewBag.BackLink = Request.Url.AbsoluteUri;

            //error tedad ke az tarafe AddmoreProduct miad yani kalahatun bish az hade mojaz ast
            if (error == "moreProduct")
            {
                ViewBag.TedadKalaMojazNist = "TedadKalaMojazNist";
            }

            //sefaresh sabt shode ast
            if (order == "true")
            {
                ViewBag.order = "true";
            }
            if (page != null)
            {
                ViewBag.page = page;
            }
            ViewBag.clientid = id;
            var k = User.Identity.GetUserId();
    

            var myid = "";
            try
            {
                myid = db.Client.Where(i => i.Id == id).FirstOrDefault().UserId;

            }
            catch (Exception)
            {

                return View("Error");
            }

            string flag = "";
            bool result = User.IsInRole("SuperAdmin");
            //agar farde login karde super admin bud
            if (result)
            {
                flag = "yes";
            }
            else
            {


                //agar agahi baraye in shakhsi ast ke login shode
                if (myid == k)
                {
                    flag = "yes";
                }
                else
                {
                    flag = "no";
                }
            }
            ViewBag.flag = flag;
            ViewBag.SelectPage = db.Client.Where(e => e.Id == id).FirstOrDefault();
            //baryae negah dashtane meghdare search dar har pagging
            ViewBag.search = search;



            #region baraye scroll kardane be kalaha bad az search:)
            if (search!=null&&search!="")
            {
                ViewBag.result = "result";
            }
            #endregion



            #region baraye namayesh kalamate kelidi
            //ViewBag.categoryWord
            List<string> ListOfKeys = new List<string>();



            var Word = db.CategoryWord.Where(d => d.ClienId == id).FirstOrDefault();
            string oneWord = "";

            //alave bar aan bayad tool tag az yek vaze bishtar bashad(hade aghal yek kalame bashad)
            if (Word != null)
            {

                string list = "";

                //baraye yek tag
                if (!Word.Word.Contains(","))
                {
                    oneWord = Word.Word;
                    ListOfKeys.Add(oneWord);
                }
                //agar bish az yek tag bud
                else
                {

                    for (int i = Word.Word.Length - 1; i >= 0; i--)
                    {

                        if (Word.Word.Substring(i, 1) != ",")
                        {
                            oneWord = Word.Word.Substring(i, 1) + oneWord;
                            list = Word.Word.Substring(i, 1) + list;
                        }


                        else if (Word.Word.Substring(i, 1) == ",")
                        {
                          
                            ListOfKeys.Add(list);
                            oneWord = "," + oneWord;
                            list = "";


                            //bia bebin aya akharin kalame ast
                            //agar are bia uno beriz tu reshte va tamam

                            if (!Word.Word.Substring(0, i).Contains(","))
                            {
                         
                                var endWord = Word.Word.Substring(0, i);
                                ListOfKeys.Add(endWord);
                                oneWord = endWord + oneWord;
                                break;
                            }
                        }



                    }
                }

          

            }

            ViewBag.ListOfKeys = ListOfKeys;
            ViewBag.categoryWord = oneWord;
            #endregion



            return View(db.MoreProducts.Where(r => r.pTitle.Contains(search) && r.ClienId == id && r.Display != "notsee" || search == null && r.ClienId == id && r.Display != "notsee" || r.DasteBandi.Contains(search) && r.ClienId == id && r.Display != "notsee" || r.pContent.Contains(search) && r.ClienId == id && r.Display != "notsee").ToList().OrderByDescending(r => r.pCreateDate).ToPagedList(page ?? 1, 8));
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult AddMoreProduct(string Id, string pContent, HttpPostedFileBase RootAddresProduct, string pTitle, string Takhfif, string GhabeliatGheymat, string GheymatGhabli, string GheymatFeli, string SefareshMahsool, string MoreProductSocialMediaLink, string DasteBandi, string anbarGardani)
        {
            MoreProducts item = new Core.MoreProducts();
            ApplicationDbContext db = new ApplicationDbContext();
            //in if vase ine ke faghat khode fard betune agahi khodesho edit kone
            //super admin ham mitune edit kone

            if (db.Client.Where(s => s.Id == Id).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {

                if (ModelState.IsValid)
                {
                    //bedune dashtane code zarin pal be in ghesmat umadan emkan pazir nist
                    //vali baz baraye etminan if ro mizaram
                    if (!Replace.hasZarinCode(Id))
                    {
                        return View("NotFound");
                    }


                    //tedade agahi ha nabayad bish az 997 adad bashad
                    List<MoreProducts> ClientProducts = new List<Core.MoreProducts>();
                    ClientProducts = db.MoreProducts.Where(t => t.ClienId == Id && t.Display != "notsee").ToList();
                    if (ClientProducts.Count() <= 2023)
                    {

                        if (RootAddresProduct != null && RootAddresProduct.ContentLength > 0)
                            try
                            {

                                string ValuePath = Path.GetFileName(Guid.NewGuid().ToString()) + TypHelper(RootAddresProduct.FileName);
                                string ValueSaverSaver = Path.Combine(Server.MapPath("~/Contents/Pic"), ValuePath);
                                WebImage img = new WebImage(RootAddresProduct.InputStream);

                                if (img.Width > 660)
                                {
                                    img.Resize(660, img.Height);
                                }

                                if (img.Height > 460)
                                {
                                    img.Resize(img.Width, 460);
                                }
                                img.Save(ValueSaverSaver);
                                item.pPicAddres = "~/Contents/Pic/" + ValuePath;
                                item.ClienId = Id;
                                if (pContent.Length < 3500)
                                {
                                    item.pContent = pContent;
                                }
                                if (pTitle.Length < 100)
                                {
                                    item.pTitle = pTitle;
                                }
                                if (Takhfif != null)
                                {
                                    item.Takhfif = "تخفیف دارد";
                                }
                                //agar ghabeliate sefaresh darad bayad gheymat ham dashte bashad
                                if (GhabeliatGheymat != null && GheymatFeli != null)
                                {
                                    item.NamayesheGheymat = "قابلیت نمایش قیمت دارد";
                                    item.GheymatFeli = GheymatFeli;
                                }
                                //gheymate bedune takhfif
                                if (GheymatGhabli != null && Takhfif != null)
                                {
                                    item.GheymatGhabli = GheymatGhabli;
                                }
                                //ghabeliate sefaresh darad
                                //baraye inke ta gheymat nadare, ghabeliate sefaresh nadashte bashe
                                if (SefareshMahsool != null && GhabeliatGheymat != null)
                                {
                                    item.SefareshMahsool = "قابلیت سفارش دارد";
                                }


                                //bia bebin tu linki ke dade instagram hast. ye mogheili linke jaye dg nade
                                //felan ba in siasat hala age ye ruzi insta nabud ja be ja mikonam
                                //pas:
                                if (MoreProductSocialMediaLink.Contains("www.instagram.com") && MoreProductSocialMediaLink.Length < 120)
                                {
                                    item.MoreProductSocialMediaLink = MoreProductSocialMediaLink;
                                }

                                //Emale Daste Bandi
                                if (DasteBandi != null && DasteBandi != "")
                                {
                                    item.DasteBandi = DasteBandi;
                                }



                                //anbar gardani
                                try
                                {
                                    if (anbarGardani != null && anbarGardani != "")
                                    {
                                        var myCount = Convert.ToInt32(anbarGardani);
                                        if (myCount < 100000 && myCount > 0)
                                        {
                                            item.count = myCount;
                                        }
                                    }

                                }
                                catch (Exception)
                                {

                                    return View("NotFound");
                                }
                                //end anbarGardani


                                item.pCreateDate = DateTime.Now;
                                db.MoreProducts.Add(item);
                                db.SaveChanges();



                            }
                            catch (Exception)
                            {

                                return View("NotFound");
                            }


                    }
                    //Agar product ha bish az 997 ta shod digar save nakon
                    else
                    {
                        return RedirectToAction("MoreProducts", "Home", new { id = Id, error = "moreProduct" });
                    }
                }




                else
                {
                    //ViewBag.Message = "شما فایلی را مشخص نکرده اید";
                    return View("NotFound");
                }

            }
            //erroresh ro hanuz moshakhas nakardam codesh ro bayad benevisam
            return RedirectToAction("MoreProducts", "Home", new { id = Id });
        }




        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult EditMoreProduct(HttpPostedFileBase pRootAddres, string Id, string pId, string pTitle, string pContent, string search,/*az inja be bad*/   string GhabeliateGheymatEdit, string EditGheymatFeli, string GhabeliateTakhfifEdit, string EditGheymatGhabli, string EditGhabeliateSefaresh, string EditMoreProductSocialMediaLink, string EditDasteBandi, string anbarGardaniEdit)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            //in if vase ine ke faghat khode fard betune agahi khodesho edit kone
            if (db.Client.Where(s => s.Id == Id).FirstOrDefault().UserId == User.Identity.GetUserId().ToString() || User.IsInRole("SuperAdmin"))
            {
                var selectedItem = db.Client.Where(r => r.Id.ToString() == Id).FirstOrDefault();
                //agar pcontent kheili ziad bud begu
                if (pContent.Length > 3500)
                {

                    return RedirectToAction("MoreProducts", "Home", new { id = Id });
                }
                //agar ptitile kheili ziad bud begu
                if (pTitle.Length > 300)
                {
                    return RedirectToAction("MoreProducts", "Home", new { id = Id });
                }

                MoreProducts item = db.MoreProducts.Where(r => r.MoreProductId == pId).FirstOrDefault();
                if (item != null)
                {
                    if (pRootAddres != null && pRootAddres.ContentLength > 0)
                        try
                        {
                            //agar file akse mahsool bud paak kon                            
                            //ta fazaye ezafi dar server nagirad

                            if (System.IO.File.Exists((Server.MapPath(item.pPicAddres))))
                            {
                                System.IO.File.Delete((Server.MapPath(item.pPicAddres)));
                            }

                            string ValuePath = Path.GetFileName(Guid.NewGuid().ToString()) + TypHelper(pRootAddres.FileName);
                            string ValueSaverSaver = Path.Combine(Server.MapPath("~/Contents/Pic"), ValuePath);
                            WebImage img = new WebImage(pRootAddres.InputStream);

                            if (img.Width > 660)
                            {
                                img.Resize(660, img.Height);
                            }

                            if (img.Height > 460)
                            {
                                img.Resize(img.Width, 460);
                            }
                            img.Save(ValueSaverSaver);
                            item.pPicAddres = "~/Contents/Pic/" + ValuePath;
                            db.SaveChanges();



                        }
                        catch (Exception)
                        {

                            return View("NotFound");
                        }

                    if (pContent != null || pContent != "")
                    {
                        item.pContent = pContent;


                    }
                    if (pTitle != null || pTitle != "")
                    {
                        item.pTitle = pTitle;

                    }



                    //AzinjaEmal mikonim code JadidRa
                    //agar ghabeliate sefaresh darad bayad gheymat ham dashte bashad
                    if (GhabeliateGheymatEdit != null && EditGheymatFeli != null && EditGheymatFeli != "")
                    {
                        item.NamayesheGheymat = "قابلیت نمایش قیمت دارد";
                        item.GheymatFeli = EditGheymatFeli;



                    }
                    else
                    {
                        item.NamayesheGheymat = null;
                        item.GheymatFeli = null;
                    }

                    //ghabeliate Takhfif
                    //gheymate bedune takhfif
                    if (GhabeliateTakhfifEdit != null && EditGheymatGhabli != null && EditGheymatGhabli != "" && GhabeliateGheymatEdit != null && EditGheymatFeli != null && EditGheymatFeli != "")
                    {
                        item.Takhfif = "تخفیف دارد";
                        item.GheymatGhabli = EditGheymatGhabli;
                    }
                    else
                    {
                        item.Takhfif = null;
                        item.GheymatGhabli = null;
                    }

                    //ijade Sefaresh

                    if (EditGhabeliateSefaresh != null && EditGheymatFeli != null && EditGheymatFeli != "" && GhabeliateGheymatEdit != null)
                    {
                        item.SefareshMahsool = "قابلیت سفارش دارد";
                    }
                    else
                    {
                        item.SefareshMahsool = null;
                    }
                    //ta inja


                    //for instaLink
                    if (EditMoreProductSocialMediaLink != null && EditMoreProductSocialMediaLink != "")
                    {
                        if (EditMoreProductSocialMediaLink.Contains("instagram.com"))
                        {
                            item.MoreProductSocialMediaLink = EditMoreProductSocialMediaLink;
                        }
                    }
                    else
                    {
                        item.MoreProductSocialMediaLink = null;
                    }

                    //for DasteBandi
                    if (EditDasteBandi != null && EditDasteBandi != "")
                    {
                        item.DasteBandi = EditDasteBandi;
                    }
                    else
                    {
                        item.DasteBandi = null;
                    }

                    if (anbarGardaniEdit != null)
                    {
                        try
                        {
                            item.count = Convert.ToInt32(anbarGardaniEdit);
                        }
                        catch (Exception)
                        {


                        }
                    }



                    db.SaveChanges();
                }
            }

            return RedirectToAction("MoreProducts", "Home", new { id = Id, search = search });
        }



        //gharar ast hich sanadi pak nashavad
        [Authorize]
        public ActionResult DeleteMoreProduct(string pId, string userId)
        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            if (User.Identity.GetUserId() == userId || User.IsInRole("SuperAdmin"))
            {
                var item = db.MoreProducts.Where(d => d.MoreProductId == pId).FirstOrDefault();
                item.Display = "notsee";
                db.SaveChanges();

                //agar file akse mahsool bud paak kon
                //ta fazaye ezafi dar server nagirad
                //if (System.IO.File.Exists(Server.MapPath(item.pPicAddres)))
                //{
                //    System.IO.File.Delete((Server.MapPath(item.pPicAddres)));
                //}
            }
            var tt = db.Client.Where(t => t.UserId == userId).FirstOrDefault().Id;
            return RedirectToAction("MoreProducts", "Home", new { id = tt });
        }



        ////aplicatio download baraye hame in aplication haman application dar dashboard ast
        //baraye modiriat
        [ValidateAntiForgeryToken]
        public FileResult ApkDownload()
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/apks/baladcheeManagement.apk"));
            var response = new FileContentResult(fileBytes, "application/octet-stream");
            response.FileDownloadName = "baladcheeM.apk";
            return response;
        }


        //برای دریافت کلمات 
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult AddMoreProductCategory(List<string> ProductCategory, string userId)

        {
            ApplicationDbContext db = new Models.ApplicationDbContext();
            var cId = db.Client.Where(t => t.UserId == userId).FirstOrDefault().Id;
            if (User.Identity.GetUserId() == userId || User.IsInRole("SuperAdmin"))
            {
                //اگر قبلا کلمات کلیدی ثبت کرده بود پاک کن و این جدید هارو جایگزین کن
                foreach (var CategoryWord in db.CategoryWord.ToList())
                {
                    if (CategoryWord.ClienId == cId)
                    {
                        db.CategoryWord.Remove(CategoryWord);
                    }
                }
       
                //حالا بیا جایگزین کن
                //in plugin listi mide ke avalin khuneye araye "" hast va bayad in shart ro bezaram ta joda kone
         

                    if (ProductCategory[1].Length < 500)
                    {
                        CategoryWord item = new CategoryWord();
                        item.ClienId = cId;
                        item.Word = ProductCategory[1];
                        db.CategoryWord.Add(item);
                        db.SaveChanges();
                    }
                


            }


            return RedirectToAction("MoreProducts", "Home", new { id = cId });
        }

        public ActionResult daf()
        {
            return View();
        }

        //donloade narmafzare safhe vitrin

        [ValidateAntiForgeryToken]
        public FileResult APKDownloader(string ClientId)
        {

            ApplicationDbContext db = new Models.ApplicationDbContext();
            var myClient = db.Client.Where(s => s.Id == ClientId).FirstOrDefault();
            try
            {
                if (myClient.AppAddress != null)
                {

                    byte[] fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/apks/" + myClient.AppAddress));
                    var response = new FileContentResult(fileBytes, "application/octet-stream");
                    response.FileDownloadName = myClient.AppAddress;
                    return response;
                }
            }
            catch (Exception)
            {

                byte[] fileBytesv = System.IO.File.ReadAllBytes(Server.MapPath("~/apks/ErrorAPK.png"));
                var responsev = new FileContentResult(fileBytesv, "application/octet-stream");
                responsev.FileDownloadName = "ErrorAPK.png";
                return responsev;
            }

            byte[] fileBytes2 = System.IO.File.ReadAllBytes(Server.MapPath("~/apks/UnderConstruction1.png"));
            var response2 = new FileContentResult(fileBytes2, "application/octet-stream");
            response2.FileDownloadName = "UnderConstruction.png";
            return response2;

        }




    }
}