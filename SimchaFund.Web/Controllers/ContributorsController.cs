using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimchaFund.Data;
using SimchaFund.Web.Models;
using System.Data.SqlClient;

namespace SimchaFund.Web.Controllers
{
    public class ContributorsController : Controller
    {
        private string _connectionString = @"Data Source=.\sqlexpress;Initial Catalog=SimchaFund;Integrated Security=true;";
        public IActionResult Index()
        {
            var db = new SimchaDB(_connectionString);
            var vm = new ContributorsViewModel
            {
                Contributors = db.GetContributors(false),
                Total = db.GetTotal()
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult New(Contributor contributor, decimal deposit)
        {
            var db = new SimchaDB(_connectionString);
            db.AddContributor(contributor, deposit);
            return Redirect("/Contributors/index");
        }

        [HttpPost]
        public IActionResult UpdateContributor(Contributor contributor)
        {
            var db = new SimchaDB(_connectionString);
            db.UpdateContributor(contributor);
            return Redirect("/Contributors/index");
        }
        [HttpPost]
        public IActionResult AddDeposit(Deposit deposit)
        {
            var db = new SimchaDB(_connectionString);
            db.AddDeposit(deposit);
            return Redirect("/contributors/index");
            
        }

        public IActionResult History(int contributorId)
        {
            var db = new SimchaDB(_connectionString);
            var vm = new HistoryViewModel
            {
                History = db.GetHistory(contributorId).OrderByDescending(t => t.Date).ToList(),
                Balance = db.GetContributorBalance(contributorId),
                ContributorName = db.GetContributorName(contributorId)
            };
            return View(vm);
        }
    }
}
