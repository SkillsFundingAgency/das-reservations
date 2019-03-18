using System;

namespace SFA.DAS.Reservations.Web.Models
{
    public class StartDateModel
    {
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }

        public override string ToString()
        {
            return $"{StartDate:dd MMMM yyyy} to {ExpiryDate:dd MMMM yyyy}";
        }
    }
}