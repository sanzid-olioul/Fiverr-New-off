using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LancasterCreditCardDiversion.Models;

public partial class PaLancCcdpDevDbContext : DbContext
{
    public PaLancCcdpDevDbContext(DbContextOptions<PaLancCcdpDevDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppDomain> AppDomains { get; set; }

    public virtual DbSet<AppDomainValue> AppDomainValues { get; set; }

    public virtual DbSet<AppParameter> AppParameters { get; set; }

    public virtual DbSet<CaseComment> CaseComments { get; set; }

    public virtual DbSet<CaseDocument> CaseDocuments { get; set; }

    public virtual DbSet<CaseHearingDate> CaseHearingDates { get; set; }

    public virtual DbSet<CaseHistory> CaseHistories { get; set; }

    public virtual DbSet<CcdpCase> CcdpCases { get; set; }

    public virtual DbSet<ConciliationHearingDate> ConciliationHearingDates { get; set; }

    public virtual DbSet<EligibilityCheckRequest> EligibilityCheckRequests { get; set; }

    public virtual DbSet<EligibilityCheckRequestDocument> EligibilityCheckRequestDocuments { get; set; }

    public virtual DbSet<LetterTemplate> LetterTemplates { get; set; }

    public virtual DbSet<ResponsesApiRequest> ResponsesApiRequests { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppDomain>(entity =>
        {
            entity.HasKey(e => e.DomainName).HasName("APP_DOMAINS_PK");

            entity.ToTable("APP_DOMAINS", "PALANC_CCDP_DEV", tb =>
                {
                    tb.HasTrigger("TR_APP_DOMAINS_AI");
                    tb.HasTrigger("TR_APP_DOMAINS_AU");
                });

            entity.HasIndex(e => e.Rowid, "ROWID$INDEX").IsUnique();

            entity.Property(e => e.DomainName)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasColumnName("DOMAIN_NAME");
            entity.Property(e => e.CreatedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_DTTM");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("CREATED_USER");
            entity.Property(e => e.Description)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.ModifiedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("MODIFIED_DTTM");
            entity.Property(e => e.ModifiedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("MODIFIED_USER");
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasDefaultValue("A")
                .HasColumnName("RECORD_STATUS");
            entity.Property(e => e.Rowid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ROWID");
            entity.Property(e => e.VersionId)
                .HasDefaultValue(1.0)
                .HasColumnName("VERSION_ID");
        });

        modelBuilder.Entity<AppDomainValue>(entity =>
        {
            entity.HasKey(e => new { e.DomainName, e.Code }).HasName("APP_DOMAIN_VALUES_PK");

            entity.ToTable("APP_DOMAIN_VALUES", "PALANC_CCDP_DEV", tb =>
                {
                    tb.HasTrigger("TR_APP_DOMAIN_VALUES_AI");
                    tb.HasTrigger("TR_APP_DOMAIN_VALUES_AU");
                });

            entity.HasIndex(e => e.Rowid, "ROWID$INDEX").IsUnique();

            entity.Property(e => e.DomainName)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasColumnName("DOMAIN_NAME");
            entity.Property(e => e.Code)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasColumnName("CODE");
            entity.Property(e => e.CreatedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_DTTM");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("CREATED_USER");
            entity.Property(e => e.Description)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.ModifiedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("MODIFIED_DTTM");
            entity.Property(e => e.ModifiedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("MODIFIED_USER");
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasDefaultValue("A")
                .HasColumnName("RECORD_STATUS");
            entity.Property(e => e.Rowid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ROWID");
            entity.Property(e => e.VersionId)
                .HasDefaultValue(1.0)
                .HasColumnName("VERSION_ID");

            entity.HasOne(d => d.DomainNameNavigation).WithMany(p => p.AppDomainValues)
                .HasForeignKey(d => d.DomainName)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("APP_DOMAIN_VALUES_R01");
        });

        modelBuilder.Entity<AppParameter>(entity =>
        {
            entity.HasKey(e => e.ParameterId).HasName("APP_PARAMETERS_PK");

            entity.ToTable("APP_PARAMETERS", "PALANC_CCDP_DEV", tb =>
                {
                    tb.HasTrigger("APP_PARAMETERS_AI");
                    tb.HasTrigger("APP_PARAMETERS_AU");
                });

            entity.HasIndex(e => e.Rowid, "ROWID$INDEX").IsUnique();

            entity.Property(e => e.ParameterId).HasColumnName("PARAMETER_ID");
            entity.Property(e => e.CreatedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_DTTM");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("CREATED_USER");
            entity.Property(e => e.Description)
                .HasMaxLength(8000)
                .IsUnicode(false)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.ModifiedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("MODIFIED_DTTM");
            entity.Property(e => e.ModifiedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("MODIFIED_USER");
            entity.Property(e => e.Name)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasColumnName("NAME");
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasDefaultValue("A")
                .HasColumnName("RECORD_STATUS");
            entity.Property(e => e.Rowid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ROWID");
            entity.Property(e => e.Value)
                .HasMaxLength(8000)
                .IsUnicode(false)
                .HasColumnName("VALUE");
            entity.Property(e => e.VersionId)
                .HasDefaultValue(1.0)
                .HasColumnName("VERSION_ID");
        });

        modelBuilder.Entity<CaseComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("CASE_COMMENTS_PK");

            entity.ToTable("CASE_COMMENTS", "PALANC_CCDP_DEV", tb =>
                {
                    tb.HasTrigger("TR_CASE_COMMENTS_AI");
                    tb.HasTrigger("TR_CASE_COMMENTS_AU");
                });

            entity.HasIndex(e => e.Rowid, "ROWID$INDEX").IsUnique();

            entity.Property(e => e.CommentId).HasColumnName("COMMENT_ID");
            entity.Property(e => e.CaseId).HasColumnName("CASE_ID");
            entity.Property(e => e.CommentText)
                .HasMaxLength(8000)
                .IsUnicode(false)
                .HasColumnName("COMMENT_TEXT");
            entity.Property(e => e.CreatedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_DTTM");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("CREATED_USER");
            entity.Property(e => e.ModifiedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("MODIFIED_DTTM");
            entity.Property(e => e.ModifiedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("MODIFIED_USER");
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasDefaultValue("A")
                .HasColumnName("RECORD_STATUS");
            entity.Property(e => e.Rowid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ROWID");
            entity.Property(e => e.VersionId)
                .HasDefaultValue(1.0)
                .HasColumnName("VERSION_ID");

            entity.HasOne(d => d.Case).WithMany(p => p.CaseComments)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_COMM_CASEID");
        });

        modelBuilder.Entity<CaseDocument>(entity =>
        {
            entity.HasKey(e => e.DocId).HasName("CASE_DOCUMENTS_PK");

            entity.ToTable("CASE_DOCUMENTS", "PALANC_CCDP_DEV", tb =>
                {
                    tb.HasTrigger("TR_CASE_DOCUMENTS_AI");
                    tb.HasTrigger("TR_CASE_DOCUMENTS_AU_DOCTYPE");
                    tb.HasTrigger("TR_CASE_DOCUMENTS_AU_META");
                });

            entity.HasIndex(e => e.Rowid, "ROWID$INDEX").IsUnique();

            entity.Property(e => e.DocId).HasColumnName("DOC_ID");
            entity.Property(e => e.CaseId).HasColumnName("CASE_ID");
            entity.Property(e => e.Content).HasColumnName("CONTENT");
            entity.Property(e => e.CreatedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_DTTM");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("CREATED_USER");
            entity.Property(e => e.DocDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("DOC_DATE");
            entity.Property(e => e.DocType)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasColumnName("DOC_TYPE");
            entity.Property(e => e.DocTypeDomainName)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasDefaultValue("DOC_TYPE")
                .HasColumnName("DOC_TYPE_DOMAIN_NAME");
            entity.Property(e => e.ModifiedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("MODIFIED_DTTM");
            entity.Property(e => e.ModifiedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("MODIFIED_USER");
            entity.Property(e => e.Name)
                .HasMaxLength(800)
                .IsUnicode(false)
                .HasColumnName("NAME");
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasDefaultValue("A")
                .HasColumnName("RECORD_STATUS");
            entity.Property(e => e.Rowid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ROWID");
            entity.Property(e => e.TextContent)
                .IsUnicode(false)
                .HasColumnName("TEXT_CONTENT");
            entity.Property(e => e.VersionId)
                .HasDefaultValue(1.0)
                .HasColumnName("VERSION_ID");
            entity.Property(e => e.WordCount).HasColumnName("WORD_COUNT");

            entity.HasOne(d => d.Case).WithMany(p => p.CaseDocuments)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CCDP_DOC");

            entity.HasOne(d => d.AppDomainValue).WithMany(p => p.CaseDocuments)
                .HasForeignKey(d => new { d.DocTypeDomainName, d.DocType })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CCDP_DOCTYPE");
        });

        modelBuilder.Entity<CaseHearingDate>(entity =>
        {
            entity.HasKey(e => e.CaseHearingId).HasName("CASE_HEARING_DATES_PK");

            entity.ToTable("CASE_HEARING_DATES", "PALANC_CCDP_DEV");

            entity.Property(e => e.CaseHearingId).HasColumnName("CASE_HEARING_ID");
            entity.Property(e => e.CaseHearingDttmId).HasColumnName("CASE_HEARING_DTTM_ID");
            entity.Property(e => e.CaseId).HasColumnName("CASE_ID");
            entity.Property(e => e.CreatedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_DTTM");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("CREATED_USER");
            entity.Property(e => e.ModifiedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("MODIFIED_DTTM");
            entity.Property(e => e.ModifiedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("MODIFIED_USER");
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasDefaultValue("A")
                .HasColumnName("RECORD_STATUS");
            entity.Property(e => e.VersionId)
                .HasDefaultValue(1.0)
                .HasColumnName("VERSION_ID");
        });

        modelBuilder.Entity<CaseHistory>(entity =>
        {
            entity.HasKey(e => e.CaseHistoryId).HasName("CASE_HISTORY_PK");

            entity.ToTable("CASE_HISTORY", "PALANC_CCDP_DEV", tb =>
                {
                    tb.HasTrigger("TR_CASE_HISTORY_AI");
                    tb.HasTrigger("TR_CASE_HISTORY_AU");
                });

            entity.HasIndex(e => e.Rowid, "ROWID$INDEX").IsUnique();

            entity.Property(e => e.CaseHistoryId).HasColumnName("CASE_HISTORY_ID");
            entity.Property(e => e.CaseId).HasColumnName("CASE_ID");
            entity.Property(e => e.CreatedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_DTTM");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("CREATED_USER");
            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.EventDate)
                .HasPrecision(0)
                .HasColumnName("EVENT_DATE");
            entity.Property(e => e.EventType)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasColumnName("EVENT_TYPE");
            entity.Property(e => e.EventTypeDomainName)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasDefaultValue("EVENT_TYPE")
                .HasColumnName("EVENT_TYPE_DOMAIN_NAME");
            entity.Property(e => e.ModifiedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("MODIFIED_DTTM");
            entity.Property(e => e.ModifiedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("MODIFIED_USER");
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasDefaultValue("A")
                .HasColumnName("RECORD_STATUS");
            entity.Property(e => e.Rowid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ROWID");
            entity.Property(e => e.VersionId)
                .HasDefaultValue(1.0)
                .HasColumnName("VERSION_ID");

            entity.HasOne(d => d.Case).WithMany(p => p.CaseHistories)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CASE_HISTORY_R01");

            entity.HasOne(d => d.AppDomainValue).WithMany(p => p.CaseHistories)
                .HasForeignKey(d => new { d.EventTypeDomainName, d.EventType })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CASE_HISTORY_R02");
        });

        modelBuilder.Entity<CcdpCase>(entity =>
        {
            entity.HasKey(e => e.CaseId).HasName("PK_CASES_ID");

            entity.ToTable("CCDP_CASES", "PALANC_CCDP_DEV", tb =>
                {
                    tb.HasTrigger("TR_CCDP_CASES_AI");
                    tb.HasTrigger("TR_CCDP_CASES_AU");
                    tb.HasTrigger("TR_Cases_AddHearing_OnInsert");
                    tb.HasTrigger("TR_Cases_AddHearing_OnUpdate");
                    tb.HasTrigger("TR_Cases_HearingChanged_History");
                    tb.HasTrigger("TR_Cases_StatusChanged_History");
                });

            entity.HasIndex(e => e.Rowid, "ROWID$INDEX").IsUnique();

            entity.Property(e => e.CaseId).HasColumnName("CASE_ID");
            entity.Property(e => e.CaseStatus)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .HasColumnName("CASE_STATUS");
            entity.Property(e => e.CaseStatusDomainName)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasDefaultValue("CASE_STATUS")
                .HasColumnName("CASE_STATUS_DOMAIN_NAME");
            entity.Property(e => e.CourtCaseNumber)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("COURT_CASE_NUMBER");
            entity.Property(e => e.CreatedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_DTTM");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("CREATED_USER");
            entity.Property(e => e.DefendantName)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasColumnName("DEFENDANT_NAME");
            entity.Property(e => e.DefendantRep2Name)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasColumnName("DEFENDANT_REP2_NAME");
            entity.Property(e => e.DefendantRepLawfirmName)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("DEFENDANT_REP_LAWFIRM_NAME");
            entity.Property(e => e.DefendantRepName)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasColumnName("DEFENDANT_REP_NAME");
            entity.Property(e => e.DefendantTwoName)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasColumnName("DEFENDANT_TWO_NAME");
            entity.Property(e => e.FilingDate)
                .HasPrecision(0)
                .HasColumnName("FILING_DATE");
            entity.Property(e => e.HearingId).HasColumnName("HEARING_ID");
            entity.Property(e => e.ModifiedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("MODIFIED_DTTM");
            entity.Property(e => e.ModifiedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("MODIFIED_USER");
            entity.Property(e => e.PlaintiffName)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasColumnName("PLAINTIFF_NAME");
            entity.Property(e => e.PlaintiffRep2Name)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasColumnName("PLAINTIFF_REP2_NAME");
            entity.Property(e => e.PlaintiffRepLawfirmName)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("PLAINTIFF_REP_LAWFIRM_NAME");
            entity.Property(e => e.PlaintiffRepName)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasColumnName("PLAINTIFF_REP_NAME");
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasDefaultValue("A")
                .HasColumnName("RECORD_STATUS");
            entity.Property(e => e.Rowid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ROWID");
            entity.Property(e => e.VersionId)
                .HasDefaultValue(1.0)
                .HasColumnName("VERSION_ID");

            entity.HasOne(d => d.Hearing).WithMany(p => p.CcdpCases)
                .HasForeignKey(d => d.HearingId)
                .HasConstraintName("CCDP_CASES_R02");

            entity.HasOne(d => d.AppDomainValue).WithMany(p => p.CcdpCases)
                .HasForeignKey(d => new { d.CaseStatusDomainName, d.CaseStatus })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("CCDP_CASES_R01");
        });

        modelBuilder.Entity<ConciliationHearingDate>(entity =>
        {
            entity.HasKey(e => e.HearingId).HasName("CONCILIATION_HEARING_DATES_PK");

            entity.ToTable("CONCILIATION_HEARING_DATES", "PALANC_CCDP_DEV", tb =>
                {
                    tb.HasTrigger("TR_CONCIL_HEARING_DATES_AI");
                    tb.HasTrigger("TR_CONCIL_HEARING_DATES_AU");
                });

            entity.HasIndex(e => e.HearingDttm, "CONCILIATION_HEARING_DATES_U01").IsUnique();

            entity.HasIndex(e => e.Rowid, "ROWID$INDEX").IsUnique();

            entity.Property(e => e.HearingId).HasColumnName("HEARING_ID");
            entity.Property(e => e.CreatedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_DTTM");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("CREATED_USER");
            entity.Property(e => e.HearingDttm)
                .HasPrecision(0)
                .HasColumnName("HEARING_DTTM");
            entity.Property(e => e.ModifiedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("MODIFIED_DTTM");
            entity.Property(e => e.ModifiedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("MODIFIED_USER");
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasDefaultValue("A")
                .HasColumnName("RECORD_STATUS");
            entity.Property(e => e.Rowid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ROWID");
            entity.Property(e => e.VersionId)
                .HasDefaultValue(1.0)
                .HasColumnName("VERSION_ID");
        });

        modelBuilder.Entity<EligibilityCheckRequest>(entity =>
        {
            entity.HasKey(e => e.ReqId).HasName("PK_DOCUMENT_ELIGIBILITY_REQUESTS");

            entity.ToTable("ELIGIBILITY_CHECK_REQUESTS", "PALANC_CCDP_DEV", tb =>
                {
                    tb.HasTrigger("TR_ELIGIBILITY_CHECK_REQUESTS_AI");
                    tb.HasTrigger("TR_ELIGIBILITY_CHECK_REQUESTS_AU");
                });

            entity.HasIndex(e => e.Rowid, "ROWID$INDEX").IsUnique();

            entity.Property(e => e.ReqId).HasColumnName("REQ_ID");
            entity.Property(e => e.AssistantId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ASSISTANT_ID");
            entity.Property(e => e.CaseId).HasColumnName("CASE_ID");
            entity.Property(e => e.CreatedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_DTTM");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("CREATED_USER");
            entity.Property(e => e.EligibilityCheckStatus)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasDefaultValue("Q")
                .HasColumnName("ELIGIBILITY_CHECK_STATUS");
            entity.Property(e => e.EligibilityCheckStatusDomainName)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasDefaultValue("ELIGIBILITY_CHECK_STATUS")
                .HasColumnName("ELIGIBILITY_CHECK_STATUS_DOMAIN_NAME");
            entity.Property(e => e.IsInProgress).HasColumnName("IS_IN_PROGRESS");
            entity.Property(e => e.ModifiedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("MODIFIED_DTTM");
            entity.Property(e => e.ModifiedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("MODIFIED_USER");
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasDefaultValue("A")
                .HasColumnName("RECORD_STATUS");
            entity.Property(e => e.Response)
                .IsUnicode(false)
                .HasColumnName("RESPONSE");
            entity.Property(e => e.Rowid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ROWID");
            entity.Property(e => e.ThreadId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("THREAD_ID");
            entity.Property(e => e.VersionId)
                .HasDefaultValue(1.0)
                .HasColumnName("VERSION_ID");

            entity.HasOne(d => d.Case).WithMany(p => p.EligibilityCheckRequests)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DOC_ELIGIBILITY_CASEID");

            entity.HasOne(d => d.AppDomainValue).WithMany(p => p.EligibilityCheckRequests)
                .HasForeignKey(d => new { d.EligibilityCheckStatusDomainName, d.EligibilityCheckStatus })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CCDP_ELIGIBILITY_CHECK_STATUS");
        });

        modelBuilder.Entity<EligibilityCheckRequestDocument>(entity =>
        {
            entity.HasKey(e => e.CheckRequestDocId).HasName("SYS_C009189");

            entity.ToTable("ELIGIBILITY_CHECK_REQUEST_DOCUMENTS", "PALANC_CCDP_DEV", tb =>
                {
                    tb.HasTrigger("TR_ECR_DOCS_AI");
                    tb.HasTrigger("TR_ECR_DOCS_AU");
                });

            entity.HasIndex(e => e.Rowid, "ROWID$INDEX").IsUnique();

            entity.Property(e => e.CheckRequestDocId).HasColumnName("CHECK_REQUEST_DOC_ID");
            entity.Property(e => e.CreatedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_DTTM");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("CREATED_USER");
            entity.Property(e => e.DocId).HasColumnName("DOC_ID");
            entity.Property(e => e.ModifiedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("MODIFIED_DTTM");
            entity.Property(e => e.ModifiedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("MODIFIED_USER");
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasDefaultValue("A")
                .HasColumnName("RECORD_STATUS");
            entity.Property(e => e.ReqId).HasColumnName("REQ_ID");
            entity.Property(e => e.Rowid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ROWID");
            entity.Property(e => e.VersionId)
                .HasDefaultValue(1.0)
                .HasColumnName("VERSION_ID");

            entity.HasOne(d => d.Doc).WithMany(p => p.EligibilityCheckRequestDocuments)
                .HasForeignKey(d => d.DocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ELIGIBILITY_CHECK_DOC_ID");

            entity.HasOne(d => d.Req).WithMany(p => p.EligibilityCheckRequestDocuments)
                .HasForeignKey(d => d.ReqId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RESPONSES_API_REQUEST_ID");
        });

        modelBuilder.Entity<LetterTemplate>(entity =>
        {
            entity.HasKey(e => e.LetterTemplateId).HasName("LETTER_TEMPLATES_PK");

            entity.ToTable("LETTER_TEMPLATES", "PALANC_CCDP_DEV", tb =>
                {
                    tb.HasTrigger("TR_LETTER_TEMPLATES_AI");
                    tb.HasTrigger("TR_LETTER_TEMPLATES_AU");
                });

            entity.HasIndex(e => e.Name, "LETTER_TEMPLATES_U01").IsUnique();

            entity.HasIndex(e => e.Rowid, "ROWID$INDEX").IsUnique();

            entity.Property(e => e.LetterTemplateId).HasColumnName("LETTER_TEMPLATE_ID");
            entity.Property(e => e.Content).HasColumnName("CONTENT");
            entity.Property(e => e.ConvertToPdf)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasDefaultValue("Y")
                .HasColumnName("CONVERT_TO_PDF");
            entity.Property(e => e.CreatedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_DTTM");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("CREATED_USER");
            entity.Property(e => e.DocType)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasColumnName("DOC_TYPE");
            entity.Property(e => e.DocTypeDomainName)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasDefaultValue("DOC_TYPE")
                .HasColumnName("DOC_TYPE_DOMAIN_NAME");
            entity.Property(e => e.ModifiedDttm)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("MODIFIED_DTTM");
            entity.Property(e => e.ModifiedUser)
                .HasMaxLength(256)
                .IsUnicode(false)
                .HasDefaultValueSql("(user_name())")
                .HasColumnName("MODIFIED_USER");
            entity.Property(e => e.Name)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasColumnName("NAME");
            entity.Property(e => e.PublishedDate)
                .HasPrecision(0)
                .HasColumnName("PUBLISHED_DATE");
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasDefaultValue("A")
                .HasColumnName("RECORD_STATUS");
            entity.Property(e => e.Rowid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ROWID");
            entity.Property(e => e.VersionId)
                .HasDefaultValue(1.0)
                .HasColumnName("VERSION_ID");

            entity.HasOne(d => d.AppDomainValue).WithMany(p => p.LetterTemplates)
                .HasForeignKey(d => new { d.DocTypeDomainName, d.DocType })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LETTEMP");
        });

        modelBuilder.Entity<ResponsesApiRequest>(entity =>
        {
            entity.HasKey(e => e.ReqId).HasName("PK__RESPONSE__06143B5B75061976");

            entity.ToTable("RESPONSES_API_REQUESTS", "PALANC_CCDP_DEV", tb =>
                {
                    tb.HasTrigger("TR_RESP_API_AFTER_INSERT");
                    tb.HasTrigger("TR_RESP_API_AFTER_UPDATE");
                });

            entity.Property(e => e.ReqId).HasColumnName("REQ_ID");
            entity.Property(e => e.CaseId).HasColumnName("CASE_ID");
            entity.Property(e => e.CreatedDttm)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("CREATED_DTTM");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(64)
                .IsUnicode(false)
                .HasDefaultValueSql("(suser_sname())")
                .HasColumnName("CREATED_USER");
            entity.Property(e => e.EligibilityCheckStatus)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasDefaultValue("Q")
                .HasColumnName("ELIGIBILITY_CHECK_STATUS");
            entity.Property(e => e.EligibilityCheckStatusDomainName)
                .HasMaxLength(400)
                .IsUnicode(false)
                .HasDefaultValue("ELIGIBILITY_CHECK_STATUS")
                .HasColumnName("ELIGIBILITY_CHECK_STATUS_DOMAIN_NAME");
            entity.Property(e => e.IsChecked).HasColumnName("IS_CHECKED");
            entity.Property(e => e.IsInProgress).HasColumnName("IS_IN_PROGRESS");
            entity.Property(e => e.ModifiedDttm)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("MODIFIED_DTTM");
            entity.Property(e => e.ModifiedUser)
                .HasMaxLength(64)
                .IsUnicode(false)
                .HasDefaultValueSql("(suser_sname())")
                .HasColumnName("MODIFIED_USER");
            entity.Property(e => e.PromptId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("PROMPT_ID");
            entity.Property(e => e.PromptVersion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("PROMPT_VERSION");
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("A")
                .IsFixedLength()
                .HasColumnName("RECORD_STATUS");
            entity.Property(e => e.Response).HasColumnName("RESPONSE");
            entity.Property(e => e.VersionId)
                .HasDefaultValue(1)
                .HasColumnName("VERSION_ID");

            entity.HasOne(d => d.Case).WithMany(p => p.ResponsesApiRequests)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RESP_API_CASEID");

            entity.HasOne(d => d.AppDomainValue).WithMany(p => p.ResponsesApiRequests)
                .HasForeignKey(d => new { d.EligibilityCheckStatusDomainName, d.EligibilityCheckStatus })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RESP_API_STATUS");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("USERS", "PALANC_CCDP_DEV", tb =>
                {
                    tb.HasTrigger("TR_USERS_AI");
                    tb.HasTrigger("TR_USERS_AU");
                });

            entity.Property(e => e.CreatedDttm)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("CREATED_DTTM");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(64)
                .IsUnicode(false)
                .HasDefaultValueSql("(suser_sname())")
                .UseCollation("Latin1_General_CI_AS")
                .HasColumnName("CREATED_USER");
            entity.Property(e => e.Email)
                .HasMaxLength(320)
                .IsUnicode(false)
                .UseCollation("Latin1_General_CI_AS")
                .HasColumnName("EMAIL");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValue("1")
                .UseCollation("Latin1_General_CI_AS")
                .HasColumnName("FULL_NAME");
            entity.Property(e => e.ModifiedDttm)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("MODIFIED_DTTM");
            entity.Property(e => e.ModifiedUser)
                .HasMaxLength(64)
                .IsUnicode(false)
                .HasDefaultValueSql("(suser_sname())")
                .UseCollation("Latin1_General_CI_AS")
                .HasColumnName("MODIFIED_USER");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(512)
                .IsUnicode(false)
                .UseCollation("Latin1_General_CI_AS")
                .HasColumnName("PASSWORD_HASH");
            entity.Property(e => e.PasswordResetCode).HasColumnName("PASSWORD_RESET_CODE");
            entity.Property(e => e.PasswordResetCodeExpiry)
                .HasColumnType("datetime")
                .HasColumnName("PASSWORD_RESET_CODE_EXPIRY");
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("A")
                .IsFixedLength()
                .UseCollation("Latin1_General_CI_AS")
                .HasColumnName("RECORD_STATUS");
            entity.Property(e => e.UserName)
                .HasMaxLength(64)
                .IsUnicode(false)
                .UseCollation("Latin1_General_CI_AS")
                .HasColumnName("USER_NAME");
            entity.Property(e => e.VersionId)
                .HasDefaultValue(1)
                .HasColumnName("VERSION_ID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
