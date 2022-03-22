using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimchaFund.Data;
namespace SimchaFund.Web.Models
{
    public class ContributorsViewModel
    {
        public List<Contributor> Contributors { get; set; }
        public decimal Total { get; set; }
    }
}
