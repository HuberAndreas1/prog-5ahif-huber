export type Product = {
  id: number;
  productName: string;
  unitPrice: number;
}

export type ReceiptLineDto = {
  productId: number;
  productName: string;
  totalPrice: number;
  quantity: number;
}

export type ReceiptLineApiDto = {
  productId: number;
  quantity: number;
  totalPrice: number;
}
