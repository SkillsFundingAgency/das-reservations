﻿namespace SFA.DAS.Reservations.Infrastructure.Configuration.Configuration
{
    public class IdentityServerConfiguration
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string BaseAddress { get; set; }
        public string ResponseType { get; set; }
        public bool SaveTokens { get; set; }
        public string Scopes { get; set; }
        public string ChangeEmailLink { get; set; }
        public string ChangePasswordLink { get; set; }
        public ClaimIdentifierConfiguration ClaimIdentifierConfiguration { get; set; }
    }
}