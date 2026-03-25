import { Directive, Input, ElementRef, OnDestroy, HostListener } from '@angular/core';

@Directive({
  selector: '[igdsTooltip]'
})
export class IgdsTooltipDirective implements OnDestroy {
  @Input('igdsTooltip') text = '';
  @Input() tooltipPosition: 'top' | 'bottom' | 'start' | 'end' = 'top';

  private tooltipEl: HTMLElement | null = null;

  constructor(private elRef: ElementRef) {}

  @HostListener('mouseenter')
  @HostListener('focusin')
  show() {
    if (!this.text || this.tooltipEl) return;
    this.tooltipEl = document.createElement('div');
    this.tooltipEl.textContent = this.text;
    this.tooltipEl.setAttribute('role', 'tooltip');
    Object.assign(this.tooltipEl.style, {
      position: 'absolute', zIndex: '10000',
      padding: 'var(--igds-space-4) var(--igds-space-8)',
      background: 'var(--igds-bg-inverted)', color: 'var(--igds-text-inverted)',
      fontFamily: 'var(--igds-font-family)', fontSize: 'var(--igds-font-size-xs)',
      borderRadius: 'var(--igds-radius-sm)', whiteSpace: 'nowrap',
      boxShadow: 'var(--igds-shadow-md)', pointerEvents: 'none',
    });
    document.body.appendChild(this.tooltipEl);
    this.position();
  }

  @HostListener('mouseleave')
  @HostListener('focusout')
  hide() {
    if (this.tooltipEl) {
      this.tooltipEl.remove();
      this.tooltipEl = null;
    }
  }

  ngOnDestroy() { this.hide(); }

  private position() {
    if (!this.tooltipEl) return;
    const hostRect = this.elRef.nativeElement.getBoundingClientRect();
    const tipRect = this.tooltipEl.getBoundingClientRect();
    const gap = 8;
    const isRtl = getComputedStyle(this.elRef.nativeElement).direction === 'rtl';
    let top = 0, left = 0;

    switch (this.tooltipPosition) {
      case 'top':
        top = hostRect.top - tipRect.height - gap;
        left = hostRect.left + (hostRect.width - tipRect.width) / 2;
        break;
      case 'bottom':
        top = hostRect.bottom + gap;
        left = hostRect.left + (hostRect.width - tipRect.width) / 2;
        break;
      case 'start':
        top = hostRect.top + (hostRect.height - tipRect.height) / 2;
        left = isRtl ? hostRect.right + gap : hostRect.left - tipRect.width - gap;
        break;
      case 'end':
        top = hostRect.top + (hostRect.height - tipRect.height) / 2;
        left = isRtl ? hostRect.left - tipRect.width - gap : hostRect.right + gap;
        break;
    }

    this.tooltipEl.style.top = `${top + window.scrollY}px`;
    this.tooltipEl.style.left = `${left + window.scrollX}px`;
  }
}
