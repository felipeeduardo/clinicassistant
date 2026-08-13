using ClinicAssistant.Application.Identity;
using ClinicAssistant.Domain.Identity;
using ClinicAssistant.Domain.Clinics;
using ClinicAssistant.Domain.Scheduling;
using ClinicAssistant.Domain.Messaging;
using ClinicAssistant.Domain.WhatsApp;
using ClinicAssistant.Domain.Conversations;
using ClinicAssistant.Domain.Operations;
using Microsoft.EntityFrameworkCore;

namespace ClinicAssistant.Infrastructure.Persistence;

public sealed class ClinicAssistantDbContext(DbContextOptions<ClinicAssistantDbContext> options, ITenantContext? tenantContext = null) : DbContext(options)
{
    private readonly ITenantContext _tenantContext = tenantContext ?? new EmptyTenantContext();

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<ClinicUnit> ClinicUnits => Set<ClinicUnit>();
    public DbSet<UnitBusinessHour> UnitBusinessHours => Set<UnitBusinessHour>();
    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<Professional> Professionals => Set<Professional>();
    public DbSet<ProfessionalSpecialty> ProfessionalSpecialties => Set<ProfessionalSpecialty>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<AvailabilityRule> AvailabilityRules => Set<AvailabilityRule>();
    public DbSet<ScheduleBlock> ScheduleBlocks => Set<ScheduleBlock>();
    public DbSet<ProfessionalVacation> ProfessionalVacations => Set<ProfessionalVacation>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<WhatsAppIntegration> WhatsAppIntegrations => Set<WhatsAppIntegration>();
    public DbSet<WhatsAppTemplate> WhatsAppTemplates => Set<WhatsAppTemplate>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();
    public DbSet<WhatsAppMedia> WhatsAppMedia => Set<WhatsAppMedia>();
    public DbSet<ConversationState> ConversationStates => Set<ConversationState>();
    public DbSet<ConversationProcessedMessage> ConversationProcessedMessages => Set<ConversationProcessedMessage>();
    public DbSet<ConversationOption> ConversationOptions => Set<ConversationOption>();
    public DbSet<HumanQueueItem> HumanQueueItems => Set<HumanQueueItem>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<AuditRecord> AuditRecords => Set<AuditRecord>();

