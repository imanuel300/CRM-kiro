using System.Linq.Expressions;
using CandidacyManagement.Application.OrgStructure;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.OrgStructure;

public class OrgStructureServiceTests
{
    private readonly Mock<IRepository<OrgSubUnit>> _subUnitRepoMock;
    private readonly Mock<IRepository<OrgPosition>> _positionRepoMock;
    private readonly Mock<IRepository<PositionAssignment>> _assignmentRepoMock;
    private readonly Mock<IRepository<OrganizationalUnit>> _orgUnitRepoMock;
    private readonly Mock<IRepository<Contact>> _contactRepoMock;
    private readonly Mock<IRepository<Candidacy>> _candidacyRepoMock;
    private readonly OrgStructureService _sut;

    public OrgStructureServiceTests()
    {
        _subUnitRepoMock = new Mock<IRepository<OrgSubUnit>>();
        _positionRepoMock = new Mock<IRepository<OrgPosition>>();
        _assignmentRepoMock = new Mock<IRepository<PositionAssignment>>();
        _orgUnitRepoMock = new Mock<IRepository<OrganizationalUnit>>();
        _contactRepoMock = new Mock<IRepository<Contact>>();
        _candidacyRepoMock = new Mock<IRepository<Candidacy>>();

        _sut = new OrgStructureService(
            _subUnitRepoMock.Object,
            _positionRepoMock.Object,
            _assignmentRepoMock.Object,
            _orgUnitRepoMock.Object,
            _contactRepoMock.Object,
            _candidacyRepoMock.Object);
    }

    #region CreateSubUnitAsync

    [Fact]
    public async Task CreateSubUnitAsync_WithValidCommand_ReturnsDto()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _subUnitRepoMock.Setup(r => r.AddAsync(It.IsAny<OrgSubUnit>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrgSubUnit e, CancellationToken _) => { e.Id = 10; return e; });

        var command = new CreateSubUnitCommand(OrgUnitId: 1, ParentOrgSubUnitId: null, Name: "לשכת תל אביב", Description: "לשכה מרכזית");

        var result = await _sut.CreateSubUnitAsync(command);

