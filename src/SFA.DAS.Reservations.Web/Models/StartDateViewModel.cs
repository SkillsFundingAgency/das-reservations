using System;
using Newtonsoft.Json;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Web.Models
{
    public class StartDateViewModel
    {
        public string Id { get; }
        public string Value { get; }
        public string Label { get; }
        public string Checked { get; }

        public StartDateViewModel(StartDateModel model, string startDate = null)
        {
            Id = $"{model.StartDate:yyyy-MM}";
            Value = JsonConvert.SerializeObject(model);
            Label = $"{model.StartDate:MMMM yyyy}";
            Checked = Id.Equals(startDate, StringComparison.InvariantCulture)
                ? "checked"
                : null;
        }
    }
}