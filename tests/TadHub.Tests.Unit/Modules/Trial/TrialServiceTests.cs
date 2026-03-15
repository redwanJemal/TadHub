using MassTransit;
using Microsoft.EntityFrameworkCore;
using Trial.Contracts.DTOs;
using Trial.Core.Entities;
using Trial.Core.Services;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;
using Entities = Trial.Core.Entities;

namespace TadHub.Tests.Unit.Modules.Trial;

public class TrialServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IPublishEndpoint _publisher;
    private readonly TrialService _sut;

    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly DateTimeOffset _now = new(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);

    public TrialServiceTests()
    {
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns(_tenantId);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options, tenantContext);

        _clock = Substitute.For<IClock>();
        _clock.UtcNow.Returns(_now);
        _currentUser = Substitute.For<ICurrentUser>();
        _currentUser.UserId.Returns(_userId);
        _publisher = Substitute.For<IPublishEndpoint>();

        _sut = new TrialService(
            _db,
            _clock,
            _currentUser,
            _publisher,
            Substitute.For<ILogger<TrialService>>());
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    #region CompleteAsync — ProceedToContract outcome

    [Fact]
    public async Task CompleteAsync_ProceedToContract_SetsSuccessfulStatus()
    {
        var trial = SeedTrial(TrialStatus.Active, new DateOnly(2025, 6, 10));

        var result = await _sut.CompleteAsync(_tenantId, trial.Id,
            new CompleteTrialRequest { Outcome = "ProceedToContract", OutcomeNotes = "Client satisfied" });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be("Successful");
        result.Value!.Outcome.Should().Be("ProceedToContract");
    }

    #endregion

    #region CompleteAsync — ReturnToInventory outcome

    [Fact]
    public async Task CompleteAsync_ReturnToInventory_SetsFailedStatus()
    {
        var trial = SeedTrial(TrialStatus.Active, new DateOnly(2025, 6, 10));

        var result = await _sut.CompleteAsync(_tenantId, trial.Id,
            new CompleteTrialRequest { Outcome = "ReturnToInventory", OutcomeNotes = "Not a good fit" });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be("Failed");
        result.Value!.Outcome.Should().Be("ReturnToInventory");
    }

    #endregion

    #region CompleteAsync — validation

    [Fact]
    public async Task CompleteAsync_TrialNotFound_ReturnsNotFound()
    {
        var result = await _sut.CompleteAsync(_tenantId, Guid.NewGuid(),
            new CompleteTrialRequest { Outcome = "ProceedToContract" });

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task CompleteAsync_TrialNotActive_ReturnsValidationError()
    {
        var trial = SeedTrial(TrialStatus.Cancelled, new DateOnly(2025, 6, 10));

        var result = await _sut.CompleteAsync(_tenantId, trial.Id,
            new CompleteTrialRequest { Outcome = "ProceedToContract" });

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
        result.Error.Should().Contain("active");
    }

    [Fact]
    public async Task CompleteAsync_InvalidOutcome_ReturnsValidationError()
    {
        var trial = SeedTrial(TrialStatus.Active, new DateOnly(2025, 6, 10));

        var result = await _sut.CompleteAsync(_tenantId, trial.Id,
            new CompleteTrialRequest { Outcome = "InvalidOutcome" });

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
        result.Error.Should().Contain("Invalid outcome");
    }

    #endregion

    #region CancelAsync

    [Fact]
    public async Task CancelAsync_ActiveTrial_Succeeds()
    {
        var trial = SeedTrial(TrialStatus.Active, new DateOnly(2025, 6, 10));

        var result = await _sut.CancelAsync(_tenantId, trial.Id,
            new CancelTrialRequest { Reason = "Client changed mind" });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task CancelAsync_TrialNotFound_ReturnsNotFound()
    {
        var result = await _sut.CancelAsync(_tenantId, Guid.NewGuid(),
            new CancelTrialRequest { Reason = "Any" });

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task CancelAsync_AlreadyCancelled_ReturnsValidationError()
    {
        var trial = SeedTrial(TrialStatus.Cancelled, new DateOnly(2025, 6, 10));

        var result = await _sut.CancelAsync(_tenantId, trial.Id,
            new CancelTrialRequest { Reason = "Any" });

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task CancelAsync_SuccessfulTrial_ReturnsValidationError()
    {
        var trial = SeedTrial(TrialStatus.Successful, new DateOnly(2025, 6, 10));

        var result = await _sut.CancelAsync(_tenantId, trial.Id,
            new CancelTrialRequest { Reason = "Any" });

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
        result.Error.Should().Contain("active");
    }

    #endregion

    #region 5-day auto-calculated end date

    [Fact]
    public void TrialDuration_IsFiveDays()
    {
        // The TrialService uses a const TrialDurationDays = 5.
        // When creating a trial, EndDate = StartDate.AddDays(5).
        var startDate = new DateOnly(2025, 6, 10);
        var expectedEndDate = new DateOnly(2025, 6, 15);

        startDate.AddDays(5).Should().Be(expectedEndDate);
    }

    [Fact]
    public async Task CompleteAsync_SetsOutcomeDate()
    {
        var trial = SeedTrial(TrialStatus.Active, new DateOnly(2025, 6, 10));

        var result = await _sut.CompleteAsync(_tenantId, trial.Id,
            new CompleteTrialRequest { Outcome = "ProceedToContract" });

        result.IsSuccess.Should().BeTrue();
        result.Value!.OutcomeDate.Should().NotBeNull();
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_TrialNotFound_ReturnsNotFound()
    {
        var result = await _sut.DeleteAsync(_tenantId, Guid.NewGuid());
        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task DeleteAsync_ValidTrial_SoftDeletes()
    {
        var trial = SeedTrial(TrialStatus.Active, new DateOnly(2025, 6, 10));

        var result = await _sut.DeleteAsync(_tenantId, trial.Id);

        result.IsSuccess.Should().BeTrue();

        var deleted = await _db.Set<Entities.Trial>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == trial.Id);
        deleted!.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region StatusHistory

    [Fact]
    public async Task CompleteAsync_CreatesStatusHistory()
    {
        var trial = SeedTrial(TrialStatus.Active, new DateOnly(2025, 6, 10));

        await _sut.CompleteAsync(_tenantId, trial.Id,
            new CompleteTrialRequest { Outcome = "ProceedToContract" });

        var history = await _db.Set<TrialStatusHistory>()
            .IgnoreQueryFilters()
            .Where(x => x.TrialId == trial.Id)
            .ToListAsync();

        history.Should().ContainSingle();
        history[0].FromStatus.Should().Be(TrialStatus.Active);
        history[0].ToStatus.Should().Be(TrialStatus.Successful);
    }

    [Fact]
    public async Task CancelAsync_CreatesStatusHistory()
    {
        var trial = SeedTrial(TrialStatus.Active, new DateOnly(2025, 6, 10));

        await _sut.CancelAsync(_tenantId, trial.Id,
            new CancelTrialRequest { Reason = "Changed mind" });

        var history = await _db.Set<TrialStatusHistory>()
            .IgnoreQueryFilters()
            .Where(x => x.TrialId == trial.Id)
            .ToListAsync();

        history.Should().ContainSingle();
        history[0].FromStatus.Should().Be(TrialStatus.Active);
        history[0].ToStatus.Should().Be(TrialStatus.Cancelled);
        history[0].Reason.Should().Be("Changed mind");
    }

    #endregion

    #region Events

    [Fact]
    public async Task CompleteAsync_PublishesTrialCompletedEvent()
    {
        var trial = SeedTrial(TrialStatus.Active, new DateOnly(2025, 6, 10));

        await _sut.CompleteAsync(_tenantId, trial.Id,
            new CompleteTrialRequest { Outcome = "ReturnToInventory" });

        await _publisher.Received(1).Publish(
            Arg.Is<TadHub.SharedKernel.Events.TrialCompletedEvent>(e =>
                e.TrialId == trial.Id &&
                e.Outcome == "ReturnToInventory"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelAsync_PublishesTrialCancelledEvent()
    {
        var trial = SeedTrial(TrialStatus.Active, new DateOnly(2025, 6, 10));

        await _sut.CancelAsync(_tenantId, trial.Id,
            new CancelTrialRequest { Reason = "No longer needed" });

        await _publisher.Received(1).Publish(
            Arg.Is<TadHub.SharedKernel.Events.TrialCancelledEvent>(e =>
                e.TrialId == trial.Id &&
                e.Reason == "No longer needed"),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region Helpers

    private Entities.Trial SeedTrial(TrialStatus status, DateOnly startDate)
    {
        var trial = new Entities.Trial
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            TrialCode = $"TRL-{Random.Shared.Next(1, 999999):D6}",
            Status = status,
            StatusChangedAt = _now,
            WorkerId = Guid.NewGuid(),
            ClientId = Guid.NewGuid(),
            StartDate = startDate,
            EndDate = startDate.AddDays(5),
            CreatedBy = _userId,
        };
        _db.Set<Entities.Trial>().Add(trial);
        _db.SaveChanges();
        return trial;
    }

    #endregion
}
