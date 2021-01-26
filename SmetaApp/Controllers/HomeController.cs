using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmetaApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (TempData.ContainsKey("Message"))
            {
                if (TempData["Message"].ToString()=="UserCreated")
                {
                    ViewBag.UserCreated = true;
                    if (TempData.ContainsKey("UserName"))
                        ViewBag.UserName = TempData["UserName"];
                }
                else if(TempData["Message"].ToString() == "TemplateCreated")
                {
                    ViewBag.TemplateCreated = true;
                    if (TempData.ContainsKey("TemplateName"))
                        ViewBag.TemplateName = TempData["TemplateName"];
                }
                else if (TempData["Message"].ToString() == "TemplateEdited")
                {
                    ViewBag.TemplateEdited = true;
                    if (TempData.ContainsKey("TemplateName"))
                        ViewBag.TemplateName = TempData["TemplateName"];
                }
            }
                
            
            return View();
        }
    }
}