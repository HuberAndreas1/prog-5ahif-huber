import { Routes } from '@angular/router';
import { TimeEntryList } from './time-entry-list/time-entry-list';
import { TimeEntryEdit } from './time-entry-edit/time-entry-edit';

export const routes: Routes = [
    { path: "timeSheet", component: TimeEntryList },
    { path: "timeEntryEdit/:timeEntryId", component: TimeEntryEdit },
    { path: "", redirectTo: "/timeSheet", pathMatch: "full" }
];
