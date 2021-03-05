﻿using Microsoft.AspNetCore.Authorization;

namespace SFA.DAS.Reservations.Web.Infrastructure
{
    public class MinimumServiceClaimRequirement : IAuthorizationRequirement
    {
        public ServiceClaim MinimumServieClaim { get; set; }
        public MinimumServiceClaimRequirement(ServiceClaim minimumServieClaim)
        {
            MinimumServieClaim = minimumServieClaim;
        }
    }
}
