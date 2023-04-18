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
    //etelaate daghigh dar table "ZirFactor" ast
    public class moshtari
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string LocationLatitude { get; set; }
        public string LocationLongitude { get; set; }
        public string City { get; set; }

        //کد اشتراک
        public string EshterakCode { get; set; }
        //برای ارسال کد اشتراک، کلا هم دوبار بیشتر ارسال نکنه چون ارت کد رو بهشون میده
        public int EshterakCodeCount { get; set; }

        public moshtari()
        {
            Id = Guid.NewGuid().ToString();
            CreateDate = DateTime.Now;
        }
    }
}