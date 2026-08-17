using System.Collections.Generic;
using AVBDelivery.Models;

namespace AVBDelivery.ViewModels
{
    public class DBLogViewModel : PageViewModel
    {
        public IEnumerable<DBLog>? DBLogs { get; set; }
    }
}
