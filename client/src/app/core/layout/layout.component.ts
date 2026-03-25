import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Subject, takeUntil } from 'rxjs';
import { AuthActions } from '../store/auth/auth.actions';
import { selectCurrentUser } from '../store/auth/auth.selectors';
import { AppState } from '../store/app.state';
import { UserProfile } from '../services/auth.service';
import { IgdsSideMenuItem } from '@igds/angular';

export interface NavItem {
  label: string;
  icon: string;
  route: string;
  permission?: string;
  dividerAfter?: boolean;
}

@Component({
  selector: 'app-layout',
  template: `
    <header class="igds-header">
      <button class="igds-header__menu-btn" type="button"
        (click)="sideMenuCollapsed = !sideMenuCollapsed"
        aria-label="פתח/סגור תפריט">
        <span class="igds-header__menu-icon">☰</span>
      </button>
      <span class="igds-header__title">מערכת ניהול מועמדויות</span>
      <span class="igds-header__spacer"></span>
      <span *ngIf="currentUser" class="igds-header__user-info">
        {{ currentUser.displayName }} | {{ currentUser.orgUnitName }}
      </span>
      <button class="igds-header__user-btn" type="button"
        (click)="userDrawerVisible = true"
        aria-label="תפריט משתמש">
        <span class="igds-header__user-icon">👤</span>
      </button>
    </header>

    <div class="igds-layout">
      <igds-side-menu
        [items]="menuItems"
        [collapsed]="sideMenuCollapsed"
        (itemClick)="onMenuItemClick($event)">
      </igds-side-menu>

      <main class="igds-layout__content">
        <app-breadcrumbs></app-breadcrumbs>
        <div class="igds-layout__page">
          <ng-content></ng-content>
        </div>
      </main>
    </div>

    <igds-drawer
      [visible]="userDrawerVisible"
      position="end"
      title="פרופיל משתמש"
      (closed)="userDrawerVisible = false">
      <div class="igds-user-drawer">
        <div *ngIf="currentUser" class="igds-user-drawer__info">
          <p class="igds-user-drawer__name">{{ currentUser.displayName }}</p>
          <p class="igds-user-drawer__org">{{ currentUser.orgUnitName }}</p>
        </div>
        <button class="igds-user-drawer__logout-btn" type="button" (click)="onLogout()">
          התנתק
        </button>
      </div>
    </igds-drawer>
  `,
  styles: [
    `
      :host {
        display: block;
        direction: inherit;
      }
      .igds-header {
        display: flex;
        align-items: center;
        height: 56px;
        padding: 0 var(--igds-space-16);
        background: var(--igds-bg-brand-default);
        color: var(--igds-text-on-brand);
        font-family: var(--igds-font-family);
      }
      .igds-header__menu-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        min-height: 44px;
        min-width: 44px;
        background: none;
        border: none;
        color: inherit;
        cursor: pointer;
        border-radius: var(--igds-radius-sm);
        font-size: var(--igds-font-size-lg);
      }
      .igds-header__menu-btn:hover {
        background: var(--igds-bg-brand-hover);
      }
      .igds-header__menu-btn:focus-visible {
        outline: 2px solid var(--igds-border-focused);
        outline-offset: -2px;
      }
      .igds-header__menu-icon {
        line-height: 1;
      }
      .igds-header__title {
        font-size: var(--igds-font-size-lg);
        font-weight: var(--igds-font-weight-bold);
        margin-inline-start: var(--igds-space-12);
      }
      .igds-header__spacer {
        flex: 1;
      }
      .igds-header__user-info {
        margin-inline-end: var(--igds-space-16);
        font-size: var(--igds-font-size-sm);
      }
      .igds-header__user-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        min-height: 44px;
        min-width: 44px;
        background: none;
        border: none;
        color: inherit;
        cursor: pointer;
        border-radius: var(--igds-radius-full);
        font-size: var(--igds-font-size-lg);
      }
      .igds-header__user-btn:hover {
        background: var(--igds-bg-brand-hover);
      }
      .igds-header__user-btn:focus-visible {
        outline: 2px solid var(--igds-border-focused);
        outline-offset: -2px;
      }
      .igds-header__user-icon {
        line-height: 1;
      }
      .igds-layout {
        display: flex;
        height: calc(100vh - 56px);
        font-family: var(--igds-font-family);
      }
      .igds-layout__content {
        display: flex;
        flex-direction: column;
        flex: 1;
        overflow: auto;
      }
      .igds-layout__page {
        padding: var(--igds-space-16);
        flex: 1;
      }
      .igds-user-drawer {
        display: flex;
        flex-direction: column;
        gap: var(--igds-space-16);
      }
      .igds-user-drawer__info {
        padding-block-end: var(--igds-space-16);
        border-block-end: 1px solid var(--igds-border-divider);
      }
      .igds-user-drawer__name {
        margin: 0;
        font-size: var(--igds-font-size-lg);
        font-weight: var(--igds-font-weight-bold);
        color: var(--igds-text-primary);
      }
      .igds-user-drawer__org {
        margin: var(--igds-space-4) 0 0;
        font-size: var(--igds-font-size-sm);
        color: var(--igds-text-secondary);
      }
      .igds-user-drawer__logout-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        min-height: 44px;
        padding: var(--igds-space-8) var(--igds-space-16);
        background: var(--igds-bg-danger-default);
        color: var(--igds-text-on-danger);
        border: none;
        border-radius: var(--igds-radius-md);
        cursor: pointer;
        font-family: var(--igds-font-family);
        font-size: var(--igds-font-size-sm);
        font-weight: var(--igds-font-weight-medium);
        transition: background var(--igds-transition-fast);
      }
      .igds-user-drawer__logout-btn:hover {
        background: var(--igds-bg-danger-hover);
      }
      .igds-user-drawer__logout-btn:focus-visible {
        outline: 2px solid var(--igds-border-focused);
        outline-offset: 2px;
      }
    `,
  ],
})
export class LayoutComponent implements OnInit, OnDestroy {
  currentUser: UserProfile | null = null;
  filteredNavItems: NavItem[] = [];
  menuItems: IgdsSideMenuItem[] = [];
  sideMenuCollapsed = false;
  userDrawerVisible = false;
  private destroy$ = new Subject<void>();

