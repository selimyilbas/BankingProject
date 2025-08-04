import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TransferCreate } from './transfer-create';

describe('TransferCreate', () => {
  let component: TransferCreate;
  let fixture: ComponentFixture<TransferCreate>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TransferCreate]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TransferCreate);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
