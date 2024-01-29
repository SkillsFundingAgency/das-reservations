using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerUrlHelper.DependencyResolution;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Employers.Api;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Infrastructure.Services;
using SFA.DAS.Reservations.Web.Services;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Infrastructure
{
    public static class TestServiceCollectionExtension
    {
        public static void ConfigureTestServiceCollection(this IServiceCollection serviceCollection, IConfigurationRoot configuration, TestData data)
        {
            var encodingService = new Mock<IEncodingService>();
            encodingService.Setup(x => x.Decode(TestDataValues.NonLevyHashedAccountId,It.IsAny<EncodingType>())).Returns(TestDataValues.NonLevyAccountId);
            var nonLevyOutVariable = TestDataValues.NonLevyAccountId;
            encodingService.Setup(x => x.TryDecode(TestDataValues.NonLevyHashedAccountId,It.IsAny<EncodingType>(), out nonLevyOutVariable)).Returns(true);
            encodingService.Setup(x => x.Decode(TestDataValues.NonLevyPublicHashedAccountId,EncodingType.PublicAccountId)).Returns(TestDataValues.NonLevyAccountId);
            encodingService.Setup(x => x.Encode(TestDataValues.NonLevyAccountId,It.IsAny<EncodingType>())).Returns(TestDataValues.NonLevyHashedAccountId);
            encodingService.Setup(x => x.Encode(TestDataValues.NonLevyAccountLegalEntityId, EncodingType.PublicAccountLegalEntityId)).Returns(TestDataValues.NonLevyHashedAccountLegalEntityId);
            encodingService.Setup(x => x.Decode(TestDataValues.NonLevyHashedAccountLegalEntityId, EncodingType.PublicAccountLegalEntityId)).Returns(TestDataValues.NonLevyAccountLegalEntityId);

            var levyOutVariable = TestDataValues.LevyAccountId;
            encodingService.Setup(x => x.Decode(TestDataValues.LevyPublicHashedAccountId,EncodingType.PublicAccountId)).Returns(TestDataValues.LevyAccountId);
            encodingService.Setup(x => x.TryDecode(TestDataValues.LevyHashedAccountId,It.IsAny<EncodingType>(), out levyOutVariable)).Returns(true);
            encodingService.Setup(x => x.Decode(TestDataValues.LevyHashedAccountId,It.IsAny<EncodingType>())).Returns(TestDataValues.LevyAccountId);
            encodingService.Setup(x => x.Encode(TestDataValues.LevyAccountId,It.IsAny<EncodingType>())).Returns(TestDataValues.LevyHashedAccountId);
            encodingService.Setup(x => x.Encode(TestDataValues.LevyAccountLegalEntityId, EncodingType.PublicAccountLegalEntityId)).Returns(TestDataValues.LevyHashedAccountLegalEntityId);
            encodingService.Setup(x => x.Decode(TestDataValues.LevyHashedAccountLegalEntityId, EncodingType.PublicAccountLegalEntityId)).Returns(TestDataValues.LevyAccountLegalEntityId);

            
            var apiClient = new Mock<IApiClient>();
            if (data != null)
            {
                apiClient.Setup(x => x.GetAll<AccountLegalEntity>(It.Is<GetAccountLegalEntitiesRequest>(c=>c.AccountId.Equals(TestDataValues.NonLevyAccountId))))
                    .ReturnsAsync(new List<AccountLegalEntity> {data.AccountLegalEntity});
                apiClient.Setup(x => x.GetAll<AccountLegalEntity>(It.Is<GetAccountLegalEntitiesRequest>(c=>c.AccountId.Equals(TestDataValues.LevyAccountId))))
                    .ReturnsAsync(new List<AccountLegalEntity> {data.AccountLegalEntity});    
            }
            
            var accountApiClient = new Mock<IAccountApiClient>();

            var reservationsService = new Mock<IReservationsOuterService>();

            var urlHelper = new Mock<IUrlHelper>();
            
            serviceCollection.AddSingleton(encodingService.Object);
            serviceCollection.AddSingleton(apiClient.Object);
            serviceCollection.AddSingleton<IEmployerAccountService, EmployerAccountService>();
            serviceCollection.AddSingleton(accountApiClient.Object);
            serviceCollection.AddSingleton(reservationsService.Object);
            serviceCollection.AddSingleton(urlHelper.Object);
            serviceCollection.AddSingleton<IUserClaimsService, UserClaimsService>();

            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.Configure<ReservationsApiConfiguration>(configuration.GetSection("ReservationsApi"));
            serviceCollection.AddSingleton(config => config.GetService<IOptions<ReservationsApiConfiguration>>().Value);
            serviceCollection.Configure<ReservationsWebConfiguration>(configuration.GetSection("ReservationsWeb"));
            serviceCollection.AddSingleton(config => config.GetService<IOptions<ReservationsWebConfiguration>>().Value);
            serviceCollection.Configure<IdentityServerConfiguration>(configuration.GetSection("Identity"));
            serviceCollection.AddSingleton(config => config.GetService<IOptions<IdentityServerConfiguration>>().Value);
            serviceCollection.Configure<ReservationsOuterApiConfiguration>(configuration.GetSection("ReservationsOuterApi"));
            serviceCollection.AddSingleton(config => config.GetService<IOptions<ReservationsOuterApiConfiguration>>().Value);


            var physicalProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
            serviceCollection.AddSingleton<IFileProvider>(physicalProvider);

            serviceCollection.AddEmployerUrlHelper(configuration);
        }
    }
}