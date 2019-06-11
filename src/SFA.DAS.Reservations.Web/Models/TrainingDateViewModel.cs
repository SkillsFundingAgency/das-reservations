using System;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Web.Models
{
    public class TrainingDateViewModel
    {
        public string Id { get; }
        public string SerializedModel { get; }
        public DateTime StartDate { get; }
        public string Checked { get; }

        public TrainingDateViewModel(TrainingDateModel model, bool isSelected = false)
        {
            Id = $"{model.StartDate:yyyy-MM}";
            SerializedModel = JsonConvert.SerializeObject(model);
            StartDate = model.StartDate;
            Checked = isSelected ? "checked" : null;
        }
    }
}