import { Routes } from '@angular/router';
import {ProductPage} from './product-page/product-page';

export const routes: Routes = [
  { path: 'products', component: ProductPage },
  { path: '', redirectTo: '/products', pathMatch: 'full' }
];
