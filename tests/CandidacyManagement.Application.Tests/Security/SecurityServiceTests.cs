using System.Linq.Expressions;
using CandidacyManagement.Application.Security;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.Security;

public class SecurityServiceTests
{
    private readonly Mock<IRepository<UserAccount>> _userAccountRepoMock;
    private readonly Mock<IRepository<PersonalDataAccessLog>> _accessLogRepoMock;
    private readonly Mock<IRepository<Contact>> _contactRepoMock;
    private readonly Mock<IRepository<AuditLogEntry>> _auditLogRepoMock;
    private readonly SecurityService _sut;

    public SecurityServiceTests()
    {
        _userAccountRepoMock = new Mock<IRepository<UserAccount>>();
        _accessLogRepoMock = new Mock<IRepository<PersonalDataAccessLog>>();
        _contactRepoMock = new Mock<IRepository<Contact>>();
        _auditLogRepoMock = new Mock<IRepository<AuditLogEntry>>();

        _auditLogRepoMock.Setup(r => r.AddAsync(It.IsAny<AuditLogEntry>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuditLogEntry e, CancellationToken _) => e);

        _sut = new SecurityService(
            _userAccountRepoMock.Object,
            _accessLogRepoMock.Object,
            _contactRepoMock.Object,
            _auditLogRepoMock.Object);
    }

    private static UserAccount CreateAccount(string username = "testuser", string password = "Pass123!", bool isAdmin = false, bool mfaEnabled = false)
    {
        return new UserAccount
        {
            Id = 1,
            Username = username,
            PasswordHash = SecurityService.ComputeHash(password),
            FailedLoginAttempts = 0,
            IsLocked = false,
            IsAdmin = isAdmin,
            MfaEnabled = mfaEnabled,
            MfaSecret = mfaEnabled ? "123456" : null,
            OrgUnitId = 1
        };
    }

