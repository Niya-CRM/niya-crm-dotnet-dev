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
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    [Column("tenant_id")]
    [Required]
    public int TenantId { get; set; }

    // Ticket Number with or w/o Suffix and Prefix
    [Required]
    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string TicketNumber { get; set; } = string.Empty;
    
    // Ticket Key (UUID) used for public access
    [Required]
    public Guid TicketKey{ get; set; } = Guid.CreateVersion7();

    // Channel
    [Required]
    public int ChannelId { get; set; }

    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string? ChannelName { get; set; }

    // Language
    [StringLength(10)]
    [Column(TypeName = "varchar(10)")]
    public string? Language { get; set; }

    // Brand and Product
    [Required]
    public int BrandId { get; set; }

    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string? BrandName { get; set; }

    // Product
    [Required]
    public int ProductId { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? ProductName { get; set; }

    // Core details
    [Required]
    [StringLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string Subject { get; set; } = string.Empty;

    [Column(TypeName = "text")]
    public string? Description { get; set; }

    // Priority
    [Required]
    public int PriorityId { get; set; }

    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string? PriorityName { get; set; }

    [Required]
    public int PriorityScore { get; set; } = 1;

    // Status
    [Required]
    public int StatusId { get; set; }

    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string? StatusName { get; set; }

    // Status Type
    [Required]
    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string StatusType { get; set; } = string.Empty;

    // Workflow
    public int? WorkFlowId { get; set; }

    public int? WorkFlowStatusId { get; set; }

    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string? WorkFlowStatusName { get; set; }

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
    public int LayoutId { get; set; }

    // Contact fields
    public int? Contact { get; set; }

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
    public int? AccountId { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? AccountName { get; set; }

    public long? SlaId { get; set; }

    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string? MilestoneStatus { get; set; }

    // Ownership & org
    public int? OwnerId { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? OwnerName { get; set; }

    public int? TeamId { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? TeamName { get; set; }

    public int? DepartmentId { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? DepartmentName { get; set; }

    public int? OrganisationId { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? OrganisationName { get; set; }

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
    public int CreatedBy { get; set; }

    [Required]
    public int UpdatedBy { get; set; }

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
