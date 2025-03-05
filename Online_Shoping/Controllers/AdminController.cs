using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Online_Shoping.Models;

namespace Online_Shoping.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        OnlineShopping db = new OnlineShopping();
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(Admin adm)
        {
            if (adm.email == null ||adm.password == null)
            {
                //if gmail or password field null
                TempData["Msg"] = "Please Enter g-mail and password";
                return View();
            }
            else
            {
                //if email id and password are provided then
                var data = db.Admins.Where(m => m.email == adm.email).FirstOrDefault();
                if(data!=null)
                {
                    //if gmail id is valid !
                    string psw = data.password;
                    if(psw==adm.password)
                    {
                        //if password matched...
                        FormsAuthentication.SetAuthCookie(data.email, false);
                        Session["Admin_Id"]=data.a_id;
                        Session["UserName"]=data.name;
                        return RedirectToAction("HomePage", "Admin");
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
        [Authorize]
        public ActionResult ViewCategory(string SearchBy)
        {
            if (Session["Admin_Id"]!=null)
            {
                IEnumerable<category> cat;
                if (SearchBy != null)
                {
                    cat = db.categories.Where(model => model.name.StartsWith(SearchBy)).ToList();
                    return View(cat);
                }
                else
                {
                    return View(db.categories.ToList());
                }
            }
            else
            {
                return RedirectToAction("Login", "Admin");
            }
        }


        // -----------------------------------------------------------  Home Page ---------------------------------------------------------------
        [Authorize]
        public ActionResult HomePage()
        {
            if (Session["Admin_Id"] == null && Session["UserName"] == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            else
            {
                return View();
            }
        }


        // ----------------------------------------------------------- Delete Categories  ---------------------------------------------------------------
        [Authorize]
        public ActionResult DeleteCategory(int id)
        {
            var data = db.categories.Where(m => m.cat_id == id).FirstOrDefault();
            if (data != null)
            {
                db.Entry(data).State = EntityState.Deleted;
                int x = db.SaveChanges();
                if (x > 0)
                {
                    TempData["Msg"] = "<script>alert('Your Category is deleted')</script>";
                    return RedirectToAction("ViewCategory", "Home");
                }
                else
                {
                    TempData["Msg"] = "<script>alert('Category is not deleted')</script>";
                    return View();
                }
            }
            else
            {
                TempData["Msg"] = "<script>alert('Category Unavailable')</script>";
                return RedirectToAction("ViewCategory", "Home");
            }

        }
        // ---------------------------------------------------------------  Edit Categories  --------------------------------------------------------------
        [Authorize]
        public ActionResult EditCategory(int id)
        {
            var data = db.categories.Where(m => m.cat_id == id).FirstOrDefault();
            if (data != null)
            {
                return View(data);
            }
            else
            {
                TempData["Msg"] = "<script>alert('Category Unavailable')</script>";
                return RedirectToAction("ViewCategory", "Home");
            }
        }
        [HttpPost]
        public ActionResult EditCategory(category cat)
        {
            string fname = Path.GetFileNameWithoutExtension(cat.File.FileName);
            string ext = Path.GetExtension(cat.File.FileName);
            fname = fname + ext;
            cat.image = "~/Assets/" + fname;
            string x = Path.Combine(Server.MapPath("~/Assets/" + fname));
            cat.File.SaveAs(x);
            cat.adm_id = Convert.ToInt32(Session["Admin_Id"]);
            db.Entry(cat).State = EntityState.Modified;
            db.SaveChanges();
            TempData["Msg"] = "<script>alert('Category Edited')</script>";
            return View(cat);
        }

        [OutputCache(Duration =600)]
        [Authorize]
        public ActionResult UploadProduct()
        {
            if (Session["Admin_Id"] !=null)
            {
                List<category> categoryList = db.categories.ToList();
                ViewBag.catList = new SelectList(categoryList, "cat_id", "name");
                List<string> status = new List<string> { "Public", "Private" };
                ViewBag.product_status = new SelectList(status);
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Admin");
            }
        }
        [HttpPost]
        public ActionResult UploadProduct(product_mst product)
        {
            
            int x = FileUploadFn(product, product.File);
            if (x == 0)
            {
                ModelState.AddModelError("Image", "Select Product Image");
                return View();
            }
            else if(x==-1)
            {
                TempData["msg"] = "Please try again to upload image";
                return View();
            }
            else
            {
                product.u_id = Convert.ToInt32(Session["Admin_Id"]);
                db.product_mst.Add(product);
                int n = db.SaveChanges();
                if (n > 0)
                {
                    ModelState.Clear();
                    ViewBag.catList = new SelectList(db.categories, "cat_id", "name");
                    List<string> status = new List<string> { "Public", "Private" };
                    ViewBag.product_status = new SelectList(status);
                    /*TempData["Msg"] = "<script>alert('Product Added')</script>";*/
                    TempData["msg"] = "Product added";
                    return View();
                }
                else
                {
                    /*TempData["Msg"] = "<script>alert('Product Not Added Please Try again Later...')</script>";*/
                    TempData["msg"] = "Unable to add new Product right Now...!";
                    return View();
                }
            }            
        }
        public int FileUploadFn(product_mst product, HttpPostedFileBase File)
        {
            try
            {
                if (File != null && File.ContentLength > 0)
                {
                    string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                    string ext = Path.GetExtension(File.FileName).ToLower();

                    if (permittedExtensions.Contains(ext))
                    {
                        string fname = Path.GetFileNameWithoutExtension(File.FileName);
                        fname += ext;
                        product.image = "~/Assets/" + fname;
                        string path = Path.Combine(Server.MapPath("~/Assets/"), fname);
                        File.SaveAs(path);
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        [Authorize]
        public ActionResult CreateCategory()
        {
            if (Session["Admin_Id"]!=null)
                return View();
            return RedirectToAction("Login", "Admin");
        }
        [HttpPost]
        public ActionResult CreateCategory(category cat)
        {
            string fname = Path.GetFileNameWithoutExtension(cat.File.FileName);
            string ext = Path.GetExtension(cat.File.FileName);
            fname = fname + ext;
            cat.image = "~/Assets/" + fname;
            string x = Path.Combine(Server.MapPath("~/Assets/" + fname));
            cat.File.SaveAs(x);
            cat.adm_id = Convert.ToInt32(Session["Admin_Id"]);
            db.categories.Add(cat);
            int n = db.SaveChanges();
            if (n > 0)
            {
                TempData["Msg"] = "<script>alert('Product Added')</script>";
            }
            else
            {
                TempData["Msg"] = "<script>alert('Product Not Added Please Try again Later...')</script>";
            }
            return View();
        }
        [Authorize]
        public ActionResult OrderRequests()
        {
            var orders= db.Order_Details_View.Where(o => o.order_status=="Processing").ToList();
            return View(orders);
        }
        [Authorize]
        public ActionResult AcceptOrder(int od_id)
        {

            var order = db.Order_Details.Where(o => o.od_id == od_id).FirstOrDefault();
            order.order_status = "Confirmed";
            db.SaveChanges();
            return RedirectToAction("OrderRequests","Admin");
        }
        [Authorize]
        public ActionResult RejectOrder(int od_id)
        {

            var order = db.Order_Details.Where(o => o.od_id == od_id).FirstOrDefault();
            order.order_status = "Canceled";
            db.SaveChanges();
            return RedirectToAction("OrderRequests","Admin");
        }
        [Authorize]
        public ActionResult Delivery()
        {
            var orders = db.Order_Details_View.Where(o => o.order_status=="Confirmed").ToList();
            return View(orders);
        }
        [Authorize]
        public ActionResult Delivered(int od_id)
        {
            var order = db.Order_Details.Where(o => o.od_id == od_id).FirstOrDefault();
            order.order_status = "Delivered";
            db.SaveChanges();
            return RedirectToAction("Delivery", "Admin");
        }
        [Authorize]
        public ActionResult CancelOrder(int od_id)
        {
            var order = db.Order_Details.Where(o => o.od_id == od_id).FirstOrDefault();
            order.order_status = "Canceled";
            db.SaveChanges();
            return RedirectToAction("Delivery", "Admin");
        }
    }
}