using QRProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
namespace QRProject.Core
{
    public class ApplyingFor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        public string PhoneNumber { get; set; }
        public string Addres { get; set; }
        public string Name { get; set; }
        public DateTime? CreateDtae { get; set; }
        public ApplyingFor()
        {
            CreateDtae = DateTime.Now;
            Id = Guid.NewGuid().ToString();
        }
    }
}