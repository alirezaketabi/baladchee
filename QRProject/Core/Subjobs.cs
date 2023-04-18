using QRProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QRProject.Core
{
    public class Subjobs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        public string SubjobName { get; set; }
        public DateTime CreatDate { get; set; }

        public virtual ICollection<ApplicationUser> ApplicationUser { get; set; }
        public Subjobs()
        {
             Id = Id = Guid.NewGuid().ToString();
            ApplicationUser = new HashSet<ApplicationUser>();
            CreatDate = DateTime.Now;
        }
    }
}