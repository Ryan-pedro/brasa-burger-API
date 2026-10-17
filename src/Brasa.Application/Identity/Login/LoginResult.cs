using Brasa.Application.Identity.Session;

namespace Brasa.Application.Identity.Login;

public sealed record LoginResult(SessionResponse? Session, LoginFailure? Failure)
{
    public static LoginResult Succeeded(SessionResponse session) => new(session, null);

    public static LoginResult Failed(LoginFailure failure) => new(null, failure);
}
