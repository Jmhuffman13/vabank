﻿using System;
using VaBank.Common.Data;
using VaBank.Common.Data.Repositories;
using VaBank.Core.Accounting.Entities;
using VaBank.Core.Common;
using VaBank.Core.Processing.Entities;
using VaBank.Services.Common;
using VaBank.Services.Common.Exceptions;
using VaBank.Services.Contracts.Common;
using VaBank.Services.Contracts.Common.Models;
using VaBank.Services.Contracts.Processing.Events;
using VaBank.Services.Contracts.Processing.Models;
using VaBank.Services.Contracts.Transfers;
using VaBank.Services.Contracts.Transfers.Commands;
using VaBank.Services.Contracts.Transfers.Models;
using VaBank.Services.Processing;

namespace VaBank.Services.Transfers
{
    public class CardTransferClientService : BaseService, ICardTransferClientService
    {
        private readonly CardTransferClientServiceDependencies _deps;

        public CardTransferClientService(BaseServiceDependencies baseDeps, CardTransferClientServiceDependencies serviceDeps)
            : base(baseDeps)
        {
            serviceDeps.EnsureIsResolved();
            _deps = serviceDeps;
        }

        public BankOperationModel Transfer(PersonalCardTransferCommand command)
        {
            EnsureIsValid(command);
            EnsureIsSecure<PersonalCardTransferCommand, CardSecurityValidator>(command);
            try
            {
                var fromCard = _deps.UserCards.SurelyFind(command.FromCardId);
                var toCard = _deps.UserCards.SurelyFind(command.ToCardId);
                var transfer = _deps.CardTransferFactory.Create(fromCard, toCard, command.Amount);
                _deps.CardTransfers.Create(transfer);
                var userOperation = new UserBankOperation(transfer, Identity.User);
                _deps.UserBankOperations.Create(userOperation);
                Commit();
                var model = transfer.ToModel<BankOperation, BankOperationModel>();
                Publish(new OperationProgressEvent(Operation.Id, model));
                return model;
            }
            catch (DomainException ex)
            {
                throw new UserMessageException(new UserMessage(ex.Message,  ex.GetType().Name));
            }
            catch (Exception ex)
            {
                throw new ServiceException("Can't create transfer.", ex);
            }
        }

        public BankOperationModel Transfer(InterbankCardTransferCommand command)
        {
            EnsureIsValid(command);
            EnsureIsSecure<InterbankCardTransferCommand, CardSecurityValidator>(command);
            try
            {
                var fromCard = _deps.UserCards.SurelyFind(command.FromCardId);                
                var toCard = _deps.UserCards.QueryOne(DbQuery.For<UserCard>().FilterBy(x => x.CardNo == command.ToCardNo));
                if (toCard == null)
                {
                    throw NotFound.ExceptionFor<UserCard>(command.ToCardNo);
                }
                var transfer = _deps.CardTransferFactory.Create(fromCard, toCard, command.Amount);
                _deps.CardTransfers.Create(transfer);
                var userOperation = new UserBankOperation(transfer, Identity.User);
                _deps.UserBankOperations.Create(userOperation);
                Commit();
                var model = transfer.ToModel<BankOperation, BankOperationModel>();
                Publish(new OperationProgressEvent(Operation.Id, model));
                return model;
            }
            catch (DomainException ex)
            {
                throw new UserMessageException(new UserMessage(ex.Message, ex.GetType().Name));
            }
            catch (Exception ex)
            {
                throw new ServiceException("Can't create transfer.", ex);
            }
        }

        public CardTransferLookupModel GetLookup()
        {
            try
            {
                var lookup = new CardTransferLookupModel
                {
                    MinimalAmountsByCurrency = _deps.CardTransferSettings.MinimalAmounts
                };
                return lookup;
            }
            catch (Exception ex)
            {
                throw new ServiceException("Can't get card transfer lookup.", ex);
            }
        }
    }
}
