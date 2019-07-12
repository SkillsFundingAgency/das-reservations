using System;
using System.Collections.Generic;
using System.Text;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Domain.Employers.Api
{
    public class GetEmployerTransferConnectionsRequest : IGetAllApiRequest
    {
        public string HashedAccountId { get; }
        public string BaseUrl { get; }
        public string GetAllUrl => $"{BaseUrl}api/accounts/{HashedAccountId}/transfers/connections";


        public GetEmployerTransferConnectionsRequest(string baseUrl, string hashedAccountId)
        {
            HashedAccountId = hashedAccountId;
            BaseUrl = baseUrl;
        }
    }
}
