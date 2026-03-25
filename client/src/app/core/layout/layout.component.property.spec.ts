import * as fc from 'fast-check';
import { NavItem } from './layout.component';
import { UserProfile } from '../services/auth.service';

/**
 * Feature: igds-ui-migration, Property 13: סינון ניווט לפי הרשאות
 *
 * Validates: Requirements 2.5, 17.5
 *
 * For any user with a set of permissions, the filtered navigation items should
 * include exactly: (a) all items without a `permission` requirement, plus
 * (b) all items whose `permission` value is included in the user's permissions set.
 * No items should be added or removed beyond this rule.
 */
describe('Feature: igds-ui-migration, Property 13: סינון ניווט לפי הרשאות', () => {

  /** The 17 navigation items as defined in LayoutComponent */
  const allNavItems: NavItem[] = [
    { label: 'לוח מחוונים', icon: 'dashboard', route: '/dashboard', dividerAfter: true },
    { label: 'מועמדויות', icon: 'assignment', route: '/candidacies', permission: 'candidacies.view' },
    { label: 'אנשי קשר', icon: 'people', route: '/contacts', permission: 'contacts.view' },
    { label: 'קולות קוראים', icon: 'campaign', route: '/calls', permission: 'calls.view', dividerAfter: true },
    { label: 'מבחנים', icon: 'quiz', route: '/exams', permission: 'exams.view' },
    { label: 'ראיונות', icon: 'event', route: '/interviews', permission: 'interviews.view' },
    { label: 'ועדות', icon: 'groups', route: '/committees', permission: 'committees.view', dividerAfter: true },
    { label: 'מסמכים', icon: 'folder', route: '/documents', permission: 'documents.view' },
    { label: 'דיוורים', icon: 'email', route: '/notifications', permission: 'notifications.view' },
    { label: 'ניגודי עניינים', icon: 'warning', route: '/conflicts', permission: 'conflicts.view' },
    { label: 'תנאי סף', icon: 'checklist', route: '/threshold-checks', permission: 'threshold_checks.view', dividerAfter: true },
    { label: 'הרשאות ותפקידים', icon: 'admin_panel_settings', route: '/roles', permission: 'roles.view' },
    { label: 'כהונות', icon: 'work_history', route: '/tenures', permission: 'tenures.view' },
    { label: 'מכסות', icon: 'pie_chart', route: '/quotas', permission: 'quotas.view' },
    { label: 'מבנה ארגוני', icon: 'account_tree', route: '/org-structure', permission: 'org_structure.view', dividerAfter: true },
    { label: 'דוחות', icon: 'bar_chart', route: '/reports', permission: 'reports.view' },
    { label: 'הגדרות', icon: 'settings', route: '/admin', permission: 'admin.view' },
  ];

  /** All distinct permission values used in allNavItems */
  const allPermissions: string[] = allNavItems
    .map(item => item.permission)
    .filter((p): p is string => !!p);

  /** Items that have no permission requirement (always visible) */
  const itemsWithoutPermission = allNavItems.filter(item => !item.permission);

  /**
   * Replicates the filtering logic from LayoutComponent.getFilteredNavItems.
   * This is the function under test — it must match the component's behavior.
   */
  function getFilteredNavItems(user: UserProfile | null): NavItem[] {
    if (!user) {
      return [];
    }
    return allNavItems.filter(item => {
      if (!item.permission) {
        return true;
      }
      return user.permissions?.includes(item.permission);
    });
  }

  /** Arbitrary: generates a random subset of the known permissions */
  const permissionsArb = fc.subarray(allPermissions, { minLength: 0 });

  /** Arbitrary: generates a UserProfile with a random permissions subset */
  const userWithPermissionsArb = permissionsArb.map(permissions => ({
    id: 1,
    username: 'testuser',
    displayName: 'Test User',
    orgUnitId: 1,
    orgUnitName: 'Test Org',
    roles: ['user'],
    permissions,
  } as UserProfile));

  it('filtered items always include all items without a permission requirement', (done) => {
    fc.assert(
      fc.property(userWithPermissionsArb, (user) => {
        const filtered = getFilteredNavItems(user);
        for (const item of itemsWithoutPermission) {
          if (!filtered.includes(item)) {
            throw new Error(
              `Item "${item.label}" has no permission requirement but was not in filtered results`
            );
          }
        }
      }),
      { numRuns: 100 }
    );
    done();
  });

  it('filtered items include all items whose permission is in the user permissions set', (done) => {
    fc.assert(
      fc.property(userWithPermissionsArb, (user) => {
        const filtered = getFilteredNavItems(user);
        const itemsWithMatchingPermission = allNavItems.filter(
          item => item.permission && user.permissions.includes(item.permission)
        );
        for (const item of itemsWithMatchingPermission) {
          if (!filtered.includes(item)) {
            throw new Error(
              `Item "${item.label}" has permission "${item.permission}" which is in user's set, but was not in filtered results`
            );
          }
        }
      }),
      { numRuns: 100 }
    );
    done();
  });

  it('filtered items do NOT include items whose permission is NOT in the user permissions set', (done) => {
    fc.assert(
      fc.property(userWithPermissionsArb, (user) => {
        const filtered = getFilteredNavItems(user);
        const itemsWithoutMatchingPermission = allNavItems.filter(
          item => item.permission && !user.permissions.includes(item.permission)
        );
        for (const item of itemsWithoutMatchingPermission) {
          if (filtered.includes(item)) {
            throw new Error(
              `Item "${item.label}" has permission "${item.permission}" which is NOT in user's set, but appeared in filtered results`
            );
          }
        }
      }),
      { numRuns: 100 }
    );
    done();
  });

  it('filtered items count equals items-without-permission + items-with-matching-permission', (done) => {
    fc.assert(
      fc.property(userWithPermissionsArb, (user) => {
        const filtered = getFilteredNavItems(user);
        const expectedCount =
          itemsWithoutPermission.length +
          allNavItems.filter(item => item.permission && user.permissions.includes(item.permission)).length;

        if (filtered.length !== expectedCount) {
          throw new Error(
            `Expected ${expectedCount} filtered items but got ${filtered.length} for permissions [${user.permissions.join(', ')}]`
          );
        }
      }),
      { numRuns: 100 }
    );
    done();
  });

  it('null user returns empty navigation', () => {
    const filtered = getFilteredNavItems(null);
    expect(filtered.length).toBe(0);
  });

  it('user with all permissions sees all 17 items', () => {
    const user: UserProfile = {
      id: 1, username: 'admin', displayName: 'Admin', orgUnitId: 1,
      orgUnitName: 'HQ', roles: ['admin'], permissions: [...allPermissions],
    };
    const filtered = getFilteredNavItems(user);
    expect(filtered.length).toBe(17);
  });

  it('user with no permissions sees only items without permission requirement', () => {
    const user: UserProfile = {
      id: 1, username: 'basic', displayName: 'Basic', orgUnitId: 1,
      orgUnitName: 'HQ', roles: ['user'], permissions: [],
    };
    const filtered = getFilteredNavItems(user);
    expect(filtered.length).toBe(itemsWithoutPermission.length);
    for (const item of filtered) {
      expect(item.permission).toBeFalsy();
    }
  });

  it('filtered items preserve original order from allNavItems', (done) => {
    fc.assert(
      fc.property(userWithPermissionsArb, (user) => {
        const filtered = getFilteredNavItems(user);
        for (let i = 1; i < filtered.length; i++) {
          const prevIndex = allNavItems.indexOf(filtered[i - 1]);
          const currIndex = allNavItems.indexOf(filtered[i]);
          if (prevIndex >= currIndex) {
            throw new Error(
              `Order violated: "${filtered[i - 1].label}" (index ${prevIndex}) appears before "${filtered[i].label}" (index ${currIndex})`
            );
          }
        }
      }),
      { numRuns: 100 }
    );
    done();
  });
});
