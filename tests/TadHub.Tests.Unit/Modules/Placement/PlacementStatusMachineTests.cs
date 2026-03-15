using Placement.Core.Entities;
using Placement.Core.Services;

namespace TadHub.Tests.Unit.Modules.Placement;

public class PlacementStatusMachineTests
{
    #region Outside-country valid transitions

    [Theory]
    [InlineData(PlacementStatus.Booked, PlacementStatus.ContractCreated)]
    [InlineData(PlacementStatus.ContractCreated, PlacementStatus.EmploymentVisaProcessing)]
    [InlineData(PlacementStatus.EmploymentVisaProcessing, PlacementStatus.TicketArranged)]
    [InlineData(PlacementStatus.TicketArranged, PlacementStatus.Arrived)]
    [InlineData(PlacementStatus.Arrived, PlacementStatus.Deployed)]
    [InlineData(PlacementStatus.Deployed, PlacementStatus.FullPaymentReceived)]
    [InlineData(PlacementStatus.FullPaymentReceived, PlacementStatus.ResidenceVisaProcessing)]
    [InlineData(PlacementStatus.ResidenceVisaProcessing, PlacementStatus.EmiratesIdProcessing)]
    [InlineData(PlacementStatus.EmiratesIdProcessing, PlacementStatus.Completed)]
    public void Validate_OutsideCountryFlow_ValidTransitions_ReturnsNull(PlacementStatus from, PlacementStatus to)
    {
        var result = PlacementStatusMachine.Validate(from, to, reason: null);
        result.Should().BeNull();
    }

    #endregion

    #region Inside-country valid transitions

    [Theory]
    [InlineData(PlacementStatus.Booked, PlacementStatus.InTrial)]
    [InlineData(PlacementStatus.InTrial, PlacementStatus.TrialSuccessful)]
    [InlineData(PlacementStatus.TrialSuccessful, PlacementStatus.ContractCreated)]
    [InlineData(PlacementStatus.ContractCreated, PlacementStatus.StatusChanged)]
    [InlineData(PlacementStatus.StatusChanged, PlacementStatus.EmploymentVisaProcessing)]
    [InlineData(PlacementStatus.EmploymentVisaProcessing, PlacementStatus.ResidenceVisaProcessing)]
    [InlineData(PlacementStatus.ResidenceVisaProcessing, PlacementStatus.EmiratesIdProcessing)]
    [InlineData(PlacementStatus.EmiratesIdProcessing, PlacementStatus.Completed)]
    public void Validate_InsideCountryFlow_ValidTransitions_ReturnsNull(PlacementStatus from, PlacementStatus to)
    {
        var result = PlacementStatusMachine.Validate(from, to, reason: null);
        result.Should().BeNull();
    }

    #endregion

    #region Cancellation transitions

    [Theory]
    [InlineData(PlacementStatus.Booked)]
    [InlineData(PlacementStatus.InTrial)]
    [InlineData(PlacementStatus.TrialSuccessful)]
    [InlineData(PlacementStatus.ContractCreated)]
    [InlineData(PlacementStatus.StatusChanged)]
    [InlineData(PlacementStatus.EmploymentVisaProcessing)]
    [InlineData(PlacementStatus.TicketArranged)]
    [InlineData(PlacementStatus.InTransit)]
    [InlineData(PlacementStatus.Arrived)]
    [InlineData(PlacementStatus.Deployed)]
    [InlineData(PlacementStatus.FullPaymentReceived)]
    [InlineData(PlacementStatus.ResidenceVisaProcessing)]
    [InlineData(PlacementStatus.EmiratesIdProcessing)]
    [InlineData(PlacementStatus.MedicalInProgress)]
    [InlineData(PlacementStatus.MedicalCleared)]
    [InlineData(PlacementStatus.GovtProcessing)]
    [InlineData(PlacementStatus.GovtCleared)]
    [InlineData(PlacementStatus.Training)]
    [InlineData(PlacementStatus.ReadyForPlacement)]
    [InlineData(PlacementStatus.Placed)]
    public void Validate_CancelledWithReason_ReturnsNull(PlacementStatus from)
    {
        var result = PlacementStatusMachine.Validate(from, PlacementStatus.Cancelled, reason: "Customer request");
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(PlacementStatus.Booked)]
    [InlineData(PlacementStatus.ContractCreated)]
    [InlineData(PlacementStatus.Deployed)]
    public void Validate_CancelledWithoutReason_ReturnsError(PlacementStatus from)
    {
        var result = PlacementStatusMachine.Validate(from, PlacementStatus.Cancelled, reason: null);
        result.Should().NotBeNull();
        result!.ToLower().Should().Contain("reason is required");
    }

