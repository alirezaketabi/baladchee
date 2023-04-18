using QRProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
namespace QRProject.Core
{
    public class factor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string Price { get; set; }

        //agar sms bud tedad ro tu inja neshoon bede
        public string smsSaleCount { get; set; }
        public string Description { get; set; }
        
        
        public bool IsFinaly { get; set; }


        [ForeignKey("client")]
        public string ClienId { get; set; }
        public virtual Client client { get; set; }
        public factor()
        {
            Id = Guid.NewGuid().ToString();
            CreateDate = DateTime.Now;
         
        }

    }
}