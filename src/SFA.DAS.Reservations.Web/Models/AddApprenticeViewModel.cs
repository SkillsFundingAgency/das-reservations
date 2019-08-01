using System;
using SFA.DAS.Reservations.Domain.Courses;

namespace SFA.DAS.Reservations.Web.Models
{
    public abstract class AddApprenticeViewModel
    {
        public AddApprenticeViewModel(string apprenticeUrl)
        {
            ApprenticeUrl = apprenticeUrl;
        }

        public string ApprenticeUrl { get; }
    }

}