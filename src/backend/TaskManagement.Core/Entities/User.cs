namespace TaskManagement.Core.Entities;

public class User
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public string Name { get; set; } = string.Empty;
	public ICollection<UserTask> Tasks { get; set; } = new List<UserTask>();
}
