namespace Financial.Core.Entities;

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Refunded,
    Cancelled,
}

public enum PaymentMethod
{
    Cash,
    Card,
    BankTransfer,
    Cheque,
    EDirham,
    Online,
}
