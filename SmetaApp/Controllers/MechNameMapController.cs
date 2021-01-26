using System.Web.Mvc;
using System.Linq;
using SmetaApp.Models;

namespace SmetaApp.Controllers
{
    public class MechNameMapController : Controller
    {
        private IMechNameMapRepository Repository;
        public MechNameMapController(IMechNameMapRepository r)
        {
            Repository = r;
        }
        // POST: MechNameMap/CheckMechs
        [HttpPost]
        public ActionResult CheckMechs(string[] ms)
        {
            if (ms != null)
            {
                var mnms = Repository.MechNameMaps.ToList();
                bool[] res = ms.Select(m => mnms.Any(item => item.MechName == m)).ToArray();
                return Json(res, JsonRequestBehavior.DenyGet);
            }
            return HttpNotFound();
        }

        // POST: MechNameMap/FindMechPartial{/Name=...}
        [HttpPost]
        public ActionResult FindMechPartial(string Name)
        {
            var mechs = Repository.MechNameMaps.Where(m => m.MechName.Contains(Name));

            if (Repository.MechNameMaps.Any(m => m.MechName == Name))
                ViewBag.Match = true;
            else
                ViewBag.Match = false;

            return PartialView(mechs.Take(5).ToList());
        }
        // POST: MechNameMap/BindName(bm=...)
        [HttpPost]
        public JsonResult BindName(BindingModel bm)
        {
            var Old = Repository.MechNameMaps.FirstOrDefault(m => m.MechName == bm.Old);
            var New = Repository.MechNameMaps.FirstOrDefault(m => m.MechName == bm.New);
            if (Old != null && New == null)
            {
                MechNameMap mnm = new MechNameMap()
                {
                    MechName = bm.New,
                    MechPriceName = Old.MechPriceName
                };
                Repository.CreateMechNameMap(mnm);
                return Json(bm.New+" успешно добален к "+bm.Old, JsonRequestBehavior.DenyGet);
            }
            else if (Old == null)
            {
                return Json(bm.Old + " не найден.", JsonRequestBehavior.DenyGet);
            }
            return Json(bm.New + " уже существует в БД.", JsonRequestBehavior.DenyGet);
        }
        [HttpPost]
        public ActionResult FindMechPriceByNamePartial(string Name)
        {
            var mnms = Repository.MechNameMaps.Where(mnm => mnm.MechName.Contains(Name));
            return PartialView("~/Views/MechNameMap/EditMechPrices/FindMechPriceByNamePartial.cshtml", mnms.Take(5).ToList());
        }
    }
}