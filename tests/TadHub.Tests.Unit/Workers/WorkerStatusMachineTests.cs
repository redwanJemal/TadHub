using Worker.Core.Entities;
using Worker.Core.Services;

namespace TadHub.Tests.Unit.Workers;

public class WorkerStatusMachineTests
{
    #region Valid transitions

    [Theory]
    [InlineData(WorkerStatus.Available, WorkerStatus.Booked)]
    [InlineData(WorkerStatus.Available, WorkerStatus.InTraining)]
    [InlineData(WorkerStatus.Available, WorkerStatus.UnderMedicalTest)]
    [InlineData(WorkerStatus.InTraining, WorkerStatus.Available)]
    [InlineData(WorkerStatus.InTraining, WorkerStatus.UnderMedicalTest)]
    [InlineData(WorkerStatus.UnderMedicalTest, WorkerStatus.Available)]
    [InlineData(WorkerStatus.NewArrival, WorkerStatus.Available)]
    [InlineData(WorkerStatus.NewArrival, WorkerStatus.InTraining)]
    [InlineData(WorkerStatus.Booked, WorkerStatus.Hired)]
    [InlineData(WorkerStatus.Booked, WorkerStatus.NewArrival)]
    [InlineData(WorkerStatus.Booked, WorkerStatus.Available)]
    [InlineData(WorkerStatus.Hired, WorkerStatus.OnProbation)]
    [InlineData(WorkerStatus.Hired, WorkerStatus.Available)]
    [InlineData(WorkerStatus.OnProbation, WorkerStatus.Active)]
    [InlineData(WorkerStatus.Active, WorkerStatus.Renewed)]
    [InlineData(WorkerStatus.Renewed, WorkerStatus.Active)]
    [InlineData(WorkerStatus.PendingReplacement, WorkerStatus.Available)]
    [InlineData(WorkerStatus.MedicallyUnfit, WorkerStatus.Available)]
    [InlineData(WorkerStatus.Absconded, WorkerStatus.Available)]
    [InlineData(WorkerStatus.Terminated, WorkerStatus.Available)]
    [InlineData(WorkerStatus.Pregnant, WorkerStatus.Active)]
    public void Validate_ValidTransitionWithoutReasonRequired_ReturnsNull(WorkerStatus from, WorkerStatus to)
    {
        var result = WorkerStatusMachine.Validate(from, to, reason: null);
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(WorkerStatus.Available, WorkerStatus.Absconded)]
    [InlineData(WorkerStatus.Available, WorkerStatus.Repatriated)]
    [InlineData(WorkerStatus.Available, WorkerStatus.Deceased)]
    [InlineData(WorkerStatus.OnProbation, WorkerStatus.Terminated)]
    [InlineData(WorkerStatus.Active, WorkerStatus.Terminated)]
    [InlineData(WorkerStatus.Active, WorkerStatus.Absconded)]
    [InlineData(WorkerStatus.Active, WorkerStatus.Pregnant)]
    [InlineData(WorkerStatus.Active, WorkerStatus.Transferred)]
    [InlineData(WorkerStatus.Active, WorkerStatus.Deceased)]
    [InlineData(WorkerStatus.Transferred, WorkerStatus.Repatriated)]
    public void Validate_ValidTransitionWithReasonRequired_ReturnsNullWhenReasonProvided(WorkerStatus from, WorkerStatus to)
    {
        var result = WorkerStatusMachine.Validate(from, to, reason: "Valid reason");
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(WorkerStatus.Active, WorkerStatus.Terminated)]
    [InlineData(WorkerStatus.Active, WorkerStatus.Absconded)]
    [InlineData(WorkerStatus.Active, WorkerStatus.Pregnant)]
    [InlineData(WorkerStatus.Active, WorkerStatus.Transferred)]
    [InlineData(WorkerStatus.Active, WorkerStatus.Deceased)]
    [InlineData(WorkerStatus.OnProbation, WorkerStatus.Terminated)]
    [InlineData(WorkerStatus.Available, WorkerStatus.Absconded)]
    [InlineData(WorkerStatus.Available, WorkerStatus.Repatriated)]
    [InlineData(WorkerStatus.Absconded, WorkerStatus.Deported)]
    public void Validate_ReasonRequiredButMissing_ReturnsError(WorkerStatus from, WorkerStatus to)
    {
        var result = WorkerStatusMachine.Validate(from, to, reason: null);
        result.Should().NotBeNull();
        result.Should().Contain("reason is required");
    }

    #endregion

    #region Invalid transitions

    [Theory]
    [InlineData(WorkerStatus.Available, WorkerStatus.Active)]
    [InlineData(WorkerStatus.Available, WorkerStatus.Hired)]
    [InlineData(WorkerStatus.Available, WorkerStatus.OnProbation)]
    [InlineData(WorkerStatus.Booked, WorkerStatus.Active)]
    [InlineData(WorkerStatus.InTraining, WorkerStatus.Booked)]
    [InlineData(WorkerStatus.Hired, WorkerStatus.Active)]
    [InlineData(WorkerStatus.NewArrival, WorkerStatus.Booked)]
    public void Validate_InvalidTransition_ReturnsError(WorkerStatus from, WorkerStatus to)
    {
        var result = WorkerStatusMachine.Validate(from, to, reason: "Some reason");
        result.Should().NotBeNull();
        result.Should().Contain("not allowed");
    }

