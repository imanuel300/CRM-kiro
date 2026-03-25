import { Injectable } from '@angular/core';
import { IgdsToastService } from '@igds/angular';

export type NotificationType = 'success' | 'error' | 'warning' | 'info';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly defaultDuration = 4000;

  constructor(private toastService: IgdsToastService) {}

  success(message: string, duration?: number): void {
    this.toastService.success(message, duration ?? this.defaultDuration);
  }

  error(message: string, duration?: number): void {
    this.toastService.error(message, duration ?? 6000);
  }

  warning(message: string, duration?: number): void {
    this.toastService.warning(message, duration ?? this.defaultDuration);
  }

  info(message: string, duration?: number): void {
    this.toastService.info(message, duration ?? this.defaultDuration);
  }
}
