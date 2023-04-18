using QRProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QRProject.Core
{
    public class Opinion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        [ForeignKey("client")]
        public string ClienId { get; set; }
        public virtual Client client { get; set; }
        public DateTime CreateDate { get; set; }
        public string Content { get; set; }
        public string ConnectContent { get; set; }
        //ip haye kasani ke nazar vared mikonand
        public string Ip { get; set; }
        //ta 4 bar mitavand ba in ip nazar vared konad
        public int Count { get; set; }
        public Opinion()
        {
            CreateDate = DateTime.Now;
            Id = Guid.NewGuid().ToString();
        }
    }
}