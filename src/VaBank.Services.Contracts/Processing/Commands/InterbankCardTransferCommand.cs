﻿using System;
using VaBank.Services.Contracts.Common.Commands;
using VaBank.Services.Contracts.Common.Models;

namespace VaBank.Services.Contracts.Processing.Commands
{
    public class InterbankCardTransferCommand : ISecurityCodeCommand
    {
        public Guid FromCardId { get; set; }

        public string ToCardNo { get; set; }

        public DateTime ToCardExpirationDateUtc { get; set; }

        public SecurityCodeModel SecurityCode { get; set; }
    }
}