using System;
using SFA.DAS.Reservations.Domain.Courses;

namespace SFA.DAS.Reservations.Web.Models
{
    public abstract class AddApprenticeViewModel
    {
        protected AddApprenticeViewModel(string apprenticeUrl)
        {
            ApprenticeUrl = apprenticeUrl;
        }

        public string ApprenticeUrl { get; }
    }

}