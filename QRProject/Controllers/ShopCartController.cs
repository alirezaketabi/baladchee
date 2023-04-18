using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QRProject.Models;
using QRProject.Core;
using QRProject.helper;
using QRProject.zarinpal;
using System.IO;
using System.Text.RegularExpressions;
using QRProject.FactorViewModel;
using QRProject.meli;
using Newtonsoft.Json.Linq;
using System.Net;

namespace QRProject.Controllers
{

    //کنترلر مربوط به سبد خرید
    //تفاوتی که ما با سایر فروشگاه ها داریم این هست که ما مجموعه ای از فروشگاه ها رو در بر میگیریم پس به تعداد
    //فروشگاه ها سبد خرید باید داشته باشیم
    //نکته دوم:فروشگاه ها از دو صفحه کالا برای صفحه خرید مبفرستن
    //پرداکت که از صفحه اصلی یعنی شو و دوم از صفحه مور پروداکتشون
    public class ShopCartController : Controller
    {



        // GET: ShopCart

        //clientId hamun esme uniq furushgahe masalan "diamond" pas yek esme
        //liste sabade kharid ro neshun mide bar asase name furushgah ke hamun clientid hast , tashkhis mide
        //har furushgah yek session ba name khodesh baraye sabade kharid misaze
        //is lowprice baraye in hast ke hadeaghal mizan baraye furush ast va az samte pardakht miad
        //felan hard code hast va hade aghal 50 toman ast
        public ActionResult myindex(string ClientId, string tab, string CommonCode, string BackLinkFromMore, string IsLowPrice)
        {
            //agar mablaghe kharid hash az 50T kamtar bud bia tu view begu behe
            if (IsLowPrice=="yes")
            {
                ViewBag.isLowPrice = "حداقل مبلغ خرید 50 هزارتومان میباشد";

             }
            //agar az moreBiad Linkro Begir back ro ke zad bargarde daghighan hamun page o hamun search more:)
            ViewBag.BackLinkFromMore = BackLinkFromMore;

            //yani code eshterak ijad karde va az samte action SetCommonCode miad
            //CommonCode ro tu tabe dovvom ke marbute be ijade code eshterake va az unja kolan charkheye karesh shuru shode
            //bebar va code ro ham behesh begu
            if (tab == "Active" && CommonCode != null)
            {
                ViewBag.CommonCode = CommonCode;
            }

            if (tab == "EshterakNotFound" && CommonCode != null)
            {
                ViewBag.EshterakNotFound = CommonCode;
            }

            //in vase moghei gozashtam ke yeki tu ijad eshterak sheytanat kone va id ro pak kone
            //yeki in ghete code ro neveshtam yeki ham tu replace 
            //Tuye ghesmate returnClientTitle
            //hamun if dakhelesh
            //.........................................

            ApplicationDbContext db = new ApplicationDbContext();
            if (ClientId == null)
            {
                ClientId = db.Client.OrderByDescending(e => e.CreateDate).ToList().FirstOrDefault().Id;

            }


            ViewBag.Client = ClientId;
            List<ShopCartViewModel> list = new List<ShopCartViewModel>();

            //بیا ببین این فرد از این فروشگاه سبد خرید دارد
            if (Session[ClientId] != null)
            {
                //اگر دارد سشن را به لیست شاپکارت تبدیل کن
                //شاپکارت یک کلاس ،حداقل چیزهای مورد نیاز که حجم رم هم کم باشه
                //مورپروداکت یا پروداکت با تعداد
                List<ShopCartItem> cart = Session[ClientId] as List<ShopCartItem>;


                //ممکن سشن داشته باشه ولی تعداد محصولات  صفر باشه که ارور میده پس بیا چک کن
                if (cart.Count >= 1)
                {

                    //حالا برو داخل لیست و ببین سبد کالاش، چندتاش از پروداکت هست و چندتاش از مور پروداکت
                    foreach (var shopCartItem in cart)
                    {

                        //pas moreProduct ast
                        if (shopCartItem.ProductId == null)
                        {
                            //bia chek kon bebin furushande mahsul ro pak nakarde inke tu sabade kharide bude(albate hichvaght pak nemishe faghat az dide furushande makhfi mishe)
                            //inkar baraye amniat va dashtane madrak badan shayad lazem beshe
                            var moreProduct = db.MoreProducts.Where(w => w.MoreProductId == shopCartItem.MoreProductId && w.Display == null).FirstOrDefault();
                            //agar null nabud yani pas pak nakarde va hanuz tu database
                            if (moreProduct != null && moreProduct.count > 0)
                            {
                                list.Add(new ShopCartViewModel()
                                {
                                    ClientId = ClientId,
                                    MoreProductId = shopCartItem.MoreProductId,
                                    Count = shopCartItem.Count,
                                    Title = moreProduct.pTitle,
                                    Price = Convert.ToInt32(moreProduct.GheymatFeli),
                                    Sum = shopCartItem.Count * Convert.ToInt32(moreProduct.GheymatFeli),
                                    ImageAddress = moreProduct.pPicAddres

                                });
                            }

                        }

                        //pas Product ast
                        if (shopCartItem.MoreProductId == null)
                        {
                            //bia chek kon bebin furushande mahsul ro pak nakarde inke tu sabade kharide bude(albate hichvaght pak nemishe faghat az dide furushande makhfi mishe)
                            //inkar baraye amniat va dashtane madrak badan shayad lazem beshe
                            var Product = db.Product.Where(e => e.pId == shopCartItem.ProductId && e.Display == null).FirstOrDefault();
                            //agar null nabud yani pas pak nakarde va hanuz tu database
                            if (Product != null && Product.count > 0)
                            {
                                list.Add(new ShopCartViewModel()
                                {
                                    ClientId = ClientId,
                                    ProductId = shopCartItem.ProductId,
                                    Count = shopCartItem.Count,
                                    Title = Product.pTitle,
                                    Price = Convert.ToInt32(Product.GheymatFeli),
                                    Sum = shopCartItem.Count * Convert.ToInt32(Product.GheymatFeli),
                                    ImageAddress = Product.pPicAddres

                                });
                            }

                        }
                    }
                }
                //agar session mosavi nul bud ya sesseion dasht vali kalayi nabud
                //1-tedade mahsulatesh 0 sesseion ijad karde vali badan mahsoolatesh ro pak karde
                else
                {
                    //فرم اطلاعات فردی رو اصلا بهش نشون نده

                    ViewBag.NotSession = "NOT";
                }
            }
            //agar session mosavi nul bud ya sesseion dasht vali kalayi nabud
            //2-session nadare
            else
            {
                //اگر سشن نداشت فرم اطلاعات فردی رو اصلا بهش نشون نده
                ViewBag.NotSession = "NOT";
            }

            //agar list be har dalili(hala furushande pak karde mahsul ro ya ...)
            //baz ham namayesh nade forme etelaat ro
            if (list.Count == 0)
            {
                ViewBag.NotSession = "NOT";
            }
            return View(list);
        }




