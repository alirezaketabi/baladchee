using QRProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
namespace QRProject.Core
{
    //riz factor ajnase kharidari shode
    public class RizSefareshat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [ForeignKey("Product")]
        public string ProductId { get; set; }
        public virtual Product Product { get; set; }


        [ForeignKey("MoreProducts")]
        public string MoreProducstId { get; set; }
        public virtual MoreProducts MoreProducts { get; set; }

        [ForeignKey("Sefareshat")]
        public string SefareshatId { get; set; }
        public virtual Sefareshat Sefareshat { get; set; }

        public DateTime CreateDate { get; set; }
        public RizSefareshat()
        {
            CreateDate = DateTime.Now;
            Id = Guid.NewGuid().ToString();
        }
    }
}