        result.Should().NotBeNull();
        result.OrgUnitId.Should().Be(1);
        result.Name.Should().Be("לשכת תל אביב");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateSubUnitAsync_OrgUnitNotFound_ThrowsNotFoundException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationalUnit?)null);

        var command = new CreateSubUnitCommand(OrgUnitId: 999, ParentOrgSubUnitId: null, Name: "לשכה", Description: null);

        var act = () => _sut.CreateSubUnitAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateSubUnitAsync_EmptyName_ThrowsValidationException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });

        var command = new CreateSubUnitCommand(OrgUnitId: 1, ParentOrgSubUnitId: null, Name: "", Description: null);

        var act = () => _sut.CreateSubUnitAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateSubUnitAsync_WithParent_ValidatesParentBelongsToSameOrgUnit()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _subUnitRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrgSubUnit { Id = 5, OrgUnitId = 2 }); // different org unit

        var command = new CreateSubUnitCommand(OrgUnitId: 1, ParentOrgSubUnitId: 5, Name: "ילד", Description: null);

        var act = () => _sut.CreateSubUnitAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateSubUnitAsync_ParentNotFound_ThrowsNotFoundException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _subUnitRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrgSubUnit?)null);

        var command = new CreateSubUnitCommand(OrgUnitId: 1, ParentOrgSubUnitId: 999, Name: "ילד", Description: null);

        var act = () => _sut.CreateSubUnitAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region UpdateSubUnitAsync

    [Fact]
    public async Task UpdateSubUnitAsync_WithValidCommand_ReturnsUpdatedDto()
    {
        var entity = new OrgSubUnit { Id = 10, OrgUnitId = 1, Name = "לשכה ישנה", IsActive = true };
        _subUnitRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var command = new UpdateSubUnitCommand(Id: 10, Name: "לשכה חדשה", Description: "עודכן", IsActive: true);

        var result = await _sut.UpdateSubUnitAsync(command);

        result.Name.Should().Be("לשכה חדשה");
        result.Description.Should().Be("עודכן");
    }

    [Fact]
    public async Task UpdateSubUnitAsync_NotFound_ThrowsNotFoundException()
    {
        _subUnitRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrgSubUnit?)null);

        var command = new UpdateSubUnitCommand(Id: 999, Name: "שם", Description: null, IsActive: true);

        var act = () => _sut.UpdateSubUnitAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region DeleteSubUnitAsync

    [Fact]
    public async Task DeleteSubUnitAsync_NoChildrenNoPositions_DeletesSuccessfully()
    {
        var entity = new OrgSubUnit { Id = 10 };
        _subUnitRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _subUnitRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<OrgSubUnit, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<OrgSubUnit>());
        _positionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<OrgPosition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<OrgPosition>());

        await _sut.DeleteSubUnitAsync(10);

        _subUnitRepoMock.Verify(r => r.DeleteAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSubUnitAsync_HasChildren_ThrowsValidationException()
    {
        var entity = new OrgSubUnit { Id = 10 };
        _subUnitRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _subUnitRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<OrgSubUnit, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrgSubUnit> { new() { Id = 20, ParentOrgSubUnitId = 10 } });

        var act = () => _sut.DeleteSubUnitAsync(10);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task DeleteSubUnitAsync_HasPositions_ThrowsValidationException()
    {
        var entity = new OrgSubUnit { Id = 10 };
        _subUnitRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _subUnitRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<OrgSubUnit, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<OrgSubUnit>());
        _positionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<OrgPosition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrgPosition> { new() { Id = 1, OrgSubUnitId = 10 } });

        var act = () => _sut.DeleteSubUnitAsync(10);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region CreatePositionAsync

    [Fact]
    public async Task CreatePositionAsync_WithValidCommand_ReturnsDto()
    {
        _subUnitRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrgSubUnit { Id = 10 });
        _positionRepoMock.Setup(r => r.AddAsync(It.IsAny<OrgPosition>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrgPosition e, CancellationToken _) => { e.Id = 1; return e; });

        var command = new CreatePositionCommand(OrgSubUnitId: 10, Title: "עוזר משפטי", MaxOccupants: 2);

        var result = await _sut.CreatePositionAsync(command);

        result.Should().NotBeNull();
        result.Title.Should().Be("עוזר משפטי");
        result.MaxOccupants.Should().Be(2);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreatePositionAsync_SubUnitNotFound_ThrowsNotFoundException()
    {
        _subUnitRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrgSubUnit?)null);

        var command = new CreatePositionCommand(OrgSubUnitId: 999, Title: "תפקיד", MaxOccupants: 1);

        var act = () => _sut.CreatePositionAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreatePositionAsync_ZeroMaxOccupants_ThrowsValidationException()
    {
        _subUnitRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrgSubUnit { Id = 10 });

        var command = new CreatePositionCommand(OrgSubUnitId: 10, Title: "תפקיד", MaxOccupants: 0);

        var act = () => _sut.CreatePositionAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region AssignToPositionAsync

    [Fact]
    public async Task AssignToPositionAsync_WithValidCommand_ReturnsAssignmentDto()
    {
        var position = new OrgPosition { Id = 1, MaxOccupants = 2, IsActive = true };
        _positionRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);
        _contactRepoMock.Setup(r => r.GetByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = 100 });
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(200, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 200 });
        _assignmentRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PositionAssignment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<PositionAssignment>());
        _assignmentRepoMock.Setup(r => r.AddAsync(It.IsAny<PositionAssignment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PositionAssignment e, CancellationToken _) => { e.Id = 1; return e; });

        var command = new AssignToPositionCommand(OrgPositionId: 1, ContactId: 100, CandidacyId: 200, StartDate: DateTime.UtcNow);

        var result = await _sut.AssignToPositionAsync(command);

        result.Should().NotBeNull();
        result.OrgPositionId.Should().Be(1);
        result.ContactId.Should().Be(100);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task AssignToPositionAsync_PositionFull_ThrowsValidationException()
    {
        var position = new OrgPosition { Id = 1, MaxOccupants = 1, IsActive = true };
        _positionRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);
        _contactRepoMock.Setup(r => r.GetByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = 100 });
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(200, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 200 });
        // First FindAsync call returns active assignments (occupancy check) - position is full
        _assignmentRepoMock.SetupSequence(r => r.FindAsync(It.IsAny<Expression<Func<PositionAssignment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PositionAssignment> { new() { Id = 1, OrgPositionId = 1, IsActive = true } });

        var command = new AssignToPositionCommand(OrgPositionId: 1, ContactId: 100, CandidacyId: 200, StartDate: DateTime.UtcNow);

        var act = () => _sut.AssignToPositionAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task AssignToPositionAsync_InactivePosition_ThrowsValidationException()
    {
        var position = new OrgPosition { Id = 1, MaxOccupants = 2, IsActive = false };
        _positionRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);

        var command = new AssignToPositionCommand(OrgPositionId: 1, ContactId: 100, CandidacyId: 200, StartDate: DateTime.UtcNow);

        var act = () => _sut.AssignToPositionAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task AssignToPositionAsync_DuplicateAssignment_ThrowsValidationException()
    {
        var position = new OrgPosition { Id = 1, MaxOccupants = 5, IsActive = true };
        _positionRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);
        _contactRepoMock.Setup(r => r.GetByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = 100 });
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(200, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 200 });
        // First call: occupancy check returns empty (not full), second call: duplicate check returns existing
        _assignmentRepoMock.SetupSequence(r => r.FindAsync(It.IsAny<Expression<Func<PositionAssignment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<PositionAssignment>())
            .ReturnsAsync(new List<PositionAssignment> { new() { Id = 1, OrgPositionId = 1, ContactId = 100, IsActive = true } });

        var command = new AssignToPositionCommand(OrgPositionId: 1, ContactId: 100, CandidacyId: 200, StartDate: DateTime.UtcNow);

        var act = () => _sut.AssignToPositionAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region UnassignFromPositionAsync

    [Fact]
    public async Task UnassignFromPositionAsync_ActiveAssignment_DeactivatesSuccessfully()
    {
        var assignment = new PositionAssignment { Id = 1, IsActive = true };
        _assignmentRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        await _sut.UnassignFromPositionAsync(new UnassignFromPositionCommand(AssignmentId: 1));

        assignment.IsActive.Should().BeFalse();
        assignment.EndDate.Should().NotBeNull();
        _assignmentRepoMock.Verify(r => r.UpdateAsync(assignment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnassignFromPositionAsync_NotFound_ThrowsNotFoundException()
    {
        _assignmentRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PositionAssignment?)null);

        var act = () => _sut.UnassignFromPositionAsync(new UnassignFromPositionCommand(AssignmentId: 999));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UnassignFromPositionAsync_AlreadyInactive_ThrowsValidationException()
    {
        var assignment = new PositionAssignment { Id = 1, IsActive = false };
        _assignmentRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        var act = () => _sut.UnassignFromPositionAsync(new UnassignFromPositionCommand(AssignmentId: 1));

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region GetPositionOccupancyAsync

    [Fact]
    public async Task GetPositionOccupancyAsync_ReturnsCorrectOccupancy()
    {
        var subUnit = new OrgSubUnit { Id = 10, Name = "לשכת ירושלים" };
        _subUnitRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subUnit);
        _positionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<OrgPosition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrgPosition>
            {
                new() { Id = 1, OrgSubUnitId = 10, Title = "עוזר משפטי", MaxOccupants = 3, IsActive = true },
                new() { Id = 2, OrgSubUnitId = 10, Title = "מזכיר", MaxOccupants = 1, IsActive = true }
            });
        // Position 1 has 2 active assignments, Position 2 has 0
        _assignmentRepoMock.SetupSequence(r => r.FindAsync(It.IsAny<Expression<Func<PositionAssignment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PositionAssignment>
            {
                new() { Id = 1, OrgPositionId = 1, IsActive = true },
                new() { Id = 2, OrgPositionId = 1, IsActive = true }
            })
            .ReturnsAsync(Enumerable.Empty<PositionAssignment>());

        var result = await _sut.GetPositionOccupancyAsync(10);

        result.Should().NotBeNull();
        result.SubUnitName.Should().Be("לשכת ירושלים");
        var positions = result.Positions.ToList();
        positions.Should().HaveCount(2);
        positions[0].FilledCount.Should().Be(2);
        positions[0].VacantCount.Should().Be(1);
        positions[1].FilledCount.Should().Be(0);
        positions[1].VacantCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPositionOccupancyAsync_SubUnitNotFound_ThrowsNotFoundException()
    {
        _subUnitRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrgSubUnit?)null);

        var act = () => _sut.GetPositionOccupancyAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetPositionOccupancyAsync_NoPositions_ReturnsEmptyList()
    {
        var subUnit = new OrgSubUnit { Id = 10, Name = "לשכה ריקה" };
        _subUnitRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subUnit);
        _positionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<OrgPosition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<OrgPosition>());

        var result = await _sut.GetPositionOccupancyAsync(10);

        result.Positions.Should().BeEmpty();
    }

    #endregion

    #region GetSubUnitTreeAsync

    [Fact]
    public async Task GetSubUnitTreeAsync_ReturnsHierarchicalTree()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _subUnitRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<OrgSubUnit, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrgSubUnit>
            {
                new() { Id = 10, OrgUnitId = 1, ParentOrgSubUnitId = null, Name = "מחוז צפון" },
                new() { Id = 20, OrgUnitId = 1, ParentOrgSubUnitId = 10, Name = "לשכת חיפה" },
                new() { Id = 30, OrgUnitId = 1, ParentOrgSubUnitId = null, Name = "מחוז דרום" }
            });
        _positionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<OrgPosition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrgPosition>
            {
                new() { Id = 1, OrgSubUnitId = 20, Title = "עוזר משפטי", MaxOccupants = 2, IsActive = true }
            });

        var result = await _sut.GetSubUnitTreeAsync(1);

        result.Should().NotBeNull();
        result.OrgUnitId.Should().Be(1);
        var rootChildren = result.Children.ToList();
        rootChildren.Should().HaveCount(2);
        rootChildren[0].Name.Should().Be("מחוז צפון");
        var northChildren = rootChildren[0].Children.ToList();
        northChildren.Should().HaveCount(1);
        northChildren[0].Name.Should().Be("לשכת חיפה");
        northChildren[0].Positions.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetSubUnitTreeAsync_OrgUnitNotFound_ThrowsNotFoundException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationalUnit?)null);

        var act = () => _sut.GetSubUnitTreeAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
