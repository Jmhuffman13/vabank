﻿using System.Data.Entity.ModelConfiguration;
using VaBank.Core.Entities;

namespace VaBank.Data.EntityFramework.Mappings
{
    public class LogMap: EntityTypeConfiguration<Log>
    {
        public LogMap()
        {
            ToTable("SystemLog", "Maintenance");
            HasKey(t => t.Id);
            Property(t => t.Id).HasColumnName("EventId");
            Property(t => t.Application).HasMaxLength(RestrictionConstants.ShortNameLength)
                .IsRequired();
            Property(t => t.Level).HasMaxLength(20).IsRequired();
            Property(t => t.TimeStampUtc).IsRequired();
            Property(t => t.Type).HasMaxLength(RestrictionConstants.ShortNameLength).IsRequired();
            Property(t => t.User).HasMaxLength(RestrictionConstants.NameLength).IsOptional();
            Property(t => t.Message).IsRequired();
            Property(t => t.Exception).IsOptional();
            Property(t => t.Source).IsRequired();

        }
    }
}
