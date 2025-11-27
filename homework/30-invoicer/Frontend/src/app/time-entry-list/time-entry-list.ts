import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { Employee, Project, TimeEntryDto } from '../api/models';
import { Api } from '../api/api';
import { employeesGet, projectsGet, timeentriesGet, timeentriesIdDelete } from '../api/functions';
import { Router } from '@angular/router';
import { ApiConfiguration } from '../api/api-configuration';
import { environment } from '../../environments/environment.development';

@Component({
  selector: 'app-time-entry-list',
  imports: [],
  templateUrl: './time-entry-list.html',
  styleUrl: './time-entry-list.css',
})
export class TimeEntryList implements OnInit {
  protected readonly employees = signal<Employee[]>([]);
  protected readonly projects = signal<Project[]>([]);

  protected readonly descriptionFilter = signal<string>('');
  protected readonly employeeFilter = signal<number | null>(null);
  protected readonly projectFilter = signal<number | null>(null);

  private readonly api = inject(Api);
  private readonly apiConfig = inject(ApiConfiguration);
  private readonly router = inject(Router);

  protected readonly timeEntries = signal<TimeEntryDto[]>([]);

  protected readonly filteredTimeEntries = computed(() => {
    let entries = this.timeEntries();

    const description = this.descriptionFilter().toLowerCase();
    if (description) {
      entries = entries.filter((e) => e.description.toLocaleLowerCase().includes(description));
    }

    return entries;
  });

  ngOnInit(): void {
    this.apiConfig.rootUrl = environment.apiBaseUrl;
    this.loadData();
  }

  private async loadData() {
    const [entries, employees, projects] = await Promise.all([
      this.api.invoke(timeentriesGet, {
        employeeId: this.employeeFilter() ?? undefined,
        projectId: this.projectFilter() ?? undefined,
      }),
      this.api.invoke(employeesGet, {}),
      this.api.invoke(projectsGet, {}),
    ]);

    this.timeEntries.set(entries.filter((e) => e !== null) as TimeEntryDto[]);
    this.employees.set(employees.filter((e) => e !== null) as Employee[]);
    this.projects.set(projects.filter((p) => p !== null) as Project[]);
  }
  async onEmployeeChange(event: Event) {
    const value = (event.target as HTMLSelectElement).value;
    this.employeeFilter.set(value === '' ? null : parseInt(value));

    await this.loadData();
  }
  async onProjectChange(event: Event) {
    const value = (event.target as HTMLSelectElement).value;
    this.projectFilter.set(value === '' ? null : parseInt(value));

    await this.loadData();
  }
  onDescriptionChange(event: Event) {
    this.descriptionFilter.set((event.target as HTMLInputElement).value);
  }
  async deleteTimeEntry(id: number) {
    await this.api.invoke(timeentriesIdDelete, { id });
    await this.loadData();
  }
  editTimeEntry(id: number) {
    this.router.navigate(['/timeEntryEdit', id]);
  }
}
