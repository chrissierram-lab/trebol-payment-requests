import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CreatePaymentRequest, PaymentRequest } from './payment-request.model';

@Injectable({ providedIn: 'root' })
export class PaymentRequestsApi {
  private readonly http = inject(HttpClient);
  private readonly endpoint = '/api/v1/payment-requests';

  getAll(): Observable<PaymentRequest[]> {
    return this.http.get<PaymentRequest[]>(this.endpoint);
  }

  create(request: CreatePaymentRequest): Observable<PaymentRequest> {
    return this.http.post<PaymentRequest>(this.endpoint, request);
  }
}