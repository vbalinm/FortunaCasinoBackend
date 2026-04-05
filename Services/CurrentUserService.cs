using System.Security.Claims;
using FortunaCasino.Services.Interfaces;

namespace FortunaCasino.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long GetUserId()
    {
        if (!TryGetUserId(out var userId))
            throw new UnauthorizedAccessException("Nem bejelentkezett felhasználó.");
        return userId;
    }

    public bool TryGetUserId(out long userId)
    {
        userId = 0;
        var claim = _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier);

        if (claim == null) return false;
        return long.TryParse(claim.Value, out userId);
    }
}