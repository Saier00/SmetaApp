using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Threading.Tasks;

using SmetaApp.Models;
using SmetaApp.ExcelParsing;

using PagedList;
using ClosedXML.Excel;

namespace SmetaApp.Controllers
{
    [Authorize (Roles = RolesNames.User)]
    public class JobController : Controller
    {
        private IJobRepository Repository;
        public JobController(IJobRepository r)
        {
            Repository = r;
        }
        // GET: Job/ListJobs
        [HttpGet]
        public ActionResult ListJobs()
        {
            return View();
        }


        // GET: Job/UDJobs
        [HttpGet]
        [Authorize(Roles = RolesNames.Admin)]
        public ActionResult UDJobs(int? page,int? size)
        {
            int pageSize = size?? 10;
            int pageNumber = page ?? 1;
            var jobs = Repository.Jobs;
            if(jobs.Any())
                return View(jobs.OrderBy(j=>j.Name).ToPagedList(pageNumber, pageSize));
            return HttpNotFound();
        }
        [HttpPost]
        [Authorize(Roles = RolesNames.Admin)]
        public void DeleteJob(int[] Ids)
        {
            foreach(int id in Ids)
                Repository.DeleteJob(id);
        }
        [HttpPost]
        [Authorize(Roles = RolesNames.Admin)]
        public void UpdateJob(List<Job> Jobs)
        {
            foreach(Job Job in Jobs)
                Repository.UpdateJob(Job);
        }
        // POST: Job/FindJobByNamePartial{Name=...}
        [HttpPost]
        [Authorize(Roles = RolesNames.Admin)]
        public ActionResult UDFindJobByNamePartial(string Name)
        {
            var jobs = Repository.Jobs.Where(j=>j.Name.Contains(Name));
            return PartialView("~/Views/Job/UDJobs/FindJobByNamePartial.cshtml",jobs.Take(5).ToList());
        }
        // POST: Job/FindJobByCodePartial{/Code=...}
        [HttpPost]
        [Authorize(Roles = RolesNames.Admin)]
        public ActionResult UDFindJobByCodePartial(string Code)
        {
            var jobs = Repository.Jobs.Where(j => j.Code.Contains(Code));
            return PartialView("~/Views/Job/UDJobs/FindJobByCodePartial.cshtml", jobs.Take(5).ToList());
        }
        [HttpPost]
        [Authorize(Roles = RolesNames.Admin)]
        public ActionResult UDFindJobByTypePartial(string Type)
        {
            var jobs = Repository.Jobs.Where(j => j.Code.Contains(Type));
            return PartialView("~/Views/Job/UDJobs/FindJobByTypePartial.cshtml", jobs.Take(5).ToList());
        }


