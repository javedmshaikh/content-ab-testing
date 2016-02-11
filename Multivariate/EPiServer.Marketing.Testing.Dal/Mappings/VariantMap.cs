﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using EPiServer.Marketing.Testing.Model;

namespace EPiServer.Marketing.Testing.Dal.Mappings
{
    public class VariantMap : EntityTypeConfiguration<Variant>
    {
        public VariantMap()
        {
            this.ToTable("tblABVariant");

            this.HasKey(hk => hk.Id);

            this.Property(m => m.ItemId)
                .IsRequired();

            this.Property(m => m.ItemVersion)
                .IsOptional();

            this.HasRequired(m => m.ABTest)
                .WithMany(m => m.Variants)
                .HasForeignKey(m => m.TestId)
                .WillCascadeOnDelete();
        }
    }
}
