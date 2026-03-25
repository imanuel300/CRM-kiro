import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'hebrewDate' })
export class HebrewDatePipe implements PipeTransform {
  transform(value: string | Date | null | undefined, format: 'short' | 'long' = 'short'): string {
    if (!value) return '';
    const date = typeof value === 'string' ? new Date(value) : value;
    if (isNaN(date.getTime())) return '';

    const options: Intl.DateTimeFormatOptions =
      format === 'long'
        ? { year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' }
        : { year: 'numeric', month: '2-digit', day: '2-digit' };

    return date.toLocaleDateString('he-IL', options);
  }
}
