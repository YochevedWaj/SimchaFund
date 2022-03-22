using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimchaFund.Data;
namespace SimchaFund.Web.Models
{
    public class SimchasPageViewModel
    {
        public string Message { get; set; }
        public List<Simcha> Simchos { get; set; }

    }
}
