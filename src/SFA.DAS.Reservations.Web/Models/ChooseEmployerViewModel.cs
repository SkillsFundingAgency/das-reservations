using System.Collections.Generic;
using SFA.DAS.Reservations.Domain.Employers;

namespace SFA.DAS.Reservations.Web.Models
{
    public class ChooseEmployerViewModel
    {
        public const string HeaderClassName = "das-table__sort";

        public string SortedByHeaderClassName
        {
            get
            {
                var sortedByHeaderClassName = HeaderClassName;

                if (SortModel.ReverseSort)
                {
                    sortedByHeaderClassName += " das-table__sort--desc";
                }
                else
                {
                    sortedByHeaderClassName += " das-table__sort--asc";
                }

                return sortedByHeaderClassName;
            }
        }

        public string SearchTerm { get; set; }

        public SortModel SortModel { get; set; } 

        public IEnumerable<AccountLegalEntity> Employers { get; set; }
    }
}
