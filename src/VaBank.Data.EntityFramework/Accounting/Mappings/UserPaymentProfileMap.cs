﻿using System.Data.Entity.ModelConfiguration;
using VaBank.Core.Accounting.Entities;
using VaBank.Data.EntityFramework.Common;

namespace VaBank.Data.EntityFramework.Accounting.Mappings
{
    internal class UserPaymentProfileMap : EntityTypeConfiguration<UserPaymentProfile>
    {
        public UserPaymentProfileMap()
        {
            ToTable("UserPaymentProfile", "Accounting").HasKey(x => x.UserId);
            Property(x => x.Address).IsRequired().HasMaxLength(Restrict.Length.BigString);
            Property(x => x.FullName).IsRequired().HasMaxLength(Restrict.Length.Name);
            Property(x => x.PayerTIN).IsRequired().HasMaxLength(Restrict.Length.TIN);
            HasRequired(x => x.User).WithOptional();
        }
    }
}
