import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { IgdsAccordionComponent } from './components/accordion/igds-accordion.component';
import { IgdsBreadcrumbsComponent } from './components/breadcrumbs/igds-breadcrumbs.component';
import { IgdsButtonComponent } from './components/button/igds-button.component';
import { IgdsCardComponent } from './components/card/igds-card.component';
import { IgdsCheckboxComponent } from './components/checkbox/igds-checkbox.component';
import { IgdsDatePickerComponent } from './components/date-picker/igds-date-picker.component';
import { IgdsDrawerComponent } from './components/drawer/igds-drawer.component';
import { IgdsDropdownComponent } from './components/dropdown/igds-dropdown.component';
import { IgdsInputFieldComponent } from './components/input-field/igds-input-field.component';
import { IgdsModalComponent } from './components/modal/igds-modal.component';
import { IgdsPaginationComponent } from './components/pagination/igds-pagination.component';
import { IgdsProgressBarComponent } from './components/progress-bar/igds-progress-bar.component';
import { IgdsRadioButtonComponent } from './components/radio-button/igds-radio-button.component';
import { IgdsSearchFieldComponent } from './components/search-field/igds-search-field.component';
import { IgdsSideMenuComponent } from './components/side-menu/igds-side-menu.component';
import { IgdsStatusBadgeComponent } from './components/status-badge/igds-status-badge.component';
import { IgdsStepIndicatorComponent } from './components/step-indicator/igds-step-indicator.component';
import { IgdsTableComponent } from './components/table/igds-table.component';
import { IgdsTabsComponent } from './components/tabs/igds-tabs.component';
import { IgdsTagComponent } from './components/tag/igds-tag.component';
import { IgdsToastComponent } from './components/toast/igds-toast.component';
import { IgdsToggleComponent } from './components/toggle/igds-toggle.component';
import { IgdsTooltipDirective } from './directives/igds-tooltip.directive';

const COMPONENTS = [
  IgdsAccordionComponent,
  IgdsBreadcrumbsComponent,
  IgdsButtonComponent,
  IgdsCardComponent,
  IgdsCheckboxComponent,
  IgdsDatePickerComponent,
  IgdsDrawerComponent,
  IgdsDropdownComponent,
  IgdsInputFieldComponent,
  IgdsModalComponent,
  IgdsPaginationComponent,
  IgdsProgressBarComponent,
  IgdsRadioButtonComponent,
  IgdsSearchFieldComponent,
  IgdsSideMenuComponent,
  IgdsStatusBadgeComponent,
  IgdsStepIndicatorComponent,
  IgdsTableComponent,
  IgdsTabsComponent,
  IgdsTagComponent,
  IgdsToastComponent,
  IgdsToggleComponent,
  IgdsTooltipDirective,
];

@NgModule({
  declarations: COMPONENTS,
  imports: [CommonModule, FormsModule],
  exports: COMPONENTS,
})
export class IgdsModule {}
