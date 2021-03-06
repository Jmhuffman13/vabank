﻿using VaBank.Common.Data.Database;
using VaBank.Common.Events;
using VaBank.Common.IoC;
using VaBank.Core.App.Repositories;
using VaBank.Core.Common;
using VaBank.Services.Common.Transactions;

namespace VaBank.Services.Common
{
    public class BaseServiceDependencies : IDependencyCollection
    {
        public IUnitOfWork UnitOfWork { get; set; }

        public IObjectFactory ObjectFactory { get; set; }

        public ServiceOperationProvider OperationProvider { get; set; }

        public ISendOnlyServiceBus ServiceBus { get; set; }

        public ITransactionProvider TransactionProvider { get; set; }

        public ISettingRepository Settings { get; set; }
    }
}
