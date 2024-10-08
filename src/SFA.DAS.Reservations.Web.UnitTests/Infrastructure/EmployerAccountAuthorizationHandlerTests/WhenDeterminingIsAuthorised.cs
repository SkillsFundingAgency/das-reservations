﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Authentication;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Reservations.Web.Infrastructure;
using SFA.DAS.Reservations.Web.Infrastructure.Authorization;
using SFA.DAS.Reservations.Web.UnitTests.Customisations;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Infrastructure.EmployerAccountAuthorizationHandlerTests
{
    public class WhenDeterminingIsAuthorised
    {
        [Test, MoqAutoData]
        public void ThenReturnsTrueIfEmployerIsAuthorised(
            EmployerAccountRequirement requirement,
            DefaultHttpContext contextFilter ,
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
            var filter = context.Resource as DefaultHttpContext;
            filter.Request.RouteValues.Add(RouteValues.EmployerAccountId, 1234);

            //Act
            var result = handler.IsEmployerAuthorised(context, false);

            //Assert
            Assert.IsTrue(result);
        }

        
        [Test, MoqAutoData]
        public void ThenReturnsFalseIfEmployerIdIsNotInUrl(
            EmployerAccountRequirement requirement,
            [ArrangeAuthorizationFilterContext] AuthorizationFilterContext contextFilter ,
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
            var result = handler.IsEmployerAuthorised(context, false);

            //Assert
            Assert.IsFalse(result);
        }

        [Test, MoqAutoData]
        public void ThenReturnsFalseIfEmployerClaimNotFound(
            EmployerAccountRequirement requirement,
            [ArrangeAuthorizationFilterContext] AuthorizationFilterContext contextFilter ,
            EmployerAccountAuthorizationHandler handler)
        {
            //Assign
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new Claim[0])});

            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, contextFilter);
            var filter = context.Resource as AuthorizationFilterContext;
            filter.RouteData.Values.Add(RouteValues.EmployerAccountId, 1234);

            //Act
            var result = handler.IsEmployerAuthorised(context, false);

            //Assert
            Assert.IsFalse(result);
        }

        [Test, MoqAutoData]
        public void ThenReturnsFalseIfEmployerClaimIsNotValid(
            EmployerAccountRequirement requirement,
            [ArrangeAuthorizationFilterContext] AuthorizationFilterContext contextFilter ,
            EmployerAccountAuthorizationHandler handler)
        {
            //Assign
            var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, "invalid");
            var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { claim }) });

            var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrinciple, contextFilter);
            var filter = context.Resource as AuthorizationFilterContext;
            filter.RouteData.Values.Add(RouteValues.EmployerAccountId, 1234);

            //Act
            var result = handler.IsEmployerAuthorised(context, false);

            //Assert
            Assert.IsFalse(result);
        }

        [Test, MoqAutoData]
        public void ThenReturnsFalseIfUserDoesNotHaveCorrectRole(
            EmployerAccountRequirement requirement,
            [ArrangeAuthorizationFilterContext] AuthorizationFilterContext contextFilter ,
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
            var result = handler.IsEmployerAuthorised(context, false);

            //Assert
            Assert.IsFalse(result);
        }

        [Test, MoqAutoData]
        public void ThenReturnsFalseIfEmployerAccountIdNotFoundAndUserIdNotFound(
            EmployerAccountRequirement requirement,
            [ArrangeAuthorizationFilterContext] AuthorizationFilterContext contextFilter ,
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
            var result = handler.IsEmployerAuthorised(context, false);

            //Assert
            Assert.IsFalse(result);
        }

        [Test, MoqAutoData]
        public void ThenFailsIfUserDoesNotHaveAValidRole(
            EmployerAccountRequirement requirement,
            [ArrangeAuthorizationFilterContext] AuthorizationFilterContext contextFilter ,
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
            var result = handler.IsEmployerAuthorised(context, false);

            //Assert
            Assert.IsFalse(result);
        }

        [Test, MoqAutoData]
        public void ThenFailsIfEmployerAccountIdNotFoundEvenAfterAccountIdRefresh(
            [Frozen] Mock<IEmployerAccountService> employerAccountService, 
            EmployerAccountRequirement requirement,
            [ArrangeAuthorizationFilterContext] AuthorizationFilterContext contextFilter,
            EmployerAccountAuthorizationHandler handler)
        {
           // AuthorizationFilterContext contextFilter = null;
            //Assign
            var employerAccounts = new Dictionary<string, EmployerIdentifier>();
            var employerAccountClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));

            var userId = Guid.NewGuid().ToString();
            var userClaim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, userId);
            
            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {employerAccountClaim, userClaim})});

            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, contextFilter);
            var filter = context.Resource as AuthorizationFilterContext;
            filter.RouteData.Values.Add(RouteValues.EmployerAccountId, 1234);

            employerAccountService.Setup(s => s.GetClaim(It.IsAny<string>(), It.IsAny<string>(),It.IsAny<string>()))
                .ReturnsAsync(new List<Claim>{employerAccountClaim});

            //Act
            var result = handler.IsEmployerAuthorised(context, false);

            //Assert
            Assert.IsFalse(result);
        }

        [Test, MoqAutoData]
        public void ThenSucceedsIfEmployerAccountIdIsFoundAfterAccountIdRefreshForGovSignIn(
            string email,
            [Frozen] Mock<IEmployerAccountService> employerAccountService, 
            [Frozen] Mock<IOptions<ReservationsWebConfiguration>> configuration, 
            EmployerAccountRequirement requirement,
             DefaultHttpContext contextFilter,
            EmployerAccountAuthorizationHandler handler)
        {
            //Assign
            var employerAccounts = new Dictionary<string, EmployerIdentifier>();
            var employerAccountClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));

            var userId = Guid.NewGuid().ToString();
            var userClaim = new Claim(ClaimTypes.NameIdentifier, userId);
            var emailClaim = new Claim(ClaimTypes.Email, email);

            var claimsPrinciple = new ClaimsPrincipal(new[] {new ClaimsIdentity(new[] {employerAccountClaim, userClaim, emailClaim})});

            var context = new AuthorizationHandlerContext(new[] {requirement}, claimsPrinciple, contextFilter);
            var filter = context.Resource as DefaultHttpContext;
            filter.Request.RouteValues.Add(RouteValues.EmployerAccountId, 1234);


            var employerIdentifier = new EmployerIdentifier
            {
                AccountId = "1234", 
                EmployerName = "Test Corp", 
                Role = "Owner"
            };
            var refreshedEmployerAccounts = new Dictionary<string, EmployerIdentifier>{{"1234", employerIdentifier}};
            var refreshedEmployerAccountClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(refreshedEmployerAccounts));
            
            employerAccountService.Setup(s => s.GetClaim(userId, It.IsAny<string>(),email))
                .ReturnsAsync(new List<Claim>{refreshedEmployerAccountClaim});

            //Act
            var result = handler.IsEmployerAuthorised(context, false);

            //Assert
            Assert.IsTrue(result);
        }
    }
}
