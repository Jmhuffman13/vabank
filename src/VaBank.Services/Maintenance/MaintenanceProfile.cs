﻿using AutoMapper;
using VaBank.Core.Accounting.Entities;
using VaBank.Core.App.Entities;
using VaBank.Core.Common;
using VaBank.Core.Maintenance.Entitities;
using VaBank.Core.Membership.Entities;
using VaBank.Core.Processing.Entities;
using VaBank.Services.Contracts.Accounting.Models;
using VaBank.Services.Contracts.Common.Models;
using VaBank.Services.Contracts.Maintenance.Commands;
using VaBank.Services.Contracts.Maintenance.Models;

namespace VaBank.Services.Maintenance
{
    internal class MaintenanceProfile : Profile
    {
        protected override void Configure()
        {
            //System Log
            CreateMap<SystemLogEntry, SystemLogEntryBriefModel>()
                .ForMember(dest => dest.EventId, src => src.MapFrom(x => x.Id));
            CreateMap<SystemLogEntry, SystemLogExceptionModel>()
                .ForMember(dest => dest.EventId, src => src.MapFrom(x => x.Id));
            CreateMap<SystemLogEntry, SystemLogTypeModel>();

            //Audit Log
            CreateMap<ApplicationAction, AppActionModel>()
                .ForMember(dest => dest.JsonData, src => src.MapFrom(x => x.Data));
            CreateMap<AuditLogBriefEntry, AuditLogEntryBriefModel>()
                .ForMember(dest => dest.OperationId, src => src.MapFrom(x => x.Operation.Id))
                .ForMember(dest => dest.UserId, src => src.MapFrom(x => x.Operation.UserId))
                .ForMember(dest => dest.UserName, src => src.MapFrom(x => x.Operation.User.UserName))
                .ForMember(dest => dest.ApplicationId, src => src.MapFrom(x => x.Operation.ClientApplicationId))
                .ForMember(dest => dest.StartedUtc, src => src.MapFrom(x => x.Operation.StartedUtc))
                .ForMember(dest => dest.AppActions, src => src.MapFrom(x => x.ApplicationActions));

            CreateMap<DatabaseOperation, DatabaseOperationModel>();
            CreateMap<VersionedDatabaseRow, DbChangeModel>()
                .ForMember(des => des.Operation, src => src.MapFrom(x => x.DatabaseOperation))
                .ForMember(des => des.Values, src => src.MapFrom(x => x.Values));
            CreateMap<DatabaseAction, DbActionModel>()
                .ForMember(dest => dest.Changes, src => src.MapFrom(x => x.Rows));
            CreateMap<AuditLogEntry, AuditLogEntryModel>()
                .ForMember(des => des.DbActions, src => src.MapFrom(x => x.DatabaseActions));

            CreateMap<LogAppActionCommand, ApplicationAction>();

            //Transaction Log
            CreateMap<ITransaction, TransactionLogEntryBriefModel>()
                .ForMember(x => x.TransactionId, cfg => cfg.MapFrom(x => x.Id))
                .ForMember(x => x.Status, cfg => cfg.MapFrom(x => (ProcessStatusModel)x.Status))
                .Include<Transaction, TransactionLogEntryBriefModel>()
                .Include<HistoricalTransaction, TransactionLogEntryHistoricalModel>();
            CreateMap<Transaction, TransactionLogEntryBriefModel>()
                .ForMember(x => x.CurrencyISOName, cfg => cfg.MapFrom(x => x.Currency.ISOName));
            CreateMap<HistoricalTransaction, TransactionLogEntryHistoricalModel>()
                .ForMember(x => x.TimestampUtc, cfg => cfg.MapFrom(x => x.HistoryTimestampUtc))
                .ForMember(x => x.ChangeOwnerUserId, cfg => cfg.MapFrom(x => x.HistoryOperation.UserId));

            CreateMap<Entity, IOwnerModel>()
                .Include<User, UserOwnerModel>()
                .Include<Bank, BankOwnerModel>();

            CreateMap<User, UserOwnerModel>()
                .ForMember(x => x.UserId, cfg => cfg.MapFrom(x => x.Id))
                .ForMember(x => x.FirstName, cfg => cfg.MapFrom(x => x.Profile.FirstName))
                .ForMember(x => x.LastName, cfg => cfg.MapFrom(x => x.Profile.LastName))
                .ForMember(x => x.Email, cfg => cfg.MapFrom(x => x.Profile.Email));
            CreateMap<Bank, BankOwnerModel>();

            CreateMap<Account, AccountBriefModel>()
                .Include<CardAccount, AccountBriefModel>()
                .Include<CorrespondentAccount, AccountBriefModel>();
            CreateMap<CardAccount, AccountBriefModel>()
                .ForMember(x => x.Owner, cfg => cfg.MapFrom(x => x.Owner));
            CreateMap<CorrespondentAccount, AccountBriefModel>()
                .ForMember(x => x.Owner, cfg => cfg.MapFrom(x => x.Bank));
        }
    }
}