    private void SetupFindAccount(UserAccount? account)
    {
        _userAccountRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserAccount, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account != null ? new[] { account } : Array.Empty<UserAccount>());
    }

    #region LoginAsync - Account Lockout

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
    {
        var account = CreateAccount();
        SetupFindAccount(account);

        var result = await _sut.LoginAsync(new LoginCommand("testuser", "Pass123!"));

        result.Success.Should().BeTrue();
        result.SessionToken.Should().NotBeNullOrEmpty();
        result.RequiresMfa.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsFailure()
    {
        var account = CreateAccount();
        SetupFindAccount(account);

        var result = await _sut.LoginAsync(new LoginCommand("testuser", "wrong"));

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("שגויים");
    }

    [Fact]
    public async Task LoginAsync_NonExistentUser_ReturnsFailure()
    {
        SetupFindAccount(null);

        var result = await _sut.LoginAsync(new LoginCommand("nouser", "pass"));

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_EmptyCredentials_ReturnsFailure()
    {
        var result = await _sut.LoginAsync(new LoginCommand("", ""));

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("חובה");
    }

    [Fact]
    public async Task LoginAsync_LockedAccount_ReturnsLockedMessage()
    {
        var account = CreateAccount();
        account.IsLocked = true;
        account.LockedAt = DateTime.UtcNow;
        SetupFindAccount(account);

        var result = await _sut.LoginAsync(new LoginCommand("testuser", "Pass123!"));

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("נעול");
    }

    [Fact]
    public async Task LoginAsync_FifthFailedAttempt_LocksAccount()
    {
        var account = CreateAccount();
        account.FailedLoginAttempts = 4; // one more will lock
        SetupFindAccount(account);

        var result = await _sut.LoginAsync(new LoginCommand("testuser", "wrong"));

        result.Success.Should().BeFalse();
        account.IsLocked.Should().BeTrue();
        account.LockedAt.Should().NotBeNull();
        account.FailedLoginAttempts.Should().Be(5);
        _auditLogRepoMock.Verify(r => r.AddAsync(
            It.Is<AuditLogEntry>(e => e.Action == "AccountLocked"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_FailedAttemptBelowThreshold_IncrementsCounter()
    {
        var account = CreateAccount();
        account.FailedLoginAttempts = 2;
        SetupFindAccount(account);

        await _sut.LoginAsync(new LoginCommand("testuser", "wrong"));

        account.FailedLoginAttempts.Should().Be(3);
        account.IsLocked.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_SuccessfulLogin_ResetsFailedAttempts()
    {
        var account = CreateAccount();
        account.FailedLoginAttempts = 3;
        SetupFindAccount(account);

        var result = await _sut.LoginAsync(new LoginCommand("testuser", "Pass123!"));

        result.Success.Should().BeTrue();
        account.FailedLoginAttempts.Should().Be(0);
    }

    #endregion

    #region MFA Verification

    [Fact]
    public async Task LoginAsync_AdminWithMfa_RequiresMfaVerification()
    {
        var account = CreateAccount(isAdmin: true, mfaEnabled: true);
        SetupFindAccount(account);

        var result = await _sut.LoginAsync(new LoginCommand("testuser", "Pass123!"));

        result.Success.Should().BeTrue();
        result.RequiresMfa.Should().BeTrue();
        result.UserId.Should().Be(account.Id);
        result.SessionToken.Should().BeNull();
    }

    [Fact]
    public async Task VerifyMfaAsync_ValidCode_ReturnsSuccess()
    {
        var account = CreateAccount(isAdmin: true, mfaEnabled: true);
        _userAccountRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _sut.VerifyMfaAsync(new VerifyMfaCommand(1, "123456"));

        result.Success.Should().BeTrue();
        result.SessionToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task VerifyMfaAsync_InvalidCode_ReturnsFailure()
    {
        var account = CreateAccount(isAdmin: true, mfaEnabled: true);
        _userAccountRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _sut.VerifyMfaAsync(new VerifyMfaCommand(1, "wrong"));

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("MFA");
    }

    [Fact]
    public async Task VerifyMfaAsync_MfaNotEnabled_ReturnsFailure()
    {
        var account = CreateAccount(isAdmin: true, mfaEnabled: false);
        _userAccountRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _sut.VerifyMfaAsync(new VerifyMfaCommand(1, "123456"));

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyMfaAsync_NonExistentUser_ThrowsNotFoundException()
    {
        _userAccountRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAccount?)null);

        var act = () => _sut.VerifyMfaAsync(new VerifyMfaCommand(999, "123456"));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Session Timeout

    [Fact]
    public async Task CheckSessionAsync_ValidSession_ReturnsValid()
    {
        var account = CreateAccount();
        account.SessionExpiresAt = DateTime.UtcNow.AddMinutes(15);
        _userAccountRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _sut.CheckSessionAsync(1);

        result.IsValid.Should().BeTrue();
        result.ExpiresAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckSessionAsync_ExpiredSession_ReturnsInvalid()
    {
        var account = CreateAccount();
        account.SessionExpiresAt = DateTime.UtcNow.AddMinutes(-5);
        _userAccountRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _sut.CheckSessionAsync(1);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task CheckSessionAsync_NoSession_ReturnsInvalid()
    {
        var account = CreateAccount();
        account.SessionExpiresAt = null;
        _userAccountRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _sut.CheckSessionAsync(1);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshSessionAsync_ExtendsSession()
    {
        var account = CreateAccount();
        account.SessionExpiresAt = DateTime.UtcNow.AddMinutes(5);
        _userAccountRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        await _sut.RefreshSessionAsync(1);

        account.SessionExpiresAt.Should().BeCloseTo(
            DateTime.UtcNow.AddMinutes(SecurityService.SessionTimeoutMinutes),
            TimeSpan.FromSeconds(5));
        _userAccountRepoMock.Verify(r => r.UpdateAsync(account, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Personal Data Access Logging

    [Fact]
    public async Task LogPersonalDataAccessAsync_CreatesLogEntry()
    {
        _accessLogRepoMock.Setup(r => r.AddAsync(It.IsAny<PersonalDataAccessLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PersonalDataAccessLog e, CancellationToken _) => e);

        await _sut.LogPersonalDataAccessAsync(5, 100, "View", "FirstName,LastName,Email", "192.168.1.1");

        _accessLogRepoMock.Verify(r => r.AddAsync(
            It.Is<PersonalDataAccessLog>(l =>
                l.UserId == 5 &&
                l.ContactId == 100 &&
                l.AccessType == "View" &&
                l.FieldsAccessed == "FirstName,LastName,Email" &&
                l.IpAddress == "192.168.1.1"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Right to Erasure

    [Fact]
    public async Task ErasePersonalDataAsync_AnonymizesContactData()
    {
        var contact = new Contact
        {
            Id = 100,
            FirstName = "ישראל",
            LastName = "ישראלי",
            IdNumber = "123456789",
            Phone = "050-1234567",
            Email = "test@example.com",
            Address = "רחוב הרצל 1",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "M"
        };
        _contactRepoMock.Setup(r => r.GetByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);

        var command = new ErasePersonalDataCommand(100, 5, "בקשת מועמד");
        await _sut.ErasePersonalDataAsync(command);

        contact.FirstName.Should().Be("[נמחק]");
        contact.LastName.Should().Be("[נמחק]");
        contact.IdNumber.Should().StartWith("ERASED-");
        contact.Phone.Should().BeNull();
        contact.Email.Should().BeNull();
        contact.Address.Should().BeNull();
        contact.DateOfBirth.Should().BeNull();
        contact.Gender.Should().BeNull();

        _contactRepoMock.Verify(r => r.UpdateAsync(contact, It.IsAny<CancellationToken>()), Times.Once);
        _auditLogRepoMock.Verify(r => r.AddAsync(
            It.Is<AuditLogEntry>(e => e.Action == "PersonalDataErased" && e.EntityId == 100),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ErasePersonalDataAsync_NonExistentContact_ThrowsNotFoundException()
    {
        _contactRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);

        var act = () => _sut.ErasePersonalDataAsync(new ErasePersonalDataCommand(999, 5, "test"));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Account Unlock

    [Fact]
    public async Task UnlockAccountAsync_UnlocksAndResetsCounter()
    {
        var account = CreateAccount();
        account.IsLocked = true;
        account.LockedAt = DateTime.UtcNow;
        account.FailedLoginAttempts = 5;
        _userAccountRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        await _sut.UnlockAccountAsync(new UnlockAccountCommand(1, 99));

        account.IsLocked.Should().BeFalse();
        account.LockedAt.Should().BeNull();
        account.FailedLoginAttempts.Should().Be(0);
        _auditLogRepoMock.Verify(r => r.AddAsync(
            It.Is<AuditLogEntry>(e => e.Action == "AccountUnlocked"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnlockAccountAsync_NonExistentAccount_ThrowsNotFoundException()
    {
        _userAccountRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAccount?)null);

        var act = () => _sut.UnlockAccountAsync(new UnlockAccountCommand(999, 5));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
