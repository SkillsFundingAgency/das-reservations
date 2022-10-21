﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Authentication;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.UnitTests.Customisations;

namespace SFA.DAS.Reservations.Web.UnitTests.Infrastructure.EmployerAccountAuthorizationHandlerTests
{
    public class WhenHandlingRequest
    {
        [Test, DomainAutoData]
        public async Task ThenSucceedsIfEmployerIsAuthorised(
            EmployerAccountRequirement requirement,
            AuthorizationFilterContext contextFilter,
            EmployerAccountAuthorizationHandler handler)
        {
            //Assign
            var employerIdentifier = new EmployerIdentifier
            {
                AccountId = "1234", 
                EmployerName = "Test Employer", 
                Role = "Owner"
            };

            var employerAccounts = new Dictionary<string, EmployerIdentifier>{{"1234", employerIdentifier}};
            var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});

            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, contextFilter);
            var filter = context.Resource as AuthorizationFilterContext;
            filter.RouteData.Values.Add(RouteValues.EmployerAccountId, 1234);

            //Act
            await handler.HandleAsync(context);

            //Assert
            Assert.IsTrue(context.HasSucceeded);
        }

        [Test, DomainAutoData]
        public async Task ThenFailsIfEmployerIdIsNotInUrl(
            EmployerAccountRequirement requirement,
            AuthorizationFilterContext contextFilter,
            EmployerAccountAuthorizationHandler handler)
        {
            //Assign
            var employerIdentifier = new EmployerIdentifier
            {
                AccountId = "1234", 
                EmployerName = "Test Employer", 
                Role = "Owner"
            };

            var employerAccounts = new Dictionary<string, EmployerIdentifier>{{"1234", employerIdentifier}};
            var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});

            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, contextFilter);

            //Act
            await handler.HandleAsync(context);

            //Assert
            Assert.IsFalse(context.HasSucceeded);
        }

        [Test, DomainAutoData]
        public async Task ThenFailsIfEmployerClaimNotFound(
            EmployerAccountRequirement requirement,
            AuthorizationFilterContext contextFilter,
            EmployerAccountAuthorizationHandler handler)
        {
            //Assign
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new Claim[0])});

            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, contextFilter);
            var filter = context.Resource as AuthorizationFilterContext;
            filter.RouteData.Values.Add(RouteValues.EmployerAccountId, 1234);

            //Act
            await handler.HandleAsync(context);

            //Assert
            Assert.IsFalse(context.HasSucceeded);
        }

        [Test, DomainAutoData]
        public async Task ThenFailsIfEmployerClaimIsNotValid(
            EmployerAccountRequirement requirement,
            AuthorizationFilterContext contextFilter,
            EmployerAccountAuthorizationHandler handler)
        {
            //Assign
            var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, "invalid");
            var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { claim }) });

            var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrinciple, contextFilter);
            var filter = context.Resource as AuthorizationFilterContext;
            filter.RouteData.Values.Add(RouteValues.EmployerAccountId, 1234);

            //Act
            await handler.HandleAsync(context);

            //Assert
            Assert.IsFalse(context.HasSucceeded);
        }

        [Test, DomainAutoData]
        public async Task ThenFailsIfUserDoesNotHaveCorrectRole(
            EmployerAccountRequirement requirement,
            AuthorizationFilterContext contextFilter,
            EmployerAccountAuthorizationHandler handler)
        {
            //Assign
            var employerIdentifier = new EmployerIdentifier
            {
                AccountId = "1234", 
                EmployerName = "Test Employer", 
                Role = "Viewer"
            };

            var employerAccounts = new Dictionary<string, EmployerIdentifier>{{"1234", employerIdentifier}};
            var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});

            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, contextFilter);
            var filter = context.Resource as AuthorizationFilterContext;
            filter.RouteData.Values.Add(RouteValues.EmployerAccountId, 1234);

            //Act
            await handler.HandleAsync(context);

            //Assert
            Assert.IsFalse(context.HasSucceeded);
        }

        [Test, DomainAutoData]
        public async Task ThenFailsIfEmployerAccountIdNotFoundAndUserIdNotFound(
            EmployerAccountRequirement requirement,
            AuthorizationFilterContext contextFilter,
            EmployerAccountAuthorizationHandler handler)
        {
            //Assign
            var employerAccounts = new Dictionary<string, EmployerIdentifier>();
            var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});

            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, contextFilter);
            var filter = context.Resource as AuthorizationFilterContext;
            filter.RouteData.Values.Add(RouteValues.EmployerAccountId, 1234);

            //Act
            await handler.HandleAsync(context);

            //Assert
            Assert.IsFalse(context.HasSucceeded);
        }

        [Test, DomainAutoData]
        public async Task ThenFailsIfUserDoesNotHaveAValidRole(
            EmployerAccountRequirement requirement,
            AuthorizationFilterContext contextFilter,
            EmployerAccountAuthorizationHandler handler)
        {
            //Assign
            var employerIdentifier = new EmployerIdentifier
            {
                AccountId = "1234", 
                EmployerName = "Test Employer", 
                Role = "I'm not a role"
            };

            var employerAccounts = new Dictionary<string, EmployerIdentifier>{{"1234", employerIdentifier}};
            var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {claim})});

            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, contextFilter);
            var filter = context.Resource as AuthorizationFilterContext;
            filter.RouteData.Values.Add(RouteValues.EmployerAccountId, 1234);

            //Act
            await handler.HandleAsync(context);

            //Assert
            Assert.IsFalse(context.HasSucceeded);
        }

        [Test, DomainAutoData]
        public async Task ThenFailsIfEmployerAccountIdNotFoundEvenAfterAccountIdRefresh(
            [Frozen] Mock<IEmployerAccountService> employerAccountService, 
            EmployerAccountRequirement requirement,
            AuthorizationFilterContext contextFilter,
            EmployerAccountAuthorizationHandler handler)
        {
            //Assign
            var employerAccounts = new Dictionary<string, EmployerIdentifier>();
            var employerAccountClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));

            var userId = Guid.NewGuid().ToString();
            var userClaim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, userId);
            
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {employerAccountClaim, userClaim})});

            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, contextFilter);
            var filter = context.Resource as AuthorizationFilterContext;
            filter.RouteData.Values.Add(RouteValues.EmployerAccountId, 1234);

            employerAccountService.Setup(s => s.GetClaim(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(employerAccountClaim);

            //Act
            await handler.HandleAsync(context);

            //Assert
            Assert.IsFalse(context.HasSucceeded);
        }

        [Test, DomainAutoData]
        public async Task ThenSucceedsIfEmployerAccountIdIsFoundAfterAccountIdRefresh(
            [Frozen] Mock<IEmployerAccountService> employerAccountService, 
            EmployerAccountRequirement requirement,
            AuthorizationFilterContext contextFilter,
            EmployerAccountAuthorizationHandler handler)
        {
            //Assign
            var employerAccounts = new Dictionary<string, EmployerIdentifier>();
            var employerAccountClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));

            var userId = Guid.NewGuid().ToString();
            var userClaim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, userId);
            
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {employerAccountClaim, userClaim})});

            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, contextFilter);
            var filter = context.Resource as AuthorizationFilterContext;
            filter.RouteData.Values.Add(RouteValues.EmployerAccountId, 1234);


            var employerIdentifier = new EmployerIdentifier
            {
                AccountId = "1234", 
                EmployerName = "Test Corp", 
                Role = "Owner"
            };
            var refreshedEmployerAccounts = new Dictionary<string, EmployerIdentifier>{{"1234", employerIdentifier}};
            var refreshedEmployerAccountClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(refreshedEmployerAccounts));
            
            employerAccountService.Setup(s => s.GetClaim(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(refreshedEmployerAccountClaim);

            //Act
            await handler.HandleAsync(context);

            //Assert
            Assert.IsTrue(context.HasSucceeded);
        }
    }
}