  /** All navigation items with optional permission requirements */
  private readonly allNavItems: NavItem[] = [
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

  constructor(private store: Store<AppState>, private router: Router) {}

  ngOnInit(): void {
    this.store
      .select(selectCurrentUser)
      .pipe(takeUntil(this.destroy$))
      .subscribe((user: UserProfile | null) => {
        this.currentUser = user;
        this.filteredNavItems = this.getFilteredNavItems(user);
        this.menuItems = this.mapToSideMenuItems(this.filteredNavItems);
      });
  }

  /**
   * Filters navigation items based on user permissions.
   * Items without a permission requirement (e.g. Dashboard) are always shown.
   */
  private getFilteredNavItems(user: UserProfile | null): NavItem[] {
    if (!user) {
      return [];
    }

    return this.allNavItems.filter((item) => {
      if (!item.permission) {
        return true;
      }
      return user.permissions?.includes(item.permission);
    });
  }

  /** Maps NavItem[] to IgdsSideMenuItem[] for igds-side-menu */
  private mapToSideMenuItems(navItems: NavItem[]): IgdsSideMenuItem[] {
    return navItems.map((item) => ({
      label: item.label,
      icon: item.icon,
      route: item.route,
    }));
  }

  onMenuItemClick(item: IgdsSideMenuItem): void {
    if (item.route) {
      this.router.navigate([item.route]);
    }
  }

  onLogout(): void {
    this.userDrawerVisible = false;
    this.store.dispatch(AuthActions.logout());
    this.router.navigate(['/login']);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
