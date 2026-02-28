namespace Financial.Core.Entities;

public enum InvoiceStatus
{
    Draft,
    Issued,
    PartiallyPaid,
    Paid,
    Overdue,
    Cancelled,
    Refunded,
}

public enum InvoiceType
{
    Standard,
    CreditNote,
    ProformaDeposit,
}

public enum MilestoneType
{
    AdvanceDeposit,
    ActivationBalance,
    Installment,
    FullPayment,
}
