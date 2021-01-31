using System.Web.Mvc;
using System.Linq;
using SmetaApp.Models;

namespace SmetaApp.Controllers
{
    [Authorize(Roles = RolesNames.User)]
    public class MatNameMapController : Controller
    {
        private IMatNameMapRepository Repository;
        public MatNameMapController(IMatNameMapRepository r)
        {
            Repository = r;
        }

        // POST: MatNameMap/CheckMats
        [HttpPost]
        public ActionResult CheckMats(string[] ms)
        {
            if (ms != null)
            {
                var mnms = Repository.MatNameMaps.ToList();
                bool[] res = ms.Select(m => mnms.Any(item => item.MatName == m)).ToArray();
                return Json(res, JsonRequestBehavior.DenyGet);
            }
            return HttpNotFound();
        }

        // POST: MatNameMap/FindMatPartial{/Name=...}
        [HttpPost]
        public ActionResult FindMatPartial(string Name)
        {
            var mats = Repository.MatNameMaps.Where(m => m.MatName.Contains(Name));

            if (Repository.MatNameMaps.Any(m => m.MatName == Name))
                ViewBag.Match = true;
            else
                ViewBag.Match = false;

            return PartialView(mats.Take(5).ToList());
        }
        // POST: MatNameMap/BindName(bm=...)
        [HttpPost]
        [Authorize(Roles = RolesNames.Admin)]
        public JsonResult BindName(BindingModel bm)
        {
            var Old = Repository.MatNameMaps.FirstOrDefault(m => m.MatName==bm.Old);
            var New = Repository.MatNameMaps.FirstOrDefault(m => m.MatName == bm.New);
            if (Old != null && New == null) {
                MatNameMap mnm = new MatNameMap()
                {
                    MatName = bm.New,
                    MatPriceName = Old.MatPriceName
                };
                Repository.CreateMatNameMap(mnm);
                return Json(bm.New + " успешно добален к " + bm.Old, JsonRequestBehavior.DenyGet);
            }
            else if (Old == null) 
            {
                return Json(bm.Old + " не найден.", JsonRequestBehavior.DenyGet);
            }
                return Json(bm.New + " уже существует в БД.", JsonRequestBehavior.DenyGet);

        }

        [HttpPost]
        public ActionResult FindMatPriceByNamePartial(string Name)
        {
            var mnms = Repository.MatNameMaps.Where(mnm => mnm.MatName.Contains(Name));
            return PartialView("~/Views/MatNameMap/EditMatPrices/FindMatPriceByNamePartial.cshtml", mnms.Take(5).ToList());
        }
    }
}