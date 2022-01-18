//using System;
namespace s3638151_a1.Models
{
	public class Customer
	{

        public int CustomerID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostCode { get; set; }
        public List<Account> Accounts { get; set; }
        public Logins Login { get; set; }
    }
}

