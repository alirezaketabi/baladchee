using QRProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
namespace QRProject.Core
{
    public class SampleContent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        public string JobName { get; set; }
        public string BigContent { get; set; }
        public string Content { get; set; }
        public string Title { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string SaleTitle { get; set; }
        public string lng { get; set; }
        public string lat { get; set; }
        public string FirstColor { get; set; }
        public string SeccondColor { get; set; }
        public string PhotoAddress { get; set; }
        public SampleContent()
        {
            Id = Id = Guid.NewGuid().ToString();
        }
    }
}