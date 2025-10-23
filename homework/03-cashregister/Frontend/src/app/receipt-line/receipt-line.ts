import {Component, Input} from '@angular/core';
import {ReceiptLineApiDto, ReceiptLineDto} from '../../types/types';

@Component({
  selector: 'app-receipt-line',
  imports: [],
  templateUrl: './receipt-line.html',
  styleUrl: './receipt-line.css',
})
export class ReceiptLine {
  @Input({ required: true }) lines!: ReceiptLineDto[];
  protected readonly Math = Math;
}
