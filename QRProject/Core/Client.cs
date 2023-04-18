using QRProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QRProject.Core
{
    public class Client
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string BigContent { get; set; }
        public string PhonNumber { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Instagram { get; set; }
        public string SaleTitle { get; set; }
        public string Lng { get; set; }
        public string Lat { get; set; }
        public string Telegram { get; set; }
        public string QrLocation { get; set; }
        public string PhotoAddress { get; set; }
        public string mobile { get; set; }
                                           //ownerName name sahebe kasbokar
        public string ownerName { get; set; }
         //Akse gharardad
        public string ContractPic { get; set; }
       
        //isOnline 1 bud kasbo karesh faghat online va agar 0 bud huzuri ham hast
        public int isOnline { get; set; }
        public string OurTicket { get; set; }
        public string SaleCode { get; set; }
        public DateTime CreateDate { get; set; }

        //adrese akse qr
        public string QrImageAddres { get; set; }

        public string Fisrtcolor { get; set; }
        public string Seccondcolor { get; set; }

        public string Percent { get; set; }

        public string VideoAddress { get; set; }

        public string AppAddress { get; set; }//applicatione mobilesh baraye karbarash

     
        public int? smsCount { get; set; }

        //کد سریال زرین پال
        //zarinpal code
        public string ZarinCode { get; set; }

        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }


        public string City { get; set; }

        //inha baraye gheymate ersale marsulat ast ke 
        //khode karbar bar asase sisat haye furushe khod por mikonad
        //ke do gheymat darnazar gerefte shode baraye ersalihaye darun shahrie furushande va borun shahri
        public string BorunshahriPrice { get; set; }
        public string DarunshahriPrice { get; set; }

        //agar 1 bud yani furush huzuri darad dar gheire in surat furushe huzuri nadarad hata null bud
        public string FurushHuzuri { get; set; }



        //Support baraye ersale payamake furushande dar nazar gerefte mishavad be hengame kharid
        //dar ebteda haman shomareye sabte nam dar in field gharar migirad ama emkane taghir vojod darad
        public string SupportNumber { get; set; }
        //laghv baraye laghe payamake furushande ast agar khast payamak be hengam kharid nayad 
        //agar 1 bud payamak biad dar gheire in surat nayad
        public string LaghvPayamak { get; set; }


        public virtual ICollection<Opinion> Opinion { get; set; }
        public virtual ICollection<SMSBox> SMSBox { get; set; }
        public virtual ICollection<CategoryWord> CategoryWord { get; set; }
        public virtual ICollection<Product> Product { get; set; }
        public virtual ICollection<CustomerClub> CustomerClub { get; set; }
        public virtual ICollection<statistic> statistic { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual ICollection<Contacts> Contacts { get; set; }
        public virtual ICollection<MoreProducts> MoreProducts { get; set; }
        public Client()
        {
            CreateDate= DateTime.Now;
            Opinion = new HashSet<Opinion>();
            SMSBox = new HashSet<SMSBox>();
            Product = new HashSet<Product>();
            statistic = new HashSet<statistic>();
            CustomerClub =new HashSet<CustomerClub>();
            Contacts= new HashSet<Contacts>();
            MoreProducts = new HashSet<MoreProducts>();
            CategoryWord= new HashSet<CategoryWord>();
        }
    }
}