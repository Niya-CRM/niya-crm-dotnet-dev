using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OXDesk.Core.Tickets;

/// <summary>
/// Represents a support ticket/case.
/// </summary>
[Table("tickets")]
public class Ticket
{
    // Identity
    [Key]
    [Required]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    [Column("tenant_id")]
    [Required]
    public Guid TenantId { get; set; }

    // Auto-increment, human-readable number
    [Required]
    public int TicketNumber { get; set; }

    [Required]
    [StringLength(10)]
    [Column(TypeName = "varchar(10)")]
    public string ChannelKey { get; set; } = string.Empty;

    [StringLength(10)]
    [Column(TypeName = "varchar(10)")]
    public string? Language { get; set; }

    // Brand and Product
    [StringLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string? BrandKey { get; set; }

    [StringLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string? BrandText {get; set;}

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? ProductKey { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? ProductText { get; set; }

    // Core details
    [Required]
    [StringLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string Subject { get; set; } = string.Empty;

    [Column(TypeName = "text")]
    public string? Description { get; set; }

    // Priority
    [Required]
    [StringLength(10)]
    [Column(TypeName = "varchar(10)")]
    public string PriorityKey { get; set; } = string.Empty;

    [Required]
    public int PriorityScore { get; set; } = 1;

    // Status
    [Required]
    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string StatusKey { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string StatusText { get; set; } = string.Empty;

    // Status Type
    [Required]
    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string StatusType { get; set; } = string.Empty;

    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string? WorkFlowStatusKey { get; set; }

    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string? WorkFlowStatusText { get; set; }

    // Flags
    [Required]
    public bool IsEscalated { get; set; } = false;

    [Required]
    public bool IsSpam { get; set; } = false;

    [Required]
    public bool IsArchived { get; set; } = false;

    [Required]
    public bool IsDeleted { get; set; } = false;

    [Required]
    public bool IsAutoClosed { get; set; } = false;

    [Required]
    public bool IsRead { get; set; } = false;

    [Required]
    public bool HasScheduledReply { get; set; } = false;

    [Required]
    public bool IsResponseOverdue { get; set; } = false;

    [Required]
    public bool IsOverdue { get; set; } = false;

    [Required]
    public bool IsReopened { get; set; } = false;

    // Classification
    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? Topic { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? SubTopic { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? RequestType { get; set; }

    [StringLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string? Skills { get; set; }

    // Layout
    [Required]
    public Guid LayoutId { get; set; }

    // Contact fields
    public Guid? Contact { get; set; }

    [StringLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string? ContactEmail { get; set; }

    [StringLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string? ContactPhone { get; set; }

    [StringLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string? ContactMobile { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? ContactName { get; set; }

    [StringLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string? SuppliedEmail { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? SuppliedCompany { get; set; }

    [StringLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string? SuppliedPhone { get; set; }

    // Account & product
    public Guid? Account { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? AccountName { get; set; }

    public long? SlaId { get; set; }

    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string? MilestoneStatus { get; set; }

    // Ownership & org
    public Guid? Owner { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? OwnerName { get; set; }

    public Guid? Team { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? TeamText { get; set; }

    public Guid? Department { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? DepartmentText { get; set; }

    public Guid? Organisation { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? OrganisationText { get; set; }

    // AI metadata
    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string? AiSentiment { get; set; }

    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string? AiTone { get; set; }

    [Column(TypeName = "text")]
    public string? AiSummary { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? AiTopic { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? AiSubtopic { get; set; }

    [StringLength(10)]
    [Column(TypeName = "varchar(10)")]
    public string? AiLanguage { get; set; }

    // Audit
    [Required]
    public Guid CreatedBy { get; set; }

    [Required]
    public Guid UpdatedBy { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    // Timeline
    [Required]
    public DateTime OpenedAt { get; set; }

    public DateTime? AssignedAt { get; set; }
    public DateTime? FirstResolutionAt { get; set; }
    public DateTime? FirstResolutionDueAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime? SlaStartAt { get; set; }
    public DateTime? SlaPausedAt { get; set; }
    public DateTime? SlaBreachAt { get; set; }
    public DateTime? DueAt { get; set; }
    public DateTime? ResponseDueAt { get; set; }
    public DateTime? CustomerRespondedAt { get; set; }
    public DateTime? OnHoldAt { get; set; }

    // Relations
    public int? ParentTicketNumber { get; set; }

    // Counters
    public int AttachmentCount { get; set; } = 0;
    public int CommentCount { get; set; } = 0;
    public int TaskCount { get; set; } = 0;
    public int ThreadCount { get; set; } = 0;
}
