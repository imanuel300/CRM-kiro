// Local barrel – replaces @igds/angular when the package is unavailable
export { IgdsModule } from './igds-local.module';

// Components
export { IgdsAccordionComponent } from './components/accordion/igds-accordion.component';
export { IgdsBreadcrumbsComponent, IgdsBreadcrumbItem } from './components/breadcrumbs/igds-breadcrumbs.component';
export { IgdsButtonComponent } from './components/button/igds-button.component';
export { IgdsCardComponent } from './components/card/igds-card.component';
export { IgdsCheckboxComponent } from './components/checkbox/igds-checkbox.component';
export { IgdsDatePickerComponent } from './components/date-picker/igds-date-picker.component';
export { IgdsDrawerComponent } from './components/drawer/igds-drawer.component';
export { IgdsDropdownComponent, IgdsDropdownOption } from './components/dropdown/igds-dropdown.component';
export { IgdsInputFieldComponent } from './components/input-field/igds-input-field.component';
export { IgdsModalComponent } from './components/modal/igds-modal.component';
export { IgdsPaginationComponent } from './components/pagination/igds-pagination.component';
export { IgdsProgressBarComponent } from './components/progress-bar/igds-progress-bar.component';
export { IgdsRadioButtonComponent } from './components/radio-button/igds-radio-button.component';
export { IgdsSearchFieldComponent } from './components/search-field/igds-search-field.component';
export { IgdsSideMenuComponent, IgdsSideMenuItem } from './components/side-menu/igds-side-menu.component';
export { IgdsStatusBadgeComponent } from './components/status-badge/igds-status-badge.component';
export { IgdsStepIndicatorComponent, IgdsStep } from './components/step-indicator/igds-step-indicator.component';
export { IgdsTableComponent, IgdsTableColumn } from './components/table/igds-table.component';
export { IgdsTabsComponent, IgdsTab } from './components/tabs/igds-tabs.component';
export { IgdsTagComponent } from './components/tag/igds-tag.component';
export { IgdsToastComponent } from './components/toast/igds-toast.component';
export { IgdsToggleComponent } from './components/toggle/igds-toggle.component';

// Directives
export { IgdsTooltipDirective } from './directives/igds-tooltip.directive';

// Services
export { IgdsModalService, IgdsModalConfig, IgdsModalRef, IGDS_MODAL_DATA, IGDS_MODAL_REF } from './services/igds-modal.service';
export { IgdsToastService } from './services/igds-toast.service';
