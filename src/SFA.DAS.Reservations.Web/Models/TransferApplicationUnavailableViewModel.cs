namespace SFA.DAS.Reservations.Web.Models;

public class TransferApplicationUnavailableViewModel
{
    public string BackUrl { get; set; }
    public string StatusLabel { get; set; }

    public static TransferApplicationUnavailableViewModel Create(string status, string backUrl)
    {
        return new TransferApplicationUnavailableViewModel
        {
            BackUrl = backUrl,
            StatusLabel = status switch
            {
                "Pending" => "Awaiting approval",
                "Approved" => "Approved, awaiting your acceptance",
                "Rejected" => "Rejected",
                "FundsUsed" => "Funds used",
                "Declined" => "Withdrawn",
                "Withdrawn" => "Withdrawn",
                "WithdrawnAfterAcceptance" => "Withdrawn",
                "FundsExpired" => "Funds no longer available",
                _ => status
            }
        };
    }
}