        //از سمت صفحه ویترین (شو و مور پروداکت) با ایجکس به این متد دسترسی پیدا میکنیم
        //سبد خرید را با توجه به اینکه پروداکت است و یا مورپروداکت است پر کن
        // نکته مهم این است که چون این پروژه شامل چند فروشگاه است سشن های مربوط به هر فروشگاه باید جدا ساخته شود
        //یعنی من از یک فروشگاه یک تعداد کالا داخل سبد خرید دارم و از یک فروشگاه دیگر یک تعداد دیگر
        //این ها باید جدا باشند به همین دلیل برای هر فروشگاه یک سشن جدا با نام فروشگاه بساز

        public ActionResult AddToCart(string ProductId, string MoreProductId, string ClientId)
        {
            List<ShopCartItem> cart = new List<ShopCartItem>();

            //اگر نال نبود یعنی این چندمین خریدش هست نه اولی
            if (Session[ClientId] != null)
            {
                //سشن را کست کن و پس بده
                cart = Session[ClientId] as List<ShopCartItem>;
            }


            //پس این محصول پروداکت است و توی پروداکت ها بگرد
            if (MoreProductId == null)
            {
                //حالا اگه از این محصول قبلا بوده یکی به تعدادش اضافه کن
                if (cart.Any(p => p.ProductId == ProductId))
                {
                    var findItem = cart.Where(r => r.ProductId == ProductId).FirstOrDefault();
                    ApplicationDbContext db = new ApplicationDbContext();
                    var myProduct = db.Product.Where(r => r.pId == ProductId && r.ClienId == ClientId).FirstOrDefault();
                    var SumAnbar = myProduct.count - findItem.Count;
                    if (SumAnbar > 0)
                    {
                        findItem.Count += 1;
                    }
                }
                //در غیر این صورت اولین بار داره این کالا رو میخره
                else
                {
                    cart.Add(new ShopCartItem()
                    {
                        ProductId = ProductId,
                        Count = 1

                    });
                }
            }

            //در غیر این صورت
            //پس این محصول مور پروداکت است و توی مور پروداکت ها دنبال آن بگرد
            if (ProductId == null)
            {
                //حالا اگه از این محصول قبلا بوده یکی به تعدادش اضافه کن
                if (cart.Any(p => p.MoreProductId == MoreProductId))
                {
                    var findItem2 = cart.Where(r => r.MoreProductId == MoreProductId).FirstOrDefault();
                    ApplicationDbContext db = new ApplicationDbContext();
                    var myMoreProduct = db.MoreProducts.Where(r => r.MoreProductId == MoreProductId && r.ClienId == ClientId).FirstOrDefault();
                    var SumAnbar2 = myMoreProduct.count - findItem2.Count;
                    if (SumAnbar2 > 0)
                    {
                        findItem2.Count += 1;
                    }
                }
                //در غیر این صورت اولین بار داره این کالا رو میخره
                else
                {
                    cart.Add(new ShopCartItem()
                    {
                        MoreProductId = MoreProductId,
                        Count = 1

                    });
                }
            }
            //حالا مقادیر را دوباره بریز توی سشن
            Session[ClientId] = cart;

            //در نهایت تعداد اقلام را برگردان
            //return cart.Sum(e => e.Count);
            return RedirectToAction("ShopCartCount", new { CIDforShopCart = ClientId });

        }




        //صفحه ای که سبد خرید در آن است، بعد از هر بار ریلود باید تعداد کالاهارا نمایش دهد
        //با این متد امکانپذیر است
        //این متد با جاوا اسکریپت صفحه سبد خرید پس از هر بار لود فراخوانی میشود
        public int ShopCartCount(string CIDforShopCart)
        {
            int count = 0;

            if (Session[CIDforShopCart] != null)
            {
                List<ShopCartItem> cart = Session[CIDforShopCart] as List<ShopCartItem>;
                if (cart.Count > 0)
                {
                    ApplicationDbContext db = new ApplicationDbContext();
                    foreach (var p in cart)
                    {
                        if (p.ProductId != null)
                        {

                            //bia bebin furushande in mahsul ro pak karde ya na
                            //chon suti mishe mahsul pak beshe heyne inke tu sabade kharide
                            //sabad tedado neshun mide vali mahsul nist
                            //ba in code payin in suti bartaraf mishe chon ghable namayeshe tedad ba database check mishe
                            var product = db.Product.Where(e => e.pId == p.ProductId && e.Display == null).FirstOrDefault();
                            if (product != null)
                            {
                                count = count + p.Count;
                            }

                        }


                        if (p.MoreProductId != null)
                        {
                            //bia bebin furushande in mahsul ro pak karde ya na
                            //chon suti mishe mahsul pak beshe heyne inke tu sabade kharide
                            //sabad tedado neshun mide vali mahsul nist
                            //ba in code payin in suti bartaraf mishe chon ghable namayeshe tedad ba database check mishe
                            var moreproduct = db.MoreProducts.Where(e => e.MoreProductId == p.MoreProductId && e.Display == null).FirstOrDefault();
                            if (moreproduct != null)
                            {
                                count = count + p.Count;
                            }

                        }

                    }
                }
                //count = cart.Sum(p => p.Count);
            }

            return count;
        }


        //baraye kharid ba code eshterak ke hazineye ersal ro behesh neshun bede

        [HttpPost]
        public JsonResult SendDeliveryPrice(string EshterakCode, string ClientId)
        {

            //aslan bia bebin sesion hast albate gheire momkene vali yek bar ye lahze sho
            //pas:
            if (Session[ClientId] == null)
            {
                //indexer baes mishe nare pain va be session barkhord nakone va errore jump nade
                return Json(new { success = true, responseText = "کد اشتراک صحیح نمیباشد" }, JsonRequestBehavior.AllowGet);


            }
            var db = new ApplicationDbContext();
            var ClientItem = db.Client.Where(t => t.Id == ClientId).FirstOrDefault();
            var eshterak = db.moshtari.Where(r => r.EshterakCode == EshterakCode).FirstOrDefault();
            if (ClientItem == null || eshterak == null)
            {

                return Json(new { success = true, responseText = "کد اشتراک صحیح نمیباشد" }, JsonRequestBehavior.AllowGet);

            }
            else
            {


                string DPrice = "";
                if (eshterak.City == ClientItem.City)
                {//yani shahre furushande ba kharidar yekie
                 //bia session sabadekharid ro begir va hazineye ersal ro por kon
                 //cast kon be jense dakhele session yani listi az ShopCartItem
                 //bebin in liste dorostesh ine ke tu yek field zakhire beshe vali eybi nadare tu yeki az aza ham berizi mablagh ro kafie va unvar giresh miaram
                    List<ShopCartItem> cart = Session[ClientId] as List<ShopCartItem>;

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
                    Session[ClientId] = cart;
                }


                else
                //agar inja biad yani shahrashun motefavete va hazineye borunshahri hesab mishe 
                {
                    List<ShopCartItem> cart = Session[ClientId] as List<ShopCartItem>;

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
                    Session[ClientId] = cart;
                }

                //if (DPrice=="0")
                //{
                //return Json(new { success = true, responseText = "✓" + " هزینه ارسال رایگان است " }, JsonRequestBehavior.AllowGet);

                return Json(new { success = true, responseText = DPrice }, JsonRequestBehavior.AllowGet);

                //}

                //return Json(new { success = true, responseText = "✓"+" هزینه ارسال به مقصد شما "+DPrice+" تومان میباشد " }, JsonRequestBehavior.AllowGet);
                //return Json(new { success = true, responseText = "✓" + " هزینه ارسال به مقصد شما " + DPrice + " تومان میباشد " }, JsonRequestBehavior.AllowGet);

            }

        }




