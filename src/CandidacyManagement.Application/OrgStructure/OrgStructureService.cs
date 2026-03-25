using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.OrgStructure;

public class OrgStructureService : IOrgStructureService
{
    private readonly IRepository<OrgSubUnit> _subUnitRepository;
    private readonly IRepository<OrgPosition> _positionRepository;
    private readonly IRepository<PositionAssignment> _assignmentRepository;
    private readonly IRepository<OrganizationalUnit> _orgUnitRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly IRepository<Candidacy> _candidacyRepository;

    public OrgStructureService(
        IRepository<OrgSubUnit> subUnitRepository,
        IRepository<OrgPosition> positionRepository,
        IRepository<PositionAssignment> assignmentRepository,
        IRepository<OrganizationalUnit> orgUnitRepository,
        IRepository<Contact> contactRepository,
        IRepository<Candidacy> candidacyRepository)
    {
        _subUnitRepository = subUnitRepository;
        _positionRepository = positionRepository;
        _assignmentRepository = assignmentRepository;
        _orgUnitRepository = orgUnitRepository;
        _contactRepository = contactRepository;
        _candidacyRepository = candidacyRepository;
    }

    #region Sub-Unit Operations

    public async Task<OrgSubUnitDto> CreateSubUnitAsync(CreateSubUnitCommand command, CancellationToken cancellationToken = default)
    {
        _ = await _orgUnitRepository.GetByIdAsync(command.OrgUnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationalUnit), command.OrgUnitId);

        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ValidationException("Name", "יש לציין שם ליחידת משנה");

        if (command.ParentOrgSubUnitId.HasValue)
        {
            var parent = await _subUnitRepository.GetByIdAsync(command.ParentOrgSubUnitId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(OrgSubUnit), command.ParentOrgSubUnitId.Value);

            if (parent.OrgUnitId != command.OrgUnitId)
                throw new ValidationException("ParentOrgSubUnitId", "יחידת האב חייבת להשתייך לאותה יחידה ארגונית");
        }

        var entity = new OrgSubUnit
        {
            OrgUnitId = command.OrgUnitId,
            ParentOrgSubUnitId = command.ParentOrgSubUnitId,
            Name = command.Name,
            Description = command.Description,
            IsActive = true
        };

