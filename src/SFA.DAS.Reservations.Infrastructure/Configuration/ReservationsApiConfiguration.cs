﻿namespace SFA.DAS.Reservations.Infrastructure.Configuration
{
    public class ReservationsApiConfiguration
    {
        public virtual string Url { get; set; }
        public string Id { get; set; }
        public string Secret { get; set; }
        public string Identifier { get; set; }
        public string Tenant { get; set; }
    }
}