import {
  ApplicationRef,
  ComponentRef,
  createComponent,
  EnvironmentInjector,
  Injectable,
} from '@angular/core';
import { IgdsToastComponent } from '../components/toast/igds-toast.component';

type ToastType = 'success' | 'warning' | 'failure' | 'info';

@Injectable({ providedIn: 'root' })
export class IgdsToastService {
  private toastRef: ComponentRef<IgdsToastComponent> | null = null;
  private hostElement: HTMLElement | null = null;

  constructor(
    private appRef: ApplicationRef,
    private environmentInjector: EnvironmentInjector
  ) {}

  success(message: string, duration?: number): void {
    this.show(message, 'success', duration);
  }

  error(message: string, duration?: number): void {
    this.show(message, 'failure', duration);
  }

  warning(message: string, duration?: number): void {
    this.show(message, 'warning', duration);
  }

  info(message: string, duration?: number): void {
    this.show(message, 'info', duration);
  }

  private show(message: string, type: ToastType, duration?: number): void {
    if (!message) return;

    this.cleanup();

    this.hostElement = document.createElement('div');
    document.body.appendChild(this.hostElement);

    this.toastRef = createComponent(IgdsToastComponent, {
      hostElement: this.hostElement,
      environmentInjector: this.environmentInjector,
    });

    const instance = this.toastRef.instance;
    instance.message = message;
    instance.type = type;
    if (duration !== undefined) {
      instance.duration = duration;
    }
    instance.visible = true;

    instance.closed.subscribe(() => this.cleanup());

    this.appRef.attachView(this.toastRef.hostView);
    this.toastRef.changeDetectorRef.detectChanges();
  }

  private cleanup(): void {
    if (this.toastRef) {
      this.appRef.detachView(this.toastRef.hostView);
      this.toastRef.destroy();
      this.toastRef = null;
    }
    if (this.hostElement) {
      this.hostElement.remove();
      this.hostElement = null;
    }
  }
}
