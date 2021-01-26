using System.Web.Mvc;
using SmetaApp.Models;
using System.Linq;
using System.Collections.Generic;

using PagedList;

namespace SmetaApp.Controllers
{
    public class MechPriceController : Controller
    {
        private IMechPriceRepository Repository;
        // GET: MechPrice
        public MechPriceController(IMechPriceRepository r)
        {
            Repository = r;
        }
        public ActionResult ListMechPrices(int? page,int? size)
        {
            int pageSize = size ?? 50;
            int pageNumber = page ?? 1;
            var mps = Repository.MechPrices.ToList();
            if (mps.Count>0)
                return View(mps.ToPagedList(pageNumber, pageSize));
            return HttpNotFound();
        }
        
        public ActionResult EditMechPrices(int? page, int? size)
        {
            int pageSize = size ?? 50;
            int pageNumber = page ?? 1;
            var mps = Repository.MechPrices.ToList();
            if (mps.Count > 0)
                return View(mps.ToPagedList(pageNumber, pageSize));
            return HttpNotFound();
        }
        [HttpPost]
        public ActionResult EditMechPrices(List<MechPriceToDel> mptds,int? page, int? size)
        {
            int pageSize = size ?? 50;
            int pageNumber = page ?? 1;

            var toDel = mptds.Where(mp => mp.toDel == true).Select(mp=>mp.MechPrice.Name);
            var toUp = mptds.Where(mp => mp.toDel == false&&mp.oldName==mp.MechPrice.Name).Select(mp => mp.MechPrice);
            var toUpPK = mptds.Where(mp => mp.toDel == false && mp.oldName != mp.MechPrice.Name);
            foreach (string Name in toDel)
                Repository.DeleteMechPrice(Name);
            foreach (MechPrice mp in toUp)
            {
                Repository.UpdateMechPrice(mp);
            }
            foreach(MechPriceToDel mptd in toUpPK)
            {
                if (Repository.ReadMechPrice(mptd.MechPrice.Name) == null)
                {
                    Repository.DeleteMechPrice(mptd.oldName);

                    Repository.CreateMechPrice(mptd.MechPrice);
                }
            }

            var mps = Repository.MechPrices.ToList();
            if (mps.Count > 0)
                return View(mps.ToPagedList(pageNumber, pageSize));
            return HttpNotFound();
        }
        [HttpGet]
        public ActionResult EditFindMechPriceByName(string Search, int? page, int? size)
        {
            if (Search == null || Search == "")
                return RedirectToAction("EditMechPrices", "MechPrice", new { page = page, size = size });
            int pageSize = size ?? 10;
            int pageNumber = page ?? 1;
            var mps = Repository.MechPrices.Where(mp => mp.AnotherNames.Select(an=>an.MechName).Any(an => an.Contains(Search)));
            if (mps.Any())
            {
                ViewBag.Type = "Name";
                ViewBag.Search = Search;
                return View("~/Views/MechPrice/EditMechPrices.cshtml", mps.ToList().ToPagedList(pageNumber, pageSize));
            }
            return HttpNotFound();
        }
        [HttpGet]
        public ActionResult ListFindMechPriceByName(string Search, int? page, int? size)
        {
            if (Search == null || Search == "")
                return RedirectToAction("ListMechPrices", "MechPrice", new { page = page, size = size });
            int pageSize = size ?? 10;
            int pageNumber = page ?? 1;
            var mps = Repository.MechPrices.Where(mp => mp.AnotherNames.Select(an => an.MechName).Any(an => an.Contains(Search)));
            if (mps.Any())
            {
                ViewBag.Type = "Name";
                ViewBag.Search = Search;
                return View("~/Views/MechPrice/ListMechPrices.cshtml", mps.ToList().ToPagedList(pageNumber, pageSize));
            }
            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult AddMechPrices()
        {
            return View();
        }
        [HttpPost]
        public void AddMechPrices(List<MechPrice> mps)
        {
            if (mps != null)
            {
                foreach (MechPrice mptd in mps)
                {
                    if (Repository.ReadMechPrice(mptd.Name) == null)
                    {
                        Repository.CreateMechPrice(mptd);
                    }
                }
            }

            //return RedirectToAction("ListMatPrices", "MatPrice", new { page = page, size = size });
        }
        [HttpPost]
        public JsonResult CheckMPs(string[] mps)
        {
            bool[] res = mps.Select(mp => Repository.MechPrices.Any(item => item.Name == mp)).ToArray();
            return Json(res, JsonRequestBehavior.DenyGet);
        }
    }
}