    public Guid? CurrentTenantId => _tenantContext.TenantId;
    public bool IsPlatformAdmin => _tenantContext.IsPlatformAdmin;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("clinic_assistant");

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");
            entity.HasKey(tenant => tenant.Id);
            entity.Property(tenant => tenant.Name).HasMaxLength(200).IsRequired();
            entity.Property(tenant => tenant.Slug).HasMaxLength(80).IsRequired();
            entity.Property(tenant => tenant.Status).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(tenant => tenant.Slug).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Name).HasMaxLength(200).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(320).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(user => user.Role).HasConversion<string>().HasMaxLength(32);
            entity.Property(user => user.Status).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(user => user.Email).IsUnique();
            entity.HasIndex(user => user.TenantId);
            entity.HasOne<Tenant>().WithMany().HasForeignKey(user => user.TenantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(user => IsPlatformAdmin || (CurrentTenantId.HasValue && user.TenantId == CurrentTenantId.Value));
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(token => token.Id);
            entity.Property(token => token.TokenHash).HasMaxLength(64).IsRequired();
            entity.Property(token => token.ReplacedByTokenHash).HasMaxLength(64);
            entity.HasIndex(token => token.TokenHash).IsUnique();
            entity.HasIndex(token => new { token.UserId, token.ExpiresAt });
            entity.HasIndex(token => token.TenantId);
            entity.HasOne(token => token.User).WithMany(user => user.RefreshTokens).HasForeignKey(token => token.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(token => IsPlatformAdmin || (CurrentTenantId.HasValue && token.TenantId == CurrentTenantId.Value));
        });
        modelBuilder.Entity<Clinic>(entity =>
        {
            entity.ToTable("clinics"); entity.Property(x => x.LegalName).HasMaxLength(200); entity.Property(x => x.TradeName).HasMaxLength(200);
            entity.Property(x => x.Document).HasMaxLength(32); entity.Property(x => x.Email).HasMaxLength(320); entity.Property(x => x.Phone).HasMaxLength(32); entity.Property(x => x.TimeZone).HasMaxLength(100);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32); entity.HasIndex(x => x.TenantId).IsUnique(); entity.HasQueryFilter(x => IsPlatformAdmin || (CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value));
        });
        modelBuilder.Entity<ClinicUnit>(entity =>
        {
            entity.ToTable("clinic_units"); entity.Property(x => x.Name).HasMaxLength(200); entity.Property(x => x.Address).HasMaxLength(500); entity.Property(x => x.Phone).HasMaxLength(32); entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(x => x.TenantId); entity.HasOne<Clinic>().WithMany().HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict); entity.HasQueryFilter(x => IsPlatformAdmin || (CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value));
        });
        modelBuilder.Entity<UnitBusinessHour>(entity =>
        {
            entity.ToTable("unit_business_hours"); entity.HasIndex(x => new { x.ClinicUnitId, x.DayOfWeek }).IsUnique();
            entity.HasOne<ClinicUnit>().WithMany().HasForeignKey(x => x.ClinicUnitId).OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(x => IsPlatformAdmin || (CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value));
        });
        modelBuilder.Entity<Specialty>(entity =>
        {
            entity.ToTable("specialties"); entity.Property(x => x.Name).HasMaxLength(160); entity.Property(x => x.Description).HasMaxLength(1000); entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(x => new { x.TenantId, x.Name }).IsUnique(); entity.HasQueryFilter(x => IsPlatformAdmin || (CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value));
        });
        modelBuilder.Entity<Professional>(entity =>
        {
            entity.ToTable("professionals"); entity.Property(x => x.Name).HasMaxLength(200); entity.Property(x => x.Email).HasMaxLength(320); entity.Property(x => x.Phone).HasMaxLength(32); entity.Property(x => x.RegistrationNumber).HasMaxLength(80); entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(x => x.TenantId); entity.HasIndex(x => new { x.TenantId, x.RegistrationNumber }).IsUnique(); entity.HasOne<ClinicUnit>().WithMany().HasForeignKey(x => x.ClinicUnitId).OnDelete(DeleteBehavior.Restrict); entity.HasQueryFilter(x => IsPlatformAdmin || (CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value));
        });
        modelBuilder.Entity<ProfessionalSpecialty>(entity =>
        {
            entity.ToTable("professional_specialties"); entity.HasKey(x => new { x.ProfessionalId, x.SpecialtyId });
            entity.HasOne(x => x.Professional).WithMany(x => x.Specialties).HasForeignKey(x => x.ProfessionalId); entity.HasOne(x => x.Specialty).WithMany().HasForeignKey(x => x.SpecialtyId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.ToTable("patients"); entity.Property(x => x.Name).HasMaxLength(200); entity.Property(x => x.Phone).HasMaxLength(32); entity.Property(x => x.Email).HasMaxLength(320); entity.Property(x => x.ConsentStatus).HasConversion<string>().HasMaxLength(32); entity.Property(x => x.Source).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(x => new { x.TenantId, x.Phone }).IsUnique(); entity.HasIndex(x => new { x.TenantId, x.Email }); entity.HasQueryFilter(x => IsPlatformAdmin || (CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value));
        });
        modelBuilder.Entity<AvailabilityRule>(entity =>
        {
            entity.ToTable("availability_rules"); entity.HasIndex(x => new { x.ProfessionalId, x.DayOfWeek }); entity.HasOne<Professional>().WithMany().HasForeignKey(x => x.ProfessionalId).OnDelete(DeleteBehavior.Cascade); entity.HasQueryFilter(x => IsPlatformAdmin || (CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value));
        });
        modelBuilder.Entity<ScheduleBlock>(entity =>
        {
            entity.ToTable("schedule_blocks"); entity.Property(x => x.Reason).HasMaxLength(500); entity.HasIndex(x => new { x.ProfessionalId, x.StartsAt }); entity.HasOne<Professional>().WithMany().HasForeignKey(x => x.ProfessionalId).OnDelete(DeleteBehavior.Cascade); entity.HasQueryFilter(x => IsPlatformAdmin || (CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value));
        });
        modelBuilder.Entity<ProfessionalVacation>(entity =>
        {
            entity.ToTable("professional_vacations"); entity.Property(x => x.Reason).HasMaxLength(500); entity.HasIndex(x => new { x.ProfessionalId, x.StartsAt });
            entity.HasOne<Professional>().WithMany().HasForeignKey(x => x.ProfessionalId).OnDelete(DeleteBehavior.Cascade); entity.HasQueryFilter(x => IsPlatformAdmin || (CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value));
        });
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("appointments"); entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32); entity.Property(x => x.Source).HasConversion<string>().HasMaxLength(32); entity.Property(x => x.Notes).HasMaxLength(1000); entity.Property(x => x.CancellationReason).HasMaxLength(500); entity.Property(x => x.Version).IsConcurrencyToken();
            entity.HasIndex(x => new { x.ProfessionalId, x.StartsAt }); entity.HasIndex(x => new { x.PatientId, x.StartsAt }); entity.HasIndex(x => new { x.Status, x.CreatedAt });
            entity.HasOne<Professional>().WithMany().HasForeignKey(x => x.ProfessionalId).OnDelete(DeleteBehavior.Restrict); entity.HasOne<Patient>().WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict); entity.HasQueryFilter(x => IsPlatformAdmin || (CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value));
        });
        modelBuilder.Entity<InboxMessage>(entity => { entity.ToTable("inbox_messages"); entity.Property(x => x.Provider).HasMaxLength(80); entity.Property(x => x.EventType).HasMaxLength(80); entity.Property(x => x.ExternalMessageId).HasMaxLength(200); entity.Property(x => x.ExternalEventId).HasMaxLength(200); entity.Property(x => x.PayloadHash).HasMaxLength(64); entity.Property(x => x.LastErrorCode).HasMaxLength(120); entity.Property(x => x.LastErrorMessage).HasMaxLength(2000); entity.Property(x => x.CorrelationId).HasMaxLength(128); entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32); entity.HasIndex(x => new { x.Provider, x.IntegrationId, x.ExternalMessageId }).IsUnique(); entity.HasIndex(x => new { x.Status, x.CreatedAt }); entity.HasQueryFilter(x => IsPlatformAdmin || (CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value)); });
        modelBuilder.Entity<OutboxMessage>(entity => { entity.ToTable("outbox_messages"); entity.Property(x => x.Type).HasMaxLength(120); entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32); entity.Property(x => x.LastError).HasMaxLength(2000); entity.HasIndex(x => new { x.Status, x.NextAttemptAt, x.CreatedAt }); entity.HasQueryFilter(x => IsPlatformAdmin || (CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value)); });
        modelBuilder.Entity<WhatsAppIntegration>(entity => { entity.ToTable("whatsapp_integrations"); entity.Property(x => x.Provider).HasConversion<string>().HasMaxLength(32); entity.Property(x => x.IntegrationKey).HasMaxLength(80); entity.Property(x => x.AccountSidReference).HasMaxLength(80); entity.Property(x => x.MessagingServiceSid).HasMaxLength(80); entity.Property(x => x.WhatsAppFrom).HasMaxLength(32); entity.Property(x => x.DisplayPhoneNumber).HasMaxLength(80); entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32); entity.Property(x => x.FailureReason).HasMaxLength(1000); entity.HasIndex(x => x.IntegrationKey).IsUnique(); entity.HasIndex(x => x.TenantId); entity.HasQueryFilter(x => IsPlatformAdmin || (CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value)); });
        modelBuilder.Entity<WhatsAppTemplate>(entity => { entity.ToTable("whatsapp_templates"); entity.Property(x => x.Provider).HasConversion<string>().HasMaxLength(32); entity.Property(x => x.ExternalTemplateId).HasMaxLength(160); entity.Property(x => x.ContentSid).HasMaxLength(80); entity.Property(x => x.Name).HasMaxLength(160); entity.Property(x => x.LanguageCode).HasMaxLength(16); entity.Property(x => x.Category).HasMaxLength(80); entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32); entity.HasIndex(x => new { x.IntegrationId, x.ContentSid }).IsUnique(); entity.HasIndex(x => x.TenantId); entity.HasQueryFilter(x => IsPlatformAdmin || (CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value)); });
        modelBuilder.Entity<Conversation>(entity => { entity.ToTable("conversations"); entity.Property(x => x.Channel).HasConversion<string>().HasMaxLength(32); entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32); entity.Property(x => x.AutomationMode).HasConversion<string>().HasMaxLength(32); entity.Property(x => x.Priority).HasConversion<string>().HasMaxLength(32); entity.Property(x => x.Version).IsConcurrencyToken(); entity.Property(x => x.ExternalContactId).HasMaxLength(120); entity.HasIndex(x => new { x.TenantId, x.IntegrationId, x.PatientId, x.Status }); entity.HasQueryFilter(x => IsPlatformAdmin || (CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value)); });
        modelBuilder.Entity<ConversationMessage>(entity => { entity.ToTable("conversation_messages"); entity.Property(x => x.Direction).HasConversion<string>().HasMaxLength(32); entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(32); entity.Property(x => x.Provider).HasConversion<string>().HasMaxLength(32); entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32); entity.Property(x => x.ExternalMessageId).HasMaxLength(200); entity.Property(x => x.ExternalReplyToMessageId).HasMaxLength(200); entity.Property(x => x.ProviderStatus).HasMaxLength(80); entity.Property(x => x.ProviderErrorCode).HasMaxLength(120); entity.Property(x => x.ProviderErrorMessage).HasMaxLength(2000); entity.HasIndex(x => new { x.ConversationId, x.CreatedAt }); entity.HasIndex(x => new { x.TenantId, x.Provider, x.ExternalMessageId }).IsUnique(); entity.HasQueryFilter(x => IsPlatformAdmin || (CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value)); });
        modelBuilder.Entity<WhatsAppMedia>(entity => { entity.ToTable("whatsapp_media"); entity.Property(x => x.SourceUrl).HasMaxLength(2048); entity.Property(x => x.ContentType).HasMaxLength(120); entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32); entity.Property(x => x.SafeReason).HasMaxLength(500); entity.HasIndex(x => new { x.ConversationMessageId, x.Index }).IsUnique(); entity.HasIndex(x => x.TenantId); entity.HasQueryFilter(x => IsPlatformAdmin || (CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value)); });
        modelBuilder.Entity<ConversationState>(entity => { entity.ToTable("conversation_states"); entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32); entity.Property(x => x.FlowState).HasConversion<string>().HasMaxLength(64); entity.Property(x => x.Intent).HasConversion<string>().HasMaxLength(64); entity.Property(x => x.ContextJson).HasColumnType("jsonb"); entity.Property(x => x.Version).IsConcurrencyToken(); entity.HasIndex(x => x.ConversationId).IsUnique(); entity.HasIndex(x => new { x.TenantId, x.ExpiresAt }); entity.HasQueryFilter(x => IsPlatformAdmin || (CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value)); });
        modelBuilder.Entity<ConversationProcessedMessage>(entity => { entity.ToTable("conversation_processed_messages"); entity.HasIndex(x => new { x.TenantId, x.ConversationMessageId }).IsUnique(); entity.HasIndex(x => new { x.TenantId, x.ConversationId, x.ProcessedAt }); entity.HasQueryFilter(x => IsPlatformAdmin || (CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value)); });
        modelBuilder.Entity<ConversationOption>(entity => { entity.ToTable("conversation_options"); entity.Property(x => x.Key).HasMaxLength(64); entity.Property(x => x.Value).HasMaxLength(256); entity.HasIndex(x => new { x.ConversationStateId, x.Key }).IsUnique(); entity.HasIndex(x => new { x.TenantId, x.ExpiresAt }); entity.HasQueryFilter(x => IsPlatformAdmin || (CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value)); });
        modelBuilder.Entity<HumanQueueItem>(entity => { entity.ToTable("human_queue_items"); entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32); entity.Property(x => x.Priority).HasConversion<string>().HasMaxLength(32); entity.Property(x => x.Reason).HasMaxLength(500); entity.Property(x => x.Version).IsConcurrencyToken(); entity.HasIndex(x => x.ConversationId).IsUnique(); entity.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt }); entity.HasQueryFilter(x => IsPlatformAdmin || (CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value)); });
        modelBuilder.Entity<IdempotencyRecord>(entity => { entity.ToTable("idempotency_records"); entity.Property(x => x.Scope).HasMaxLength(120); entity.Property(x => x.Key).HasMaxLength(200); entity.Property(x => x.ResponseJson).HasMaxLength(8000); entity.HasIndex(x => new { x.Scope, x.Key }).IsUnique(); });
        modelBuilder.Entity<AuditRecord>(entity => { entity.ToTable("audit_records"); entity.Property(x => x.Action).HasMaxLength(120); entity.Property(x => x.ResourceType).HasMaxLength(120); entity.Property(x => x.Result).HasMaxLength(32); entity.Property(x => x.Details).HasMaxLength(2000); entity.HasIndex(x => new { x.TenantId, x.CreatedAt }); });
    }

    private sealed class EmptyTenantContext : ITenantContext
    {
        public Guid? TenantId => null;
        public Guid? UserId => null;
        public bool IsPlatformAdmin => false;
    }
}
