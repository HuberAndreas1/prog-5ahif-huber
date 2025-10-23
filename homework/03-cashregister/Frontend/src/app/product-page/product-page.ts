import {Component, computed, inject, OnInit, signal, WritableSignal} from '@angular/core';
import {Product, ReceiptLineDto} from '../../types/types';
import {CashRegisterService} from '../cashRegister-service';
import {ProductCard} from '../product-card/product-card';
import {ReceiptLine} from '../receipt-line/receipt-line';
import {CurrencyPipe} from '@angular/common';

@Component({
  selector: 'app-product-page',
  imports: [
    ProductCard,
    ReceiptLine,
    CurrencyPipe
  ],
  templateUrl: './product-page.html',
  styleUrl: './product-page.css',
})
export class ProductPage implements OnInit {

  private cashRegisterService: CashRegisterService = inject(CashRegisterService);
  protected products: WritableSignal<Product[]> = signal([]);
  protected cart: WritableSignal<ReceiptLineDto[]> = signal([]);
  protected totalPrice = computed(() => this.cart().reduce((acc, item) => acc + item.totalPrice, 0));

  async ngOnInit(): Promise<void> {
    this.products.set(await this.cashRegisterService.getProducts());
  }

  addToCart(product: Product) {
    this.cart.update(currentCart => {
      const existingItem = currentCart.find(x => x.productId === product.id);

      if (existingItem) {
        return currentCart.map(item =>
          item.productId === product.id
            ? {
                ...item,
                quantity: item.quantity + 1,
                totalPrice: product.unitPrice * (item.quantity + 1)
              }
            : item
        );
      }

      return [
        ...currentCart,
        {
          productId: product.id,
          productName: product.productName,
          quantity: 1,
          totalPrice: product.unitPrice
        }
      ];
    });
  }

  async checkout() {
    await this.cashRegisterService.checkout(this.cart())
    this.cart.set([])
  }
}
