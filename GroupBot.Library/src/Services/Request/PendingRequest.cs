namespace GroupBot.Library.Services.Request;

public readonly record struct PendingRequest(
    long TargetUserTelegramId,
    long UserDbId,
    long TargetUserDbId,
    long ListDbId)
{
    public long TargetUserTelegramId { get; } = TargetUserTelegramId;
    public long UserDbId { get; } = UserDbId;
    public long TargetUserDbId { get; } = TargetUserDbId;
    public long ListDbId { get; } = ListDbId;
}
