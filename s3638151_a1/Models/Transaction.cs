//using System;
namespace s3638151_a1.Models
{
    public class Transaction: Show
    {
        public int TransactionID { get; set; }
        public string TransactionType { get; set; }
        public int AccountNumber { get; set; }
        public int DestinationAccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string Comment { get; set; }
        public string TransactionTimeUtc { get; set; }
        protected override void Display()
        {
            Console.WriteLine("{0, -19}{1, -17}{2, -28}{3, -10}{4, -22}{5, -11}", TransactionType, AccountNumber, DestinationAccountNumber, Math.Round(Amount, 2), DateTime.Parse(TransactionTimeUtc).AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours).ToString("dd/MM/yyyy"), Comment);
        }
    }
}

