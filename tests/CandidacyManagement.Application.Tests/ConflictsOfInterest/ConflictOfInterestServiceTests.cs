using System.Linq.Expressions;
using CandidacyManagement.Application.ConflictsOfInterest;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.ConflictsOfInterest;

public class ConflictOfInterestServiceTests
{
    private readonly Mock<IRepository<ConflictOfInterest>> _conflictRepoMock;
    private readonly Mock<IRepository<FamilyRelation>> _familyRelationRepoMock;
    private readonly Mock<IRepository<Candidacy>> _candidacyRepoMock;
    private readonly Mock<IRepository<Contact>> _contactRepoMock;
    private readonly ConflictOfInterestService _sut;

    public ConflictOfInterestServiceTests()
    {
        _conflictRepoMock = new Mock<IRepository<ConflictOfInterest>>();
        _familyRelationRepoMock = new Mock<IRepository<FamilyRelation>>();
        _candidacyRepoMock = new Mock<IRepository<Candidacy>>();
        _contactRepoMock = new Mock<IRepository<Contact>>();

        _sut = new ConflictOfInterestService(
            _conflictRepoMock.Object,
            _familyRelationRepoMock.Object,
            _candidacyRepoMock.Object,
            _contactRepoMock.Object);
    }

    #region CreateConflictAsync

