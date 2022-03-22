using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimchaFund.Data;
using SimchaFund.Web.Models;
namespace SimchaFund.Web.Controllers
{
    public class SimchasController : Controller
    {
        private string _connectionString = @"Data Source=.\sqlexpress;Initial Catalog=SimchaFund;Integrated Security=true;";
        public IActionResult Index()
        {
            var db = new SimchaDB(_connectionString);
            var vm = new SimchasPageViewModel
            {
                Message = (string)TempData["message"],
                Simchos = db.GetSimchos()
            };
            return View(vm);
        }
        [HttpPost]
        public IActionResult New(Simcha simcha)
        {
            var db = new SimchaDB(_connectionString);
            db.AddSimcha(simcha);
            TempData["message"] = $"simcha added successesfully! ID - {simcha.ID}";
            return Redirect("/simchas/index");
        }

        public IActionResult Contributions(int simchaId)
        {
            var db = new SimchaDB(_connectionString);
            var vm = new ContributionsViewModel 
            { 
                Contributors = db.GetContributors(true, simchaId),
                SimchaName = db.GetSimchaName(simchaId),
                SimchaID = simchaId
            };
            return View(vm);
        }
        [HttpPost]
        public IActionResult UpdateSimcha(List<Contributor> contributors, int simchaId)
        {
            contributors = contributors.Where(c => c.Include).ToList();
            var db = new SimchaDB(_connectionString);
            db.DeleteContributionsForSimcha(simchaId);
            db.AddContributionsForSimcha(contributors, simchaId);
            TempData["message"] = "Simcha updated successfully";
            return Redirect("/simchas/index");
        }
    }
}
