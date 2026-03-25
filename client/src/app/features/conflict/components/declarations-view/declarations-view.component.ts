import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import {
  CandidacyDeclarations,
  ConflictOfInterest,
  FamilyRelation,
  RELATION_TYPES,
} from '../../models/conflict.models';
import { ConflictService } from '../../services/conflict.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsModalService } from '@igds/angular';
import { ConfirmDialogComponent } from '@shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-declarations-view',
  template: `
    <div class="page-header">
      <h1>הצהרות ניגוד עניינים וקרבה משפחתית</h1>
      <span class="spacer"></span>
      <igds-button variant="primary" [iconBefore]="true" (onClick)="onAddConflict()">
        <span igds-icon-before>➕</span>
        הצהרת ניגוד עניינים
      </igds-button>
      <igds-button variant="primary" [iconBefore]="true" (onClick)="onAddFamilyRelation()">
        <span igds-icon-before>👥</span>
        הצהרת קרבה משפחתית
      </igds-button>
    </div>

    <app-loading-spinner [loading]="loading"></app-loading-spinner>

    <div *ngIf="!loading && declarations">
      <h2>הצהרות ניגוד עניינים</h2>
      <igds-card *ngIf="declarations.conflictsOfInterest.length === 0" class="empty-card">
        <p class="empty-text">לא קיימות הצהרות ניגוד עניינים למועמדות זו</p>
      </igds-card>

      <igds-card *ngFor="let conflict of declarations.conflictsOfInterest" class="declaration-card">
        <div igds-card-header>
          <div class="card-title-row">
            <span class="card-title">הצהרת ניגוד עניינים #{{ conflict.id }}</span>
            <span class="status-tags">
              <igds-tag
                [label]="conflict.hasConflict ? 'קיים ניגוד' : 'אין ניגוד'"
                [variant]="conflict.hasConflict ? 'warning' : 'success'">
              </igds-tag>
              <igds-tag
                *ngIf="conflict.requiresManualReview"
                label="דורש בדיקה ידנית"
                variant="failure">
              </igds-tag>
              <igds-tag
                *ngIf="conflict.reviewedAt"
                label="נבדק"
                variant="success">
              </igds-tag>
            </span>
          </div>
        </div>
        <p><strong>תשובות שאלון:</strong></p>
        <p class="questionnaire-text">{{ conflict.questionnaireResponses }}</p>
        <p *ngIf="conflict.reviewedAt">
          <strong>נבדק בתאריך:</strong> {{ conflict.reviewedAt | hebrewDate }}
        </p>
        <div igds-card-footer>
          <div class="card-actions">
            <igds-button variant="secondary" [iconBefore]="true"
                        *ngIf="conflict.requiresManualReview"
                        (onClick)="onReviewConflict(conflict)">
              <span igds-icon-before>✅</span>
              סמן כנבדק
            </igds-button>
            <igds-button variant="secondary" [iconOnly]="true"
                        ariaLabel="עריכה"
                        [igdsTooltip]="'עריכה'"
                        (onClick)="onEditConflict(conflict)">
              <span igds-icon>✏️</span>
            </igds-button>
            <igds-button variant="secondary" [iconOnly]="true"
                        ariaLabel="מחיקה"
                        [igdsTooltip]="'מחיקה'"
                        (onClick)="onDeleteConflict(conflict)">
              <span igds-icon>🗑️</span>
            </igds-button>
          </div>
        </div>
      </igds-card>

      <h2>הצהרות קרבה משפחתית</h2>
      <igds-card *ngIf="declarations.familyRelations.length === 0" class="empty-card">
        <p class="empty-text">לא קיימות הצהרות קרבה משפחתית למועמדות זו</p>
      </igds-card>

      <igds-card *ngFor="let relation of declarations.familyRelations" class="declaration-card">
        <div igds-card-header>
          <div class="card-title-row">
            <span class="card-title">{{ relation.relatedPersonName }} - {{ getRelationLabel(relation.relationType) }}</span>
            <span class="status-tags">
              <igds-tag
                *ngIf="relation.requiresManualReview"
                label="דורש בדיקה ידנית"
                variant="failure">
              </igds-tag>
            </span>
          </div>
        </div>
        <p><strong>סוג קרבה:</strong> {{ getRelationLabel(relation.relationType) }}</p>
        <p><strong>שם הקרוב:</strong> {{ relation.relatedPersonName }}</p>
        <p *ngIf="relation.relatedPersonRole">
          <strong>תפקיד הקרוב:</strong> {{ relation.relatedPersonRole }}
        </p>
        <div igds-card-footer>
          <div class="card-actions">
            <igds-button variant="secondary" [iconOnly]="true"
                        ariaLabel="עריכה"
                        [igdsTooltip]="'עריכה'"
                        (onClick)="onEditFamilyRelation(relation)">
              <span igds-icon>✏️</span>
            </igds-button>
            <igds-button variant="secondary" [iconOnly]="true"
                        ariaLabel="מחיקה"
                        [igdsTooltip]="'מחיקה'"
                        (onClick)="onDeleteFamilyRelation(relation)">
              <span igds-icon>🗑️</span>
            </igds-button>
          </div>
        </div>
      </igds-card>
    </div>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .page-header {
      display: flex;
      align-items: center;
      gap: var(--igds-space-8);
      margin-block-end: var(--igds-space-16);
    }
    .page-header h1 {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-xl);
      color: var(--igds-text-primary);
      margin: 0;
    }
    .spacer { flex: 1; }
    .declaration-card { margin-block-end: var(--igds-space-12); }
    .empty-card { margin-block-end: var(--igds-space-12); }
    .empty-text {
      text-align: center;
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }
    .card-title-row {
      display: flex;
      align-items: center;
      gap: var(--igds-space-8);
      flex-wrap: wrap;
    }
    .card-title {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-md);
      font-weight: var(--igds-font-weight-medium);
      color: var(--igds-text-primary);
    }
    .status-tags {
      display: inline-flex;
      gap: var(--igds-space-4);
      margin-inline-start: var(--igds-space-8);
    }
    .questionnaire-text {
      white-space: pre-wrap;
      background: var(--igds-bg-neutral-secondlevel);
      padding: var(--igds-space-12);
      border-radius: var(--igds-radius-md);
      font-family: var(--igds-font-family);
    }
    .card-actions {
      display: flex;
      justify-content: flex-end;
      gap: var(--igds-space-8);
    }
    h2 {
      margin-block-start: var(--igds-space-24);
      margin-block-end: var(--igds-space-12);
      font-family: var(--igds-font-family);
      color: var(--igds-text-primary);
    }
  `],
})
export class DeclarationsViewComponent implements OnInit {
  candidacyId!: number;
  declarations: CandidacyDeclarations | null = null;
  loading = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private conflictService: ConflictService,
    private modalService: IgdsModalService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.candidacyId = Number(this.route.snapshot.paramMap.get('candidacyId'));
    this.loadDeclarations();
  }

  loadDeclarations(): void {
    this.loading = true;
    this.conflictService.getDeclarationsForCandidacy(this.candidacyId).subscribe({
      next: (data: CandidacyDeclarations) => {
        this.declarations = data;
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת הצהרות');
        this.loading = false;
      },
    });
  }

  getRelationLabel(type: string): string {
    return RELATION_TYPES.find(r => r.value === type)?.label ?? type;
  }

  onAddConflict(): void {
    this.router.navigate(['/conflicts/candidacy', this.candidacyId, 'questionnaire', 'new']);
  }

  onEditConflict(conflict: ConflictOfInterest): void {
    this.router.navigate(['/conflicts/candidacy', this.candidacyId, 'questionnaire', conflict.id]);
  }

  onDeleteConflict(conflict: ConflictOfInterest): void {
    const modalRef = this.modalService.open<boolean>({
      title: 'מחיקת הצהרה',
      component: ConfirmDialogComponent,
      data: {
        title: 'מחיקת הצהרה',
        message: 'האם למחוק את הצהרת ניגוד העניינים?',
        confirmText: 'מחיקה',
        cancelText: 'ביטול',
      },
    });
    modalRef.afterClosed().subscribe((confirmed: boolean | undefined) => {
      if (confirmed) {
        this.conflictService.deleteConflict(conflict.id).subscribe({
          next: () => {
            this.notification.success('ההצהרה נמחקה בהצלחה');
            this.loadDeclarations();
          },
          error: () => this.notification.error('שגיאה במחיקת ההצהרה'),
        });
      }
    });
  }

  onReviewConflict(conflict: ConflictOfInterest): void {
    this.conflictService.reviewConflict({ id: conflict.id, reviewedByUserId: 0 }).subscribe({
      next: () => {
        this.notification.success('ההצהרה סומנה כנבדקה');
        this.loadDeclarations();
      },
      error: () => this.notification.error('שגיאה בסימון ההצהרה'),
    });
  }

  onAddFamilyRelation(): void {
    this.router.navigate(
      ['/conflicts/candidacy', this.candidacyId, 'questionnaire', 'new'],
      { queryParams: { type: 'family' } }
    );
  }

  onEditFamilyRelation(relation: FamilyRelation): void {
    this.router.navigate(
      ['/conflicts/candidacy', this.candidacyId, 'questionnaire', relation.id],
      { queryParams: { type: 'family' } }
    );
  }

  onDeleteFamilyRelation(relation: FamilyRelation): void {
    const modalRef = this.modalService.open<boolean>({
      title: 'מחיקת הצהרה',
      component: ConfirmDialogComponent,
      data: {
        title: 'מחיקת הצהרה',
        message: `האם למחוק את הצהרת הקרבה המשפחתית של "${relation.relatedPersonName}"?`,
        confirmText: 'מחיקה',
        cancelText: 'ביטול',
      },
    });
    modalRef.afterClosed().subscribe((confirmed: boolean | undefined) => {
      if (confirmed) {
        this.conflictService.deleteFamilyRelation(relation.id).subscribe({
          next: () => {
            this.notification.success('ההצהרה נמחקה בהצלחה');
            this.loadDeclarations();
          },
          error: () => this.notification.error('שגיאה במחיקת ההצהרה'),
        });
      }
    });
  }
}
