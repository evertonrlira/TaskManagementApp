namespace TaskManagement.Core.Entities;

public class UserTask
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public Guid UserId { get; set; }
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	public UserTaskStatus Status => CompletedAt.HasValue ? UserTaskStatus.COMPLETE : UserTaskStatus.TODO;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? CompletedAt { get; set; }
	public DateTime? DeletedAt { get; set; }
}