    #endregion

    #region Terminal states

    [Theory]
    [InlineData(WorkerStatus.Repatriated)]
    [InlineData(WorkerStatus.Deported)]
    [InlineData(WorkerStatus.Deceased)]
    public void Validate_FromTerminalState_ReturnsError(WorkerStatus terminalStatus)
    {
        var result = WorkerStatusMachine.Validate(terminalStatus, WorkerStatus.Available, reason: null);
        result.Should().NotBeNull();
        result.Should().Contain("terminal");
    }

    [Theory]
    [InlineData(WorkerStatus.Repatriated)]
    [InlineData(WorkerStatus.Deported)]
    [InlineData(WorkerStatus.Deceased)]
    public void IsTerminal_TerminalStatuses_ReturnsTrue(WorkerStatus status)
    {
        WorkerStatusMachine.IsTerminal(status).Should().BeTrue();
    }

    [Theory]
    [InlineData(WorkerStatus.Available)]
    [InlineData(WorkerStatus.Active)]
    [InlineData(WorkerStatus.Booked)]
    [InlineData(WorkerStatus.Terminated)]
    public void IsTerminal_NonTerminalStatuses_ReturnsFalse(WorkerStatus status)
    {
        WorkerStatusMachine.IsTerminal(status).Should().BeFalse();
    }

    #endregion

    #region GetAllowedTransitions

    [Fact]
    public void GetAllowedTransitions_Available_ReturnsExpectedTargets()
    {
        var transitions = WorkerStatusMachine.GetAllowedTransitions(WorkerStatus.Available);
        transitions.Should().Contain(WorkerStatus.Booked);
        transitions.Should().Contain(WorkerStatus.InTraining);
        transitions.Should().Contain(WorkerStatus.UnderMedicalTest);
        transitions.Should().NotContain(WorkerStatus.Active);
    }

    [Fact]
    public void GetAllowedTransitions_TerminalState_ReturnsEmpty()
    {
        WorkerStatusMachine.GetAllowedTransitions(WorkerStatus.Repatriated).Should().BeEmpty();
        WorkerStatusMachine.GetAllowedTransitions(WorkerStatus.Deported).Should().BeEmpty();
        WorkerStatusMachine.GetAllowedTransitions(WorkerStatus.Deceased).Should().BeEmpty();
    }

    #endregion

    #region Categories

    [Theory]
    [InlineData(WorkerStatus.Available, WorkerStatusCategory.Pool)]
    [InlineData(WorkerStatus.InTraining, WorkerStatusCategory.Pool)]
    [InlineData(WorkerStatus.UnderMedicalTest, WorkerStatusCategory.Pool)]
    [InlineData(WorkerStatus.NewArrival, WorkerStatusCategory.Arrival)]
    [InlineData(WorkerStatus.Booked, WorkerStatusCategory.Placement)]
    [InlineData(WorkerStatus.Active, WorkerStatusCategory.Placement)]
    [InlineData(WorkerStatus.Terminated, WorkerStatusCategory.NegativeSpecial)]
    [InlineData(WorkerStatus.Absconded, WorkerStatusCategory.NegativeSpecial)]
    [InlineData(WorkerStatus.Repatriated, WorkerStatusCategory.Terminal)]
    [InlineData(WorkerStatus.Deported, WorkerStatusCategory.Terminal)]
    [InlineData(WorkerStatus.Deceased, WorkerStatusCategory.Terminal)]
    public void GetCategory_ReturnsCorrectCategory(WorkerStatus status, WorkerStatusCategory expected)
    {
        WorkerStatusMachine.GetCategory(status).Should().Be(expected);
    }

    #endregion

    #region Reason requirements

    [Theory]
    [InlineData(WorkerStatus.Terminated)]
    [InlineData(WorkerStatus.Absconded)]
    [InlineData(WorkerStatus.MedicallyUnfit)]
    [InlineData(WorkerStatus.PendingReplacement)]
    [InlineData(WorkerStatus.Transferred)]
    [InlineData(WorkerStatus.Repatriated)]
    [InlineData(WorkerStatus.Deported)]
    [InlineData(WorkerStatus.Pregnant)]
    [InlineData(WorkerStatus.Deceased)]
    public void IsReasonRequired_RequiredStatuses_ReturnsTrue(WorkerStatus status)
    {
        WorkerStatusMachine.IsReasonRequired(status).Should().BeTrue();
    }

    [Theory]
    [InlineData(WorkerStatus.Available)]
    [InlineData(WorkerStatus.Active)]
    [InlineData(WorkerStatus.Booked)]
    [InlineData(WorkerStatus.Hired)]
    [InlineData(WorkerStatus.InTraining)]
    [InlineData(WorkerStatus.OnProbation)]
    [InlineData(WorkerStatus.Renewed)]
    [InlineData(WorkerStatus.NewArrival)]
    [InlineData(WorkerStatus.UnderMedicalTest)]
    public void IsReasonRequired_NonRequiredStatuses_ReturnsFalse(WorkerStatus status)
    {
        WorkerStatusMachine.IsReasonRequired(status).Should().BeFalse();
    }

    #endregion
}
