﻿using System;
using System.Linq;
using System.Security.Claims;
using AutoMapper;
using FluentValidation;
using PagedList;
using VaBank.Common.Data;
using VaBank.Common.Data.Repositories;
using VaBank.Core.Common;
using VaBank.Core.Membership;
using VaBank.Services.Common;
using VaBank.Services.Contracts.Common;
using VaBank.Services.Contracts.Common.Models;
using VaBank.Services.Contracts.Common.Queries;
using VaBank.Services.Contracts.Membership;
using VaBank.Services.Contracts.Membership.Commands;
using VaBank.Services.Contracts.Membership.Models;
using VaBank.Services.Contracts.Membership.Queries;

namespace VaBank.Services.Membership
{
    public class UserManagementService : BaseService, IUserManagementService
    {
        private readonly UserManagementRepositories _db;

        public UserManagementService(IUnitOfWork unitOfWork, IValidatorFactory validatorFactory, UserManagementRepositories repositories) 
            : base(unitOfWork, validatorFactory)
        {
            repositories.EnsureIsResolved();
            _db = repositories;
        }

        public IPagedList<UserBriefModel> GetUsers(UsersQuery query)
        {
            EnsureIsValid(query);
            try
            {
                var usersPage = _db.Users.ProjectThenQueryPage<UserBriefModel>(query.ToDbQuery());
                return usersPage;
            }
            catch (Exception ex)
            {
                throw new ServiceException("Can't get users.", ex);
            }
        }

        public UserBriefModel GetUser(IdentityQuery<Guid> id)
        {
            EnsureIsValid(id);
            try
            {
                var user = _db.Users.ProjectIdentity<Guid, User, UserBriefModel>(id);
                return user;
            }
            catch (Exception ex)
            {
                throw new ServiceException("Can't get user.", ex);
            }
        }

        public UserProfileModel GetProfile(IdentityQuery<Guid> id)
        {
            EnsureIsValid(id);
            try
            {
                var user = _db.Users.QueryIdentity(id);
                if (user == null || user.Deleted || user.Profile == null)
                {
                    return null;
                }
                var profile = user.Profile.ToModel<UserProfile, UserProfileModel>();
                return profile;
            }
            catch (Exception ex)
            {
                throw new ServiceException("Can't get profile.", ex);
            }
        }

        public UserBriefModel CreateUser(CreateUserCommand command)
        {
            EnsureIsValid(command);
            try
            {
                var user = command.ToEntity<CreateUserCommand, User>();
                _db.Users.Create(user);
                UnitOfWork.Commit();
                return user.ToModel<User, UserBriefModel>();
            }
            catch (Exception ex)
            {
                throw new ServiceException("Can't create user.", ex);
            }
        }

        public bool UpdateUser(UpdateUserCommand command)
        {
            EnsureIsValid(command);
            try
            {
                var user = _db.Users.Find(command.UserId);
                if (user == null || user.Deleted)
                {
                    return false;
                }
                Mapper.Map(command, user);
                var role = UserClaim.Role.Create(command.UserId, command.Role);
                var existingRoles = user.Claims.Where(x => x.Type == ClaimTypes.Role).ToList();
                foreach (var existingRole in existingRoles)
                {
                    user.Claims.Remove(existingRole);
                }
                user.Claims.Add(role);
                if (command.ChangePassword)
                {
                    user.UpdatePassword(command.Password);
                }
                UnitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                throw new ServiceException("Can't update user.", ex);
            }
        }

        public bool UpdateProfile(UpdateProfileCommand command)
        {
            EnsureIsValid(command);
            try
            {
                var user = _db.Users.Find(command.UserId);
                if (user == null || user.Deleted || user.Profile == null)
                {
                    return false;
                }
                Mapper.Map(command, user.Profile);
                _db.UserProfiles.Update(user.Profile);
                UnitOfWork.Commit();
                return true;
            }
            catch (Exception ex)
            {
                throw new ServiceException("Can't update profile.", ex);
            }
        }

        public UserMessage ChangePassword(ChangePasswordCommand command)
        {
            EnsureIsValid(command);
            try
            {
                var user = _db.Users.Find(command.UserId);
                if (user == null || user.Deleted)
                {
                    return null;
                }
                if (!user.ValidatePassword(command.CurrentPassword))
                {
                    throw AccessFailure.ExceptionBecause(AccessFailureReason.BadCredentials);
                }
                user.UpdatePassword(command.NewPassword);
                _db.Tokens.Delete(DbQuery.For<ApplicationToken>().FilterBy(x => x.User.Id == command.UserId));
                UnitOfWork.Commit();
                return UserMessage.Resource(() => Messages.PasswordChanged);
            }
            catch (ServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ServiceException("Can't change password.", ex);
            }
        }
    }
}