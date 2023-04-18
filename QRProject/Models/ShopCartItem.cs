using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QRProject.Models
{
    //برای پر کردن مقادیر داخل سشن سبد کالا
    public class ShopCartItem
    {
        public string MoreProductId { get; set; }
        public string ProductId { get; set; }
        public int Count { get; set; }

        public string DarsadTakhfif { get; set; }
        public string DeliveryPrice { get; set; }
    }

    //ویو مدل نمایش سبد کالا
    public class ShopCartViewModel
    {
        public string ClientId { get; set; }
        public string MoreProductId { get; set; }
        public string ProductId { get; set; }
        public int Count { get; set; }
        public int Price { get; set; }
        public int Sum { get; set; }
        public string ImageAddress { get; set; }
        public string Title { get; set; }

    }

    //ویو مدل نمایش فاکتور برای فروشنده کالا
    public class FactorVM
    {
     
        public string OrderId { get; set; }
        public string ClientId { get; set; }
        public string MoreProductId { get; set; }
        public string ProductId { get; set; }
        public int Count { get; set; }
        public int Price { get; set; }
        public int Sum { get; set; }
        public string ImageAddress { get; set; }
        public string Title { get; set; }
    
    }
}