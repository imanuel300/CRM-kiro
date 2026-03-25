import {
  ApplicationRef,
  ComponentRef,
  createComponent,
  EnvironmentInjector,
  Injectable,
  Injector,
  TemplateRef,
  Type,
} from '@angular/core';
import { Subject, Observable } from 'rxjs';
import { IgdsModalComponent } from '../components/modal/igds-modal.component';

export interface IgdsModalConfig {
  title: string;
  component?: Type<any>;
  template?: TemplateRef<any>;
  data?: any;
  closable?: boolean;
}

export interface IgdsModalRef<T = any> {
  afterClosed(): Observable<T | undefined>;
  close(result?: T): void;
}

@Injectable({ providedIn: 'root' })
export class IgdsModalService {
  constructor(
    private appRef: ApplicationRef,
    private injector: Injector,
    private environmentInjector: EnvironmentInjector
  ) {}

  open<T = any>(config: IgdsModalConfig): IgdsModalRef<T> {
    const afterClosed$ = new Subject<T | undefined>();
    let destroyed = false;

    // Create a host element in the DOM
    const hostElement = document.createElement('div');
    document.body.appendChild(hostElement);

    // Create the modal component dynamically
    const modalRef: ComponentRef<IgdsModalComponent> = createComponent(IgdsModalComponent, {
      hostElement,
      environmentInjector: this.environmentInjector,
    });

    // Configure the modal
    modalRef.instance.title = config.title;
    modalRef.instance.visible = true;
    modalRef.instance.closable = config.closable !== false;

    // Cleanup function
    const cleanup = () => {
      if (destroyed) return;
      destroyed = true;
      if (contentRef) {
        this.appRef.detachView(contentRef.hostView);
        contentRef.destroy();
      }
      this.appRef.detachView(modalRef.hostView);
      modalRef.destroy();
      hostElement.remove();
    };

    // Close handler (called programmatically via IgdsModalRef.close)
    const close = (result?: T) => {
      if (destroyed) return;
      modalRef.instance.visible = false;
      afterClosed$.next(result);
      afterClosed$.complete();
      cleanup();
    };

    // Listen for the modal's own close event (overlay click, escape key, close button)
    modalRef.instance.closed.subscribe(() => {
      if (destroyed) return;
      afterClosed$.next(undefined);
      afterClosed$.complete();
      cleanup();
    });

    // Attach the modal view to the application
    this.appRef.attachView(modalRef.hostView);

    // Optionally create and inject a content component
    let contentRef: ComponentRef<any> | null = null;

    if (config.component) {
      const contentInjector = Injector.create({
        providers: [
          { provide: IGDS_MODAL_DATA, useValue: config.data },
          { provide: IGDS_MODAL_REF, useValue: { afterClosed: () => afterClosed$.asObservable(), close } },
        ],
        parent: this.injector,
      });

      contentRef = createComponent(config.component, {
        environmentInjector: this.environmentInjector,
        elementInjector: contentInjector,
      });

      this.appRef.attachView(contentRef.hostView);

      // Insert the content component's DOM into the modal body
      const modalBody = hostElement.querySelector('.igds-modal__body');
      if (modalBody) {
        modalBody.appendChild(contentRef.location.nativeElement);
      }
    }

    modalRef.changeDetectorRef.detectChanges();

    return {
      afterClosed: () => afterClosed$.asObservable(),
      close,
    };
  }
}

import { InjectionToken } from '@angular/core';

/** Injection token for passing data into a dynamically created modal content component. */
export const IGDS_MODAL_DATA = new InjectionToken<any>('IgdsModalData');

/** Injection token for accessing the modal ref from within a content component. */
export const IGDS_MODAL_REF = new InjectionToken<IgdsModalRef>('IgdsModalRef');
