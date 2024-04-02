﻿namespace SFA.DAS.Reservations.Web.Infrastructure;

public static class ProviderClaims
{
    public static string ProviderUkprn => "http://schemas.portal.com/ukprn";
    public static string DisplayName => "http://schemas.portal.com/displayname";
    public static string Service => "http://schemas.portal.com/service";
    public static string AssociatedAccountsClaimsTypeIdentifier => "http://das/provider/identity/claims/associatedAccounts";
}