        [HttpPost]
        public JsonResult DoTakhfifPrice(string TakhfifCode, string ClientId)
        {
            //aslan bia bebin sesion hast albate gheire momkene vali yek bar ye lahze sho
            //pas:
            if (Session[ClientId] == null)
            {
                //indexer baes mishe nare pain va be session barkhord nakone va errore jump nade
                return Json(new { success = true, responseText = "کد تخفیف صحیح نمیباشد" }, JsonRequestBehavior.AllowGet);


            }

            var db = new ApplicationDbContext();
            //bebin in client hast agar hast bebin ba in code takhfif in client hast


            //in dota bayad baham check shavad code takhfif baraye in furushande hast
            //chon shayad etefaghi donafar code takhfife yeksan darand!
            //ba in code in etefagh nemiofte
            var myTakhfifCode = db.Client.Where(r => r.SaleCode == TakhfifCode && r.Id == ClientId).FirstOrDefault();



            if (myTakhfifCode == null)
            {

                return Json(new { success = true, responseText = "کد تخفیف صحیح نمیباشد" }, JsonRequestBehavior.AllowGet);

            }

            else
            {    //in furushande ba in code takhfif darad hala chand darsad?
                var darsad = myTakhfifCode.Percent;
                var MeghdareEmal = 100 - Convert.ToInt32(darsad);
                var TanzimeMeghdar = "0." + MeghdareEmal.ToString();

                //hala in adad (TanzimeMeghdar) bayad zarbdar meghdare nahai kalaha beshe
                List<ShopCartItem> cart = Session[ClientId] as List<ShopCartItem>;
                cart.FirstOrDefault().DarsadTakhfif = TanzimeMeghdar;
                Session[ClientId] = cart;
                return Json(new { success = true, responseText = TanzimeMeghdar }, JsonRequestBehavior.AllowGet);


            }

        }



        //برای پاک کردن محصول از سبد کالا
        //برای کدام فروشگاه و چه محصولی
        public ActionResult RemoveFromCart(string ClientId, string ProductId, string MoreProduct)
        {
            List<ShopCartItem> cart = Session[ClientId] as List<ShopCartItem>;

            //pas in mahsoul product ast. albate inja mohem nist va baraye jelogiri az error tashkhis bayad bedim
            if (ProductId != null && MoreProduct == null)
            {
                var index = cart.Where(e => e.ProductId == ProductId).FirstOrDefault();
                cart.Remove(index);
                Session[ClientId] = cart;
            }
            //pas in mahsoul Moreproduct ast. albate inja mohem nist va baraye jelogiri az error tashkhis bayad bedim
            if (ProductId == null && MoreProduct != null)
            {
                var index = cart.Where(e => e.MoreProductId == MoreProduct).FirstOrDefault();
                cart.Remove(index);
                Session[ClientId] = cart;
            }


            return RedirectToAction("myindex", new { ClientId = ClientId });
        }







        //ببین قیمت نهایی بر اساس مجموع قیمت محصولات به علاوه قیمت ارسال محصولات به علاوه درصد تخفیف
        //پس باید قبل از پرداخت این ها بررسی شود، خب تمامی اطلاعات خرید یک فروشگاه یک فرد در سشن خرید آن از آن فروشگاه است
        //پس دو پراپرتی در سشن است که با توجه به وارد کردن مقصد خریدار و درصد تخفیف در مدل داخل سشن ذخیره میشود
        //yeki DarsadTakhfif va deliveryiPrice    








        //az samte myindex miad inja

