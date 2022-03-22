using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimchaFund.Data;
namespace SimchaFund.Web.Models
{
    public class HistoryViewModel
    {
        public List<Transaction> History { get; set; }
        public string ContributorName { get; set; }
        public decimal Balance { get; set; }
    }
}
