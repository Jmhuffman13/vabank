﻿using System.Data.Entity.ModelConfiguration;
using VaBank.Core.Payments.Entities;

namespace VaBank.Data.EntityFramework.Payments.Mappings
{
    internal class PaymentMap : EntityTypeConfiguration<Payment>
    {
        public PaymentMap()
        {
            ToTable("Payment", "Payments");
            HasKey(x => x.Id).Property(x => x.Id).HasColumnName("OperationId");

            Property(x => x.Form).IsRequired().IsMaxLength();
            Property(x => x.Info).IsOptional().IsMaxLength();
            Ignore(x => x.TemplateCode);

            HasRequired(x => x.Order).WithOptional().Map(x => x.MapKey("OrderNo"));
        }
    }
}
