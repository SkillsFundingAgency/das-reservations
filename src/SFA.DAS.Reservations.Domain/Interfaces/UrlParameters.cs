namespace SFA.DAS.Reservations.Domain.Interfaces;

public class UrlParameters
{
    public string Id { get; set; }
    public string Controller { get; set; }
    public string Action { get; set; }
    public string SubDomain { get; set; }
    public string Folder { get; set; }
    public string QueryString { get; set; }
    public string RelativeRoute { get; set; }
}