        [HttpGet]
        public ActionResult FindJobByName(string Search, int? page, int? size)
        {
            if (Search == null || Search == "")
                return RedirectToAction("UDJobs", "Job", new { page = page, size = size });
            int pageSize = size ?? 10;
            int pageNumber = page ?? 1;
            var jobs = Repository.Jobs.Where(j => j.Name.Contains(Search));
            if (jobs.Any())
            {
                ViewBag.Type = "Name";
                ViewBag.Search = Search;
                return View("~/Views/Job/UDJobs.cshtml", jobs.OrderBy(j => j.Name).ToPagedList(pageNumber, pageSize));
            }
            return HttpNotFound();
        }
        [HttpGet]
        public ActionResult FindJobByCode(string Search, int? page, int? size)
        {
            if (Search == null || Search == "")
                return RedirectToAction("UDJobs", "Job", new { page = page, size = size });
            int pageSize = size ?? 10;
            int pageNumber = page ?? 1;
            var jobs = Repository.Jobs.Where(j => j.Code.Contains(Search));
            if (jobs.Any())
            {
                ViewBag.Type = "Code";
                ViewBag.Search = Search;
                return View("~/Views/Job/UDJobs.cshtml", jobs.OrderBy(j=>j.Name).ToPagedList(pageNumber, pageSize));
            }
            return HttpNotFound();
        }
        [HttpGet]
        public ActionResult FindJobByType(string Search, int? page, int? size)
        {
            if (Search == null || Search == "")
                return RedirectToAction("UDJobs", "Job", new { page = page, size = size });
            int pageSize = size ?? 10;
            int pageNumber = page ?? 1;
            var jobs = Repository.Jobs.Where(j => j.Type.Contains(Search));
            if (jobs.Any())
            {
                ViewBag.Type = "Type";
                ViewBag.Search = Search;
                return View("~/Views/Job/UDJobs.cshtml", jobs.OrderBy(j=>j.Name).ToPagedList(pageNumber, pageSize));
            }
            return HttpNotFound();
        }
        [HttpGet]
        public ActionResult FindJobByMat(string Search, int? page, int? size)
        {
            if (Search == null || Search == "")
                return RedirectToAction("UDJobs", "Job", new { page = page, size = size });
            int pageSize = size ?? 10;
            int pageNumber = page ?? 1;
            var jobs = Repository.Jobs.Where(j => j.Mats.Select(m=>m.Name).Any(n=>n.Contains(Search)));
            if (jobs.Any())
            {
                ViewBag.Type = "Mat";
                ViewBag.Search = Search;
                return View("~/Views/Job/UDJobs.cshtml", jobs.OrderBy(j=>j.Name).ToPagedList(pageNumber, pageSize));
            }
            return HttpNotFound();
        }
        [HttpGet]
        public ActionResult FindJobByMech(string Search, int? page, int? size)
        {
            if (Search == null || Search == "")
                return RedirectToAction("UDJobs", "Job", new { page = page, size = size });
            int pageSize = size ?? 10;
            int pageNumber = page ?? 1;
            var jobs = Repository.Jobs.Where(j => j.Mechs.Select(m => m.Name).Any(n => n.Contains(Search)));
            if (jobs.Any())
            {
                ViewBag.Type = "Mech";
                ViewBag.Search = Search;
                return View("~/Views/Job/UDJobs.cshtml", jobs.OrderBy(j=>j.Name).ToPagedList(pageNumber, pageSize));
            }
            return HttpNotFound();
        }




        [HttpPost]
        public ActionResult FindJobByNamePartial(string Name, string Type = null, string Code = null, string Mat = null, string Mech = null)
        {
            var jobs = Repository.Jobs;
            jobs=JobContains(jobs, Name, Type, Code, Mat, Mech);
            ViewBag.By = "Name";
            return PartialView("~/Views/Job/ListJobs/FindJobByPartial.cshtml", jobs.Take(5).ToList());
        }
        [HttpPost]
        public ActionResult FindJobByCodePartial(string Code,string Name = null, string Type = null, string Mat = null, string Mech = null)
        {
            var jobs = Repository.Jobs;
            jobs=JobContains(jobs, Name, Type, Code, Mat, Mech);
            ViewBag.By = "Code";
            return PartialView("~/Views/Job/ListJobs/FindJobByPartial.cshtml", jobs.Take(5).ToList());
        }
        [HttpPost]
        public ActionResult FindJobByTypePartial(string Type, string Name = null, string Code = null, string Mat = null, string Mech = null)
        {
            var jobs = Repository.Jobs;
            jobs=JobContains(jobs, Name, Type, Code, Mat, Mech);
            ViewBag.By = "Type";
            return PartialView("~/Views/Job/ListJobs/FindJobByPartial.cshtml", jobs.Take(5).ToList());
        }
        [HttpPost]
        public ActionResult FindJobByMatPartial(string Mat, string Name = null, string Type = null, string Code = null, string Mech = null)
        {
            var jobs = Repository.Jobs;
            jobs=JobContains(jobs, Name, Type, Code, Mat, Mech);
            ViewBag.By = "Mat";
            return PartialView("~/Views/Job/ListJobs/FindJobByPartial.cshtml", jobs.Take(5).ToList());
        }
        [HttpPost]
        public ActionResult FindJobByMechPartial(string Mech, string Name = null, string Type = null, string Code = null, string Mat = null)
        {
            var jobs = Repository.Jobs;
            jobs=JobContains(jobs, Name, Type, Code, Mat, Mech);
            ViewBag.By = "Mech";
            return PartialView("~/Views/Job/ListJobs/FindJobByPartial.cshtml", jobs.Take(5).ToList());
        }

