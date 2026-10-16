import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { registerLocaleData } from '@angular/common';
import localeEsCl from '@angular/common/locales/es-CL';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { App } from './app';

registerLocaleData(localeEsCl);

describe('App payment request form', () => {
  let fixture: ComponentFixture<App>;
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    httpTesting = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    httpTesting.expectOne('/api/v1/payment-requests').flush([]);
    fixture.detectChanges();
  });

  afterEach(() => httpTesting.verify());

  it('shows a validation error and does not submit an empty reference', () => {
    const root = fixture.nativeElement as HTMLElement;
    root.querySelector<HTMLButtonElement>('.submit-button')?.click();
    fixture.detectChanges();

    expect(root.querySelector('#reference-error')?.textContent).toContain('Ingresa una referencia.');
    httpTesting.expectNone((request) => request.method === 'POST');
  });

  it('rejects an amount less than or equal to zero', () => {
    const root = fixture.nativeElement as HTMLElement;
    const referenceInput = root.querySelector<HTMLInputElement>('#externalReference');
    const amountInput = root.querySelector<HTMLInputElement>('#amount');

    if (!referenceInput || !amountInput) {
      throw new Error('Expected payment request form controls.');
    }

    referenceInput.value = 'PAY-2026-0001';
    referenceInput.dispatchEvent(new Event('input'));
    amountInput.value = '0';
    amountInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    root.querySelector<HTMLButtonElement>('.submit-button')?.click();
    fixture.detectChanges();

    expect(root.querySelector('#amount-error')?.textContent).toContain('El monto debe ser mayor que cero.');
    httpTesting.expectNone((request) => request.method === 'POST');
  });

  it('keeps form values when the API rejects the request', () => {
    const root = fixture.nativeElement as HTMLElement;
    const referenceInput = root.querySelector<HTMLInputElement>('#externalReference');
    const amountInput = root.querySelector<HTMLInputElement>('#amount');

    if (!referenceInput || !amountInput) {
      throw new Error('Expected payment request form controls.');
    }

    referenceInput.value = 'PAY-2026-0001';
    referenceInput.dispatchEvent(new Event('input'));
    amountInput.value = '12.50';
    amountInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    root.querySelector<HTMLButtonElement>('.submit-button')?.click();

    const createRequest = httpTesting.expectOne('/api/v1/payment-requests');
    createRequest.flush(
      { code: 'CALCULATION_SERVICE_UNAVAILABLE', message: 'No fue posible calcular la comisión.' },
      { status: 503, statusText: 'Service Unavailable' },
    );
    fixture.detectChanges();

    expect(referenceInput.value).toBe('PAY-2026-0001');
    expect(amountInput.value).toBe('12.50');
    expect(root.querySelector('[role="alert"]')?.textContent).toContain('No fue posible calcular la comisión.');
  });
});
