﻿using SFA.DAS.EAS.Account.Api.Client;

namespace SFA.DAS.Reservations.Infrastructure.Configuration.Configuration
{
    public class AccountApiConfiguration : IAccountApiConfiguration
    {
        public string ApiBaseUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string IdentifierUri { get; set; }
        public string Tenant { get; set; }
    }
}