        [HttpPost]
        public ActionResult FindJobPartial(string Name = null, string Type = null, string Code = null, string Mat = null, string Mech = null)
        {
            if (!new string[5]{Name, Type, Code, Mat, Mech}.Any(s=>!(s==null||s=="")))
                return PartialView("~/Views/Job/ListJobs/FindJobPartial.cshtml");

            var jobs = Repository.Jobs;

            jobs=JobContains(jobs, Name, Type, Code, Mat, Mech);

            if (Mat == null || Mat == "")
                ViewBag.noMat = true;
            if (Mech == null || Mech == "")
                ViewBag.noMech = true;

            return PartialView("~/Views/Job/ListJobs/FindJobPartial.cshtml",jobs.Take(10).ToList());
        }

        private IQueryable<Job> JobContains(IQueryable<Job> jobs, string Name = null, string Type = null, string Code = null, string Mat = null, string Mech = null)
        {
            if (!(Name == null||Name==""))
                jobs = jobs.Where(j => j.Name.Contains(Name));
            if (!(Type == null||Type==""))
                jobs = jobs.Where(j => j.Type.Contains(Type));
            if (!(Code == null||Code==""))
                jobs = jobs.Where(j => j.Code.Contains(Code));
            if (!(Mat == null||Mat==""))
                jobs = jobs.Where(j => j.Mats.Any(m => m.Name.Contains(Mat)));
            if (!(Mech == null||Mech==""))
                jobs = jobs.Where(j => j.Mechs.Any(m => m.Name.Contains(Mech)));
            //new IQueryable
            return jobs;
        }

        // GET: Job/AddJobs
        [HttpGet]
        [Authorize(Roles = RolesNames.Admin)]
        public ActionResult AddJobs()
        {
            return View();
        }
        // POST: Job/AddJobs
        [HttpPost]
        [Authorize(Roles = RolesNames.Admin)]
        public void AddJobs(List<Job> Jobs, bool? noprice=null)
        {
            if (noprice!=null&&(bool)noprice)
            {
                foreach (Job j in Jobs)
                    Repository.CreateJobWithMaps(j);
            }
            else
            {
                foreach (Job j in Jobs)
                    Repository.CreateJob(j);
            }
        }
        [HttpPost]
        [Authorize(Roles = RolesNames.Admin)]
        public JsonResult CheckJobs(NameType[] js)
        {
            bool[] res = js.Select(j => Repository.Jobs.Any(item => item.Name == j.Name&&item.Type==j.Type)).ToArray();
            return Json(res, JsonRequestBehavior.DenyGet);
        }


        static object locker = new object();
        [HttpPost]
        [Authorize(Roles = RolesNames.Admin)] 
        public JsonResult ParseExcel(HttpPostedFileBase file, int? alg)
        {
            if (file != null && file.ContentLength > 0 && Path.GetExtension(file.FileName).ToLower() == ".xlsx"&&alg!=null)
            {
                string path = Path.Combine(Server.MapPath("~/UploadFile"), Path.GetFileName(file.FileName));
                try
                {
                    //save file
                    lock (locker)
                    {
                        file.SaveAs(path);
                    }

                    JobProxy[] m = null;
                    IJobParsing jp;

                    if (alg == 1)
                        jp = new Alg1();
                    else if (alg == 2)
                        jp = new Alg2();
                    else
                        return Json(new { error = new { message = "Был передан неправильный номер алгоритма" } }, JsonRequestBehavior.DenyGet);


                    using (IWorkbook workbook = new SAXWorkbook(path))
                    {
                        try
                        {
                            m = jp.Parse(workbook);

                        }
                        catch (SyntaxError e)
                        {
                            return Json(new { error = new { message = e } }, JsonRequestBehavior.DenyGet);
                        }
                    }

                    return Json(m, JsonRequestBehavior.DenyGet);
                }
                finally
                {
                    if (System.IO.File.Exists(path))
                    {
                        Task.Factory.StartNew(() => { lock (locker) { System.IO.File.Delete(path); } });
                    }
                }
            }
            else if (file == null)
            {
                return Json(new { error = new { message = "Файл не найден." } }, JsonRequestBehavior.DenyGet);
            }
            else if (file.ContentLength <= 0)
            {
                return Json(new { error = new { message = "Файл пуст." } }, JsonRequestBehavior.DenyGet);
            }
            else if(!(Path.GetExtension(file.FileName).ToLower() == ".xlsx"))
            {
                return Json(new { error = new { message = "Файл должен иметь расширение .xlsx" } }, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json(new { error = new { message = "Номер алгоритма не был передан" } }, JsonRequestBehavior.DenyGet);
            }
            
        }
    }
}
