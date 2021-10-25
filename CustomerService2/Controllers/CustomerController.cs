using System.Collections.Generic;
using CustomerService2.Models;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : Controller
    {
        [HttpGet("list")]
        public IEnumerable<Customer> GetCustomers()
        {
            for (int i = 100; i < 105; i++)
            {
                yield return new Customer() { CustomerId = i, Name = $"Customer-{i}" };
            }
        }
    }
}