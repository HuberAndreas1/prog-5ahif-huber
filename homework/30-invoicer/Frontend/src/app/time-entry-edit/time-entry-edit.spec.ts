import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TimeEntryEdit } from './time-entry-edit';

describe('TimeEntryEdit', () => {
  let component: TimeEntryEdit;
  let fixture: ComponentFixture<TimeEntryEdit>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TimeEntryEdit]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TimeEntryEdit);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
