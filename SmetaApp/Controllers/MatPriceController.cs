using System.Web.Mvc;
using SmetaApp.Models;
using System.Linq;
using System.Collections.Generic;

using PagedList;

namespace SmetaApp.Controllers
{
    [Authorize(Roles = RolesNames.User)]
    public class MatPriceController : Controller
    {
        private IMatPriceRepository Repository;
        public MatPriceController(IMatPriceRepository r)
        {
            Repository = r;
        }
        [HttpGet]
        public ActionResult ListMatPrices(int? page,int? size)
        {
            int pageSize = size ?? 50;
            int pageNumber = page ?? 1;
            var mps = Repository.MatPrices.ToList();
            if (mps.Count>0)
                return View(mps.ToPagedList(pageNumber, pageSize));
            return HttpNotFound();
        }
        [HttpGet]
        [Authorize(Roles = RolesNames.Admin)]
        public ActionResult EditMatPrices(int? page, int? size)
        {
            int pageSize = size ?? 50;
            int pageNumber = page ?? 1;
            var mps = Repository.MatPrices.ToList();
            if (mps.Count > 0)
                return View(mps.ToPagedList(pageNumber, pageSize));
            return HttpNotFound();
        }
        [HttpPost]
        [Authorize(Roles = RolesNames.Admin)]
        public ActionResult EditMatPrices(List<MatPriceToDel> mptds,int? page, int? size)
        {
            int pageSize = size ?? 50;
            int pageNumber = page ?? 1;

            var toDel = mptds.Where(mp => mp.toDel == true).Select(mp=>mp.MatPrice.Name);
            var toUp = mptds.Where(mp => mp.toDel == false&&mp.oldName==mp.MatPrice.Name).Select(mp => mp.MatPrice);
            var toUpPK = mptds.Where(mp => mp.toDel == false && mp.oldName != mp.MatPrice.Name);
            foreach (string Name in toDel)
                Repository.DeleteMatPrice(Name);
            foreach (MatPrice mp in toUp)
            {
                Repository.UpdateMatPrice(mp);
            }
            foreach(MatPriceToDel mptd in toUpPK)
            {
                if (Repository.ReadMatPrice(mptd.MatPrice.Name) == null)
                {
                    Repository.DeleteMatPrice(mptd.oldName);

                    Repository.CreateMatPrice(mptd.MatPrice);
                }
            }

            var mps = Repository.MatPrices.ToList();
            if (mps.Count > 0)
                return View(mps.ToPagedList(pageNumber, pageSize));
            return HttpNotFound();
        }
        [HttpGet]
        [Authorize(Roles = RolesNames.Admin)]
        public ActionResult EditFindMatPriceByName(string Search, int? page, int? size)
        {
            if (Search == null || Search == "")
                return RedirectToAction("EditMatPrices", "MatPrice", new { page = page, size = size });
            int pageSize = size ?? 10;
            int pageNumber = page ?? 1;
            var mps = Repository.MatPrices.Where(mp => mp.AnotherNames.Select(an=>an.MatName).Any(an => an.Contains(Search)));
            if (mps.Any())
            {
                ViewBag.Type = "Name";
                ViewBag.Search = Search;
                return View("~/Views/MatPrice/EditMatPrices.cshtml", mps.ToList().ToPagedList(pageNumber, pageSize));
            }
            return HttpNotFound();
        }
        [HttpGet]
        public ActionResult ListFindMatPriceByName(string Search, int? page, int? size)
        {
            if (Search == null || Search == "")
                return RedirectToAction("ListMatPrices", "MatPrice", new { page = page, size = size });
            int pageSize = size ?? 10;
            int pageNumber = page ?? 1;
            var mps = Repository.MatPrices.Where(mp => mp.AnotherNames.Select(an => an.MatName).Any(an => an.Contains(Search)));
            if (mps.Any())
            {
                ViewBag.Type = "Name";
                ViewBag.Search = Search;
                return View("~/Views/MatPrice/ListMatPrices.cshtml", mps.ToList().ToPagedList(pageNumber, pageSize));
            }
            return HttpNotFound();
        }
        [HttpGet]
        [Authorize(Roles = RolesNames.Admin)]
        public ActionResult AddMatPrices()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = RolesNames.Admin)]
        public void AddMatPrices(List<MatPrice> mps)
        {
            if (mps != null)
            {
                foreach (MatPrice mptd in mps)
                {
                    if (Repository.ReadMatPrice(mptd.Name) == null)
                    {
                        Repository.CreateMatPrice(mptd);
                    }
                }
            }

            //return RedirectToAction("ListMatPrices", "MatPrice", new { page = page, size = size });
        }
        [HttpPost]
        public JsonResult CheckMPs(string[] mps)
        {
            bool[] res = mps.Select(mp => Repository.MatPrices.Any(item => item.Name == mp)).ToArray();
            return Json(res, JsonRequestBehavior.DenyGet);
        }
    }
}
