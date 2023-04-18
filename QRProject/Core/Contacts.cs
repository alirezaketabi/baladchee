using QRProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace QRProject.Core
{
    public class Contacts
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        [ForeignKey("client")]
        public string ClienId { get; set; }
        public virtual Client client { get; set; }
        public DateTime CreateDate { get; set; }
        public string PhoneNumber { get; set; }
        public string Description { get; set; }

        public Contacts()
        {
            CreateDate = DateTime.Now;
            Id = Guid.NewGuid().ToString();
        }
    }
}