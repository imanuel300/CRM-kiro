import { Component, Inject } from '@angular/core';
import {
  IGDS_MODAL_DATA,
  IGDS_MODAL_REF,
  IgdsModalRef,
} from '../../igds/services/igds-modal.service';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
}

@Component({
  selector: 'app-confirm-dialog',
  template: `
    <p class="igds-confirm-dialog__message">{{ data.message }}</p>
    <div class="igds-confirm-dialog__actions">
      <igds-button variant="secondary" (click)="onCancel()">{{ data.cancelText || 'ביטול' }}</igds-button>
      <igds-button variant="primary" (click)="onConfirm()">{{ data.confirmText || 'אישור' }}</igds-button>
    </div>
  `,
  styles: [
    `
      :host {
        display: block;
        direction: inherit;
      }
      .igds-confirm-dialog__message {
        font-family: var(--igds-font-family);
        font-size: var(--igds-font-size-md);
        color: var(--igds-text-primary);
        margin: 0 0 var(--igds-space-24) 0;
        line-height: 1.6;
      }
      .igds-confirm-dialog__actions {
        display: flex;
        justify-content: flex-end;
        gap: var(--igds-space-8);
      }
    `,
  ],
})
export class ConfirmDialogComponent {
  constructor(
    @Inject(IGDS_MODAL_REF) private modalRef: IgdsModalRef,
    @Inject(IGDS_MODAL_DATA) public data: ConfirmDialogData
  ) {}

  onConfirm(): void {
    this.modalRef.close(true);
  }

  onCancel(): void {
    this.modalRef.close(false);
  }
}
