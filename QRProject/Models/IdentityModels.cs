using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using QRProject.Core;

namespace QRProject.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public DateTime? CreateDate { get; set; }


        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public virtual ICollection<Client> Client { get; set; }
        [ForeignKey("Subjob")]
        public string SubjobId { get; set; }

        public virtual Subjobs Subjob { get; set; }
        public ApplicationUser()
        {

            Client = new HashSet<Client>();
            CreateDate = DateTime.Now;
        }


    
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public virtual DbSet<factor> factors { get; set; }
        public virtual DbSet<Subjobs> Subjobs { get; set; }
        public virtual DbSet<ApplyingFor> ApplyingFor { get; set; }
        public virtual DbSet<Client> Client { get; set; }
        public virtual DbSet<SMSBox> SMSBox { get; set; }
        public virtual DbSet<CategoryWord> CategoryWord { get; set; }
        public virtual DbSet<baladcheeProduct> baladcheeProduct { get; set; }
        public virtual DbSet<Opinion> Opinion { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<RizSefareshat> RizSefareshat { get; set; }
        public virtual DbSet<moshtari> moshtari { get; set; }
        public virtual DbSet<SampleContent> SampleContent { get; set; }
        public virtual DbSet<Sefareshat> Sefareshat { get; set; }
        public virtual DbSet<statistic> statistic { get; set; }
        public virtual DbSet<PardakhtVitrin> PardakhtVitrin { get; set; }
        public virtual DbSet<Contacts> Contacts { get; set; }
        public virtual DbSet<MoreProducts> MoreProducts { get; set; }
        public virtual DbSet<CustomerClub> CustomerClub { get; set; }
        public ApplicationDbContext()
            : base("QRDb", throwIfV1Schema: false)
        {
        }
        static ApplicationDbContext()
        {
            Database.SetInitializer<ApplicationDbContext>(new IdentityDbInit());
        }
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }


    }
    public class IdentityDbInit : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {

        protected override void Seed(ApplicationDbContext context)
        {
            PerformInitialSetup(context);
            base.Seed(context);
        }

        public void PerformInitialSetup(ApplicationDbContext cntxt)
        {
            UserManager<ApplicationUser> _usermgr = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(cntxt));
            var _rolemgr = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(cntxt));

            string rolname3 = "SuperAdmin";
            string rolname = "Administrators";
            string rolname2 = "User";

            string username = "oaliketabi25";
      
            string pass = "00140973620014097362";
            string email = "alirezaketabi70@gmail.com";


            if (!_rolemgr.RoleExists(rolname) && !_rolemgr.RoleExists(rolname2) && !_rolemgr.RoleExists(rolname3))
            {
                _rolemgr.Create(new IdentityRole(rolname));
                _rolemgr.Create(new IdentityRole(rolname2));
                _rolemgr.Create(new IdentityRole(rolname3));
            }
            ApplicationUser user = _usermgr.FindByName(username);

            if (user == null)
            {
                var result = _usermgr.Create(new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                


                }, pass);
                user = _usermgr.FindByName(username);

                if (result.Succeeded)
                {
                    if (!_usermgr.IsInRole(user.Id, rolname3))
                    {
                        _usermgr.AddToRole(user.Id, rolname3);
                    }
                }
            }


            #region AddProductsSmsAndVitrinPrice

            baladcheeProduct p1 = new baladcheeProduct();
            baladcheeProduct p2 = new baladcheeProduct();
            baladcheeProduct p3 = new baladcheeProduct();
            baladcheeProduct v1 = new baladcheeProduct();
            baladcheeProduct v2 = new baladcheeProduct();

            p1.Name = "200 عدد پیامک";
            p1.Price = "10000";
            p1.SmsCount = 200;

            p2.Name = "400 عدد پیامک";
            p2.Price = "18000";
            p2.SmsCount = 400;

            p3.Name = "600 عدد پیامک";
            p3.Price = "24000";
            p3.SmsCount = 600;

            v1.Name = "تمدید بلدچی با 25 درصد تخفیف";
            v1.Price = "172000";
            v1.SmsCount = 20;
            v1.Id = "v1";

            v2.Name = "تمدید بلدچی با 20 درصد تخفیف";
            v2.Price = "182000";
            v2.SmsCount = 20;
            v2.Id = "v2";

            cntxt.baladcheeProduct.Add(p1);
            cntxt.baladcheeProduct.Add(p2);
            cntxt.baladcheeProduct.Add(p3);
            cntxt.baladcheeProduct.Add(v1);
            cntxt.baladcheeProduct.Add(v2);
            #endregion

            #region AddJobs
            Subjobs j1 = new Subjobs();
            Subjobs j2 = new Subjobs();
            Subjobs j3 = new Subjobs();
            Subjobs j4 = new Subjobs();
            Subjobs j5 = new Subjobs();
            Subjobs j6 = new Subjobs();
            Subjobs jn = new Subjobs();
            
            Subjobs z1 = new Subjobs();
            Subjobs z2 = new Subjobs();
            Subjobs z3 = new Subjobs();

            j1.Id = "a1";
            j1.SubjobName = "پزشکی و درمانی";

            //j2.Id = "a2";
            //j2.SubjobName = "رستوران و فست فود";

            j3.Id = "a3";
            j3.SubjobName = "فروشگاه و سوپرمارکت";

            j4.Id = "a4";
            j4.SubjobName = "آرایشی و بهداشتی";

            j5.Id = "a5";
            j5.SubjobName = "آموزشی";

            j6.Id = "a6";
            j6.SubjobName = "پوشاک";

        
            z1.Id = "a8";
            z1.SubjobName = "صنایع دستی";

            z2.Id = "a9";
            z2.SubjobName = "فرهنگی و هنری";

            z3.Id = "a10";
            z3.SubjobName = "تجهیزات ساختمانی و ایمنی";

            jn.Id = "a11";
            jn.SubjobName = "سایر مشاغل";


            cntxt.Subjobs.Add(j1);
            //cntxt.Subjobs.Add(j2);
            cntxt.Subjobs.Add(j3);
            cntxt.Subjobs.Add(j4);
            cntxt.Subjobs.Add(j5);
            cntxt.Subjobs.Add(j6);          
            cntxt.Subjobs.Add(z1);
            cntxt.Subjobs.Add(z2);
            cntxt.Subjobs.Add(z3);
            cntxt.Subjobs.Add(jn);
            #endregion


            #region ClientSample
            SampleContent s1 = new SampleContent();
            SampleContent s2 = new SampleContent();
            SampleContent s3 = new SampleContent();
            SampleContent s4 = new SampleContent();
            //SampleContent s5 = new SampleContent();
            SampleContent s6 = new SampleContent();
            SampleContent s7 = new SampleContent();
            SampleContent s8 = new SampleContent();
            SampleContent s9 = new SampleContent();
            SampleContent s10 = new SampleContent();
            s1.Id = "A1";
            s1.Content = "زیبایی به رنگ یاقوت ، با بهترین و با تجربه ترین آرایشگر ها در خدمت شما زیبا رویان میباشد";
            s1.Title = "سالن زیبایی یاقوت";
            s1.Address = "تهران-پونک-خیابان میرزابابایی-جنب بانک ملی-پلاک 39";
            s1.PhoneNumber = "021-44455432";
            s1.Email = "yaghoutbeauty@yahoo.com";
            s1.SaleTitle = "خدمات ما";
            s1.FirstColor = "#f932a2";
            s1.SeccondColor = "#7d5ec6";
            s1.PhotoAddress = "~/SampleImage/arayeshi/2019_12_09_83333_1575827116._large.jpg";
            s1.JobName = "آرایشی و بهداشتی";

            s2.Id = "A2";
            s2.Content = "برگزاری آزمون های تشریحی ، مشاوره و برنامه ریزی تحصیلی و تدریس توسط اساتید برتر ، فرصتی استثنایی برای متقاضیان";
            s2.Title = "آموزشگاه علمی نو اندیشان";
            s2.Address = "تهران-پونک-خیابان میرزابابایی-جنب بانک ملی-پلاک 39";
            s2.PhoneNumber = "021-44455432";
            s2.Email = "noandishan@gmail.com";
            s2.SaleTitle = "کلاس های ما";
            s2.FirstColor = "#f932a2";
            s2.SeccondColor = "#7d5ec6";
            s2.PhotoAddress = "~/SampleImage/amuzeshi/amuzeshi.jpg";
            s2.JobName = "آموزشی";

            s3.Id = "A3";
            s3.Content = "بهترین لباس های ترک و برند با نازلترین قیمت ، بهترین در عین سادگی";
            s3.Title = "فروشگاه لباس لوتوس";
            s3.Address = "تهران-پونک-خیابان میرزابابایی-جنب بانک ملی-پلاک 39";
            s3.PhoneNumber = "021-22232676";
            s3.Email = "lotus@yahoo.com";
            s3.SaleTitle = "پرفروش ترین ها";
            s3.FirstColor = "#c0392b";
            s3.SeccondColor = "#7f8c8d";
            s3.PhotoAddress = "~/SampleImage/pooshak/pushak.jpg";
            s3.JobName = "پوشاک";

            s4.Id = "A4";
            s4.Content = "محصولات لبنی تازه و با قیمت درج شده روی محصولات است ، هدف ما رضایت مشتری میباشد:)";
            s4.Title = "سوپرمارکت پاییز";
            s4.Address = "تهران-مجیدیه-خیابان میرزابابایی-جنب بانک ملی-پلاک 39";
            s4.PhoneNumber = "021-22232676";
            s4.Email = "paizmarket@gmail.com";
            s4.SaleTitle = "تخفیف های امروز";
            s4.FirstColor = "#30cc71";
            s4.SeccondColor = "#2980b9";
            s4.PhotoAddress = "~/SampleImage/supermarket/supermarket-REUT.jpg";
            s4.JobName = "فروشگاه و سوپرمارکت";

            //s5.Id = "A5";
            //s5.Content = "پخت بدون روغن کلیه ساندویچ ها به صورت ذغالی ، تهیه شده از گوشت گرم و تازه";
            //s5.Title = "آتش برگر";
            //s5.Address = "تهران-ونک-خیابان شیخ بهایی-جنب بانک ملی-پلاک 29";
            //s5.PhoneNumber = "021-88823021";
            //s5.Email = "atashburger@gmail.com";
            //s5.SaleTitle = "! خوشمزه های ما";
            //s5.FirstColor = "#d35400";
            //s5.SeccondColor = "#f1c40d";
            //s5.PhotoAddress = "~/SampleImage/fastfood/fastfood.jpg";
            //s5.JobName = "رستوران و فست فود";

            s6.Id = "A6";
            s6.Content = "متخصص پوست، مو و زیبایی عضو رسمی انجمن پوست و مو ایران";
            s6.Title = "دکتر سلاله ارجمند";
            s6.Address = "تهران-ونک-خیابان شیخ بهایی-جنب بانک ملی-پلاک 15";
            s6.PhoneNumber = "021-88876032";
            s6.Email = "solaleharjmand@gmail.com";
            s6.SaleTitle = "آلبوم کارها";
            s6.FirstColor = "#913d88";
            s6.SeccondColor = "#e62079";
            s6.PhotoAddress = "~/SampleImage/pezeshki/pezeshki.jpg";
            s6.JobName = "پزشکی و درمانی";

            s7.Id = "A7";
            s7.Content = "توضیح مختصری  در حد دو خط راجع به کسب و کار خود بنویسید";
            s7.Title = "عنوان شغلی خود را بنویسید";
            s7.Address = "تهران-ونک-خیابان شیخ بهایی-جنب بانک ملی-پلاک 15";
            s7.PhoneNumber = "021-88823021";
            s7.Email = "testemail@gmail.com";
            s7.SaleTitle = "...محصولات ، خدمات و یا ";
            s7.FirstColor = "#1ba39c";
            s7.SeccondColor = "#22313f";
            s7.PhotoAddress = "~/SampleImage/custom/custom.png";
            s7.JobName = "سایر مشاغل";


            s8.Id = "A8";
            s8.Content = "دست ها، توانایی لمس احساس را دارند";
            s8.Title = "انگشتر و گردنبندهای رزین";
            s8.Address = "شیراز-خیابان زند-جنب بانک ملت-پلاک 43";
            s8.PhoneNumber = "071-36474308";
            s8.Email = "SabzRezin@gmail.com";
            s8.SaleTitle = "محصولات";
            s8.FirstColor = "#9c88ff";
            s8.SeccondColor = "#00a8ff";
            s8.PhotoAddress = "~/SampleImage/SanayeDasti/SanayeDasti.jpg";
            s8.JobName = "صنایع دستی";


            s9.Id = "A9";
            s9.Content = "انواع تابلو های آبرنگ و رنگ روغن";
            s9.Title = "نمایشگاه نقاشی لوتوس";
            s9.Address = "اصفهان-خیابان طالقانی-پلاک 97";
            s9.PhoneNumber = "031-32267854";
            s9.Email = "lotusgallery@gmail.com";
            s9.SaleTitle = "تابلوها";
            s9.FirstColor = "#e84118";
            s9.SeccondColor = "#2f3640";
            s9.PhotoAddress = "~/SampleImage/honari/honari.jpg";
            s9.JobName = "فرهنگی و هنری";

            s10.Id = "A10";
            s10.Content = "انواع تجهیزات ساختمانی و ایمنی";
            s10.Title = "تجهیزات ساختمانی و ایمنی دنا";
            s10.Address = "خوزستان-خیابان شهدا-پلاک 23";
            s10.PhoneNumber = "061-34467454";
            s10.Email = "mohamadTazhizat@gmail.com";
            s10.SaleTitle = "تجهیزات";
            s10.FirstColor = "#273c75";
            s10.SeccondColor = "#9c88ff";
            s10.PhotoAddress = "~/SampleImage/tajhizat/tajhizat.jpg";
            s10.JobName = "تجهیزات ساختمانی و ایمنی";


            cntxt.SampleContent.Add(s1);
            cntxt.SampleContent.Add(s2);
            cntxt.SampleContent.Add(s3);
            cntxt.SampleContent.Add(s4);
            //cntxt.SampleContent.Add(s5);
            cntxt.SampleContent.Add(s6);
            cntxt.SampleContent.Add(s7);
            cntxt.SampleContent.Add(s8);
            cntxt.SampleContent.Add(s9);
            cntxt.SampleContent.Add(s10);
            #endregion


            cntxt.SaveChanges();




        }
    }


}