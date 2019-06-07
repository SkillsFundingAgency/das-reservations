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

        public StartDateViewModel(StartDateModel model, bool isSelected = false)
        {
            Id = $"{model.StartDate:yyyy-MM}";
            Value = JsonConvert.SerializeObject(model);
            Label = $"{model.StartDate:MMMM yyyy}";
            Checked = isSelected ? "checked" : null;
        }
    }
}