namespace SFA.DAS.Reservations.Web.Models
{
    public class ProviderStartViewModel
    {
        public bool IsFromManage { get; set; }
        public GlobalRuleViewModel ActiveGlobalRule { get; internal set; }
    }
}