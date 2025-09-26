using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Entities;

namespace TaskManagement.Infrastructure.Persistence;

internal class DataSeeder
{
	private ModelBuilder _modelBuilder;
	private static readonly Guid userId01 = Guid.Parse("c7d3d975-9b97-4c4f-9e0a-041e31fd60f5");
	private static readonly Guid userId02 = Guid.Parse("d4e7d976-9c97-4c4f-9e0a-041e31fd60f6");
	private static readonly Guid userId03 = Guid.Parse("e5f8d977-9d97-4c4f-9e0a-041e31fd60f7");

	public DataSeeder(ModelBuilder modelBuilder)
	{
		_modelBuilder = modelBuilder;
	}

	internal void SeedUsers()
	{
		_modelBuilder.Entity<User>().HasData(
			new User { Id = userId01, Name = new Bogus.DataSets.Name().FullName() },
			new User { Id = userId02, Name = new Bogus.DataSets.Name().FullName() },
			new User { Id = userId03, Name = new Bogus.DataSets.Name().FullName() }
		);
	}

	internal void SeedTasks()
	{
		var faker = new Bogus.Faker();
		var statuses = Enum.GetValues<UserTaskStatus>();
		var tasks = new List<UserTask>();
		for (int i = 0; i < 50; i++)
		{
			var status = faker.PickRandom(statuses);
			tasks.Add(new UserTask
			{
				Id = Guid.NewGuid(),
				Title = faker.Lorem.Sentence(10, 20),
				Description = faker.Lorem.Paragraph(10),
				CreatedAt = faker.Date.Past(1),
				CompletedAt = status == UserTaskStatus.COMPLETE ? faker.Date.Recent(10) : null,
				UserId = faker.PickRandom(new[] { userId01, userId02, userId03 })
			});
		}
		_modelBuilder.Entity<UserTask>().HasData(tasks);
	}
}
