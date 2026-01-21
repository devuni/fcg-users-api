namespace Contracts.Events;

public enum PaymentStatus { Approved, Rejected }

public record PaymentProcessedEvent(Guid OrderId, Guid UserId, Guid GameId, decimal Price, PaymentStatus Status);
