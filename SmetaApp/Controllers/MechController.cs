using System.Web.Mvc;
using SmetaApp.Models;
using System.Linq;

namespace SmetaApp.Controllers
{
    public class MechController : Controller
    {
        private IMechRepository Repository;
        // GET: MatPrice
        public MechController(IMechRepository r)
        {
            Repository = r;
        }
        public ActionResult ListMechs()
        {
            return View(Repository.Mechs);
        }
        public ActionResult UDFindJobByMechPartial(string Mech)
        {
            var names = Repository.Mechs.Where(m => m.Job != null && m.Name.Contains(Mech)).Select(m => m.Name).Distinct();
            return PartialView("~/Views/Job/UDJobs/FindJobByMechPartial.cshtml", names.Take(5).ToList());
        }
    }
}
