using Microsoft.Ajax.Utilities;
using Online_Shoping.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Data.Entity;
using System.Web.UI.WebControls;
using System.Xml.Schema;
using System.Security.Policy;


namespace Online_Shoping.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        OnlineShopping db = new OnlineShopping();
        
        //[Authorize]
        public ActionResult UserHome( string SearchBy)
        {
            IEnumerable<category> cat;
            if (SearchBy != null)
            {
                cat = db.categories.Where(model => model.name.StartsWith(SearchBy)).ToList();
                return View(cat);
            }
            else
            {
                cat=db.categories.ToList();
                cat=cat.OrderBy(r => Guid.NewGuid()).ToList();
                return View(cat);
            }
        }
        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("UserLogin","User");
        }
        public ActionResult UserLogin()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UserLogin(user_mst user, string ReturnUrl, string SearchBy)
        {
            if (SearchBy != null)
            {
                return RedirectToAction("ViewProduct", new { SearchBy });
            }
            if (user.email == null || user.password == null)
            {
                //if gmail or password field null
                TempData["Msg"] = "Please Enter g-mail and password";
                return View();
            }
            else
            {
                //if email id and password are provided then
                var data = db.user_mst.Where(m => m.email == user.email).FirstOrDefault();
                if (data != null)
                {
                    //if gmail id is valid !
                    string psw = data.password;
                    if (psw == user.password)
                    {
                        //if password matched...
                        FormsAuthentication.SetAuthCookie(data.email, false);
                        Session["Admin_Id"] = data.u_id;
                        Session["UserName"] = data.name;
                        if (ReturnUrl != null)
                            return Redirect(ReturnUrl);
                        else
                        {                            
                            return RedirectToAction("UserHome", "User");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("password", "* Invalid Password");
                        return View();
                    }
                }
                else
                {
                    //if gmail id invalid
                    ModelState.AddModelError("email", "* Invalid g-mail Id");
                    return View();
                }
            }
        }
        //-------------------------------------------------- User Sign Up Page ------------------------------------------------------
        [Authorize]
        public ActionResult SignUp(string SearchBy)
        {
            if (SearchBy != null)
            {
                return RedirectToAction("ViewProduct", new { SearchBy });
            }
            return View();
        }
        [HttpPost]
        public ActionResult SignUp(user_mst user)
        {
            if (ModelState.IsValid==true)
            {
                if(user.password==user.ConfirmPass)
                {
                    db.user_mst.Add(user);
                    db.SaveChanges();
                    return RedirectToAction("UserLogin");
                }
                else
                {
                    ModelState.AddModelError("ConfirmPass", "Password Missmatched...");
                    return View();
                }
            }
            else
            {
                return View();
            }
        }
        /*--------------------------------------Upload Products-------------------------------------------*/
        [Authorize]
        public ActionResult ViewProduct(int? cat_id, string SearchBy)
        {
            int userId = Convert.ToInt32(Session["Admin_Id"]);
            var cartItems = db.my_cart.Where(cart => cart.u_id == userId && cart.cart_statu == "active").Select(cart => cart.p_id).ToList();
            ViewBag.CartItems = cartItems;
            List<product_mst> products;
            if (SearchBy!=null)
            {
                products = db.product_mst
                    .Where(model => model.p_name.StartsWith(SearchBy) && model.p_status == "public")
                    .ToList();
            }
            else if (cat_id.HasValue)
            {
                products = db.product_mst.Where(model => model.c_id == cat_id && model.p_status == "public").ToList();
            }
            else
            {
                products = db.product_mst.Where(model => model.p_status == "public").ToList();
            }
            // Calculate discounted prices
            foreach (var product in products)
            {
                product.CostPrice = product.price - (product.price * product.discount / 100);
            }
            return View(products);
        }
        [Authorize]
        public ActionResult AddToCart(int pid,my_cart cart,int? cat_id,string returnUrl)
        {
            cart.u_id = Convert.ToInt32(Session["Admin_Id"]);
            var existingCartItem = db.my_cart.Where(c => c.p_id == pid && c.u_id == cart.u_id && c.cart_statu == "active").FirstOrDefault(); 
            if (existingCartItem != null) 
            { 
                existingCartItem.quantity += 1; 
            } 
            else 
            { 
                double price=db.product_mst.Where(p => p.p_id==pid).Select(p => p.price).FirstOrDefault();
                int discount=db.product_mst.Where(p => p.p_id==pid).Select(p => p.discount).FirstOrDefault();
                double costPrice = CalculateCostprice(price, discount);
                cart.u_id = cart.u_id; 
                cart.cart_statu = "active"; 
                cart.p_id = pid; 
                cart.quantity = 1;
                cart.added_date= DateTime.Now;
                cart.total_price = costPrice*cart.quantity;
                db.my_cart.Add(cart); 
            }
            db.SaveChanges(); 
            Session["AddedToCart"] = pid;
            if (returnUrl != null)
            {
                returnUrl = returnUrl + "?pid="+pid;
                return Redirect(returnUrl);
            }
            return RedirectToAction("ViewProduct", "User", new { cat_id });
        }
        //---------------------------------------------MyCart ActionResult --------------------------------------------
        [Authorize]
        public ActionResult MyCart(string SearchBy)
        {
            if (SearchBy != null)
            {
                
                return RedirectToAction("ViewProduct",new {SearchBy});
            }
            int UserId=Convert.ToInt32(Session["Admin_Id"]);
            double TotalPrice = 0;
            int count = 0;
            //CartManagment cartm = new CartManagment();
            IEnumerable<CartManagment> mycart;
            mycart = db.MyCartViews.Where(user => user.u_id==UserId && user.cart_statu=="active").Select(x => new CartManagment
            {
                P_Id = x.p_id,
                P_Name = x.p_name,
                Image = x.image,
                Desc=x.p_desc,
                Rating = x.rating,
                MPrice=x.price,
                Quantity=x.quantity,
                TotalPrice=x.total_price,
                Offer=x.discount
            }).ToList();
            var address = db.User_Address.Where(user => user.U_id == UserId).Select(a => new CartManagment
            {
                Address=a.Address_line1,
                Add_Type=a.Add_Type,
                City=a.City,
                State=a.State,
                UName=a.Name,
                PIN=a.PIN
            }).ToList();
            //ViewBag.Items = cartItems;
            mycart = mycart.Select(c =>
            {
                var a = address.FirstOrDefault();
                if (a != null)
                {
                    c.Address = a.Address;
                    c.Add_Type = a.Add_Type;
                    c.City = a.City;
                    c.State = a.State;
                    c.PIN = a.PIN;
                    c.UName = a.UName;
                }
                return c;
            });            
            foreach (var cart in mycart)
            {
                cart.CPrice = CalculateCostprice(cart.MPrice, cart.Offer.Value);
                TotalPrice += cart.CPrice*cart.Quantity.Value;
                if(cart.Offer.Value > 0)
                {
                    count++;
                }
            }
            TempData["TotalMarketPrice"] = mycart.Sum(mp => mp.MPrice*mp.Quantity);
            TempData["TotalPrice"] = TotalPrice;
            TempData["NoOfOffers"] = count;
            return View(mycart);
        }
        //------------------Calculate The Cost Price After applying The Discount---------------Function
        public double CalculateCostprice(double MPrice, int discount)
        {
            double CostPrice = MPrice - (MPrice * discount / 100);
            return CostPrice;
        }
        public ActionResult Increment(int pid)
        {
            int UserId = Convert.ToInt32(Session["Admin_Id"]);
            var cartItem = db.my_cart.Where(u => u.u_id == UserId&& u.p_id==pid).FirstOrDefault();
            if(cartItem != null && cartItem.quantity >= 1)
            {
                cartItem.quantity++; 
                db.SaveChanges();
            }
            return RedirectToAction("MyCart", "User");
        }
        public ActionResult Decrement(int pid)
        {
            int UserId = Convert.ToInt32(Session["Admin_Id"]);
            var cartItem = db.my_cart.Where(u => u.u_id == UserId && u.p_id == pid&& u.cart_statu=="active").FirstOrDefault();
            if(cartItem != null && cartItem.quantity >= 2)
            {
                cartItem.quantity -- ;
                db.SaveChanges();
            }
            return RedirectToAction("MyCart", "User");
        }        
        //-----------------------------------------------Remove Products from my cart---------------------------- Date: 25th December 2024
        public ActionResult RemoveProduct(int pid)
        {
            var raw = db.my_cart.Where(p => p.p_id == pid).FirstOrDefault();
            db.Entry(raw).State =EntityState.Deleted;
            db.SaveChanges();
            return RedirectToAction("MyCart", "User");
        }
        /*-----------------------------------------------Change Password After Login --------------------------------------dt:25th December 2024*/
        [Authorize]
        public ActionResult ChangePassword()
        {
            
            TempData["Captcha"]= GenerateCaptcha();
            TempData.Keep();
            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(user_mst user)
        {
            int UserId = Convert.ToInt32(Session["Admin_Id"]);
            int OriginalCap = Convert.ToInt32(TempData["Captcha"]);
            if (user.ConfirmPass != null && user.password != null && user.NewPass != null && user.CaptchaCode != 0)
            {
                var row=db.user_mst.Where(u => u.u_id== UserId).FirstOrDefault();
                if (row != null)
                {
                    string oldPass=row.password;
                    if(oldPass ==user.password)
                    {
                        if(user.NewPass==user.ConfirmPass)
                        {
                            if(user.CaptchaCode==OriginalCap)
                            {
                                row.password = user.ConfirmPass;
                                int x = db.SaveChanges();
                                if (x > 0)
                                {
                                    TempData["Msg"] = "<script>alert('Password Changed...')</script>";
                                }
                            }
                            else
                            {
                                TempData["Msg"] = "<script>alert('Invalid Captcha...')</script>";
                            }
                        }
                        else
                        {
                            TempData["Msg"] = "<script>alert('Password Mismatched...')</script>";
                        }
                    }
                    else
                    {
                        TempData["Msg"] = "<script>alert('Old Password Password Is Wrong ...')</script>";
                    }
                }
                else
                {
                    TempData["Msg"] = "<script>alert('Something Went Wrong Please Try again Later ...')</script>";
                }
            }
            else
            {
                TempData["Msg"] = "<script>alert('Fields are can\'t be blank ! ...')</script>";
            }
            TempData["Captcha"] = GenerateCaptcha();
            TempData.Keep();
            return View();
        }
        //-------------------------------- Function For Generate Otp -------------------------------dt-25th December-2024
         public int GenerateCaptcha()
         {
            var x = new Random();
            int cap = x.Next(1000, 9999);
            return cap;
         }
        //-------------------------------------------------Address Page Management---------------------------------Dt: 26th December 2024 

        [Authorize]
        public ActionResult DeliveryAddress(int? pid, string SearchBy)
        {
            if (SearchBy != null)
            {
                return RedirectToAction("ViewProduct", new { SearchBy });
            }
            int UserId = Convert.ToInt32(Session["Admin_Id"]);
            var address = db.User_Address.Where(a => a.U_id == UserId).FirstOrDefault();
            if (address == null)
                return RedirectToAction("AddNewAddress", "User",new {pid});
            IEnumerable<AddressMng> data;
            if(pid.HasValue)
            {
                var product = db.product_mst.Where(p => p.p_id == pid.Value).FirstOrDefault();
                var cartItem=db.my_cart.Where(c=>c.p_id==pid.Value).FirstOrDefault();
                if(cartItem==null)
                {
                    my_cart cart = new my_cart()
                    {
                        u_id = UserId,
                        p_id = pid.Value,
                        quantity = 1,
                        total_price = CalculateCP(product.price, product.discount, 1),
                        added_date = DateTime.Now,
                        cart_statu = "active"
                    };
                    db.my_cart.Add(cart);
                    db.SaveChanges();
                }
            }
            data = db.Address_View.Where(u => u.u_id == UserId && u.cart_statu == "active").Select(x => new AddressMng
            {
                  Add_Id = x.Add_id,
                  Address = x.Address_line1,
                  Mobile = x.Mobile,
                  A_Mobile = x.Alt_Mobile,
                  Name = x.Name,
                  PIN = x.PIN,
                  State = x.State,
                  Dist = x.Dist,
                  Country = x.Country,
                  Add_Type = x.Add_Type,
                  Cart_status = x.cart_statu,
                  TotalPrice = x.TotalPrice.Value
            }).ToList();             
            //TempData["BuyId"]=pid;
            return View(data);
        }
        [HttpPost]
        public ActionResult DeliveryAddress(int add_id,int? pid)
        {            
            return RedirectToAction("PlaceOrder", "User", new {add_id,pid});
        }
        [Authorize]
        public ActionResult AddNewAddress(int? pid, string SearchBy)
        {
            if (SearchBy != null)
            {
                return RedirectToAction("ViewProduct", new { SearchBy });
            }
            List<string>State = new List<string> { "Andhra Pradesh", "Arunachal Pradesh", "Assam", "Bihar", "Chhattisgarh", "Goa", "Gujarat", "Haryana", "Himachal Pradesh", "Jharkhand", "Karnataka", "Kerala", "Madhya Pradesh", "Maharashtra", "Manipur", "Meghalaya", "Mizoram", "Nagaland", "Odisha", "Punjab", "Rajasthan", "Sikkim", "Tamil Nadu", "Telangana", "Tripura", "Uttar Pradesh", "Uttarakhand", "West Bengal", "Andaman and Nicobar Islands", "Chandigarh","Lakshadweep", "Delhi", "Puducherry"};
            ViewBag.StateList = new SelectList(State);
            TempData["pid"] = pid;
            return View();
        }
        [HttpPost]
        public ActionResult AddNewAddress(User_Address address)
        {
            if(ModelState.IsValid)
            {
                int UserId = Convert.ToInt32(Session["Admin_Id"]);
                address.U_id = UserId;
                db.User_Address.Add(address);
                int x=db.SaveChanges();
                if(x>0)
                {
                    return RedirectToAction("DeliveryAddress", "User",new { pid = TempData["pid"] });
                }
                else
                {
                    return View();
                }                
            }
            else
            {
                return View();
            }
        }
        /*--------------------------------Remove Address ----------------------------------------------------*/
        /* public ActionResult DeleteAddress(int add_id)
         {
             int UserId = Convert.ToInt32(Session["Admin_Id"]);
             var row=db.User_Address.Where(u=> u.U_id == UserId && u.Add_id==add_id).FirstOrDefault();
             db.Entry(row).State=EntityState.Deleted;
             db.SaveChanges();
             return RedirectToAction("DeliveryAddress", "User");
         }*/
        [Authorize]
        public ActionResult EditAddress(int add_id, string SearchBy)
        {
            int UserId = Convert.ToInt32(Session["Admin_Id"]);
            if (SearchBy != null)
            {
                return RedirectToAction("ViewProduct", new { SearchBy });
            }
            List<string> State = new List<string> { "Andhra Pradesh", "Arunachal Pradesh", "Assam", "Bihar", "Chhattisgarh", "Goa", "Gujarat", "Haryana", "Himachal Pradesh", "Jharkhand", "Karnataka", "Kerala", "Madhya Pradesh", "Maharashtra", "Manipur", "Meghalaya", "Mizoram", "Nagaland", "Odisha", "Punjab", "Rajasthan", "Sikkim", "Tamil Nadu", "Telangana", "Tripura", "Uttar Pradesh", "Uttarakhand", "West Bengal", "Andaman and Nicobar Islands", "Chandigarh", "Lakshadweep", "Delhi", "Puducherry" };
            ViewBag.StateList = new SelectList(State);
            var address = db.User_Address.Where(model => model.U_id == UserId && model.Add_id == add_id).FirstOrDefault();
            List<int> U_Id = new List<int> { UserId };
            ViewBag.User = new SelectList(U_Id);
            if (address!=null)
            {
                return View(address);
            }
            else
            {
                TempData["ErrMsg"] = "<script>alert('Address Unavailable...')</script>";
                return RedirectToAction("DeliveryAddress", "User");
            }
        }
        [HttpPost]
        public ActionResult EditAddress(User_Address address)
        {
            address.U_id= Convert.ToInt32(Session["Admin_Id"]);
            db.Entry(address).State=EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("DeliveryAddress", "User");
        }
        [Authorize]
        public ActionResult PlaceOrder(int? add_id, int? pid, string SearchBy)
        {

            int UserId = Convert.ToInt32(Session["Admin_Id"]);
            if (SearchBy != null)
            {
                return RedirectToAction("ViewProduct", new { SearchBy });
            }
            IEnumerable<PlaceOrder> data;
            TempData["pid"] = 0;
            if (pid.HasValue)
            {
                data = db.product_mst.Where(p => p.p_id == pid).Select(p => new PlaceOrder
                {
                    Product = p.p_name,
                    Image = p.image,
                    Price = p.price,
                    Quantity = 1,
                    P_Id = pid.Value,
                    Discount = p.discount
                }).ToList();
                //-------------------calculating cost price
                var tot = data.Select(t => new PlaceOrder
                {
                    TotalPrice = CalculateCostprice(t.Price, t.Discount),
                }).ToList();
                data = data.Select(t =>
                {
                    var tt = tot.FirstOrDefault();
                    if (tt != null)
                    {
                        t.TotalPrice = tt.TotalPrice;
                    }
                    return t;
                });
                TempData["pid"]= pid;
            }
            else
            {
                data = db.MyCartViews.Where(p => p.u_id == UserId && p.cart_statu == "active").Select(p => new PlaceOrder
                {
                    Product = p.p_name,
                    Image = p.image,
                    Discount = p.discount.Value,
                    Price = p.price,
                    TotalPrice = p.total_price.Value,
                    Quantity = p.quantity.Value,
                    P_Id = p.p_id,
                    //P_Ids = new List<int> { pid.Value },
                }).ToList();
            }
            var address = db.Address_View.Where(u => u.u_id == UserId && u.Add_id == add_id).Select(x => new PlaceOrder
            {
                Add_Id = x.Add_id,
                Addr = x.Address_line1,
                Mobile = x.Mobile,
                Name = x.Name,
                PIN = x.PIN,
                State = x.State,
                Dist = x.Dist,
                Country = x.Country,
                Add_Type = x.Add_Type,
                //TotalPrice = x.TotalPrice.Value
            }).ToList();
            double total_cost = 0;
            double total_mp = 0;
            foreach(var p in data)
            {
                total_cost=total_cost+p.TotalPrice;
                total_mp = total_mp+p.Price;
            }
            TempData["TotalCost"]=total_cost;
            TempData.Keep("TotalCost");
            TempData["Saving"]=total_mp-total_cost;
            data = data.Select(a =>
            {
                var add = address.FirstOrDefault();
                if(add != null)
                {
                    a.Addr = add.Addr;
                    a.Add_Type = add.Add_Type;
                    a.City = add.City;
                    a.Country = add.Country;
                    a.State = add.State;
                    a.PIN = add.PIN;
                    a.Mobile = add.Mobile;
                    a.Dist = add.Dist;
                    a.Name = add.Name;
                    a.Add_Id = add.Add_Id;
                    a.Payment_Mode = "Cash On Delivery";
                }
                return a;
            }).ToList();            
            return View(data);
        }
        public double CalculateCP(double MPrice, int discount,int quantity)
        {
            double CostPrice = MPrice - (MPrice * discount / 100);
            return CostPrice*quantity;
        }
        [Authorize]
        public ActionResult ConfirmPlaceOrder(int  Add_Id, string SearchBy)
        {            
            int UserId = Convert.ToInt32(Session["Admin_Id"]);
            if (SearchBy != null)
            {
                return RedirectToAction("ViewProduct", new { SearchBy });
            }
            PlaceOrder po=new PlaceOrder();
            Order_mst order = new Order_mst()
            {
                u_id = UserId,
                Add_id = Add_Id,
                order_dt = DateTime.Now,
                total_amount = Convert.ToDouble(TempData["TotalCost"]),  //Error 1              
            };
            db.Order_mst.Add(order);
            db.SaveChanges();
            var od=db.Order_mst.Where(o => o.u_id == UserId).OrderByDescending(i => i.o_id).FirstOrDefault();
            int pid = Convert.ToInt32(TempData["pid"]);
            if (pid!=0)
            {
                var product = db.product_mst.Where(p => p.p_id == pid && p.p_status == "Public").FirstOrDefault();
                Order_Details order_Details = new Order_Details()
                {
                    o_id = od.o_id,
                    p_id = pid,
                    quantity=1,                    
                    price = CalculateCP(product.price,product.discount,1),
                    mrp = product.price,
                    discount = product.discount,
                    order_status = "processing",
                    action_dt= DateTime.Now
                };
                db.Order_Details.Add(order_Details);
                db.SaveChanges();
                RemoveProduct(pid);
            }
            else
            {
                var cartItems = db.MyCartViews.Where(c => c.u_id == UserId && c.cart_statu == "active").ToList();
                Order_Details order_Details = new Order_Details();
                foreach (var cart in cartItems)
                {
                    order_Details.o_id = od.o_id;
                    order_Details.p_id = cart.p_id;
                    order_Details.quantity = cart.quantity;
                    order_Details.price = CalculateCP(cart.price, cart.discount.Value,cart.quantity.Value);
                    order_Details.mrp = cart.price;
                    order_Details.discount = cart.discount.Value;
                    order_Details.order_status = "processing";
                    db.Order_Details.Add(order_Details);
                    db.SaveChanges();
                    RemoveProduct(cart.p_id);
                }
            }
            return RedirectToAction("OrderSuccess","User");
        }
        [Authorize]
        public ActionResult OrderSuccess()
        {
            return View();
        }
        [Authorize]
        public ActionResult Orders()
        {
            int UserId = Convert.ToInt32(Session["Admin_Id"]);
            IEnumerable<Order_View> orders = db.Order_View.Where(o => o.u_id == UserId).OrderByDescending(od => od.od_id).ToList();            
            return View(orders);
        }
        [Authorize]
        public ActionResult ProductDetails(int pid, string SearchBy)
        {
            int UserId = Convert.ToInt32(Session["Admin_Id"]);
            if (SearchBy != null)
            {
                return RedirectToAction("ViewProduct", new { SearchBy });
            }
            var cartItems = db.my_cart.Where(cart => cart.u_id == UserId && cart.cart_statu == "active").Select(cart => cart.p_id).ToList();
            ViewBag.CartItems = cartItems;
            IEnumerable<product_mst> products;
            var data = db.product_mst.Where(p => p.p_id == pid).ToList();
            products=data.Select(a => new product_mst
            {
                price=a.price,
                p_name = a.p_name,
                p_desc = a.p_desc,
                image = a.image,
                discount = a.discount,
                rating = a.rating,
                quantity = 1,
                p_id = a.p_id,
                CostPrice = CalculateCostprice(a.price, a.discount),
            }).ToList();
            return View(products);
        }
        [Authorize]
        public ActionResult Order_Details(int? od_id, string SearchBy)
        {
            int UserId = Convert.ToInt32(Session["Admin_Id"]);
            if (SearchBy != null)
            {
                return RedirectToAction("ViewProduct", new { SearchBy });
            }
            IEnumerable<Order_Details_View> order=db.Order_Details_View.Where(u => u.u_id==UserId && u.od_id==od_id).ToList();
            return View(order);
        }
        [Authorize]
        public ActionResult CancelOrder(int od_id,string reason)
        {
            int UserId = Convert.ToInt32(Session["Admin_Id"]);
            var order = db.Order_Details.Where(o => o.od_id == od_id).FirstOrDefault();
            order.order_status = "Canceled";
            order.action_dt=DateTime.Now;
            db.SaveChanges();
            Cancel_Order cancel = new Cancel_Order();
            cancel.od_id=order.od_id;
            cancel.cancel_reason = reason;
            cancel.cancel_dt = DateTime.Now;
            db.Cancel_Order.Add(cancel);
            db.SaveChanges();
            return RedirectToAction("Order_Details", "User",new {od_id});
        }
    }
}