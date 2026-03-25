import { TestBed } from '@angular/core/testing';
import { IgdsToastService } from '@igds/angular';
import { IgdsToastComponent } from '@igds/angular';

describe('IgdsToastService', () => {
  let service: IgdsToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [IgdsToastComponent],
    });
    service = TestBed.inject(IgdsToastService);
  });

  afterEach(() => {
    document.querySelectorAll('body > div > igds-toast').forEach(el => {
      el.parentElement?.remove();
    });
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should show a success toast', () => {
    service.success('Operation succeeded');
    const toast = document.querySelector('.igds-toast--success');
    expect(toast).toBeTruthy();
    expect(toast?.querySelector('.igds-toast__message')?.textContent).toBe('Operation succeeded');
  });

  it('should show an error toast with failure type', () => {
    service.error('Something failed');
    const toast = document.querySelector('.igds-toast--failure');
    expect(toast).toBeTruthy();
    expect(toast?.querySelector('.igds-toast__message')?.textContent).toBe('Something failed');
  });

  it('should show a warning toast', () => {
    service.warning('Be careful');
    const toast = document.querySelector('.igds-toast--warning');
    expect(toast).toBeTruthy();
    expect(toast?.querySelector('.igds-toast__message')?.textContent).toBe('Be careful');
  });

  it('should show an info toast', () => {
    service.info('FYI');
    const toast = document.querySelector('.igds-toast--info');
    expect(toast).toBeTruthy();
    expect(toast?.querySelector('.igds-toast__message')?.textContent).toBe('FYI');
  });

  it('should replace previous toast when a new one is shown', () => {
    service.success('First');
    service.warning('Second');

    const toasts = document.querySelectorAll('.igds-toast');
    expect(toasts.length).toBe(1);
    expect(toasts[0].classList).toContain('igds-toast--warning');
    expect(toasts[0].querySelector('.igds-toast__message')?.textContent).toBe('Second');
  });

  it('should not show toast for empty message', () => {
    service.success('');
    const toast = document.querySelector('.igds-toast');
    expect(toast).toBeFalsy();
  });

  it('should clean up DOM after toast closes', () => {
    service.info('Temporary');
    expect(document.querySelector('.igds-toast')).toBeTruthy();

    const closeBtn = document.querySelector('.igds-toast__close') as HTMLButtonElement;
    closeBtn?.click();

    expect(document.querySelector('.igds-toast')).toBeFalsy();
  });

  it('should accept a custom duration', () => {
    service.success('Quick', 1000);
    const toast = document.querySelector('.igds-toast--success');
    expect(toast).toBeTruthy();
  });
});
