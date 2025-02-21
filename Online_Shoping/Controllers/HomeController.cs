using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Online_Shoping.Models;
using System.Data.Entity;

namespace Online_Shoping.Controllers
{
    public class HomeController : Controller
    {

        OnlineShopping db = new OnlineShopping();
        // -----------------------------------------------------------  Home Page ---------------------------------------------------------------
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
        //------------------ My Cart ----------------------
        public ActionResult MyCart()
        {
            return View();
        }


        /*public ActionResult DeleteCategory(int id)
        {
            var data=db.categories.Where(m => m.cat_id == id).FirstOrDefault();
                return View(data);

        }
        [HttpPost]
        public ActionResult DeleteCategory(category cat)
        {
            var row=db.categories.Where(m => m.cat_id==cat.cat_id).FirstOrDefault();
            db.Entry(row).State=EntityState.Deleted;
            int x=db.SaveChanges();
            if(x > 0)
            {
                TempData["Msg"] = "<script>alert('Your Category is deleted')</script>";
                return RedirectToAction("ViewCategory", "Home");
            }
            else
            {
                TempData["Msg"] = "<script>alert('Category is not deleted')</script>";
                return View();
            }
        }*/
    }
}