using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;
using TaskManagement.Core.Commands;
using TaskManagement.Core.Constants;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Handlers;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Tests.Features.Tasks;

public class CreateTaskHandlerTests
{
    private readonly Mock<ITaskDbContext> _mockContext;
    private readonly Mock<IValidator<UserTask>> _mockValidator;
    private readonly Mock<DbSet<UserTask>> _mockDbSet;
    private readonly CreateTaskHandler _handler;

    public CreateTaskHandlerTests()
    {
        _mockContext = new Mock<ITaskDbContext>();
        _mockValidator = new Mock<IValidator<UserTask>>();
        _mockDbSet = new Mock<DbSet<UserTask>>();
        
        _mockContext.Setup(c => c.Tasks).Returns(_mockDbSet.Object);
        _handler = new CreateTaskHandler(_mockContext.Object, _mockValidator.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidTask_ShouldCreateTask()
    {
        // Arrange
        var command = new CreateTaskCommand(
            UserId: Guid.NewGuid(),
            Title: "Test Task",
            Description: "Test Description"
        );

        var validationResult = new ValidationResult();
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<UserTask>(), default))
                     .ReturnsAsync(validationResult);

        _mockContext.Setup(c => c.SaveChangesAsync(default))
                   .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.ShouldNotBeNull();
        result.Title.ShouldBe(command.Title);
        result.Description.ShouldBe(command.Description);
        result.Status.ShouldBe(UserTaskStatus.TODO);
        result.CreatedAt.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.CompletedAt.ShouldBeNull();

        _mockDbSet.Verify(d => d.Add(It.Is<UserTask>(t =>
            t.Title == command.Title &&
            t.Description == command.Description &&
            t.UserId == command.UserId &&
            t.Status == UserTaskStatus.TODO
        )), Times.Once);

        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidTask_ShouldThrowValidationException()
    {
        // Arrange
        var command = new CreateTaskCommand(
            UserId: Guid.NewGuid(),
            Title: "", // Invalid empty title
            Description: "Test Description"
        );

        var validationErrors = new List<ValidationFailure>
        {
            new ValidationFailure("Title", "Title is required")
        };
        var validationResult = new ValidationResult(validationErrors);

        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<UserTask>(), default))
                     .ReturnsAsync(validationResult);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _handler.HandleAsync(command));

        exception.Errors.Count().ShouldBe(1);
        exception.Errors.First().ErrorMessage.ShouldBe("Title is required");

        _mockDbSet.Verify(d => d.Add(It.IsAny<UserTask>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithNullDescription_ShouldCreateTaskSuccessfully()
    {
        // Arrange
        var command = new CreateTaskCommand(
            UserId: Guid.NewGuid(),
            Title: "Test Task",
            Description: null
        );

        var validationResult = new ValidationResult();
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<UserTask>(), default))
                     .ReturnsAsync(validationResult);

        _mockContext.Setup(c => c.SaveChangesAsync(default))
                   .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.ShouldNotBeNull();
        result.Title.ShouldBe(command.Title);
        result.Description.ShouldBeNull();
        result.Status.ShouldBe(UserTaskStatus.TODO);

        _mockDbSet.Verify(d => d.Add(It.Is<UserTask>(t =>
            t.Title == command.Title &&
            t.Description == null &&
            t.UserId == command.UserId
        )), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenContextThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateTaskCommand(
            UserId: Guid.NewGuid(),
            Title: "Test Task",
            Description: "Test Description"
        );

        var validationResult = new ValidationResult();
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<UserTask>(), default))
                     .ReturnsAsync(validationResult);

        _mockContext.Setup(c => c.SaveChangesAsync(default))
                   .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.HandleAsync(command));

        exception.Message.ShouldBe("Database error");
    }

    [Fact]
    public async Task HandleAsync_WithSingleCharacterTitle_ShouldCreateTaskSuccessfully()
    {
        // Arrange
        var command = new CreateTaskCommand(
            UserId: Guid.NewGuid(),
            Title: "A", // Single character should now be allowed
            Description: "Test Description"
        );

        var validationResult = new ValidationResult();
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<UserTask>(), default))
                     .ReturnsAsync(validationResult);

        _mockContext.Setup(c => c.SaveChangesAsync(default))
                   .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.ShouldNotBeNull();
        result.Title.ShouldBe("A");
        result.Title.Length.ShouldBe(1);
        result.Description.ShouldBe(command.Description);

        _mockDbSet.Verify(d => d.Add(It.Is<UserTask>(t =>
            t.Title == "A" &&
            t.Description == command.Description &&
            t.UserId == command.UserId
        )), Times.Once);

        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithTitleExceedingMaxLength_ShouldThrowValidationException()
    {
        // Arrange
        var longTitle = new string('A', TaskValidationConstants.MaxTitleLength + 1); // Exceeds max limit
        var command = new CreateTaskCommand(
            UserId: Guid.NewGuid(),
            Title: longTitle,
            Description: "Test Description"
        );

        var validationErrors = new List<ValidationFailure>
        {
            new ValidationFailure("Title", $"Title cannot exceed {TaskValidationConstants.MaxTitleLength} characters")
        };
        var validationResult = new ValidationResult(validationErrors);

        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<UserTask>(), default))
                     .ReturnsAsync(validationResult);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _handler.HandleAsync(command));

        exception.Errors.Count().ShouldBe(1);
        exception.Errors.First().ErrorMessage.ShouldBe($"Title cannot exceed {TaskValidationConstants.MaxTitleLength} characters");

        _mockDbSet.Verify(d => d.Add(It.IsAny<UserTask>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithTitleAtMaxLength_ShouldCreateTaskSuccessfully()
    {
        // Arrange
        var maxLengthTitle = new string('A', TaskValidationConstants.MaxTitleLength); // Exactly at the limit
        var command = new CreateTaskCommand(
            UserId: Guid.NewGuid(),
            Title: maxLengthTitle,
            Description: "Test Description"
        );

        var validationResult = new ValidationResult();
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<UserTask>(), default))
                     .ReturnsAsync(validationResult);

        _mockContext.Setup(c => c.SaveChangesAsync(default))
                   .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.ShouldNotBeNull();
        result.Title.ShouldBe(maxLengthTitle);
        result.Title.Length.ShouldBe(TaskValidationConstants.MaxTitleLength);
        result.Description.ShouldBe(command.Description);

        _mockDbSet.Verify(d => d.Add(It.Is<UserTask>(t =>
            t.Title == maxLengthTitle &&
            t.Description == command.Description &&
            t.UserId == command.UserId
        )), Times.Once);

        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithDescriptionExceedingMaxLength_ShouldThrowValidationException()
    {
        // Arrange
        var longDescription = new string('B', TaskValidationConstants.MaxDescriptionLength + 1); // Exceeds max limit
        var command = new CreateTaskCommand(
            UserId: Guid.NewGuid(),
            Title: "Test Task",
            Description: longDescription
        );

        var validationErrors = new List<ValidationFailure>
        {
            new ValidationFailure("Description", $"Description cannot exceed {TaskValidationConstants.MaxDescriptionLength} characters")
        };
        var validationResult = new ValidationResult(validationErrors);

        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<UserTask>(), default))
                     .ReturnsAsync(validationResult);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _handler.HandleAsync(command));

        exception.Errors.Count().ShouldBe(1);
        exception.Errors.First().ErrorMessage.ShouldBe($"Description cannot exceed {TaskValidationConstants.MaxDescriptionLength} characters");

        _mockDbSet.Verify(d => d.Add(It.IsAny<UserTask>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithDescriptionAtMaxLength_ShouldCreateTaskSuccessfully()
    {
        // Arrange
        var maxLengthDescription = new string('B', TaskValidationConstants.MaxDescriptionLength); // Exactly at the limit
        var command = new CreateTaskCommand(
            UserId: Guid.NewGuid(),
            Title: "Test Task",
            Description: maxLengthDescription
        );

        var validationResult = new ValidationResult();
        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<UserTask>(), default))
                     .ReturnsAsync(validationResult);

        _mockContext.Setup(c => c.SaveChangesAsync(default))
                   .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.ShouldNotBeNull();
        result.Title.ShouldBe(command.Title);
        result.Description.ShouldBe(maxLengthDescription);
        result.Description.ShouldNotBeNull();
        result.Description.Length.ShouldBe(TaskValidationConstants.MaxDescriptionLength);

        _mockDbSet.Verify(d => d.Add(It.Is<UserTask>(t =>
            t.Title == command.Title &&
            t.Description == maxLengthDescription &&
            t.UserId == command.UserId
        )), Times.Once);

        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithBothTitleAndDescriptionExceedingMaxLength_ShouldThrowValidationException()
    {
        // Arrange
        var longTitle = new string('A', TaskValidationConstants.MaxTitleLength + 1); // Exceeds title limit
        var longDescription = new string('B', TaskValidationConstants.MaxDescriptionLength + 1); // Exceeds description limit
        var command = new CreateTaskCommand(
            UserId: Guid.NewGuid(),
            Title: longTitle,
            Description: longDescription
        );

        var validationErrors = new List<ValidationFailure>
        {
            new ValidationFailure("Title", $"Title cannot exceed {TaskValidationConstants.MaxTitleLength} characters"),
            new ValidationFailure("Description", $"Description cannot exceed {TaskValidationConstants.MaxDescriptionLength} characters")
        };
        var validationResult = new ValidationResult(validationErrors);

        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<UserTask>(), default))
                     .ReturnsAsync(validationResult);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _handler.HandleAsync(command));

        exception.Errors.Count().ShouldBe(2);
        exception.Errors.ShouldContain(e => e.ErrorMessage == $"Title cannot exceed {TaskValidationConstants.MaxTitleLength} characters");
        exception.Errors.ShouldContain(e => e.ErrorMessage == $"Description cannot exceed {TaskValidationConstants.MaxDescriptionLength} characters");

        _mockDbSet.Verify(d => d.Add(It.IsAny<UserTask>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Never);
    }
}
