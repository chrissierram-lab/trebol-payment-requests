export type PaymentChannel = 'WEB' | 'MOBILE' | 'BRANCH';
export type PaymentStatus = 'PENDING';

export interface PaymentRequest {
  id: string;
  externalReference: string;
  amount: number;
  channel: PaymentChannel;
  fee: number;
  total: number;
  status: PaymentStatus;
}

export interface CreatePaymentRequest {
  externalReference: string;
  amount: number;
  channel: PaymentChannel;
}

export interface ApiErrorResponse {
  code: string;
  message: string;
}