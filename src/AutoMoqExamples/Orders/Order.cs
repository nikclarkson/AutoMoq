using System.Collections.Generic;

namespace Orders
{
    public class Order 
    {
        public string CustomerName { get; set; }

        public IEnumerable<string> OrderItems { get; set; }

        public decimal TotalPrice { get; set; }
        public string PaymentMethod { get; set; }
    }
}
