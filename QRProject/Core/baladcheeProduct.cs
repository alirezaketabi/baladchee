using QRProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace QRProject.Core
{
    public class baladcheeProduct
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }
        public DateTime? CreateDate { get; set; }

        //agar sms bud tedad ra moshakhas kon
        public int SmsCount { get; set; }

        public baladcheeProduct()
        {
            Id = Guid.NewGuid().ToString();
            CreateDate = DateTime.Now;
        }
    }
}