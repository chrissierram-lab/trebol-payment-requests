import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { PaymentChannel, PaymentRequest } from './payment-request.model';
import { PaymentRequestsApi } from './payment-requests.api';

const nonBlankReference: ValidatorFn = (control: AbstractControl): ValidationErrors | null =>
  String(control.value ?? '').trim().length > 0 ? null : { required: true };

const maxNormalizedReferenceLength: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const length = String(control.value ?? '').trim().length;
  return length <= 50 ? null : { maxlength: { requiredLength: 50, actualLength: length } };
};

const maxTwoDecimalPlaces: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  if (control.value === null || control.value === '') {
    return null;
  }

  const fraction = String(control.value).split('.')[1];
  return fraction && fraction.length > 2 ? { decimalPlaces: true } : null;
};

@Component({
  imports: [DecimalPipe, ReactiveFormsModule],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './dashboard.html',
})
export class App implements OnInit {
  private readonly api = inject(PaymentRequestsApi);

  protected readonly requestForm = new FormGroup({
    externalReference: new FormControl('', {
      nonNullable: true,
      validators: [nonBlankReference, maxNormalizedReferenceLength],
    }),
    amount: new FormControl<number | null>(null, [
      Validators.required,
      Validators.min(0.01),
      maxTwoDecimalPlaces,
    ]),
    channel: new FormControl<PaymentChannel>('WEB', {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  protected readonly payments = signal<PaymentRequest[]>([]);
  protected readonly loading = signal(true);
  protected readonly submitting = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly submitError = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loadRequests();
  }

  protected loadRequests(): void {
    this.loading.set(true);
    this.loadError.set(null);

    this.api.getAll().subscribe({
      next: (requests) => {
        this.payments.set(requests);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loadError.set(this.getErrorMessage(error));
        this.loading.set(false);
      },
    });
  }

  protected createRequest(): void {
    this.requestForm.markAllAsTouched();
    this.submitError.set(null);
    this.successMessage.set(null);

    if (this.requestForm.invalid || this.submitting()) {
      return;
    }

    const formValue = this.requestForm.getRawValue();
    this.submitting.set(true);
    this.api.create({
      externalReference: formValue.externalReference.trim(),
      amount: Number(formValue.amount),
      channel: formValue.channel,
    }).subscribe({
      next: (createdRequest) => {
        this.payments.update((requests) => [createdRequest, ...requests]);
        this.requestForm.reset({ externalReference: '', amount: null, channel: 'WEB' });
        this.successMessage.set('Solicitud registrada correctamente.');
        this.submitting.set(false);
      },
      error: (error: unknown) => {
        this.submitError.set(this.getErrorMessage(error));
        this.submitting.set(false);
      },
    });
  }

  private getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const body: unknown = error.error;
      if (isApiErrorResponse(body)) {
        return body.message;
      }

      if (error.status === 0) {
        return 'No fue posible conectar con la API. Intenta nuevamente.';
      }
    }

    return 'Ocurrió un error inesperado. Intenta nuevamente.';
  }
}

function isApiErrorResponse(value: unknown): value is { code: string; message: string } {
  if (typeof value !== 'object' || value === null || !('code' in value) || !('message' in value)) {
    return false;
  }

  return typeof value.code === 'string' && typeof value.message === 'string';
}