        await _subUnitRepository.AddAsync(entity, cancellationToken);
        return ToSubUnitDto(entity);
    }

    public async Task<OrgSubUnitDto> UpdateSubUnitAsync(UpdateSubUnitCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _subUnitRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(OrgSubUnit), command.Id);

        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ValidationException("Name", "יש לציין שם ליחידת משנה");

        entity.Name = command.Name;
        entity.Description = command.Description;
        entity.IsActive = command.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _subUnitRepository.UpdateAsync(entity, cancellationToken);
        return ToSubUnitDto(entity);
    }

    public async Task DeleteSubUnitAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _subUnitRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(OrgSubUnit), id);

        // Check for child sub-units
        var children = await _subUnitRepository.FindAsync(
            s => s.ParentOrgSubUnitId == id, cancellationToken);
        if (children.Any())
            throw new ValidationException("Id", "לא ניתן למחוק יחידת משנה שיש לה יחידות משנה צאצאיות");

        // Check for positions
        var positions = await _positionRepository.FindAsync(
            p => p.OrgSubUnitId == id, cancellationToken);
        if (positions.Any())
            throw new ValidationException("Id", "לא ניתן למחוק יחידת משנה שיש בה תפקידים");

        await _subUnitRepository.DeleteAsync(entity, cancellationToken);
    }

    public async Task<OrgSubUnitTreeDto> GetSubUnitTreeAsync(int orgUnitId, CancellationToken cancellationToken = default)
    {
        _ = await _orgUnitRepository.GetByIdAsync(orgUnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationalUnit), orgUnitId);

        var allSubUnits = await _subUnitRepository.FindAsync(
            s => s.OrgUnitId == orgUnitId, cancellationToken);

        var allPositions = await _positionRepository.FindAsync(
            p => allSubUnits.Select(s => s.Id).Contains(p.OrgSubUnitId), cancellationToken);

        var subUnitList = allSubUnits.ToList();
        var positionList = allPositions.ToList();

        // Build tree from root nodes (no parent)
        var rootNodes = subUnitList.Where(s => s.ParentOrgSubUnitId == null);
        var children = rootNodes.Select(r => BuildTreeNode(r, subUnitList, positionList));

        return new OrgSubUnitTreeDto(
            Id: 0,
            OrgUnitId: orgUnitId,
            ParentOrgSubUnitId: null,
            Name: "Root",
            Description: null,
            IsActive: true,
            Children: children,
            Positions: Enumerable.Empty<OrgPositionDto>());
    }

    #endregion

    #region Position Operations

    public async Task<OrgPositionDto> CreatePositionAsync(CreatePositionCommand command, CancellationToken cancellationToken = default)
    {
        _ = await _subUnitRepository.GetByIdAsync(command.OrgSubUnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrgSubUnit), command.OrgSubUnitId);

        if (string.IsNullOrWhiteSpace(command.Title))
            throw new ValidationException("Title", "יש לציין כותרת לתפקיד");

        if (command.MaxOccupants <= 0)
            throw new ValidationException("MaxOccupants", "מספר המאיישים המקסימלי חייב להיות חיובי");

        var entity = new OrgPosition
        {
            OrgSubUnitId = command.OrgSubUnitId,
            Title = command.Title,
            MaxOccupants = command.MaxOccupants,
            IsActive = true
        };

        await _positionRepository.AddAsync(entity, cancellationToken);
        return ToPositionDto(entity);
    }

    public async Task<OrgPositionDto> UpdatePositionAsync(UpdatePositionCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _positionRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(OrgPosition), command.Id);

        if (string.IsNullOrWhiteSpace(command.Title))
            throw new ValidationException("Title", "יש לציין כותרת לתפקיד");

        if (command.MaxOccupants <= 0)
            throw new ValidationException("MaxOccupants", "מספר המאיישים המקסימלי חייב להיות חיובי");

        entity.Title = command.Title;
        entity.MaxOccupants = command.MaxOccupants;
        entity.IsActive = command.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _positionRepository.UpdateAsync(entity, cancellationToken);
        return ToPositionDto(entity);
    }

    public async Task DeletePositionAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _positionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(OrgPosition), id);

        // Check for active assignments
        var activeAssignments = await _assignmentRepository.FindAsync(
            a => a.OrgPositionId == id && a.IsActive, cancellationToken);
        if (activeAssignments.Any())
            throw new ValidationException("Id", "לא ניתן למחוק תפקיד שיש לו שיוכים פעילים");

        await _positionRepository.DeleteAsync(entity, cancellationToken);
    }

    #endregion

    #region Assignment Operations

    public async Task<PositionAssignmentDto> AssignToPositionAsync(AssignToPositionCommand command, CancellationToken cancellationToken = default)
    {
        var position = await _positionRepository.GetByIdAsync(command.OrgPositionId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrgPosition), command.OrgPositionId);

        if (!position.IsActive)
            throw new ValidationException("OrgPositionId", "לא ניתן לשייך לתפקיד לא פעיל");

        _ = await _contactRepository.GetByIdAsync(command.ContactId, cancellationToken)
            ?? throw new NotFoundException(nameof(Contact), command.ContactId);

        _ = await _candidacyRepository.GetByIdAsync(command.CandidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), command.CandidacyId);

        // Check occupancy limit
        var activeAssignments = await _assignmentRepository.FindAsync(
            a => a.OrgPositionId == command.OrgPositionId && a.IsActive, cancellationToken);
        if (activeAssignments.Count() >= position.MaxOccupants)
            throw new ValidationException("OrgPositionId", "התפקיד מאויש במלואו");

        // Check duplicate assignment
        var existingAssignment = await _assignmentRepository.FindAsync(
            a => a.OrgPositionId == command.OrgPositionId
                 && a.ContactId == command.ContactId
                 && a.IsActive, cancellationToken);
        if (existingAssignment.Any())
            throw new ValidationException("ContactId", "איש קשר זה כבר משויך לתפקיד זה");

        var entity = new PositionAssignment
        {
            OrgPositionId = command.OrgPositionId,
            ContactId = command.ContactId,
            CandidacyId = command.CandidacyId,
            StartDate = command.StartDate,
            IsActive = true
        };

        await _assignmentRepository.AddAsync(entity, cancellationToken);
        return ToAssignmentDto(entity);
    }

    public async Task UnassignFromPositionAsync(UnassignFromPositionCommand command, CancellationToken cancellationToken = default)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(command.AssignmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(PositionAssignment), command.AssignmentId);

        if (!assignment.IsActive)
            throw new ValidationException("AssignmentId", "שיוך זה כבר לא פעיל");

        assignment.IsActive = false;
        assignment.EndDate = DateTime.UtcNow;
        assignment.UpdatedAt = DateTime.UtcNow;

        await _assignmentRepository.UpdateAsync(assignment, cancellationToken);
    }

    #endregion

    #region Occupancy

    public async Task<SubUnitOccupancyDto> GetPositionOccupancyAsync(int subUnitId, CancellationToken cancellationToken = default)
    {
        var subUnit = await _subUnitRepository.GetByIdAsync(subUnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrgSubUnit), subUnitId);

        var positions = await _positionRepository.FindAsync(
            p => p.OrgSubUnitId == subUnitId && p.IsActive, cancellationToken);

        var positionOccupancies = new List<PositionOccupancyDto>();

        foreach (var position in positions)
        {
            var activeAssignments = await _assignmentRepository.FindAsync(
                a => a.OrgPositionId == position.Id && a.IsActive, cancellationToken);

            var filledCount = activeAssignments.Count();
            positionOccupancies.Add(new PositionOccupancyDto(
                PositionId: position.Id,
                Title: position.Title,
                MaxOccupants: position.MaxOccupants,
                FilledCount: filledCount,
                VacantCount: position.MaxOccupants - filledCount));
        }

        return new SubUnitOccupancyDto(
            SubUnitId: subUnit.Id,
            SubUnitName: subUnit.Name,
            Positions: positionOccupancies);
    }

    #endregion

    #region Private Helpers

    private OrgSubUnitTreeDto BuildTreeNode(
        OrgSubUnit node,
        List<OrgSubUnit> allSubUnits,
        List<OrgPosition> allPositions)
    {
        var childSubUnits = allSubUnits.Where(s => s.ParentOrgSubUnitId == node.Id);
        var nodePositions = allPositions.Where(p => p.OrgSubUnitId == node.Id);

        return new OrgSubUnitTreeDto(
            Id: node.Id,
            OrgUnitId: node.OrgUnitId,
            ParentOrgSubUnitId: node.ParentOrgSubUnitId,
            Name: node.Name,
            Description: node.Description,
            IsActive: node.IsActive,
            Children: childSubUnits.Select(c => BuildTreeNode(c, allSubUnits, allPositions)),
            Positions: nodePositions.Select(ToPositionDto));
    }

    private static OrgSubUnitDto ToSubUnitDto(OrgSubUnit entity) =>
        new(entity.Id, entity.OrgUnitId, entity.ParentOrgSubUnitId,
            entity.Name, entity.Description, entity.IsActive, entity.CreatedAt);

    private static OrgPositionDto ToPositionDto(OrgPosition entity) =>
        new(entity.Id, entity.OrgSubUnitId, entity.Title,
            entity.MaxOccupants, entity.IsActive, entity.CreatedAt);

    private static PositionAssignmentDto ToAssignmentDto(PositionAssignment entity) =>
        new(entity.Id, entity.OrgPositionId, entity.ContactId, entity.CandidacyId,
            entity.StartDate, entity.EndDate, entity.IsActive, entity.CreatedAt);

    #endregion
}
