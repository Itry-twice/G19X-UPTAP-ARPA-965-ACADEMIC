namespace PluriOne.HrService.Dtos;

public sealed record CreateTicketRequest(string Subject, string? Description, Guid? CategoryId, string? Priority, string? Source);

public sealed record UpdateStatusRequest(string Status, Guid? AssigneeUserId);

public sealed record AddCommentRequest(string Body);

public sealed record CategoryDto(Guid Id, string Name, string? Description);

public sealed record CommentDto(Guid Id, Guid AuthorUserId, string AuthorName, string Body, DateTimeOffset CreatedAt);

public sealed record TicketDto(
    Guid Id,
    string Subject,
    string? Description,
    Guid? CategoryId,
    string? CategoryName,
    Guid RequesterUserId,
    string RequesterName,
    Guid? AssigneeUserId,
    string Status,
    string Priority,
    string Source,
    bool Escalated,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record TicketDetailDto(
    Guid Id,
    string Subject,
    string? Description,
    Guid? CategoryId,
    string? CategoryName,
    Guid RequesterUserId,
    string RequesterName,
    Guid? AssigneeUserId,
    string Status,
    string Priority,
    string Source,
    bool Escalated,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    List<CommentDto> Comments,
    List<StatusHistoryDto> History);

public sealed record StatusHistoryDto(string? FromStatus, string ToStatus, DateTimeOffset ChangedAt);
