using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using QRProject.Models;
namespace QRProject.helper
{
    public class Replace
    {
        public static string littletextPreamble(string contant)
        {
            if (contant == null)
            {
                return "";
            }
            if (contant.Length <= 85)
            {
                return contant;
            }
            else
            {
                string mytext = "";

                for (int i = 0; i < 84; i++)
                {
                    mytext = mytext + contant.Substring(i, 1);
                }
                return mytext + "...";
            }

        }

        public static string littletextTitle(string contant)
        {
            if (contant == null)
            {
                return "";
            }
            if (contant.Length <= 30)
            {
                return contant;
            }
            else
            {
                string mytext = "";

                for (int i = 0; i < 29; i++)
                {
                    mytext = mytext + contant.Substring(i, 1);
                }
                return mytext + "...";
            }

        }

        static public string ToShamsi(DateTime MiladiDate)
        {
            PersianCalendar Date = new PersianCalendar();
            int year = Date.GetYear(MiladiDate);
            int month = Date.GetMonth(MiladiDate);
            int day = Date.GetDayOfMonth(MiladiDate);

            //int hour = Date.GetHour(MiladiDate);
            //int minute = Date.GetMinute(MiladiDate);

            // DateTime persiandate = new DateTime(year, month, day, hour, minute, 0);
            return year + "/" + month + "/" + day /*+ " " + hour + ":" + minute*/;
            //return persiandate.ToString("yyy/MM/dd HH:mm");

        }

        static public string ToShamsiForPdf(DateTime MiladiDate)
        {
            PersianCalendar Date = new PersianCalendar();
            int year = Date.GetYear(MiladiDate);
            int month = Date.GetMonth(MiladiDate);
            int day = Date.GetDayOfMonth(MiladiDate);

            //int hour = Date.GetHour(MiladiDate);
            //int minute = Date.GetMinute(MiladiDate);

            // DateTime persiandate = new DateTime(year, month, day, hour, minute, 0);
            return day + "/" + month + "/" + year  /*+ " " + hour + ":" + minute*/;
            //return persiandate.ToString("yyy/MM/dd HH:mm");

        }
        static public string withOutZiro(string phonNumber)
        {
            string phone = "";
            for (int i = 1; i < phonNumber.Length; i++)
            {
                phone = phone + phonNumber.Substring(i, 1);
            }
            return phone;
        }

        //in baraye in ast ke befahmim aya mahsuli ke karbar paak mikonad
        //sefaresh darad ya kheyr,dar vaghe agar mahsuli sefaresh darad be oo yadavari konim ke
        //aya motmaen ast ke paak shavad,zira ba paak kardane mahsul tamamie sefareshat ham paak mishavad

        //1-aval baraye moreProduct
        static public bool MoreProductHasOrder(string moreProductId)
        {
            ApplicationDbContext db = new ApplicationDbContext();

            //bia bebin in sefaresh factoresh true shode(furushe vaghei dashte)
            var sefareshItem = db.RizSefareshat.Where(r => r.MoreProducstId == moreProductId && r.Sefareshat.isFinaly == true).FirstOrDefault();


            if (sefareshItem != null)
            {
                return true;
            }
            else
                return false;
        }

        //1-dovom baraye Product
        static public bool ProductHasOrder(string ProductId)
        {
            ApplicationDbContext db = new ApplicationDbContext();

            //bia bebin in sefaresh factoresh true shode(furushe vaghei dashte)
            var sefareshItem = db.RizSefareshat.Where(r => r.ProductId == ProductId && r.Sefareshat.isFinaly == true).FirstOrDefault();


            if (sefareshItem != null)
            {
                return true;
            }
            else
                return false;
        }

