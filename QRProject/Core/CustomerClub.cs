using QRProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace QRProject.Core
{
    public class CustomerClub
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [ForeignKey("client")]
        public string ClienId { get; set; }
        public virtual Client client { get; set; }
        public DateTime CreateDate { get; set; }
        public string PhoneNumber { get; set; }
        public string Ip { get; set; }
        public CustomerClub()
        {
            CreateDate = DateTime.Now;
            Id = Guid.NewGuid().ToString();
        }
    }
}