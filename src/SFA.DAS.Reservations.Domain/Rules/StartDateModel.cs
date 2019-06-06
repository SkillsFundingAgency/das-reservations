using System;

namespace SFA.DAS.Reservations.Domain.Rules
{
    public class StartDateModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public override string ToString()
        {
            return $"{StartDate:MMM yyyy} to {EndDate:MMM yyyy}";
        }
    }
}