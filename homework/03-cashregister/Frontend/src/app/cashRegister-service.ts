import {inject, Injectable} from '@angular/core';
import {environment} from '../environments/environment';
import {Product, ReceiptLineApiDto, ReceiptLineDto} from '../types/types';
import {HttpClient} from '@angular/common/http';
import {firstValueFrom} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CashRegisterService {

  private readonly baseUrl: string = environment.apiBaseUrl;

  private httpClient: HttpClient = inject(HttpClient);

  public async getProducts(): Promise<Product[]> {
    return await firstValueFrom(this.httpClient.get<Product[]>(`${this.baseUrl}/api/cashregister/products`));
  }

  async checkout(receiptLines: ReceiptLineDto[]) {
    await firstValueFrom(this.httpClient.post(`${this.baseUrl}/api/cashregister/checkout`, receiptLines.map(rld => {
      return {
        productId: rld.productId,
        quantity: rld.quantity,
        totalPrice: rld.totalPrice
      } as ReceiptLineApiDto
    })));
  }
}