    #endregion

    #region Invalid transitions

    [Theory]
    [InlineData(PlacementStatus.Booked, PlacementStatus.Deployed)]
    [InlineData(PlacementStatus.Booked, PlacementStatus.Completed)]
    [InlineData(PlacementStatus.Booked, PlacementStatus.Arrived)]
    [InlineData(PlacementStatus.InTrial, PlacementStatus.ContractCreated)]
    [InlineData(PlacementStatus.InTrial, PlacementStatus.Deployed)]
    [InlineData(PlacementStatus.TrialSuccessful, PlacementStatus.Deployed)]
    [InlineData(PlacementStatus.ContractCreated, PlacementStatus.Arrived)]
    [InlineData(PlacementStatus.TicketArranged, PlacementStatus.Deployed)]
    [InlineData(PlacementStatus.Deployed, PlacementStatus.Arrived)]
    [InlineData(PlacementStatus.FullPaymentReceived, PlacementStatus.Deployed)]
    [InlineData(PlacementStatus.ResidenceVisaProcessing, PlacementStatus.Completed)]
    public void Validate_InvalidTransition_ReturnsError(PlacementStatus from, PlacementStatus to)
    {
        var result = PlacementStatusMachine.Validate(from, to, reason: "Some reason");
        result.Should().NotBeNull();
        result.Should().Contain("not allowed");
    }

    #endregion

    #region Terminal states

    [Theory]
    [InlineData(PlacementStatus.Completed)]
    [InlineData(PlacementStatus.Cancelled)]
    public void Validate_FromTerminalState_ReturnsError(PlacementStatus terminalStatus)
    {
        var result = PlacementStatusMachine.Validate(terminalStatus, PlacementStatus.Booked, reason: null);
        result.Should().NotBeNull();
        result.Should().Contain("terminal");
    }

    [Theory]
    [InlineData(PlacementStatus.Completed)]
    [InlineData(PlacementStatus.Cancelled)]
    public void IsTerminal_TerminalStatuses_ReturnsTrue(PlacementStatus status)
    {
        PlacementStatusMachine.IsTerminal(status).Should().BeTrue();
    }

    [Theory]
    [InlineData(PlacementStatus.Booked)]
    [InlineData(PlacementStatus.ContractCreated)]
    [InlineData(PlacementStatus.Deployed)]
    [InlineData(PlacementStatus.InTrial)]
    [InlineData(PlacementStatus.EmiratesIdProcessing)]
    public void IsTerminal_NonTerminalStatuses_ReturnsFalse(PlacementStatus status)
    {
        PlacementStatusMachine.IsTerminal(status).Should().BeFalse();
    }

    #endregion

    #region GetAllowedTransitions

    [Fact]
    public void GetAllowedTransitions_Booked_IncludesContractCreatedAndInTrial()
    {
        var transitions = PlacementStatusMachine.GetAllowedTransitions(PlacementStatus.Booked);
        transitions.Should().Contain(PlacementStatus.ContractCreated);
        transitions.Should().Contain(PlacementStatus.InTrial);
        transitions.Should().Contain(PlacementStatus.Cancelled);
    }

    [Fact]
    public void GetAllowedTransitions_ContractCreated_IncludesEmploymentVisaAndStatusChanged()
    {
        var transitions = PlacementStatusMachine.GetAllowedTransitions(PlacementStatus.ContractCreated);
        transitions.Should().Contain(PlacementStatus.EmploymentVisaProcessing);
        transitions.Should().Contain(PlacementStatus.StatusChanged);
        transitions.Should().Contain(PlacementStatus.Cancelled);
    }

    [Fact]
    public void GetAllowedTransitions_EmploymentVisaProcessing_IncludesTicketAndResidenceVisa()
    {
        var transitions = PlacementStatusMachine.GetAllowedTransitions(PlacementStatus.EmploymentVisaProcessing);
        transitions.Should().Contain(PlacementStatus.TicketArranged);
        transitions.Should().Contain(PlacementStatus.ResidenceVisaProcessing);
        transitions.Should().Contain(PlacementStatus.Cancelled);
    }

    [Fact]
    public void GetAllowedTransitions_TerminalState_ReturnsEmpty()
    {
        PlacementStatusMachine.GetAllowedTransitions(PlacementStatus.Completed).Should().BeEmpty();
        PlacementStatusMachine.GetAllowedTransitions(PlacementStatus.Cancelled).Should().BeEmpty();
    }

    #endregion

    #region GetNextStep

