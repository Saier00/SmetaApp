using System.Web.Mvc;
using SmetaApp.Models;
using System.Linq;

namespace SmetaApp.Controllers
{
    public class MatController : Controller
    {
        private IMatRepository Repository;
        // GET: MatPrice
        public MatController(IMatRepository r)
        {
            Repository = r;
        }
        public ActionResult ListMats()
        {
            return View(Repository.Mats);
        }
        public ActionResult UDFindJobByMatPartial(string Mat)
        {
            var names = Repository.Mats.Where(m => m.Job != null && m.Name.Contains(Mat)).Select(m=>m.Name).Distinct();
            return PartialView("~/Views/Job/UDJobs/FindJobByMatPartial.cshtml", names.Take(5).ToList());
        }
    }
}
