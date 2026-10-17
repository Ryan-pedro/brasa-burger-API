namespace Brasa.Application.Identity;

public sealed record PasswordCheckResult(PasswordCheckOutcome Outcome, UserInfo? User);