    [Theory]
    [InlineData(PlacementStatus.Booked, PlacementStatus.ContractCreated)]
    [InlineData(PlacementStatus.ContractCreated, PlacementStatus.EmploymentVisaProcessing)]
    [InlineData(PlacementStatus.EmploymentVisaProcessing, PlacementStatus.TicketArranged)]
    [InlineData(PlacementStatus.TicketArranged, PlacementStatus.Arrived)]
    [InlineData(PlacementStatus.Arrived, PlacementStatus.Deployed)]
    [InlineData(PlacementStatus.Deployed, PlacementStatus.FullPaymentReceived)]
    [InlineData(PlacementStatus.FullPaymentReceived, PlacementStatus.ResidenceVisaProcessing)]
    [InlineData(PlacementStatus.ResidenceVisaProcessing, PlacementStatus.EmiratesIdProcessing)]
    public void GetNextStep_OutsideCountry_ReturnsCorrectNext(PlacementStatus current, PlacementStatus expectedNext)
    {
        var next = PlacementStatusMachine.GetNextStep(current, PlacementFlowType.OutsideCountry);
        next.Should().Be(expectedNext);
    }

    [Fact]
    public void GetNextStep_OutsideCountry_LastStep_ReturnsCompleted()
    {
        var next = PlacementStatusMachine.GetNextStep(PlacementStatus.EmiratesIdProcessing, PlacementFlowType.OutsideCountry);
        next.Should().Be(PlacementStatus.Completed);
    }

    [Theory]
    [InlineData(PlacementStatus.Booked, PlacementStatus.InTrial)]
    [InlineData(PlacementStatus.InTrial, PlacementStatus.TrialSuccessful)]
    [InlineData(PlacementStatus.TrialSuccessful, PlacementStatus.ContractCreated)]
    [InlineData(PlacementStatus.ContractCreated, PlacementStatus.StatusChanged)]
    [InlineData(PlacementStatus.StatusChanged, PlacementStatus.EmploymentVisaProcessing)]
    [InlineData(PlacementStatus.EmploymentVisaProcessing, PlacementStatus.ResidenceVisaProcessing)]
    [InlineData(PlacementStatus.ResidenceVisaProcessing, PlacementStatus.EmiratesIdProcessing)]
    public void GetNextStep_InsideCountry_ReturnsCorrectNext(PlacementStatus current, PlacementStatus expectedNext)
    {
        var next = PlacementStatusMachine.GetNextStep(current, PlacementFlowType.InsideCountry);
        next.Should().Be(expectedNext);
    }

    [Fact]
    public void GetNextStep_InsideCountry_LastStep_ReturnsCompleted()
    {
        var next = PlacementStatusMachine.GetNextStep(PlacementStatus.EmiratesIdProcessing, PlacementFlowType.InsideCountry);
        next.Should().Be(PlacementStatus.Completed);
    }

    [Fact]
    public void GetNextStep_StatusNotInPipeline_ReturnsNull()
    {
        var next = PlacementStatusMachine.GetNextStep(PlacementStatus.Completed, PlacementFlowType.OutsideCountry);
        next.Should().BeNull();
    }

    #endregion

    #region GetStepNumber

    [Theory]
    [InlineData(PlacementStatus.Booked, PlacementFlowType.OutsideCountry, 1)]
    [InlineData(PlacementStatus.ContractCreated, PlacementFlowType.OutsideCountry, 2)]
    [InlineData(PlacementStatus.EmploymentVisaProcessing, PlacementFlowType.OutsideCountry, 3)]
    [InlineData(PlacementStatus.TicketArranged, PlacementFlowType.OutsideCountry, 4)]
    [InlineData(PlacementStatus.Arrived, PlacementFlowType.OutsideCountry, 5)]
    [InlineData(PlacementStatus.Deployed, PlacementFlowType.OutsideCountry, 6)]
    [InlineData(PlacementStatus.FullPaymentReceived, PlacementFlowType.OutsideCountry, 7)]
    [InlineData(PlacementStatus.ResidenceVisaProcessing, PlacementFlowType.OutsideCountry, 8)]
    [InlineData(PlacementStatus.EmiratesIdProcessing, PlacementFlowType.OutsideCountry, 9)]
    [InlineData(PlacementStatus.Booked, PlacementFlowType.InsideCountry, 1)]
    [InlineData(PlacementStatus.InTrial, PlacementFlowType.InsideCountry, 2)]
    [InlineData(PlacementStatus.TrialSuccessful, PlacementFlowType.InsideCountry, 3)]
    [InlineData(PlacementStatus.ContractCreated, PlacementFlowType.InsideCountry, 4)]
    [InlineData(PlacementStatus.StatusChanged, PlacementFlowType.InsideCountry, 5)]
    [InlineData(PlacementStatus.EmploymentVisaProcessing, PlacementFlowType.InsideCountry, 6)]
    [InlineData(PlacementStatus.ResidenceVisaProcessing, PlacementFlowType.InsideCountry, 7)]
    [InlineData(PlacementStatus.EmiratesIdProcessing, PlacementFlowType.InsideCountry, 8)]
    public void GetStepNumber_ReturnsCorrectStepNumber(PlacementStatus status, PlacementFlowType flowType, int expected)
    {
        PlacementStatusMachine.GetStepNumber(status, flowType).Should().Be(expected);
    }