        [ValidateAntiForgeryToken]
        public ActionResult GoPay(string id /*clientId Hast*/, string Name, string Address, string Phone, string PostalCode, string Tozihat, string LocationLatitude, string LocationLongitude, string EshterakCode /*agar ba code eshterak bud*/, string EshterakTozihat)
        {




            if (PostalCode == "" || PostalCode == null)
            {
                PostalCode = "404";
            }

            //قسمت اول اگر با کد اشتراک خرید کرد
            //در هر ویو یک بار میشود ریکپچا استفاده کرد که من برای خرید با فرم در نظر گرفتم
            //و بنظرم آنتیفرجری برای خرید با کد اشتراک کافی باشه




            if (EshterakCode != null && EshterakCode.Length < 40)
            {
                //bia moshtarak ro peyda kon

                ApplicationDbContext db = new ApplicationDbContext();
                var moshtarak = db.moshtari.Where(r => r.EshterakCode == EshterakCode).FirstOrDefault();
                //agar true bud pas moshtarak peyda shode
                if (moshtarak != null)
                {          //بیا چک کن آیا این خریدار سشن سبد خرید این فروشگاه رو داره
                    if (Session[id] != null)
                    {
                        //bia session ro pas bede
                        List<ShopCartItem> cart = Session[id] as List<ShopCartItem>;


                        #region Create tables

                        //در این قسمت دیگر نیاز نیست جدول مشتری ساخته شود چرا که از قبل در سیستم بوده است
                        //و مشترک شده است

                        //ببین هم مشترکین و هم غیر مشترکین در یک جدول ساخته شدن مدل به این صورت
                        //factor table
                        Sefareshat Order = new Sefareshat();
                        Order.isFinaly = false;
                        Order.Tozihat = EshterakTozihat;
                        Order.isSeen = false;
                        Order.Tracking_Code = Replace.CreateTarackingCode();
                        Order.clientId = id;
                        Order.moshtariId = moshtarak.Id;
                        db.Sefareshat.Add(Order);
                        db.SaveChanges();
                        //Order.FactorAddres==>تکمیل و بررسی شود

                        #endregion


                        //ببین از حجم این کد نترس فقط داره از سشن اطلاعات دقیق و جامع محصولات رو میده تا ما بتونیم قیمت و 
                        //اطلاعات رو استخراج کنیم
                        //in moteghayer flag vase ine ke agar mojudi nadasht va count sefr shod bedune inke karbar bedune va
                        //safhe ruye sabade kala munde vali kala dg tamum shode(hameye kala hash) reload kone khali nare tu dargah
                        bool flager = false;
                        foreach (var shopCartItem in cart)
                        {
                            //pas moreProduct ast
                            if (shopCartItem.ProductId == null)
                            {
                                //bia chek kon bebin furushande mahsul ro pak nakarde inke tu sabade kharide bude
                                var moreProduct = db.MoreProducts.Where(e => e.MoreProductId == shopCartItem.MoreProductId && e.Display == null).FirstOrDefault();
                                //agar null nabud yani pas pak nakarde va hanuz tu database
                                if (moreProduct != null)
                                {
                                    flager = true;
                                    //از آن محصول هر تعداد هست ، به همان تعداد در ریز فاکتور قید کن
                                    for (int i = 1; i <= shopCartItem.Count; i++)
                                    {
                                        //add kon tu riz sefareshat
                                        RizSefareshat rizsefaresh = new RizSefareshat();
                                        rizsefaresh.MoreProducstId = shopCartItem.MoreProductId;
                                        rizsefaresh.SefareshatId = Order.Id;

                                        //harbar be gheymat factor bar asase gheymate mahsoolat ezafe kon
                                        Order.Sum = Order.Sum + Convert.ToInt32(moreProduct.GheymatFeli);
                                        db.RizSefareshat.Add(rizsefaresh);
                                        db.SaveChanges();
                                    }



                                }

                            }

                            //pas Product ast
                            if (shopCartItem.MoreProductId == null)
                            {
                                //bia chek kon bebin furushande mahsul ro pak nakarde inke tu sabade kharide bude
                                var Product = db.Product.Where(r => r.pId == shopCartItem.ProductId && r.Display == null).FirstOrDefault();
                                //agar null nabud yani pas pak nakarde va hanuz tu database
                                if (Product != null)
                                {
                                    flager = true;
                                    //از آن محصول هر تعداد هست ، به همان تعداد در ریز فاکتور قید کن
                                    for (int i = 1; i <= shopCartItem.Count; i++)
                                    {
                                        //add kon tu riz sefareshat
                                        RizSefareshat rizsefaresh = new RizSefareshat();
                                        rizsefaresh.ProductId = shopCartItem.ProductId;
                                        rizsefaresh.SefareshatId = Order.Id;

                                        //harbar be gheymat factor bar asase gheymate mahsoolat ezafe kon
                                        Order.Sum = Order.Sum + Convert.ToInt32(Product.GheymatFeli);
                                        db.RizSefareshat.Add(rizsefaresh);
                                        db.SaveChanges();
                                    }
                                }

                            }


                            if (flager != true)
                            {
                                Session[id] = null;
                                return RedirectToAction("show", "home", new { id = id });
                            }



                            //bia dar akhar mablaghe ersal ro az session begir ke tuye action SendDeliveryPrice  be session ezafe shode
                            //faghat ghablesh chek kon bebin takhfif dare agar dare ruye gheymate mahsulat (be gheir az hazine ersal)
                            //tasmim bar ine ke faghat dar ghesmate kharid ba eshterak bashe code takhfif
                            //pas
                            //yek moteghayer ezafe mikonim: bad beshine jaye sy
                            foreach (var item in cart)
                            {
                                if (item.DeliveryPrice != null)
                                {
                                    if (Order.DeliveryPrice == null || Order.DeliveryPrice == "")
                                    {//agar in if nabashe be tedade kalaha hazineye ersal ezafe mikone                                                              

                                        Order.Sum = Convert.ToInt32(item.DeliveryPrice) + Order.Sum;
                                        Order.DeliveryPrice = item.DeliveryPrice;
                                        db.SaveChanges();
                                    }

                                }
                            }

                        }



                        //halabia mablaghe factor ro dar biar gheymate ersal ro bekesh birun darsad ro emal kon
                        //dobare ba mablaghe ersal jam kon

                        foreach (var item in cart)
                        {
                            if (item.DarsadTakhfif != null && float.Parse(item.DarsadTakhfif) < 1 && float.Parse(item.DarsadTakhfif) > 0)
                            {//in yek kalake ke yek bar emal kone az if bala komak gereftam
                                //yekam fekr koni motevaje mishi chon yek bar bishtar inja nemiad ba in if ha
                                //va inke aslan nayad gheyre momkene
                                if (Order.DarsadTakhfif == null || Order.DarsadTakhfif == "")
                                {

                                    var CustomOrder = db.Sefareshat.Where(e => e.Id == Order.Id).FirstOrDefault();
                                    var SumWithOutDeliveryPrice = CustomOrder.Sum - Convert.ToInt32(CustomOrder.DeliveryPrice);
                                    var AmaliateTakhfif = SumWithOutDeliveryPrice * float.Parse(item.DarsadTakhfif);
                                    Order.Sum = Convert.ToInt32(AmaliateTakhfif) + Convert.ToInt32(Order.DeliveryPrice);
                                    Order.DarsadTakhfif = item.DarsadTakhfif;
                                    db.SaveChanges();

                                }

                            }
                        }




                        #region ZarinPalCode......................................................................


                        System.Net.ServicePointManager.Expect100Continue = false;
                        zarinpal.PaymentGatewayImplementationService zp = new zarinpal.PaymentGatewayImplementationService();
                        string Authority;

                        //server
                        var ClientMobile = db.Client.Where(w => w.Id == id).FirstOrDefault().mobile;
                        //int Status = zp.PaymentRequest("e1dbe04a-4a9a-11ea-b2ac-000c295eb8fc",Order.Sum,id, "alirezaketabi70@gmail.com",ClientMobile, "http://www.baladchee.com/ShopCart/Verify/" + Order.Id, out Authority);

                        //local
                        //int Status = zp.PaymentRequest("e1dbe04a-4a9a-11ea-b2ac-000c295eb8fc", int.Parse(bproduct.Price), bproduct.Name, "alirezaketabi70@gmail.com", "09059092414", "http://localhost:22900/Account/Verify/" + factor.Id, out Authority);
                        try
                        {
                            //bia ghable raftan be pardakht bebin mablaghe kharid az 50T bishtar hast ya na
                            //agar kamtar az 50 bud boro be sabade kharid va be gu hade aghal kharid 50T ast
                            if (Order.Sum < 50000)
                            {
                                return RedirectToAction("myindex", new { IsLowPrice = "yes" });
                            }
                            ViewBag.clientid = id;
                            //bayad client ro bekeshim birun ta az zarincodesh estefade konim
                            var ClientIdZarinPal = db.Client.Where(r => r.Id == id).FirstOrDefault().ZarinCode;
                            int Status = zp.PaymentRequest(ClientIdZarinPal, Order.Sum, id, null, ClientMobile, "http://www.baladchee.com/ShopCart/Verify/" + Order.Id, out Authority);

                            //اگر تایید شد برو به بانک
                            if (Status > 0)
                            {
                                Response.Redirect("https://www.zarinpal.com/pg/StartPay/" + Authority);
                            }
                            //اگر نه،برو یک ویو بساز و ارور رو نشون بده
                            else
                            {
                                ViewBag.error = "error: " + Status;

                            }

                        }

                        catch (Exception)
                        {

                            return View("_NotFoundForPay");
                        }


                        #endregion

                        return View();

                    }

                }
                else
                {
                    return RedirectToAction("myindex", "ShopCart", new { ClientId = id, tab = "EshterakNotFound", CommonCode = EshterakCode });
                }
            }








            //.............................................................................................................
            //قسمت دوم اگر با فرم خرید کرد

            //کدهای مربوط به ریکپچا.................................

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
                return RedirectToAction("myindex", "ShopCart", new { ClientId = id });
            }
            //............................................................


