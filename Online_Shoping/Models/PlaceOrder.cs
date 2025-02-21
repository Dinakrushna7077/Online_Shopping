using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace Online_Shoping.Models
{
    public class PlaceOrder
    {
        public string Addr {  get; set; }
        
        public string Product {  get; set; }
        public string Image { get; set; }
        
        public string Payment_Mode {  get; set; }
        //--------------------------------------------------------
        public int Add_Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Dist { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PIN { get; set; }
        public string Add_Type { get; set; }
        public long Mobile { get; set; }
        public double Price {  get; set; }
        public double TotalPrice { get; set; }
        public int Quantity { get; set; }
        public int P_Id { get; set; }
        public List<int> P_Ids { get; set; }
        public int Discount { get; set; }
        /*public double Saving {  get; set; }*/

    }
}