using AwesomeAssertions;
using Humans.Domain.Entities;
using Humans.Domain.Enums;
using NodaTime;
using NodaTime.Testing;
using Xunit;

namespace Humans.Domain.Tests.Entities;

public class GuideEventTests
{
    private readonly FakeClock _clock;

    public GuideEventTests()
    {
        _clock = new FakeClock(Instant.FromUtc(2026, 3, 18, 12, 0));
    }

    [Fact]
    public void Submit_FromDraft_SetsPendingAndTimestamps()
    {
        var guideEvent = CreateEvent(GuideEventStatus.Draft);

        guideEvent.Submit(_clock);

        guideEvent.Status.Should().Be(GuideEventStatus.Pending);
        guideEvent.SubmittedAt.Should().Be(_clock.GetCurrentInstant());
        guideEvent.LastUpdatedAt.Should().Be(_clock.GetCurrentInstant());
    }

    [Fact]
    public void Submit_FromWithdrawn_Throws()
    {
        var guideEvent = CreateEvent(GuideEventStatus.Withdrawn);

        var action = () => guideEvent.Submit(_clock);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot submit event in Withdrawn state");
    }

    [Fact]
    public void Withdraw_FromPending_SetsWithdrawnAndLastUpdated()
    {
        var guideEvent = CreateEvent(GuideEventStatus.Pending);

        guideEvent.Withdraw(_clock);

        guideEvent.Status.Should().Be(GuideEventStatus.Withdrawn);
        guideEvent.LastUpdatedAt.Should().Be(_clock.GetCurrentInstant());
    }

    [Fact]
    public void Withdraw_FromApproved_Throws()
    {
        var guideEvent = CreateEvent(GuideEventStatus.Approved);

        var action = () => guideEvent.Withdraw(_clock);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot withdraw event in Approved state");
    }

    [Theory]
    [InlineData(ModerationActionType.Approved, GuideEventStatus.Approved)]
    [InlineData(ModerationActionType.Rejected, GuideEventStatus.Rejected)]
    [InlineData(ModerationActionType.ResubmitRequested, GuideEventStatus.ResubmitRequested)]
    public void ApplyModerationAction_FromPending_TransitionsToExpectedStatus(
        ModerationActionType action,
        GuideEventStatus expectedStatus)
    {
        var guideEvent = CreateEvent(GuideEventStatus.Pending);

        guideEvent.ApplyModerationAction(action, _clock);

        guideEvent.Status.Should().Be(expectedStatus);
        guideEvent.LastUpdatedAt.Should().Be(_clock.GetCurrentInstant());
    }

    [Fact]
    public void ApplyModerationAction_FromDraft_Throws()
    {
        var guideEvent = CreateEvent(GuideEventStatus.Draft);

        var action = () => guideEvent.ApplyModerationAction(ModerationActionType.Approved, _clock);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot moderate event in Draft state");
    }

    [Fact]
    public void ApplyModerationAction_WithUnknownAction_Throws()
    {
        var guideEvent = CreateEvent(GuideEventStatus.Pending);

        var action = () => guideEvent.ApplyModerationAction((ModerationActionType)999, _clock);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Submit_FromApproved_MovesBackToPending()
    {
        var guideEvent = CreateEvent(GuideEventStatus.Approved);

        guideEvent.Submit(_clock);

        guideEvent.Status.Should().Be(GuideEventStatus.Pending);
        guideEvent.SubmittedAt.Should().Be(_clock.GetCurrentInstant());
        guideEvent.LastUpdatedAt.Should().Be(_clock.GetCurrentInstant());
    }

    private GuideEvent CreateEvent(GuideEventStatus status)
    {
        return new GuideEvent
        {
            Id = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            SubmitterUserId = Guid.NewGuid(),
            Title = "Test event",
            Description = "Test description",
            StartAt = Instant.FromUtc(2026, 7, 1, 10, 0),
            DurationMinutes = 60,
            PriorityRank = 1,
            Status = status,
            SubmittedAt = Instant.MinValue,
            LastUpdatedAt = Instant.MinValue
        };
    }
}