        //تولید کد اشتراک
        static public string CommonCode(string PhonNumber)
        {

            int counter = 0;
            ///................
            StartAgain:

            string StringValue = "";
            string WordFirst = "";
            Random x = new Random();
            StringValue = x.Next(1, 10).ToString();
            switch (StringValue)
            {
                case "1":
                    WordFirst = "z";
                    break;
                case "2":
                    WordFirst = "a";
                    break;
                case "3":
                    WordFirst = "b";
                    break;
                case "4":
                    WordFirst = "c";
                    break;
                case "5":
                    WordFirst = "d";
                    break;
                case "6":
                    WordFirst = "e";
                    break;
                case "7":
                    WordFirst = "f";
                    break;
                case "8":
                    WordFirst = "g";
                    break;
                case "9":
                    WordFirst = "h";
                    break;
                case "10":
                    WordFirst = "k";
                    break;
            }

            //partdovom
            string StringValue2 = "";
            string WordSecond = "";
            Random g = new Random();
            StringValue2 = g.Next(1, 10).ToString();
            switch (StringValue2)
            {
                case "1":
                    WordSecond = "p";
                    break;
                case "2":
                    WordSecond = "o";
                    break;
                case "3":
                    WordSecond = "i";
                    break;
                case "4":
                    WordSecond = "u";
                    break;
                case "5":
                    WordSecond = "y";
                    break;
                case "6":
                    WordSecond = "t";
                    break;
                case "7":
                    WordSecond = "r";
                    break;
                case "8":
                    WordSecond = "l";
                    break;
                case "9":
                    WordSecond = "x";
                    break;
                case "10":
                    WordSecond = "n";
                    break;
            }

            var CommonCode = WordFirst + WordSecond + PhonNumber;

            //bia baz tu data base tu table moshtari ha begard bebin hamchin codi dare kasi
            ApplicationDbContext db = new ApplicationDbContext();
            var item = db.moshtari.Where(r => r.EshterakCode == CommonCode).FirstOrDefault();
            //agar true shod yani in code eshterak ro dare
            if (item != null)
            {
                counter++;
                //تا 4 بار سعی کن بسازی براش اگر نتونستی به دیتابیس و رم فشار نیار و همون جی یو آیدی بده بهش 
                if (counter < 5)
                {
                    //دوباره برو بالا مراحل رو تکرار کن
                    goto StartAgain;
                }
                //dg dare aziat mikone va tabi nist behesh guid bede halesho bebare:)
                else
                {
                    return Guid.NewGuid().ToString();
                }


            }
            //yani in code eshterak ro nadare va hamin ro ersal kon
            //ghalebesh in sheklie gp09059093515
            else
            {
                return CommonCode;
            }

        }
        static public string CreateTarackingCode()
        {
            string word = "";
            string word2 = "";
            string StringValue = "";
            string StringValue2 = "";
            string NumberCode = "";
            Random y = new Random();
            NumberCode = y.Next(100, 12000).ToString();

            Random x = new Random();
            StringValue = x.Next(1, 10).ToString();
            switch (StringValue)
            {
                case "1":
                    word = "z";
                    break;
                case "2":
                    word = "a";
                    break;
                case "3":
                    word = "b";
                    break;
                case "4":
                    word = "c";
                    break;
                case "5":
                    word = "d";
                    break;
                case "6":
                    word = "e";
                    break;
                case "7":
                    word = "f";
                    break;
                case "8":
                    word = "g";
                    break;
                case "9":
                    word = "h";
                    break;
                case "10":
                    word = "k";
                    break;
            }

            Random g = new Random();
            StringValue2 = g.Next(1, 10).ToString();
            switch (StringValue2)
            {
                case "1":
                    word2 = "p";
                    break;
                case "2":
                    word2 = "o";
                    break;
                case "3":
                    word2 = "i";
                    break;
                case "4":
                    word2 = "u";
                    break;
                case "5":
                    word2 = "y";
                    break;
                case "6":
                    word2 = "t";
                    break;
                case "7":
                    word2 = "r";
                    break;
                case "8":
                    word2 = "l";
                    break;
                case "9":
                    word2 = "x";
                    break;
                case "10":
                    word2 = "n";
                    break;
            }
            Random Part2Random = new Random();
            var Part2 = Part2Random.Next(11, 99);
            return word + NumberCode + word2 + Part2;
        }
        static public string retuenClientNumber(string cId)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var ClientPhoneNumber = db.Client.Where(r => r.Id == cId).FirstOrDefault().PhonNumber;
            return ClientPhoneNumber;
        }



        //baraye inke ta zarin code nadarad dar show va more(hata khode dokmeye more)
        //ejazeye darje mahsul nadashte bashad

        static public bool hasZarinCode(string ClientId)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var MyClient = db.Client.Where(r => r.Id == ClientId).FirstOrDefault();
            if (MyClient.ZarinCode == "" || MyClient.ZarinCode == null)
            {
                return false;
            }
            //dar ghere in surat az if ubur karde yani zarin code darad
            return true;

        }


        static public string returnClientTitle(string cId)
        {
            ApplicationDbContext db = new ApplicationDbContext();

            //baraye zamanie ke url ro moghe eshterak khali mikone ye karbar sheytanat kone
            if (cId==null)
            {
                var ff = db.Client.OrderByDescending(e => e.CreateDate).ToList().FirstOrDefault().Title;
                return ff;
            }
            var ClientTitle = db.Client.Where(r => r.Id == cId).FirstOrDefault().Title;
            return ClientTitle;
        }


        static public string returnProductName(string pId)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var ProductTitle = db.Product.Where(r => r.pId == pId).FirstOrDefault().pTitle;
            return ProductTitle;
        }

        static public string returnProductPicAddress(string pId)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var ProducAddress = db.Product.Where(r => r.pId == pId).FirstOrDefault().pPicAddres;
            return ProducAddress;
        }

        static public string returnMoreProductPicAddress(string pId)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var moreProducAddress = db.MoreProducts.Where(r => r.MoreProductId == pId).FirstOrDefault().pPicAddres;
            return moreProducAddress;
        }
        static public string returnMoreProductName(string pId)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var MoreProductName = db.MoreProducts.Where(r => r.MoreProductId == pId).FirstOrDefault().pTitle;
            return MoreProductName;
        }



        static public string returnProductPrice(string pId)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var ProductPrice = db.Product.Where(r => r.pId == pId).FirstOrDefault().GheymatFeli;
            return ProductPrice;
        }


        static public string returnMoreProductPrice(string pId)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var MoreProductPrice = db.MoreProducts.Where(r => r.MoreProductId == pId).FirstOrDefault().GheymatFeli;
            return MoreProductPrice;
        }

    }
}