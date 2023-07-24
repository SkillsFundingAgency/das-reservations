using SFA.DAS.DfESignIn.Auth.Enums;
using SFA.DAS.DfESignIn.Auth.Interfaces;
using SFA.DAS.Reservations.Web.Infrastructure;

namespace SFA.DAS.Reservations.Web.AppStart;

public class CustomServiceRole : ICustomServiceRole
{
    public string RoleClaimType => ProviderClaims.Service;

    // <inherit-doc/>
    public CustomServiceRoleValueType RoleValueType => CustomServiceRoleValueType.Code;
}