# das-reservations

## Build Status

![Build Status](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_apis/build/status/Manage%20Funding/das-reservations?branchName=master)

## Sonar Cloud Status

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SkillsFundingAgency_das-reservations&metric=alert_status)](https://sonarcloud.io/dashboard?id=SkillsFundingAgency_das-reservations)

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
- (This step can be skipped if you have loaded the [config](https://github.com/SkillsFundingAgency/das-employer-config) using [config loader tool](https://github.com/SkillsFundingAgency/das-employer-config-updater).) Create table in Azure Storage called Configuration, and add an extra column called **Data**. Set partitionkey to `LOCAL`, Rowkey to `SFA.DAS.Reservations.Web_1.0`, then for the Data column add the following:
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

### Using API stubs

Reservations Web depends on several other API's. To facilitate local development all but the reservations API have been stubbed. In order to use the stubs you will need to ensure that `appSettings.json` contains the setting `"UseStub": true`. Also the stub has been configured to use a single employer account ID of 123. But, you will need to use the hashed version of this.

So, you'll also need an account legal entity to match with the account id: 

If `"UseStub": true` then the starting url will be https://localhost:5001/accounts/WM6XRM/reservations (If reservation.web is running on port 5001)

```
use [SFA.DAS.Reservations]

insert into AccountLegalEntity
(Id, AccountId, LegalEntityId, AccountLegalEntityId, AccountLegalEntityName, ReservationLimit, AgreementSigned, IsLevy, AgreementType)
values
(newid(), 123, 456, 789, 'stubs r us', null, 1, 1, 0)

Insert into [SFA.DAS.Reservations].[dbo].[Account] (Id, Name, IsLevy, ReservationLimit)
Values (123, 'Account1', 0,10)
```

Likewise you if you are running in provider mode you will need to ensure you have db records that match values provided in provider permissions api stub, i.e. something like this: 

```
use [SFA.DAS.Reservations]

insert into AccountLegalEntity
(Id, AccountId, LegalEntityId, AccountLegalEntityId, AccountLegalEntityName, ReservationLimit, AgreementSigned, IsLevy, AgreementType)
values
(newid(), 1, 1, 123, 'Legal Entity 2', null, 1, 1, 0),
(newid(), 1, 2, 456, 'Legal Entity 2', null, 1, 1, 0),
(newid(), 1, 3, 789, 'Legal Entity 3', null, 1, 1, 0)
```



## Authorization

The API uses AzureAD for authentication. When running in DEV or LOCAL modes, the authentication attribute is not added. If you do enable authentication you will need to add the ```Authorization Bearer [TOKEN]``` header attribute to all requests. 

#### Note:
To run it locally - I had to replace method GetAccessTokenAsync of class ApiClient with one that returns an empty string. 
