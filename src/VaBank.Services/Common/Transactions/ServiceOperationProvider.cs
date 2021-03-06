﻿using System;
using System.Security.Claims;
using System.Threading;
using NLog;
using VaBank.Core.App.Entities;
using VaBank.Core.App.Providers;
using VaBank.Core.App.Repositories;

namespace VaBank.Services.Common.Transactions
{
    public class ServiceOperationProvider : IOperationProvider
    {
        private readonly IOperationProvider _operationProvider;

        private readonly IOperationRepository _operationRepository;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private Operation _currentOperation = null;

        public ServiceOperationProvider(IOperationRepository operationRepository, IOperationProvider operationProvider)
        {
            if (operationRepository == null)
            {
                throw new ArgumentNullException("operationRepository");
            }
            if (operationProvider == null)
            {
                throw new ArgumentNullException("operationProvider");
            }
            _operationProvider = operationProvider;
            _operationRepository = operationRepository;
        }

        public bool HasCurrent
        {
            get { return _currentOperation != null; }
        }

        public Operation GetCurrent()
        {
            if (_currentOperation != null)
            {
                return _currentOperation;
            }
            var dbOperation = _operationProvider.GetCurrent();
            if (dbOperation != null)
            {
                _logger.Warn("Db operation started earlier than service operation. Application identity information will be lost.");
                _currentOperation = dbOperation;
                return _currentOperation;
            }
            var identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
            _currentOperation = _operationRepository.Start("APP-SERVICE", identity);
            return _currentOperation;
        }
    }
}