            //اگر نام ،آدرس،تلفن و کدپستی خالی نبود چون این 4 تا باید پر باشن برای خرید مهمترین نکات هستند البته اعتبار سنجی
            //سمت کلاینت هم انجام میشود
            if (Name != null && Address != null && Phone != null && PostalCode != null)
            {
                //طول اطلاعات وارد شده زیاد نباشد
                if (Name.Length <= 25 && Address.Length <= 120 && Phone.Length <= 22 && PostalCode.Length <= 20)
                {


                    //بیا چک کن آیا این خریدار سشن سبد خرید این فروشگاه رو داره
                    if (Session[id] != null)
                    {
                        //bia session ro pas bede
                        List<ShopCartItem> cart = Session[id] as List<ShopCartItem>;


                        #region Create tables

                        ApplicationDbContext db = new ApplicationDbContext();

                        //moshtari table
                        moshtari mymoshtari = new moshtari();
                        mymoshtari.Address = Address;
                        mymoshtari.PhoneNumber = Phone;
                        mymoshtari.Name = Name;
                        mymoshtari.PostalCode = PostalCode;
                        mymoshtari.LocationLatitude = LocationLatitude;
                        mymoshtari.LocationLongitude = LocationLongitude;
                        db.moshtari.Add(mymoshtari);


                        //factor table
                        Sefareshat Order = new Sefareshat();
                        Order.isFinaly = false;
                        Order.Tozihat = Tozihat;
                        Order.isSeen = false;
                        Order.Tracking_Code = Replace.CreateTarackingCode();
                        Order.clientId = id;
                        Order.moshtariId = mymoshtari.Id;
                        db.Sefareshat.Add(Order);
                        db.SaveChanges();
                        //Order.FactorAddres==>تکمیل و بررسی شود

                        #endregion


                        //ببین از حجم این کد نترس فقط داره از سشن اطلاعات دقیق و جامع محصولات رو میده تا ما بتونیم قیمت و 
                        //اطلاعات رو استخراج کنیم
                        //safhe ruye sabade kala munde vali kala dg tamum shode(hameye kala hash) reload kone khali nare tu dargah
                        bool flager2 = false;
                        foreach (var shopCartItem in cart)
                        {
                            //pas moreProduct ast
                            if (shopCartItem.ProductId == null)
                            {
                                //bia chek kon bebin furushande mahsul ro pak nakarde inke tu sabade kharide bude
                                var moreProduct = db.MoreProducts.Where(e => e.MoreProductId == shopCartItem.MoreProductId && e.Display == null).FirstOrDefault();
                                //agar null nabud yani pas pak nakarde va hanuz tu database
                                if (moreProduct != null)
                                {
                                    flager2 = true;
                                    //از آن محصول هر تعداد هست ، به همان تعداد در ریز فاکتور قید کن
                                    for (int i = 1; i <= shopCartItem.Count; i++)
                                    {
                                        //add kon tu riz sefareshat
                                        RizSefareshat rizsefaresh = new RizSefareshat();
                                        rizsefaresh.MoreProducstId = shopCartItem.MoreProductId;
                                        rizsefaresh.SefareshatId = Order.Id;

                                        //harbar be gheymat factor bar asase gheymate mahsoolat ezafe kon
                                        Order.Sum = Order.Sum + Convert.ToInt32(moreProduct.GheymatFeli);
                                        db.RizSefareshat.Add(rizsefaresh);
                                        db.SaveChanges();
                                    }



                                }

                            }

                            //pas Product ast
                            if (shopCartItem.MoreProductId == null)
                            {
                                //bia chek kon bebin furushande mahsul ro pak nakarde inke tu sabade kharide bude
                                var Product = db.Product.Where(r => r.pId == shopCartItem.ProductId && r.Display == null).FirstOrDefault();
                                //agar null nabud yani pas pak nakarde va hanuz tu database
                                if (Product != null)
                                {
                                    flager2 = true;
                                    //از آن محصول هر تعداد هست ، به همان تعداد در ریز فاکتور قید کن
                                    for (int i = 1; i <= shopCartItem.Count; i++)
                                    {
                                        //add kon tu riz sefareshat
                                        RizSefareshat rizsefaresh = new RizSefareshat();
                                        rizsefaresh.ProductId = shopCartItem.ProductId;
                                        rizsefaresh.SefareshatId = Order.Id;

                                        //harbar be gheymat factor bar asase gheymate mahsoolat ezafe kon
                                        Order.Sum = Order.Sum + Convert.ToInt32(Product.GheymatFeli);
                                        db.RizSefareshat.Add(rizsefaresh);
                                        db.SaveChanges();
                                    }
                                }

                            }


                            if (flager2 != true)
                            {
                                Session[id] = null;
                                return RedirectToAction("show", "home", new { id = id });
                            }


                            //bia dar akhar mablaghe ersal ro az session begir ke tuye action loadmap account be session ezafe shode moghe entekhabe shahre kharidar
                            //pas
                            foreach (var item in cart)
                            {
                                if (item.DeliveryPrice != null)
                                {
                                    if (Order.DeliveryPrice == null || Order.DeliveryPrice == "")
                                    {//agar in if nabashe be tedade kalaha hazineye ersal ezafe mikone
                                        Order.Sum = Convert.ToInt32(item.DeliveryPrice) + Order.Sum;
                                        Order.DeliveryPrice = item.DeliveryPrice;
                                        db.SaveChanges();
                                    }

                                }
                            }

                        }




                        #region ZarinPalCode......................................................................


                        System.Net.ServicePointManager.Expect100Continue = false;
                        zarinpal.PaymentGatewayImplementationService zp = new zarinpal.PaymentGatewayImplementationService();
                        string Authority;

                        //server
                        var ClientMobile = db.Client.Where(w => w.Id == id).FirstOrDefault().mobile;
                        //int Status = zp.PaymentRequest("e1dbe04a-4a9a-11ea-b2ac-000c295eb8fc",Order.Sum,id, "alirezaketabi70@gmail.com",ClientMobile, "http://www.baladchee.com/ShopCart/Verify/" + Order.Id, out Authority);

                        //local
                        //int Status = zp.PaymentRequest("e1dbe04a-4a9a-11ea-b2ac-000c295eb8fc", int.Parse(bproduct.Price), bproduct.Name, "alirezaketabi70@gmail.com", "09059092414", "http://localhost:22900/Account/Verify/" + factor.Id, out Authority);
                        try
                        {


                            //bia ghable raftan be pardakht bebin mablaghe kharid az 50T bishtar hast ya na
                            //agar kamtar az 50 bud boro be sabade kharid va be gu hade aghal kharid 50T ast
                            if (Order.Sum < 50000)
                            {
                                return RedirectToAction("myindex", new { IsLowPrice = "yes" });
                            }
                            ViewBag.clientid = id;
                            var ClientIdZarinPal = db.Client.Where(r => r.Id == id).FirstOrDefault().ZarinCode;
                            int Status = zp.PaymentRequest(ClientIdZarinPal, Order.Sum, id, null, ClientMobile, "http://www.baladchee.com/ShopCart/Verify/" + Order.Id, out Authority);

                            //اگر تایید شد برو به بانک
                            if (Status > 0)
                            {
                                Response.Redirect("https://www.zarinpal.com/pg/StartPay/" + Authority);
                            }
                            //اگر نه،برو یک ویو بساز و ارور رو نشون بده
                            else
                            {
                                ViewBag.error = "error: " + Status;

                            }

                        }

                        catch (Exception)
                        {

                            return View("_NotFoundForPay");
                        }





                        #endregion

                        return View();

                    }
                }
            }
            //اگر هم سشن نداشت و هم اطلاعات فردی رو درست وارد نکرده بود
            //البته هیچوت اینجا نمیرسه واسه اطمینان
            // چون تو ویو مای ایندکس اگر سشن نداشته باشه اصلا فرم اطلاعات فردی رو بهش نشون نمیدم که بیاد اینجا مگه اینکه 
            //فرم اطلاعات فردی رو خواسته دور بزنه دلیلی نداره چون با این کار چیزی به نفعش نیست
            //حالا شاید خواست سایت رو تست کنه
            return RedirectToAction("myindex", "ShopCart", new { ClientId = id });

        }



