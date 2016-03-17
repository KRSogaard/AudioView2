using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AudioView.Common.Data;
using AudioView.Common.Services;
using AudioView.Web.Filters;
using AudioView.Web.Models;
using AudioView.Web.Tools;

namespace AudioView.Web.Controllers
{
    [AuthFilter]
    public class MeasurementsController : Controller
    {
        private DatabaseService databaseService;

        public MeasurementsController()
        {
            this.databaseService = new DatabaseService();
        }

        public Task<ActionResult> Index()
        {
            return Index(new MeasurementsSearchResultModel()
            {
                From = DateTime.Now.AddMonths(-1).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                To = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
            });
        }

        [HttpPost]
        public async Task<ActionResult> Index(MeasurementsSearchResultModel model)
        {
            var from = DateTime.Now.AddMonths(-1);
            var to = DateTime.Now;

            var keys = Request.Form.AllKeys;
            // Model binding It is being very strange
            if (model.From == null && keys.Contains("From"))
            {
                model.From = Request.Form["From"];
            }
            if (model.To == null && keys.Contains("To"))
            {
                model.To = Request.Form["To"];
            }
            if (model.ProjectName == null && keys.Contains("ProjectName"))
            {
                model.ProjectName = Request.Form["ProjectName"];
            }
            if (model.ProjectNumber == null && keys.Contains("ProjectNumber"))
            {
                model.ProjectNumber = Request.Form["ProjectNumber"];
            }

            if (ModelState.IsValid)
            {
                if (model.From != null)
                {
                    DateTime tryDate;
                    if (DateTime.TryParseExact(model.From.Trim() + " 00:00:00", "dd/MM/yyyy hh:mm:ss",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal, out tryDate))
                    {
                        from = tryDate;
                    }
                    else
                    {
                        FlashHelper.Add(
                            string.Format("\"{0}\" was not reconiced as a date of the format dd/mm/yyyy.", model.From),
                            FlashType.Error);
                    }
                }
                if (model.To != null)
                {
                    DateTime tryDate;
                    if (DateTime.TryParseExact(model.To.Trim() + " 23:59:59", "dd/MM/yyyy HH:mm:ss",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal, out tryDate))
                    {
                        to = tryDate;
                    }
                    else
                    {
                        FlashHelper.Add(
                            string.Format("\"{0}\" was not reconiced as a date of the format dd/mm/yyyy.", model.To),
                            FlashType.Error);
                    }
                }
            }
            model.Projects = await databaseService.SearchProjects(model.ProjectName, model.ProjectNumber, from, to);
            return View(model);
        }

        public async Task<ActionResult> Readings(Guid id)
        {
            Project project = await databaseService.GetProject(id);
            if (project == null)
            {
                FlashHelper.Add(string.Format("Project with id \"{0}\" did not exist.", id), FlashType.Notice);
                return new RedirectToRouteResult(new RouteValueDictionary(){
                        { "controller", "Measurements" },
                        { "action", "Index" }
                    });
            }
            var readings = await databaseService.GetReading(id);

            return View(new ReadingsModel()
            {
                Project = project,
                MajorReadings = readings.Where(x => x.Major).ToList(),
                MinorReadings = readings.Where(x => !x.Major).ToList()
            });
        }

        public async Task<ActionResult> Delete(Guid id, string redirect)
        {
            Project project = await databaseService.GetProject(id);
            if (project == null)
            {
                FlashHelper.Add(string.Format("Project with id \"{0}\" did not exist.", id), FlashType.Notice);
                return new RedirectToRouteResult(new RouteValueDictionary(){
                        { "controller", "Measurements" },
                        { "action", "Index" }
                    });
            }
            return View("AreYouSureModel", new AreYouSureModel()
            {
                Message = string.Format("Are you sure you want to delete \"{0}\"", project.Name),
                RedirectAction = "Index",
                Redirect = new RouteValueDictionary()
            });
        }

        [HttpPost]
        public async Task<ActionResult> Delete(Guid id, AreYouSureModel model)
        {
            Project project = await databaseService.GetProject(id);
            await databaseService.DeleteProject(id);
            FlashHelper.Add(string.Format("{0} have been deleted.", project.Name), FlashType.Notice);
            return new RedirectToRouteResult(new RouteValueDictionary(){
                        { "controller", "Measurements" },
                        { "action", "Index" }
                    });
        }

        public async Task<ActionResult> Download(Guid id)
        {
            var readings = await databaseService.GetReading(id);
            var csvFile = Reading.CSV(readings);
            return File(Encoding.UTF8.GetBytes(csvFile), "text/csv", string.Format("{0}.csv", id));
        }
    }
}