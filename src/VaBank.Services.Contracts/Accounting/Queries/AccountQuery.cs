﻿using VaBank.Common.Data.Filtering;
using VaBank.Common.Data.Paging;
using VaBank.Common.Data.Sorting;

namespace VaBank.Services.Contracts.Accounting.Queries
{
    public class AccountQuery : IClientFilterable, IClientPageable
    {
        public AccountQuery()
        {
            ClientFilter = new AlwaysTrueFilter();
            ClientSort = new DynamicLinqSort("OpenDateUtc DESC");
            ClientPage = new ClientPage {PageNumber = 1, PageSize = 10};
        }

        public IFilter ClientFilter { get; set; }
        public ISort ClientSort { get; set; }
        public ClientPage ClientPage { get; set; }
    }
}
