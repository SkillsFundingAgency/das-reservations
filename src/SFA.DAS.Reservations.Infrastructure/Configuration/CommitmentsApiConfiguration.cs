using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Reservations.Infrastructure.Configuration
{
    public class CommitmentsApiConfiguration
    {
        public string Tenant { get; set; }
        public string IdentifierUri { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ApiBaseUrl { get; set; }
    }
}
