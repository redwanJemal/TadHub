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
    [InlineData(WorkerStatus.NewArrival, WorkerStatus.InAccommodation)]
    [InlineData(WorkerStatus.Booked, WorkerStatus.Hired)]
    [InlineData(WorkerStatus.Booked, WorkerStatus.VisaProcessing)]
    [InlineData(WorkerStatus.Booked, WorkerStatus.NewArrival)]
    [InlineData(WorkerStatus.Booked, WorkerStatus.Available)]
    [InlineData(WorkerStatus.VisaProcessing, WorkerStatus.Traveling)]
    [InlineData(WorkerStatus.VisaProcessing, WorkerStatus.Available)]
    [InlineData(WorkerStatus.Traveling, WorkerStatus.NewArrival)]
    [InlineData(WorkerStatus.Traveling, WorkerStatus.Available)]
    [InlineData(WorkerStatus.InAccommodation, WorkerStatus.Active)]
    [InlineData(WorkerStatus.InAccommodation, WorkerStatus.Available)]
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
    [InlineData(WorkerStatus.Active, WorkerStatus.Returnee)]
    [InlineData(WorkerStatus.Renewed, WorkerStatus.Returnee)]
    [InlineData(WorkerStatus.Transferred, WorkerStatus.Repatriated)]
    [InlineData(WorkerStatus.Returnee, WorkerStatus.Repatriated)]
    [InlineData(WorkerStatus.Returnee, WorkerStatus.Deceased)]
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
    [InlineData(WorkerStatus.Active, WorkerStatus.Returnee)]
    [InlineData(WorkerStatus.OnProbation, WorkerStatus.Terminated)]
    [InlineData(WorkerStatus.Available, WorkerStatus.Absconded)]
    [InlineData(WorkerStatus.Available, WorkerStatus.Repatriated)]
    [InlineData(WorkerStatus.Absconded, WorkerStatus.Deported)]
    [InlineData(WorkerStatus.Returnee, WorkerStatus.Repatriated)]
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
    [InlineData(WorkerStatus.VisaProcessing, WorkerStatus.Active)]
    [InlineData(WorkerStatus.Traveling, WorkerStatus.Active)]
    [InlineData(WorkerStatus.InAccommodation, WorkerStatus.Booked)]
    [InlineData(WorkerStatus.Returnee, WorkerStatus.Active)]
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
    [InlineData(WorkerStatus.VisaProcessing)]
    [InlineData(WorkerStatus.Traveling)]
    [InlineData(WorkerStatus.InAccommodation)]
    [InlineData(WorkerStatus.Returnee)]
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
    public void GetAllowedTransitions_Booked_IncludesVisaProcessing()
    {
        var transitions = WorkerStatusMachine.GetAllowedTransitions(WorkerStatus.Booked);
        transitions.Should().Contain(WorkerStatus.VisaProcessing);
        transitions.Should().Contain(WorkerStatus.Hired);
        transitions.Should().Contain(WorkerStatus.Available);
    }

    [Fact]
    public void GetAllowedTransitions_Active_IncludesReturnee()
    {
        var transitions = WorkerStatusMachine.GetAllowedTransitions(WorkerStatus.Active);
        transitions.Should().Contain(WorkerStatus.Returnee);
        transitions.Should().Contain(WorkerStatus.Renewed);
    }

    [Fact]
    public void GetAllowedTransitions_Returnee_IncludesAvailableAndRepatriated()
    {
        var transitions = WorkerStatusMachine.GetAllowedTransitions(WorkerStatus.Returnee);
        transitions.Should().Contain(WorkerStatus.Available);
        transitions.Should().Contain(WorkerStatus.Repatriated);
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
    [InlineData(WorkerStatus.VisaProcessing, WorkerStatusCategory.Arrival)]
    [InlineData(WorkerStatus.Traveling, WorkerStatusCategory.Arrival)]
    [InlineData(WorkerStatus.InAccommodation, WorkerStatusCategory.Arrival)]
    [InlineData(WorkerStatus.Booked, WorkerStatusCategory.Placement)]
    [InlineData(WorkerStatus.Active, WorkerStatusCategory.Placement)]
    [InlineData(WorkerStatus.Terminated, WorkerStatusCategory.NegativeSpecial)]
    [InlineData(WorkerStatus.Absconded, WorkerStatusCategory.NegativeSpecial)]
    [InlineData(WorkerStatus.Returnee, WorkerStatusCategory.NegativeSpecial)]
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
    [InlineData(WorkerStatus.Returnee)]
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
    [InlineData(WorkerStatus.VisaProcessing)]
    [InlineData(WorkerStatus.Traveling)]
    [InlineData(WorkerStatus.InAccommodation)]
    public void IsReasonRequired_NonRequiredStatuses_ReturnsFalse(WorkerStatus status)
    {
        WorkerStatusMachine.IsReasonRequired(status).Should().BeFalse();
    }

    #endregion

    #region Lifecycle stages

    [Fact]
    public void GetLifecycleStages_ReturnsOrderedStages()
    {
        var stages = WorkerStatusMachine.GetLifecycleStages();
        stages.Should().HaveCount(7);
        stages[0].Should().Be(WorkerStatus.Available);
        stages[1].Should().Be(WorkerStatus.Booked);
        stages[2].Should().Be(WorkerStatus.VisaProcessing);
        stages[3].Should().Be(WorkerStatus.Traveling);
        stages[4].Should().Be(WorkerStatus.NewArrival);
        stages[5].Should().Be(WorkerStatus.InAccommodation);
        stages[6].Should().Be(WorkerStatus.Active);
    }

    #endregion
}
