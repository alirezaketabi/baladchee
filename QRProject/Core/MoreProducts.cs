using QRProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QRProject.Core
{
    public class MoreProducts
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string MoreProductId { get; set; }
        public string pTitle { get; set; }
        public string pContent { get; set; }
        public string pPicAddres { get; set; }
        public string Takhfif { get; set; }
        public string NamayesheGheymat { get; set; }

        public string DasteBandi { get; set; }

        public string SefareshMahsool { get; set; }
        public DateTime? pCreateDate { get; set; }


        public string MoreProductSocialMediaLink { get; set; }
        //hich mahsuli pak nemishavad faghat az dide furushande pak mishavad
        //chera ke agar takhalofi surat gereft sanadi mujud bashad
        //agar in property por bud "notsee" bud digar baraye furushande ghabele did nist
        //yani agar delete kard mahsul ra meghdare in property ba matne "notsee" pormishavad
        public string Display { get; set; }

        public string GheymatFeli { get; set; }
        public string GheymatGhabli { get; set; }

        //anabar gardani
        //in ghesmat marbut be anbar gardani ast
        public int count { get; set; }

        [ForeignKey("client")]
        public string ClienId { get; set; }
        public virtual Client client { get; set; }
        
        public MoreProducts()
        {
            MoreProductId = Guid.NewGuid().ToString();
        }

    }

}