    [Fact]
    public void GetStepNumber_StatusNotInPipeline_ReturnsZero()
    {
        PlacementStatusMachine.GetStepNumber(PlacementStatus.Completed, PlacementFlowType.OutsideCountry).Should().Be(0);
        PlacementStatusMachine.GetStepNumber(PlacementStatus.Cancelled, PlacementFlowType.InsideCountry).Should().Be(0);
    }

    #endregion

    #region Pipeline arrays

    [Fact]
    public void OutsideCountryPipeline_Has9Steps()
    {
        PlacementStatusMachine.OutsideCountryPipeline.Should().HaveCount(9);
        PlacementStatusMachine.OutsideCountryPipeline[0].Should().Be(PlacementStatus.Booked);
        PlacementStatusMachine.OutsideCountryPipeline[^1].Should().Be(PlacementStatus.EmiratesIdProcessing);
    }

    [Fact]
    public void InsideCountryPipeline_Has8Steps()
    {
        PlacementStatusMachine.InsideCountryPipeline.Should().HaveCount(8);
        PlacementStatusMachine.InsideCountryPipeline[0].Should().Be(PlacementStatus.Booked);
        PlacementStatusMachine.InsideCountryPipeline[^1].Should().Be(PlacementStatus.EmiratesIdProcessing);
    }

    [Fact]
    public void GetPipeline_InsideCountry_ReturnsInsidePipeline()
    {
        PlacementStatusMachine.GetPipeline(PlacementFlowType.InsideCountry)
            .Should().BeEquivalentTo(PlacementStatusMachine.InsideCountryPipeline);
    }

    [Fact]
    public void GetPipeline_OutsideCountry_ReturnsOutsidePipeline()
    {
        PlacementStatusMachine.GetPipeline(PlacementFlowType.OutsideCountry)
            .Should().BeEquivalentTo(PlacementStatusMachine.OutsideCountryPipeline);
    }

    #endregion

    #region IsReasonRequired

    [Fact]
    public void IsReasonRequired_Cancelled_ReturnsTrue()
    {
        PlacementStatusMachine.IsReasonRequired(PlacementStatus.Cancelled).Should().BeTrue();
    }

    [Theory]
    [InlineData(PlacementStatus.Booked)]
    [InlineData(PlacementStatus.ContractCreated)]
    [InlineData(PlacementStatus.Completed)]
    [InlineData(PlacementStatus.Deployed)]
    public void IsReasonRequired_NonCancelled_ReturnsFalse(PlacementStatus status)
    {
        PlacementStatusMachine.IsReasonRequired(status).Should().BeFalse();
    }

    #endregion

    #region Legacy transitions

    [Theory]
    [InlineData(PlacementStatus.TicketArranged, PlacementStatus.InTransit)]
    [InlineData(PlacementStatus.InTransit, PlacementStatus.Arrived)]
    [InlineData(PlacementStatus.Arrived, PlacementStatus.MedicalInProgress)]
    [InlineData(PlacementStatus.MedicalInProgress, PlacementStatus.MedicalCleared)]
    [InlineData(PlacementStatus.MedicalCleared, PlacementStatus.GovtProcessing)]
    [InlineData(PlacementStatus.GovtProcessing, PlacementStatus.GovtCleared)]
    [InlineData(PlacementStatus.GovtCleared, PlacementStatus.Training)]
    [InlineData(PlacementStatus.GovtCleared, PlacementStatus.ReadyForPlacement)]
    [InlineData(PlacementStatus.Training, PlacementStatus.ReadyForPlacement)]
    [InlineData(PlacementStatus.ReadyForPlacement, PlacementStatus.Placed)]
    [InlineData(PlacementStatus.ReadyForPlacement, PlacementStatus.Deployed)]
    [InlineData(PlacementStatus.Placed, PlacementStatus.Completed)]
    public void Validate_LegacyTransitions_ReturnsNull(PlacementStatus from, PlacementStatus to)
    {
        var result = PlacementStatusMachine.Validate(from, to, reason: null);
        result.Should().BeNull();
    }

    #endregion
}
