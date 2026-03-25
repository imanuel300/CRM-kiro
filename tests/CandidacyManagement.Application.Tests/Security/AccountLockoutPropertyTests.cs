using System.Linq.Expressions;
using CandidacyManagement.Application.Security;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using FsCheck;
using FsCheck.Xunit;
using Moq;

namespace CandidacyManagement.Application.Tests.Security;

/// <summary>
/// Feature: unified-candidacy-management, Property 11: נעילת חשבון (Account Lockout)
/// 
/// **Validates: Requirements 18.5**
/// 
/// For any user account, after 5 consecutive failed login attempts the account is locked
/// and even a correct password login attempt fails.
/// </summary>
public class AccountLockoutPropertyTests
{
    /// <summary>
    /// Data container for a generated account lockout scenario.
    /// </summary>
    public record AccountLockoutScenario(
        string Username,
        string CorrectPassword,
        string[] WrongPasswords);

    /// <summary>
    /// Custom Arbitrary that generates scenarios with a username, a correct password,
    /// and exactly 5 wrong passwords (all different from the correct one).
    /// </summary>
    private static Arbitrary<AccountLockoutScenario> AccountLockoutScenarioArb()
    {
        var usernameGen = Gen.Elements(
            "admin", "user1", "manager", "clerk", "auditor",
            "judge_assistant", "rep_coordinator", "sys_admin",
            "unit_head", "secretary", "reviewer", "operator");

        var correctPasswordGen = Gen.Elements(
            "Correct!Pass1", "MyP@ss2024", "Str0ng#Key", "Valid$ecure9",
            "Auth!Token7", "S@feLogin3", "P@ssW0rd!", "Secur3#Me");

        var wrongPasswordGen = Gen.Elements(
            "wrong1", "bad_pass", "incorrect!", "nope123",
            "fail_attempt", "invalid#1", "guess_wrong", "not_right",
            "try_again", "bad_guess", "wrong_pw!", "miss_123");

        return Arb.From(
            from username in usernameGen
            from correctPassword in correctPasswordGen
            from w1 in wrongPasswordGen
            from w2 in wrongPasswordGen
            from w3 in wrongPasswordGen
            from w4 in wrongPasswordGen
            from w5 in wrongPasswordGen
            let wrongPasswords = new[] { w1, w2, w3, w4, w5 }
            where wrongPasswords.All(wp => wp != correctPassword)
            select new AccountLockoutScenario(username, correctPassword, wrongPasswords));
    }

    /// <summary>
    /// Creates a SecurityService with mocked repositories wired to track a single UserAccount.
    /// </summary>
    private static (SecurityService Service, UserAccount Account) CreateServiceWithAccount(
        string username, string correctPassword)
    {
        var account = new UserAccount
        {
            Id = 1,
            Username = username,
            PasswordHash = SecurityService.ComputeHash(correctPassword),
            FailedLoginAttempts = 0,
            IsLocked = false,
            IsAdmin = false,
            MfaEnabled = false,
            OrgUnitId = 1
        };

        var userAccountRepoMock = new Mock<IRepository<UserAccount>>();
        userAccountRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserAccount, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { account });
        userAccountRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<UserAccount>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAccount a, CancellationToken _) => a);

        var accessLogRepoMock = new Mock<IRepository<PersonalDataAccessLog>>();
        var contactRepoMock = new Mock<IRepository<Contact>>();
        var auditLogRepoMock = new Mock<IRepository<AuditLogEntry>>();
        auditLogRepoMock
            .Setup(r => r.AddAsync(It.IsAny<AuditLogEntry>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuditLogEntry e, CancellationToken _) => e);

        var service = new SecurityService(
            userAccountRepoMock.Object,
            accessLogRepoMock.Object,
            contactRepoMock.Object,
            auditLogRepoMock.Object);

        return (service, account);
    }

    /// <summary>
    /// Feature: unified-candidacy-management, Property 11: נעילת חשבון
    /// **Validates: Requirements 18.5**
    /// 
    /// After 5 consecutive failed login attempts, the account is locked and
    /// even a correct password login attempt fails.
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(AccountLockoutPropertyTests) })]
    public async Task<bool> AccountLockedAfterFiveFailedAttemptsRejectsCorrectPassword(
        AccountLockoutScenario scenario)
    {
        var (service, account) = CreateServiceWithAccount(scenario.Username, scenario.CorrectPassword);

        // Perform 5 failed login attempts with wrong passwords
        for (int i = 0; i < scenario.WrongPasswords.Length; i++)
        {
            var failResult = await service.LoginAsync(
                new LoginCommand(scenario.Username, scenario.WrongPasswords[i]));

            if (failResult.Success)
                return false; // Wrong password should never succeed
        }

        // Verify the account is now locked
        if (!account.IsLocked)
            return false;

        if (account.FailedLoginAttempts != 5)
            return false;

        // Attempt login with the correct password — must still fail because account is locked
        var correctPasswordResult = await service.LoginAsync(
            new LoginCommand(scenario.Username, scenario.CorrectPassword));

        return !correctPasswordResult.Success;
    }

    // Expose the Arbitrary for FsCheck discovery
    public static Arbitrary<AccountLockoutScenario> Arbitrary() => AccountLockoutScenarioArb();
}
