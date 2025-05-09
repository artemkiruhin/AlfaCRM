namespace AlfaCRM.Api.Contracts.Request;

public record PinMessageApiRequest(Guid MessageId, bool IsPinned);