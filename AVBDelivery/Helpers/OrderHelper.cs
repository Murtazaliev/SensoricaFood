using AVBDelivery.Models;
using System.Collections.Generic;
using System.Linq;

namespace AVBDelivery.Helpers
{
    public static class OrderHelper
    {
        public static double GetDiscount(Organization organization)
        {
            if (organization == null)
                return 1;
       
            return (double)(organization?.CalculateDiscount() ?? 1m);
        }
    }
}
