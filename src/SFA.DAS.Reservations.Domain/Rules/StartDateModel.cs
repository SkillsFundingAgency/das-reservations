using System;

namespace SFA.DAS.Reservations.Domain.Rules
{
    public class StartDateModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public override string ToString()
        {
            return $"{StartDate:MMMM yyyy} to {EndDate:MMMM yyyy}";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is StartDateModel targetModel)) return false;

            return StartDate.Year.Equals(targetModel.StartDate.Year) && 
                   StartDate.Month.Equals(targetModel.StartDate.Month) && 
                   StartDate.Day.Equals(targetModel.StartDate.Day) && 
                   EndDate.Year.Equals(targetModel.EndDate.Year) && 
                   EndDate.Month.Equals(targetModel.EndDate.Month) && 
                   EndDate.Day.Equals(targetModel.EndDate.Day);
        }

        public override int GetHashCode()
        {
            return -1;
        }
    }
}