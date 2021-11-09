using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.Extensions
{
    public static class AccountLegalEntityExtension
    {
        public static List<AccountLegalEntity> Order(this List<AccountLegalEntity> accountLegalEntities, SortModel sortModel)
        {
            if (string.IsNullOrWhiteSpace(sortModel.SortField)) return accountLegalEntities;

            if (sortModel.SortField == nameof(AccountLegalEntity.AccountLegalEntityName))
            {
                accountLegalEntities = (sortModel.ReverseSort
                    ? accountLegalEntities.OrderByDescending(ale => ale.AccountLegalEntityName)
                        .ThenBy(ale => ale.AccountName)
                        .ThenBy(ale => ale.AccountLegalEntityPublicHashedId)
                    : accountLegalEntities.OrderBy(ale => ale.AccountLegalEntityName)
                        .ThenBy(ale => ale.AccountName)
                        .ThenBy(ale => ale.AccountLegalEntityPublicHashedId)).ToList();
            }
            else
            {
                accountLegalEntities = (sortModel.ReverseSort
                    ? accountLegalEntities.OrderByDescending(ale => ale.AccountName)
                        .ThenBy(ale => ale.AccountLegalEntityName)
                        .ThenBy(ale => ale.AccountLegalEntityPublicHashedId)
                    : accountLegalEntities.OrderBy(ale => ale.AccountName)
                        .ThenBy(ale => ale.AccountLegalEntityName)
                        .ThenBy(ale => ale.AccountLegalEntityPublicHashedId)).ToList();
            }

            return accountLegalEntities;
        }
    }
}
