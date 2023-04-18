using QRProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
namespace QRProject.Core
{

    //in factore asli ast
    //etelaate daghighe har factor dar table "ZirFactor" ast
    public class Sefareshat //فاکتور مشتریان
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        public string Tozihat { get; set; }
        //public string productTitle { get; set; }
        public DateTime CreateDate { get; set; }
        public string TakhfifCode { get; set; }
        public string Tracking_Code { get; set; }
        public string FactorAddres { get; set; }
        //baraye seen kardan sefareshat ast
        public bool isSeen { get; set; }

        //tedade sefareshe in mahsool
        //public int Count { get; set; }

        // کد پیگیری از طرف بانک در صورت موفقیت آمیز بودن پرداخت 
        public string RefId { get; set; }
        //baraye nahai shodane khrid hast
        public bool isFinaly { get; set; }

        //mablaghe kole factore kharide moshtari
        public int Sum { get; set; }

        //hazineye ersal
        public string DeliveryPrice { get; set; }


        [ForeignKey("Client")]
        public string clientId { get; set; }
        public virtual Client Client { get; set; }


        [ForeignKey("moshtari")]
        public string moshtariId { get; set; }
        public virtual moshtari moshtari { get; set; }

        public string DarsadTakhfif { get; set; }

        public Sefareshat()
        {
            Id = Guid.NewGuid().ToString();
            CreateDate = DateTime.Now;
        }

    }
}