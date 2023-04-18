using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QRProject.Models
{
    public class registerModel
    {

        public string city { get; set; }
        public string PhoneNumber { get; set; }
        public string urlName { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public string LocationLatitude { get; set; }
        public string LocationLongitude { get; set; }
        public string smsCode { get; set; }
        public string JobName { get; set; }
        public string mobile { get; set; }
        public string FurushHuzuri { get; set; }



        //inha baraye gheymate ersale marsulat ast ke 
        //khode karbar bar asase sisat haye furushe khod por mikonad
        //ke do gheymat darnazar gerefte shode baraye ersalihaye darun shahrie furushande va borun shahri
        public string BorunshahriPrice { get; set; }
        public string DarunshahriPrice { get; set; }

        //product sample 1 
        public string pTitle { get; set; }
        public string pContent { get; set; }
        public string pPicAddres { get; set; }
        public string Takhfif { get; set; }

        //product sample 2 
        public string pTitle2 { get; set; }
        public string pContent2 { get; set; }
        public string pPicAddres2 { get; set; }
        public string Takhfif2 { get; set; }

        //product sample 3
        public string pTitle3 { get; set; }
        public string pContent3 { get; set; }
        public string pPicAddres3 { get; set; }
        public string Takhfif3 { get; set; }

        //expire time baraye sesion hast ke pak konimesh
        //ssmCount ham baraye tedade sms hast ke 2ta darnazar gereftam felan
        //vase ersale code faal sazi
        public DateTime Expiretime { get; set; }
        public int smsCount { get; set; }

        //Support baraye ersale payamake furushande dar nazar gerefte mishavad be hengame kharid
        //dar ebteda haman shomareye sabte nam dar in field gharar migirad ama emkane taghir vojod darad
        public string SupportNumber { get; set; }
        //laghv baraye laghe payamake furushande ast agar khast payamak be hengam kharid nayad 
        //agar 1 bud payamak biad dar gheire in surat nayad
        public string LaghvPayamak { get; set; }
    }
}