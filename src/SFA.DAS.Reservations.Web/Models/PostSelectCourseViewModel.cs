using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Web.Models
{
    public class PostSelectCourseViewModel
    {
        public string SelectedCourseId { get; set; }
        public bool? ApprenticeTrainingKnown { get; set; }
    }
}
