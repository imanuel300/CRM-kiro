import { Injectable, NgZone } from '@angular/core';
import { Subject, fromEvent, merge, timer, Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { environment } from '@env/environment';

/**
 * Monitors user activity and emits a timeout event after
 * the configured inactivity period (default: 30 minutes).
 * Requirement 18.3: auto-logout after 30 min inactivity.
 */
@Injectable({ providedIn: 'root' })
export class IdleService {
  private readonly timeoutMs = environment.sessionTimeoutMinutes * 60 * 1000;
  private timeoutSubscription: Subscription | null = null;

  onTimeout$ = new Subject<void>();

  constructor(private ngZone: NgZone) {}

  startWatching(): void {
    this.stopWatching();

    this.ngZone.runOutsideAngular(() => {
      const activityEvents$ = merge(
        fromEvent(document, 'mousemove'),
        fromEvent(document, 'mousedown'),
        fromEvent(document, 'keypress'),
        fromEvent(document, 'touchstart'),
        fromEvent(document, 'scroll')
      );

      this.timeoutSubscription = activityEvents$
        .pipe(switchMap(() => timer(this.timeoutMs)))
        .subscribe(() => {
          this.ngZone.run(() => this.onTimeout$.next());
        });

      // Also start the initial timer (no activity yet)
      const initialTimer = timer(this.timeoutMs).subscribe(() => {
        this.ngZone.run(() => this.onTimeout$.next());
        initialTimer.unsubscribe();
      });
    });
  }

  stopWatching(): void {
    if (this.timeoutSubscription) {
      this.timeoutSubscription.unsubscribe();
      this.timeoutSubscription = null;
    }
  }
}
