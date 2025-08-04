import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TransferHistory } from './transfer-history';

describe('TransferHistory', () => {
  let component: TransferHistory;
  let fixture: ComponentFixture<TransferHistory>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TransferHistory]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TransferHistory);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
