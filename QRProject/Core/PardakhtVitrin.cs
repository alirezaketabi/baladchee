using QRProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QRProject.Core
{
    public class PardakhtVitrin
    {   //baraye pardakht haye safheye vitrin ast va ba factor tafavot darad
        //testtime baraye kasani ast ya account hayi(chon momken ast taraf chanta safhe sabte nam kone)
        //taze va baraye avalin bar sabtenam kardehand
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        public DateTime CreateDate { get; set; }
        //shoroe ijad safhe
        public DateTime start { get; set; }
        //tarikhe payan va bayad ghab az inke be in tarikh beresad 
        //tamdid anjam shavad
        public DateTime? End { get; set; }
        //baraye mohlate yek haftei va teste safheye vitrin
        public DateTime? testTime { get; set; }

        //agar 1 bud pardakht anjam shode agar 0 bud hanuz pardakhti anjam nashode
        public int pardakht { get; set; }
        //agar FirstSale yek shod yani takhfif dg nadare ya takhfifesh nesf shode
        public int FirstSale { get; set; }

        [ForeignKey("client")]
        public string ClienId { get; set; }
        public virtual Client client { get; set; }



        public PardakhtVitrin()
        {
            Id = Guid.NewGuid().ToString();
            CreateDate = DateTime.Now;
        }
    }
}