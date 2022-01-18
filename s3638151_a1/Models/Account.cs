//using System;
namespace s3638151_a1.Models
{
    public class Account : Show
    {
        public int AccountNumber { get; set; }
        public string AccountType { get; set; }
        public int CustomerID { get; set; }
        public decimal Balance { get; set; }
        public List<Transaction> Transactions { get; set; }
        protected override void Display()
        {
            Console.WriteLine("{0, -17}{1, -15}{2, -11}", AccountNumber, AccountType == "S" ? "Savings" : "Checking", Math.Round(Balance, 2));
        }
    }
}

