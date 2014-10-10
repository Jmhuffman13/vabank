﻿using System;
using FluentValidation;
using VaBank.Services.Common.Validation;
using VaBank.Services.Contracts.Membership;
using VaBank.Services.Contracts.Membership.Commands;

namespace VaBank.Services.Membership
{
    [StaticValidator]
    internal class LoginCommandValidator : AbstractValidator<LoginCommand>
    {        
        public LoginCommandValidator()
        {
            RuleFor(x => x.Login).UseValidator(new LoginValidator());
            RuleFor(x => x.Password).NotEmpty().Length(6, 256);
        }
    }

    [StaticValidator]
    internal class CreateTokenCommandValidator : AbstractValidator<CreateTokenCommand>
    {
        public CreateTokenCommandValidator()
        {
            RuleFor(x => x.ClientId).NotEmpty().Length(1, 256);
            RuleFor(x => x.Id).NotEmpty().Length(1, 256);
            RuleFor(x => x.ExpiresUtc).Must((command, expireUtc) => expireUtc >= command.IssuedUtc)
                .WithLocalizedMessage(() => "ExpreUtc value can't be less than IssuedUtc value.");
            RuleFor(x => x.ProtectedTicket).NotEmpty().Length(1, 512);
            RuleFor(x => x.UserId).Must(x => x != Guid.Empty);
        }
    }
}
