import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute, NavigationEnd } from '@angular/router';
import { Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';
import { IgdsBreadcrumbItem } from '@igds/angular';

export interface Breadcrumb {
  label: string;
  url: string;
}

@Component({
  selector: 'app-breadcrumbs',
  template: `
    <div class="igds-breadcrumbs-wrapper" *ngIf="breadcrumbItems.length > 1">
      <igds-breadcrumbs
        [items]="breadcrumbItems"
        (navigate)="onNavigate($event)">
      </igds-breadcrumbs>
    </div>
  `,
  styles: [
    `
      :host {
        display: block;
        direction: inherit;
      }
      .igds-breadcrumbs-wrapper {
        padding: var(--igds-space-8) var(--igds-space-16);
        background: var(--igds-bg-neutral);
        border-block-end: 1px solid var(--igds-border-divider);
      }
    `,
  ],
})
export class BreadcrumbsComponent implements OnInit, OnDestroy {
  breadcrumbs: Breadcrumb[] = [];
  breadcrumbItems: IgdsBreadcrumbItem[] = [];
  private destroy$ = new Subject<void>();

  constructor(private router: Router, private activatedRoute: ActivatedRoute) {}

  ngOnInit(): void {
    this.router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.breadcrumbs = this.buildBreadcrumbs(this.activatedRoute.root);
        this.breadcrumbItems = this.mapToBreadcrumbItems(this.breadcrumbs);
      });

    // Build initial breadcrumbs
    this.breadcrumbs = this.buildBreadcrumbs(this.activatedRoute.root);
    this.breadcrumbItems = this.mapToBreadcrumbItems(this.breadcrumbs);
  }

  /** Maps Breadcrumb[] to IgdsBreadcrumbItem[] for igds-breadcrumbs */
  private mapToBreadcrumbItems(breadcrumbs: Breadcrumb[]): IgdsBreadcrumbItem[] {
    return breadcrumbs.map((crumb) => ({
      label: crumb.label,
      url: crumb.url,
    }));
  }

  onNavigate(item: IgdsBreadcrumbItem): void {
    if (item.url) {
      this.router.navigate([item.url]);
    }
  }

  private buildBreadcrumbs(
    route: ActivatedRoute,
    url: string = '',
    breadcrumbs: Breadcrumb[] = []
  ): Breadcrumb[] {
    const children = route.children;

    if (children.length === 0) {
      return breadcrumbs;
    }

    for (const child of children) {
      const routeURL: string = child.snapshot.url
        .map((segment) => segment.path)
        .join('/');

      if (routeURL !== '') {
        url += `/${routeURL}`;
      }

      const label = child.snapshot.data['breadcrumb'];
      if (label) {
        breadcrumbs.push({ label, url: url || '/' });
      }

      return this.buildBreadcrumbs(child, url, breadcrumbs);
    }

    return breadcrumbs;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
