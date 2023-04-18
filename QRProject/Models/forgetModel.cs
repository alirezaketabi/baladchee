using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QRProject.Models
{
    public class forgetModel
    {
        public string userName { get; set; }
        public string smsCode { get; set; }
        public DateTime? Expiretime { get; set; }
    }
}