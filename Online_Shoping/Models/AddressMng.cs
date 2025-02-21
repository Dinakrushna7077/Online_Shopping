using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Online_Shoping.Models
{
    public class AddressMng
    {
        public int Add_Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Dist { get; set; }
        public string State { get; set; }
        public string Country {  get; set; }
        public string PIN { get; set; }
        public string Add_Type {  get; set; }
        public long Mobile {  get; set; }
        public Nullable<long> A_Mobile {  get; set; }
        public double TotalPrice {  get; set; }
        public string Cart_status { get; set; }
        public int Quantity {  get; set; }
        public int? P_Id {  get; set; }
    }
}