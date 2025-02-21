using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Online_Shoping.Models
{
    public class CartManagment
    {
        public int P_Id {  get; set; }
        public string UName { get; set; }
        public string Image {  get; set; }
        public string P_Name { get; set; }
        public double MPrice {  get; set; }
        public double CPrice {  get; set; }
        public Nullable <double> TotalPrice {  get; set; }
        public Nullable <int> Quantity {  get; set; }
        public string Desc {  get; set; }
        public Nullable<double> Rating {  get; set; }
        public string Address {  get; set; }
        public string Add_Type {  get; set; }
        public string City {  get; set; }
        public string State {  get; set; }
        public string PIN { get; set; }
        public Nullable<int> Offer {  get; set; }
        public string Cart_status {  get; set; }


    }
}