        public ActionResult Verify(string id)
        {




            ApplicationDbContext db = new ApplicationDbContext();
            var order = db.Sefareshat.Find(id);


            //آی دی فاکتور برای اینکه پاس بده به اکشن فاکتور برای ایجاد
            //پی دی اف فاکتور
            ViewBag.factorId = order.Id;

            var ClientId = db.Sefareshat.Where(i => i.Id == id).FirstOrDefault().clientId;
            ViewBag.clientid = ClientId;
            var ClientIdZarinPal = db.Client.Where(i => i.Id == ClientId).FirstOrDefault().ZarinCode;

            if (Request.QueryString["Status"] != "" && Request.QueryString["Status"] != null && Request.QueryString["Authority"] != "" && Request.QueryString["Authority"] != null)
            {
                if (Request.QueryString["Status"].ToString().Equals("OK"))
                {
                    int Amount = order.Sum;
                    long RefID;
                    System.Net.ServicePointManager.Expect100Continue = false;
                    zarinpal.PaymentGatewayImplementationService zp = new zarinpal.PaymentGatewayImplementationService();

                    int Status = 0;
                    try
                    {
                        Status = zp.PaymentVerification(ClientIdZarinPal, Request.QueryString["Authority"].ToString(), Amount, out RefID);

                    }
                    catch (Exception)
                    {

                        return View("_verifyPayError");
                    }


                    //کد زرین پال خودم هست
                    //"e1dbe04a-4a9a-11ea-b2ac-000c295eb8fc"

                    if (Status > 0)
                    {
                        order.isFinaly = true;
                        order.RefId = RefID.ToString();
                        db.SaveChanges();
                        ViewBag.IsSuccess = true;
                        ViewBag.RefId = RefID;




                        //in cod baraye in hast ke az count product ha kam kone
                        foreach (var item in db.RizSefareshat.ToList())
                        {
                            //اگر ترو بود پس محصولات این فاکتور
                            if (item.SefareshatId == id)
                            {
                                //این محصول از جدول پروداکت است و برو محصول رو از اون جدول بکش بیرون
                                if (item.ProductId != null)
                                {
                                    var productName = db.Product.Where(r => r.pId == item.ProductId).FirstOrDefault();
                                    if (productName.count > 0)
                                    {
                                        productName.count = productName.count - 1;
                                    }
                                }


                                //این محصول از جدول مورپروداکت است و برو محصول رو از اون جدول بکش بیرون
                                if (item.MoreProducstId != null)
                                {
                                    var moreproductName = db.MoreProducts.Where(r => r.MoreProductId == item.MoreProducstId).FirstOrDefault();
                                    if (moreproductName.count > 0)
                                    {
                                        moreproductName.count = moreproductName.count - 1;
                                    }
                                }

                            }
                        }

                        db.SaveChanges();
                        //end







                        //ممکن در روند اس ام اس خطا بده و کلا خرید رو مختل کنه واسه همین میزارمش تو ترای کش
                        try
                        {
                            //khob kharid taid shode,be kharidar o furushande etela bede 
                            #region SenSms                     

                            ////furushande
                            ////furushande
                            meli.Send senderForSeller = new Send();
                            var clientIdforPhone = db.Sefareshat.Where(i => i.Id == id).FirstOrDefault().clientId;

                            //bia bebin furushande laghv nakarde(agar gheir az 1 bud ya Null bud laghv ast)
                            var CheckSms = db.Client.Where(e => e.Id == ClientId).FirstOrDefault().LaghvPayamak;
                            if (CheckSms == "1")
                            {

                                var SellerPhoneNumber = db.Client.Where(i => i.Id == clientIdforPhone).FirstOrDefault().SupportNumber;
                                senderForSeller.SendByBaseNumber2("9017037365", "Aa30081370@", order.Tracking_Code, SellerPhoneNumber, Convert.ToInt32(51554));

                            }
                            ////kharidar
                            meli.Send senderForBuyer = new Send();
                            ////شماره خریدار
                            var MoshtariIdforPhone = db.Sefareshat.Where(i => i.Id == id).FirstOrDefault().moshtariId;
                            var moshtarPhonNumber = db.moshtari.Where(i => i.Id == MoshtariIdforPhone).FirstOrDefault().PhoneNumber;
                            senderForBuyer.SendByBaseNumber2("9017037365", "Aa30081370@", clientIdforPhone /*نام فروشگاه*/+ ";" + order.Tracking_Code, moshtarPhonNumber, Convert.ToInt32(48746));

                            #endregion


                        }
                        catch (Exception)
                        {


                        }


                        //khob kharidesh taid shod,sessioni ke ba in furush gah dasht ro pak kon
                        var clientId = db.Sefareshat.Where(i => i.Id == id).FirstOrDefault().clientId;
                        Session[clientId] = null;


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
            return View();
        }






        //این اکشن برای دانلود پی دی ام فاکتور است
        //کار آن ایجاد فاکتور و ذخیره در سرور و ذخیره آدرس آن در دیتابیس است
        public ActionResult pdf(string FactorId)
        {

            factorViewModel myFactorModel = new factorViewModel();


            ApplicationDbContext db = new ApplicationDbContext();
            myFactorModel.shopName = db.Sefareshat.Where(t => t.Id == FactorId).FirstOrDefault().clientId;
            myFactorModel.TrackingCode = db.Sefareshat.Where(t => t.Id == FactorId).FirstOrDefault().Tracking_Code;
            myFactorModel.buyerPhone = db.Sefareshat.Where(t => t.Id == FactorId).FirstOrDefault().moshtari.PhoneNumber;
            myFactorModel.ClintNumber = db.Sefareshat.Where(t => t.Id == FactorId).FirstOrDefault().Client.PhonNumber;
            myFactorModel.SubmitDate = db.Sefareshat.Where(t => t.Id == FactorId).FirstOrDefault().CreateDate;
            myFactorModel.ProductPrice = db.Sefareshat.Where(t => t.Id == FactorId).FirstOrDefault().Sum.ToString("#,0 تومان");
            myFactorModel.DeliveryPrice = db.Sefareshat.Where(t => t.Id == FactorId).FirstOrDefault().DeliveryPrice;
            myFactorModel.DarsadTakhfif = db.Sefareshat.Where(t => t.Id == FactorId).FirstOrDefault().DarsadTakhfif;


            List<string> FactorMahsoulName = new List<string>();

            //کلا این برای پر کردن محصولات فاکتور
            foreach (var item in db.RizSefareshat.ToList())
            {
                //اگر ترو بود پس محصولات این فاکتور
                if (item.SefareshatId == FactorId)
                {
                    //این محصول از جدول پروداکت است و برو محصول رو از اون جدول بکش بیرون
                    if (item.ProductId != null)
                    {
                        var productName = db.Product.Where(r => r.pId == item.ProductId).FirstOrDefault().pTitle + " قیمت واحد (تومان): " + db.Product.Where(r => r.pId == item.ProductId).FirstOrDefault().GheymatFeli;
                        FactorMahsoulName.Add(productName);
                    }


                    //این محصول از جدول مورپروداکت است و برو محصول رو از اون جدول بکش بیرون
                    if (item.MoreProducstId != null)
                    {
                        var moreproductName = db.MoreProducts.Where(r => r.MoreProductId == item.MoreProducstId).FirstOrDefault().pTitle + " قیمت واحد (تومان): " + db.MoreProducts.Where(r => r.MoreProductId == item.MoreProducstId).FirstOrDefault().GheymatFeli;
                        FactorMahsoulName.Add(moreproductName);
                    }

                }
            }

            myFactorModel.SefareshId = FactorId;




            //ابتدا فایل را ایجاد میکنیم
            iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 10, 10, 10, 10);

            string filename = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00")
            + DateTime.Now.Hour.ToString("00") + DateTime.Now.Minute.ToString("00") + DateTime.Now.Second.ToString("00") + DateTime.Now.Millisecond.ToString("00") + "-" + "فاکتور سفارش" + ".pdf";
            string path = Path.Combine(Server.MapPath("~/Contents/PDF/"), filename);

            //Create our file stream and bind the writer to the document and the stream
            iTextSharp.text.pdf.PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, new FileStream(path, FileMode.Create));



            //ثبت آدرس فاکتور در جدول سفارشات کلاینت

            var TheOrder = db.Sefareshat.Where(w => w.Id == myFactorModel.SefareshId).FirstOrDefault();
            TheOrder.FactorAddres = "~/Contents/PDF/" + filename;
            db.SaveChanges();
            //پایان کد



            //Open the document for writing
            document.Open();

            //Add a new page
            document.NewPage();


            iTextSharp.text.pdf.BaseFont bfArialUniCode = iTextSharp.text.pdf.BaseFont.CreateFont(Server.MapPath("~/fonts/tahoma.ttf"), iTextSharp.text.pdf.BaseFont.IDENTITY_H, iTextSharp.text.pdf.BaseFont.EMBEDDED);
            //Create a font from the base font
            iTextSharp.text.Font font = new iTextSharp.text.Font(bfArialUniCode, 10, 1, iTextSharp.text.BaseColor.BLACK);

            //Use a table so that we can set the text direction
            iTextSharp.text.pdf.PdfPTable table = new iTextSharp.text.pdf.PdfPTable(1);
            //Ensure that wrapping is on, otherwise Right to Left text will not display
            table.DefaultCell.NoWrap = false;
            //Create a regex expression to detect hebrew or arabic code points
            const string regex_match_arabic_hebrew = @"[\u0600-\u06FF,\u0590-\u05FF]+";
            if (Regex.IsMatch("م الموافق", regex_match_arabic_hebrew, RegexOptions.IgnoreCase))
            {
                table.RunDirection = iTextSharp.text.pdf.PdfWriter.RUN_DIRECTION_RTL;
            }


            //تصویر
            iTextSharp.text.Image myImage = iTextSharp.text.Image.GetInstance(Server.MapPath("~/images/PdfLogo.png"));
            iTextSharp.text.pdf.PdfPCell cellImg = new iTextSharp.text.pdf.PdfPCell(myImage);
            cellImg.Border = 0;
            cellImg.FixedHeight = 80;
            cellImg.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            cellImg.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            cellImg.PaddingBottom = 5;
            table.AddCell(cellImg);

            //کد های بدنه
            //title foorushga
            string namec = Replace.returnClientTitle(myFactorModel.shopName);
            iTextSharp.text.pdf.PdfPCell shopName1 = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(namec, font));

            shopName1.PaddingBottom = 18;
            shopName1.PaddingTop = 18;
            shopName1.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.White);
            shopName1.NoWrap = false;
            shopName1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            shopName1.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            table.AddCell(shopName1);



