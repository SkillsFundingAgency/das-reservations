# das-reservations

## Build Status

![Build Status](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_apis/build/status/Manage%20Funding/das-reservations?branchName=master)

## Requirements

DotNet Core 2.2 and any supported IDE for DEV running.

## About

The reservations website is responsible for creating and viewing reservations at an account level. The reservation guarantees you to an apprenticeship starting in a defined start and end period, using any restrictions that are currently in place. The website has two separate journeys, one for employers and one for providers who can create commitments for a given set of employers.

## Local running

You are able to run the website by doing the following:

### Clone and deploy DAS Reservation API to Local environment

- Clone das-reservation-api repository (https://github.com/SkillsFundingAgency/das-reservations-api)
- Follow the readme for das-reservations-api and get it up and running locally

### Clone and deploy DAS Reservations Website

- Set **Environment** to **LOCAL** which will require a SQL instance and also Azure Storage to run in this mode
- Clone repository
- Create table in Azure Storage called Configuration, and add an extra column called **Data**. Set partitionkey to `LOCAL`, Rowkey to `SFA.DAS.Reservations.Web_1.0`, then for the Data column add the following:
```
{
  "ReservationsWeb": {
    "EmployerAccountHashSalt": "EmployerAccountHashSalt",
    "EmployerAccountHashLength": 6,
    "EmployerAccountHashAlphabet": "EmployerAccountHashAlphabet",
    "sessionTimeoutHours": 1,
    "DashboardUrl": "DashboardUrl",
    "ApprenticeUrl": "ApprenticeUrl",
    "EmployerDashboardUrl": "EmployerDashboardUrl"
  },
  "ReservationsApi": {
    "url": "url",
    "id": "id",
    "secret": "secret",
    "identifier": "identifiert",
    "tenant": "tenant"
  },
  "Identity": {
    "AccountActivationUrl": "AccountActivationUrl",
    "AuthorizeEndpoint": "AuthorizeEndpoint",
    "BaseAddress": "BaseAddress",
    "ChangeEmailUrl": "ChangeEmailUrl",
    "ChangePasswordUrl": "ChangePasswordUrl",
    "ClientId": "ClientId",
    "ClientSecret": "ClientSecret",
    "LogoutEndpoint": "LogoutEndpoint",
    "Scopes": "Scopes",
    "TokenCertificateThumbprint": "TokenCertificateThumbprint",
    "TokenEndpoint": "TokenEndpoint",
    "UseCertificate": true,
    "UserInfoEndpoint": "UserInfoEndpoint"
  },
  "ProviderIdams": {
    "MetadataAddress": "MetadataAddress",
    "Wtrealm": "Wtrealm"
  }
}
```
 *if you are running on an environment other than LOCAL or DEV you will need to supply configuration for AzureAd* 


### Selecting a Employer/Provider Authenication type

To select which authenication system the web project uses you will need to set the  **AuthType** setting in appsetting.json in the web project to either ```Employer``` or ```Provider```

## Authorization

The API uses AzureAD for authentication. When running in DEV or LOCAL modes, the authentication attribute is not added. If you do enable authentication you will need to add the ```Authorization Bearer [TOKEN]``` header attribute to all requests. 

