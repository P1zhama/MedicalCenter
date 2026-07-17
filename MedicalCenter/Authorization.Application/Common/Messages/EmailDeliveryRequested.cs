namespace Authorization.Application.Common.Messages;

public record EmailDeliveryRequested(string ToEmail, string Subject, string Body);