            foreach (var item in FactorMahsoulName)
            {
                //نام محصول اضافه شود
                iTextSharp.text.pdf.PdfPCell orderName = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("نام محصول : " + item, font));
                orderName.Border = 1;
                orderName.PaddingBottom = 8;
                orderName.PaddingTop = 8;
                orderName.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.White);
                orderName.NoWrap = false;
                orderName.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                orderName.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                table.AddCell(orderName);
            }


            //هزینه ی ارسال
            iTextSharp.text.pdf.PdfPCell DeliveryPrice = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(" هزینه ارسال : " + myFactorModel.DeliveryPrice, font));
            DeliveryPrice.Border = 1;
            DeliveryPrice.PaddingBottom = 8;
            DeliveryPrice.PaddingTop = 8;
            DeliveryPrice.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.White);
            DeliveryPrice.NoWrap = false;
            DeliveryPrice.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            DeliveryPrice.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            table.AddCell(DeliveryPrice);


            //درصد تخفیف
            //درصد تخفیف
            if (myFactorModel.DarsadTakhfif != null && myFactorModel.DarsadTakhfif != "")
            {
                var TD = (100 - (float.Parse(myFactorModel.DarsadTakhfif) * 100)).ToString();
                iTextSharp.text.pdf.PdfPCell DarsadTakhfif = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(" درصد تخفیف : " + TD + "%", font));
                DarsadTakhfif.Border = 1;
                DarsadTakhfif.PaddingBottom = 8;
                DarsadTakhfif.PaddingTop = 8;
                DarsadTakhfif.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.White);
                DarsadTakhfif.NoWrap = false;
                DarsadTakhfif.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                DarsadTakhfif.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                table.AddCell(DarsadTakhfif);
            }

            //قیمت محصول
            iTextSharp.text.pdf.PdfPCell ProductPrice = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(" جمع نهایی فاکتور : " + myFactorModel.ProductPrice, font));
            ProductPrice.Border = 1;
            ProductPrice.PaddingBottom = 8;
            ProductPrice.PaddingTop = 8;
            ProductPrice.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.White);
            ProductPrice.NoWrap = false;
            ProductPrice.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            ProductPrice.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            table.AddCell(ProductPrice);

            //نام فروشگاه اضافه شود
            iTextSharp.text.pdf.PdfPCell ShopName = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("نام فروشگاه : " + myFactorModel.shopName, font));
            ShopName.Border = 1;
            ShopName.PaddingBottom = 5;
            ShopName.PaddingTop = 5;
            ShopName.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.White);
            ShopName.NoWrap = false;
            ShopName.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            ShopName.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            table.AddCell(ShopName);

            //تاریخ ثبت سفارش
            string mydate = Replace.ToShamsiForPdf(myFactorModel.SubmitDate);
            iTextSharp.text.pdf.PdfPCell date = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("تاریخ ثبت سفارش : " + mydate, font));
            date.Border = 1;
            date.PaddingBottom = 5;
            date.PaddingTop = 5;
            date.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.White);
            date.NoWrap = false;
            date.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            date.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            table.AddCell(date);



            //شماره تماس
            iTextSharp.text.pdf.PdfPCell Phone = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("شماره تماس فروشنده : " + myFactorModel.ClintNumber, font));
            Phone.Border = 1;
            Phone.PaddingBottom = 7;
            Phone.PaddingTop = 7;
            Phone.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.White);
            Phone.NoWrap = false;
            Phone.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            Phone.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            table.AddCell(Phone);



            //شماره پیگیری
            iTextSharp.text.pdf.PdfPCell kodePeygiri = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("کد پیگیری سفارش: " + myFactorModel.TrackingCode, font));
            kodePeygiri.Border = 1;
            kodePeygiri.PaddingBottom = 7;
            kodePeygiri.PaddingTop = 7;
            kodePeygiri.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.White);
            kodePeygiri.NoWrap = false;
            kodePeygiri.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            kodePeygiri.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            table.AddCell(kodePeygiri);



            //فوتر

            iTextSharp.text.pdf.PdfPCell Footer = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("آسودگی خاطر از یک خرید مطمئن", font));
            Footer.Border = 0;
            Footer.PaddingBottom = 5;
            Footer.PaddingTop = 20;
            Footer.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.White);
            Footer.NoWrap = false;
            Footer.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            Footer.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            table.AddCell(Footer);



            iTextSharp.text.pdf.PdfPCell FooterEnd = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("www.baladchee.com", font));
            FooterEnd.Border = 0;
            FooterEnd.PaddingBottom = 5;
            FooterEnd.PaddingTop = 20;
            FooterEnd.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.Color.White);
            FooterEnd.NoWrap = false;
            FooterEnd.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            FooterEnd.VerticalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            table.AddCell(FooterEnd);

            document.Add(table);

            document.Close();


            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, filename);


        }

        //safheye code eshterak ke az samte myindex miad
        public ActionResult CommonCode(string clientid)
        {
            ViewBag.clientid = clientid;
            return View();
        }




        //ijad code eshterak
        [ValidateAntiForgeryToken]
        public ActionResult SetCommonCode(string clientid, string PhoneNumber, string Name, string Address, string PostalCode, string LocationLatitude, string LocationLongitude, string city)
        {

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
                return RedirectToAction("myindex", "ShopCart", new { ClientId = clientid });
            }


            if (PhoneNumber != null && Name != null && PostalCode != null && Address != null && LocationLatitude != null && LocationLongitude != null)
            {

                if (PhoneNumber.Length < 30 && Name.Length <= 30 && PostalCode.Length <= 20 && Address.Length <= 150 && LocationLatitude.Length < 22 && LocationLongitude.Length < 22)
                {
                    ApplicationDbContext db = new ApplicationDbContext();
                    //ایجاد یک شی از مشتری
                    moshtari item = new moshtari();
                    item.Address = Address;
                    item.Name = Name;
                    item.PhoneNumber = PhoneNumber;
                    item.City = city;
                    item.PostalCode = PostalCode;
                    item.LocationLatitude = LocationLatitude;
                    item.LocationLongitude = LocationLongitude;
                    item.EshterakCode = Replace.CommonCode(PhoneNumber);

                    db.moshtari.Add(item);
                    db.SaveChanges();
                    //tab mosavie active yani inke bad az inke rafti besafhe my index tabi ke baraye ijad kode
                    //eshterak hast ro active kon
                    //hala chejuri? meghdar ro my index migire agar value dasht tu jquery safhe ok mikone in dastan ro
                    return RedirectToAction("myindex", new { ClientId = clientid, tab = "Active", CommonCode = item.EshterakCode });
                }
            }




            return RedirectToAction("index", "home");
        }


    }
}