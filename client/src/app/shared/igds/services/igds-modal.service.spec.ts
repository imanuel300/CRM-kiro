import { TestBed } from '@angular/core/testing';
import { Component, Inject } from '@angular/core';
import { IgdsModalService, IgdsModalConfig, IGDS_MODAL_DATA, IGDS_MODAL_REF, IgdsModalRef } from '@igds/angular';
import { IgdsModalComponent } from '@igds/angular';

@Component({
  selector: 'test-content',
  template: '<p>Test content: {{ data?.message }}</p>',
})
class TestContentComponent {
  constructor(
    @Inject(IGDS_MODAL_DATA) public data: any,
    @Inject(IGDS_MODAL_REF) public modalRef: IgdsModalRef
  ) {}
}

describe('IgdsModalService', () => {
  let service: IgdsModalService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [IgdsModalComponent, TestContentComponent],
    });
    service = TestBed.inject(IgdsModalService);
  });

  afterEach(() => {
    // Clean up any leftover modal host elements
    document.querySelectorAll('body > div > igds-modal').forEach(el => {
      el.parentElement?.remove();
    });
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should open a modal and attach it to the DOM', () => {
    const ref = service.open({ title: 'Test Modal' });
    expect(ref).toBeTruthy();
    expect(ref.afterClosed).toBeDefined();
    expect(ref.close).toBeDefined();

    const overlay = document.querySelector('.igds-modal__overlay');
    expect(overlay).toBeTruthy();

    const title = document.querySelector('.igds-modal__title');
    expect(title?.textContent).toBe('Test Modal');

    ref.close();
  });

  it('should emit the result value when close(result) is called', (done) => {
    const ref = service.open<string>({ title: 'Result Modal' });

    ref.afterClosed().subscribe(result => {
      expect(result).toBe('confirmed');
      done();
    });

    ref.close('confirmed');
  });

  it('should emit undefined when modal is dismissed without a result', (done) => {
    const ref = service.open<string>({ title: 'Dismiss Modal' });

    ref.afterClosed().subscribe(result => {
      expect(result).toBeUndefined();
      done();
    });

    ref.close();
  });

  it('should remove the host element from DOM after close', () => {
    const ref = service.open({ title: 'Cleanup Modal' });
    expect(document.querySelector('.igds-modal__overlay')).toBeTruthy();

    ref.close();
    expect(document.querySelector('.igds-modal__overlay')).toBeFalsy();
  });

  it('should set closable to true by default', () => {
    const ref = service.open({ title: 'Closable Modal' });
    const closeBtn = document.querySelector('.igds-modal__close');
    expect(closeBtn).toBeTruthy();
    ref.close();
  });

  it('should respect closable: false config', () => {
    const ref = service.open({ title: 'Non-closable', closable: false });
    const closeBtn = document.querySelector('.igds-modal__close');
    expect(closeBtn).toBeFalsy();
    ref.close();
  });

  it('should inject data and modalRef into content component', () => {
    const ref = service.open<string>({
      title: 'With Content',
      component: TestContentComponent,
      data: { message: 'hello' },
    });

    const contentEl = document.querySelector('.igds-modal__body test-content');
    expect(contentEl).toBeTruthy();
    expect(contentEl?.textContent).toContain('hello');

    ref.close();
  });

  it('should complete the afterClosed observable after close', (done) => {
    const ref = service.open<number>({ title: 'Complete Test' });

    ref.afterClosed().subscribe({
      next: (val) => expect(val).toBe(42),
      complete: () => done(),
    });

    ref.close(42);
  });
});
