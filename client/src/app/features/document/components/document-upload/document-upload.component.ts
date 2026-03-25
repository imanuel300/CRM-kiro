import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { DocumentService } from '../../services/document.service';
import { NotificationService } from '@core/services/notification.service';

@Component({
  selector: 'app-document-upload',
  template: `
    <div class="page-header">
      <h1>העלאת מסמך</h1>
    </div>

    <igds-card>
      <div igds-card-body>
        <form [formGroup]="form" (ngSubmit)="onSubmit()">
          <igds-input-field
            label="מזהה מועמדות"
            type="number"
            formControlName="candidacyId"
            [required]="true"
            [error]="form.get('candidacyId')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-input-field>

          <igds-input-field
            label="סוג מסמך"
            formControlName="documentType"
            [required]="true"
            [error]="form.get('documentType')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-input-field>

          <div
            class="drop-zone"
            [class.drag-over]="isDragOver"
            (dragover)="onDragOver($event)"
            (dragleave)="onDragLeave($event)"
            (drop)="onDrop($event)"
            (click)="fileInput.click()"
          >
            <span class="drop-icon">{{ selectedFile ? '📄' : '☁️' }}</span>
            <p *ngIf="!selectedFile">גררו קובץ לכאן או לחצו לבחירה</p>
            <p *ngIf="selectedFile" class="file-info">
              <strong>{{ selectedFile.name }}</strong>
              <br />
              {{ formatFileSize(selectedFile.size) }}
            </p>
            <input
              #fileInput
              type="file"
              hidden
              (change)="onFileSelected($event)"
            />
          </div>

          <div class="actions-row">
            <igds-button variant="primary" type="submit"
                         [disabled]="form.invalid || !selectedFile || uploading">
              {{ uploading ? 'מעלה...' : 'העלאה' }}
            </igds-button>
            <igds-button variant="secondary" type="button" routerLink="/documents">
              ביטול
            </igds-button>
          </div>
        </form>
      </div>
    </igds-card>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .page-header { margin-block-end: var(--igds-space-16); }
    .page-header h1 {
      font-family: var(--igds-font-family);
      color: var(--igds-text-primary);
      margin: 0;
    }
    .drop-zone {
      border: 2px dashed var(--igds-border-divider);
      border-radius: var(--igds-radius-md);
      padding: var(--igds-space-40) var(--igds-space-20);
      text-align: center;
      cursor: pointer;
      transition: border-color 0.2s, background-color 0.2s;
      margin: var(--igds-space-16) 0;
      font-family: var(--igds-font-family);
      color: var(--igds-text-secondary);
    }
    .drop-zone:hover, .drop-zone.drag-over {
      border-color: var(--igds-border-focused);
      background-color: var(--igds-bg-neutral-secondlevel);
    }
    .drop-icon { font-size: 48px; display: block; margin-block-end: var(--igds-space-8); }
    .file-info { margin: var(--igds-space-8) 0 0; }
    .actions-row {
      display: flex;
      gap: var(--igds-space-12);
      margin-block-start: var(--igds-space-16);
    }
  `],
})
export class DocumentUploadComponent implements OnInit {
  form!: FormGroup;
  selectedFile: File | null = null;
  isDragOver = false;
  uploading = false;

  constructor(
    private fb: FormBuilder,
    private documentService: DocumentService,
    private router: Router,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      candidacyId: [null, Validators.required],
      documentType: ['', Validators.required],
    });
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.selectedFile = files[0];
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
    }
  }

  onSubmit(): void {
    if (this.form.invalid || !this.selectedFile) return;

    this.uploading = true;
    const { candidacyId, documentType } = this.form.value;

    this.documentService.upload({
      candidacyId,
      documentType,
      fileName: this.selectedFile.name,
      blobUrl: '', // Set by backend after storage
      contentType: this.selectedFile.type,
      sizeBytes: this.selectedFile.size,
    }).subscribe({
      next: () => {
        this.notification.success('המסמך הועלה בהצלחה');
        this.router.navigate(['/documents'], { queryParams: { candidacyId } });
      },
      error: () => {
        this.notification.error('שגיאה בהעלאת המסמך');
        this.uploading = false;
      },
    });
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }
}
