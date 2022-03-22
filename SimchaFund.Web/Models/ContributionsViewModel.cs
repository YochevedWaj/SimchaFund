using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimchaFund.Data;
namespace SimchaFund.Web.Models
{
    public class ContributionsViewModel
    {
        public List<Contributor> Contributors { get; set; }
        public string SimchaName { get; set; }
        public int SimchaID { get; set; }
        public string Message { get; set; }
    }
}
