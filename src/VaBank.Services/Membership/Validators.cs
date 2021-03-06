﻿using System;
using FluentValidation;
using FluentValidation.Validators;
using VaBank.Common.Data;
using VaBank.Common.Data.Repositories;
using VaBank.Common.Validation;
using VaBank.Core.Membership;
using VaBank.Core.Membership.Entities;
using VaBank.Services.Contracts.Membership.Commands;
using VaBank.Services.Contracts.Membership.Queries;

namespace VaBank.Services.Membership
{


    [StaticValidator]
    internal class LoginCommandValidator : AbstractValidator<LoginCommand>
    {        
        public LoginCommandValidator()
        {
            RuleFor(x => x.Login).NotEmpty().WithLocalizedMessage(() => Messages.NotEmptyLogin);
            RuleFor(x => x.Password).NotEmpty().WithLocalizedMessage(() =>Messages.NotEmptyPassword);
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
                .WithLocalizedMessage(() => "ExpireUtc value can't be less than IssuedUtc value.");
            RuleFor(x => x.ProtectedTicket).NotEmpty().Length(1, 512);
            RuleFor(x => x.UserId).Must(x => x != Guid.Empty);
        }
    }

    internal class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        private readonly IQueryRepository<User> _userRepository; 
        public CreateUserCommandValidator(IQueryRepository<User> userRepository)
        {
            if (userRepository == null)
            {
                throw new ArgumentNullException("userRepository");
            }
            _userRepository = userRepository;
            RuleFor(x => x.Email).EmailAddress();
            RuleFor(x => x.FirstName)
                .NotEmpty().WithLocalizedMessage(() => Messages.NotEmpty)
                .Matches(@"^\p{L}+$").WithLocalizedMessage(() => Messages.OnlyLetters); ;
            RuleFor(x => x.LastName)
                .NotEmpty().WithLocalizedMessage(() => Messages.NotEmpty)
                .Matches(@"^\p{L}+$").WithLocalizedMessage(() => Messages.OnlyLetters); ;
            RuleFor(x => x.UserName).UseValidator(new UserNameValidator()).Must(IsUserNameUnique)
                .WithLocalizedMessage(() => Messages.UserNameUnique);
            RuleFor(x => x.Password).UseValidator(new PasswordValidator());
            RuleFor(x => x.PasswordConfirmation).NotEmpty().WithLocalizedMessage(() => Messages.NotEmpty)
                .Equal(x => x.Password).WithLocalizedMessage(() => Messages.PasswordConfirmation);
            RuleFor(x => x.PhoneNumber).UseValidator(new PhoneNumberValidator()).When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
            RuleFor(x => x.PhoneNumberConfirmed).Equal(false).When(x => string.IsNullOrWhiteSpace(x.PhoneNumber)).WithLocalizedMessage(() => Messages.EmptyPhoneNumberConfirmed);
            RuleFor(x => x.SecretPhrase).NotEmpty().Length(5, 1024);
            RuleFor(x => x.FullName)
                .NotEmpty().WithLocalizedMessage(() => Messages.NotEmpty).When(x => x.Role == "Customer")
                .Length(1, 100).When(x => x.Role == "Customer");
            RuleFor(x => x.Address)
                .NotEmpty().WithLocalizedMessage(() => Messages.NotEmpty).When(x => x.Role == "Customer")
                .Length(1, 1024).When(x => x.Role == "Customer");
        }
        private bool IsUserNameUnique(CreateUserCommand command, string userName)
        {
            var query = DbQuery.For<User>().FilterBy(x => x.Deleted == false && x.UserName == userName);
            var existingUser = _userRepository.QueryOne(query);
            return existingUser == null;
        }
    }

    internal class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.UserId).Must(x => x != Guid.Empty);
            RuleFor(x => x.CurrentPassword).NotEmpty().WithLocalizedMessage(() => Messages.NotEmpty);
            RuleFor(x => x.NewPassword).Must((c, x) => x != c.CurrentPassword).WithLocalizedMessage(() => Messages.NewPasswordNotChanged).UseValidator(new PasswordValidator());
            RuleFor(x => x.NewPasswordConfirmation).UseValidator(new PasswordValidator()).Equal(x => x.NewPassword)
                .WithLocalizedMessage(() => Messages.PasswordConfirmation);
        }
    }

    internal class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        private readonly IQueryRepository<User> _userRepository; 
        public UpdateUserCommandValidator(IQueryRepository<User> userRepository)
        {
            if (userRepository == null)
            {
                throw new ArgumentNullException("userRepository");
            }
            _userRepository = userRepository;
            RuleFor(x => x.Email).EmailAddress();
            RuleFor(x => x.FirstName)
                .NotEmpty().WithLocalizedMessage(() => Messages.NotEmpty)
                .Matches(@"^(\p{L}|-)+$").WithLocalizedMessage(() => Messages.OnlyLetters);
            RuleFor(x => x.LastName)
                .NotEmpty().WithLocalizedMessage(() => Messages.NotEmpty)
                .Matches(@"^(\p{L}|-)+$").WithLocalizedMessage(() => Messages.OnlyLetters); ;
            RuleFor(x => x.UserName).UseValidator(new UserNameValidator()).Must(IsUserNameUnique)
                .WithLocalizedMessage(() => Messages.UserNameUnique);
            RuleFor(x => x.Password).NotEmpty().WithLocalizedMessage(() => Messages.NotEmpty)
                .UseValidator(new PasswordValidator()).When(x => x.ChangePassword);
            RuleFor(x => x.PasswordConfirmation).NotEmpty().WithLocalizedMessage(() => Messages.NotEmpty)
                .Equal(x => x.Password).WithLocalizedMessage(() => Messages.PasswordConfirmation)
                .When(x => x.ChangePassword);
            RuleFor(x => x.PhoneNumber).UseValidator(new PhoneNumberValidator()).When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
            RuleFor(x => x.PhoneNumberConfirmed).Equal(false).When(x => string.IsNullOrWhiteSpace(x.PhoneNumber)).WithLocalizedMessage(() => Messages.EmptyPhoneNumberConfirmed);
            RuleFor(x => x.SecretPhrase).NotEmpty().Length(5, 1024);
            RuleFor(x => x.FullName)
                .NotEmpty().WithLocalizedMessage(() => Messages.NotEmpty).When(x => x.Role == "Customer")
                .Length(1, 100).When(x => x.Role == "Customer");
            RuleFor(x => x.Address)
                .NotEmpty().WithLocalizedMessage(() => Messages.NotEmpty).When(x => x.Role == "Customer")
                .Length(1, 1024).When(x => x.Role == "Customer");
        }
        private bool IsUserNameUnique(UpdateUserCommand command, string userName)
        {
            var query = DbQuery.For<User>().FilterBy(x => x.Deleted == false && x.UserName == userName);
            var existingUser = _userRepository.QueryOne(query);
            return existingUser == null || existingUser.Id == command.UserId;
        }
    }

    internal class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileCommandValidator()
        {
            RuleFor(x => x.Email).EmailAddress();
            RuleFor(x => x.PhoneNumber).UseValidator(new PhoneNumberValidator());
            RuleFor(x => x.UserId).Must(x => x != Guid.Empty);
            RuleFor(x => x.SecretPhrase).NotEmpty().Length(1, 1024);
        }
    }

    internal class UsersQueryValidator : AbstractValidator<UsersQuery>
    {
        public UsersQueryValidator()
        {
            RuleFor(x => x.ClientFilter).NotEmpty();
            RuleFor(x => x.ClientPage).NotEmpty();
        }
    }
}
