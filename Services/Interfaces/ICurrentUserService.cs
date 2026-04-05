namespace FortunaCasino.Services.Interfaces;

public interface ICurrentUserService
{
    long GetUserId();
    bool TryGetUserId(out long userId);
}
