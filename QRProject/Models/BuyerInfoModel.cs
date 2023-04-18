using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QRProject.Models
{
    public class BuyerInfoModel
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string Tozihat { get; set; }
        public string LocationLatitude { get; set; }
        public string LocationLongitude { get; set; }
        public BuyerInfoModel()
        {

        }
    }
}