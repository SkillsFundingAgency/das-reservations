namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface IExternalUrlHelper
    {
        /// <summary>
        /// usage https://subDomain.baseUrl/folder/id/controller/action?queryString
        /// </summary>
        /// <param name="urlParameters"></param>
        /// <returns></returns>
        string GenerateUrl(UrlParameters urlParameters);

        /// <summary>
        /// usage https://subDomain.baseUrl/folder/id/controller/action?queryString
        /// </summary>
        /// <param name="urlParameters"></param>
        /// <returns></returns>
        string GenerateAddApprenticeUrl(UrlParameters urlParameters);
    }
}