    [Fact]
    public async Task CreateConflictAsync_WithValidCommand_ReturnsDto()
    {
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 1 });
        _contactRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = 2 });
        _conflictRepoMock.Setup(r => r.AddAsync(It.IsAny<ConflictOfInterest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConflictOfInterest e, CancellationToken _) => { e.Id = 10; return e; });

        var command = new CreateConflictOfInterestCommand(
            CandidacyId: 1, ContactId: 2,
            QuestionnaireResponses: "{\"q1\":\"no\"}", HasConflict: false);

        var result = await _sut.CreateConflictAsync(command);

        result.Should().NotBeNull();
        result.CandidacyId.Should().Be(1);
        result.ContactId.Should().Be(2);
        result.HasConflict.Should().BeFalse();
        result.RequiresManualReview.Should().BeFalse();
    }

    [Fact]
    public async Task CreateConflictAsync_WithConflict_MarksForManualReview()
    {
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 1 });
        _contactRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = 2 });
        _conflictRepoMock.Setup(r => r.AddAsync(It.IsAny<ConflictOfInterest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConflictOfInterest e, CancellationToken _) => { e.Id = 10; return e; });

        var command = new CreateConflictOfInterestCommand(
            CandidacyId: 1, ContactId: 2,
            QuestionnaireResponses: "{\"q1\":\"yes\"}", HasConflict: true);

        var result = await _sut.CreateConflictAsync(command);

        result.HasConflict.Should().BeTrue();
        result.RequiresManualReview.Should().BeTrue();
    }

    [Fact]
    public async Task CreateConflictAsync_EmptyQuestionnaire_ThrowsValidationException()
    {
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 1 });
        _contactRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = 2 });

        var command = new CreateConflictOfInterestCommand(
            CandidacyId: 1, ContactId: 2,
            QuestionnaireResponses: "", HasConflict: false);

        var act = () => _sut.CreateConflictAsync(command);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateConflictAsync_InvalidCandidacy_ThrowsNotFoundException()
    {
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidacy?)null);

        var command = new CreateConflictOfInterestCommand(
            CandidacyId: 999, ContactId: 2,
            QuestionnaireResponses: "{}", HasConflict: false);

        var act = () => _sut.CreateConflictAsync(command);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region UpdateConflictAsync

    [Fact]
    public async Task UpdateConflictAsync_WithValidCommand_UpdatesAndResetsReview()
    {
        var existing = new ConflictOfInterest
        {
            Id = 10, CandidacyId = 1, ContactId = 2,
            QuestionnaireResponses = "{\"q1\":\"no\"}", HasConflict = false,
            ReviewedByUserId = 5, ReviewedAt = DateTime.UtcNow
        };
        _conflictRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var command = new UpdateConflictOfInterestCommand(
            Id: 10, QuestionnaireResponses: "{\"q1\":\"yes\"}", HasConflict: true);

        var result = await _sut.UpdateConflictAsync(command);

        result.HasConflict.Should().BeTrue();
        result.RequiresManualReview.Should().BeTrue();
        result.ReviewedByUserId.Should().BeNull();
        result.ReviewedAt.Should().BeNull();
    }

    #endregion

    #region CreateFamilyRelationAsync

    [Fact]
    public async Task CreateFamilyRelationAsync_WithValidCommand_ReturnsDto()
    {
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 1 });
        _contactRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = 2 });
        _familyRelationRepoMock.Setup(r => r.AddAsync(It.IsAny<FamilyRelation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FamilyRelation e, CancellationToken _) => { e.Id = 20; return e; });

        var command = new CreateFamilyRelationCommand(
            CandidacyId: 1, ContactId: 2,
            RelationType: "אח", RelatedPersonName: "ישראל ישראלי",
            RelatedPersonRole: "שופט");

        var result = await _sut.CreateFamilyRelationAsync(command);

        result.Should().NotBeNull();
        result.RelationType.Should().Be("אח");
        result.RelatedPersonName.Should().Be("ישראל ישראלי");
        result.RequiresManualReview.Should().BeTrue();
    }

    [Fact]
    public async Task CreateFamilyRelationAsync_EmptyRelationType_ThrowsValidationException()
    {
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 1 });
        _contactRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = 2 });

        var command = new CreateFamilyRelationCommand(
            CandidacyId: 1, ContactId: 2,
            RelationType: "", RelatedPersonName: "ישראל",
            RelatedPersonRole: null);

        var act = () => _sut.CreateFamilyRelationAsync(command);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateFamilyRelationAsync_EmptyRelatedPersonName_ThrowsValidationException()
    {
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 1 });
        _contactRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = 2 });

        var command = new CreateFamilyRelationCommand(
            CandidacyId: 1, ContactId: 2,
            RelationType: "אח", RelatedPersonName: "",
            RelatedPersonRole: null);

        var act = () => _sut.CreateFamilyRelationAsync(command);
        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region ReviewConflictAsync

    [Fact]
    public async Task ReviewConflictAsync_MarksAsReviewedAndClearsFlag()
    {
        var existing = new ConflictOfInterest
        {
            Id = 10, CandidacyId = 1, ContactId = 2,
            QuestionnaireResponses = "{\"q1\":\"yes\"}", HasConflict = true,
            RequiresManualReview = true
        };
        _conflictRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var command = new ReviewConflictCommand(Id: 10, ReviewedByUserId: 5);

        var result = await _sut.ReviewConflictAsync(command);

        result.ReviewedByUserId.Should().Be(5);
        result.ReviewedAt.Should().NotBeNull();
        result.RequiresManualReview.Should().BeFalse();
    }

    [Fact]
    public async Task ReviewConflictAsync_NotFound_ThrowsNotFoundException()
    {
        _conflictRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConflictOfInterest?)null);

        var command = new ReviewConflictCommand(Id: 999, ReviewedByUserId: 5);

        var act = () => _sut.ReviewConflictAsync(command);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetDeclarationsForCandidacyAsync

    [Fact]
    public async Task GetDeclarationsForCandidacyAsync_ReturnsAllDeclarations()
    {
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 1 });

        _conflictRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ConflictOfInterest, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConflictOfInterest>
            {
                new() { Id = 10, CandidacyId = 1, ContactId = 2, QuestionnaireResponses = "{}", HasConflict = true, RequiresManualReview = true }
            });

        _familyRelationRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<FamilyRelation, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FamilyRelation>
            {
                new() { Id = 20, CandidacyId = 1, ContactId = 2, RelationType = "אח", RelatedPersonName = "ישראל", RequiresManualReview = true }
            });

        var result = await _sut.GetDeclarationsForCandidacyAsync(1);

        result.CandidacyId.Should().Be(1);
        result.ConflictsOfInterest.Should().HaveCount(1);
        result.FamilyRelations.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetDeclarationsForCandidacyAsync_InvalidCandidacy_ThrowsNotFoundException()
    {
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidacy?)null);

        var act = () => _sut.GetDeclarationsForCandidacyAsync(999);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetCandidacyIdsRequiringManualReviewAsync

    [Fact]
    public async Task GetCandidacyIdsRequiringManualReviewAsync_ReturnsFlaggedCandidacyIds()
    {
        _conflictRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ConflictOfInterest, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConflictOfInterest>
            {
                new() { Id = 10, CandidacyId = 1, RequiresManualReview = true },
                new() { Id = 11, CandidacyId = 2, RequiresManualReview = true }
            });

        _familyRelationRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<FamilyRelation, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FamilyRelation>
            {
                new() { Id = 20, CandidacyId = 2, RequiresManualReview = true },
                new() { Id = 21, CandidacyId = 3, RequiresManualReview = true }
            });

        var result = (await _sut.GetCandidacyIdsRequiringManualReviewAsync(new ManualReviewQueryParams())).ToList();

        result.Should().Contain(1);
        result.Should().Contain(2);
        result.Should().Contain(3);
        result.Should().HaveCount(3); // distinct
    }

    #endregion

    #region DeleteConflictAsync

    [Fact]
    public async Task DeleteConflictAsync_ExistingEntity_Deletes()
    {
        var existing = new ConflictOfInterest { Id = 10 };
        _conflictRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await _sut.DeleteConflictAsync(10);

        _conflictRepoMock.Verify(r => r.DeleteAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteConflictAsync_NotFound_ThrowsNotFoundException()
    {
        _conflictRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConflictOfInterest?)null);

        var act = () => _sut.DeleteConflictAsync(999);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region DeleteFamilyRelationAsync

    [Fact]
    public async Task DeleteFamilyRelationAsync_ExistingEntity_Deletes()
    {
        var existing = new FamilyRelation { Id = 20 };
        _familyRelationRepoMock.Setup(r => r.GetByIdAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await _sut.DeleteFamilyRelationAsync(20);

        _familyRelationRepoMock.Verify(r => r.DeleteAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
