export interface Receipt {
  receiptId: number,
  receiptName: string,
}
export interface ReceiptDetailInRow {
  quantity: number,
  unit: string,
  price: number,
  totalValue: number,
}

export interface ReceiptSearch extends Receipt {

  totalValue: number
  receiptDetails: ReceiptDetailSearch[]
}

export interface ReceiptDetailSearch {
  instanceName: string
  quantity: number,
  price: number,
  totalValue: number,
}

export interface ReceiptDetail extends ReceiptDetailInRow {
  isInstance: boolean,
  instanceName: string,
}

export interface ReceiptDetailData extends ReceiptDetailInRow {
  instanceName: string | null,
  instanceId: number | null,
  newInstance: string | null,
}

export interface ReceiptResponse {
  receiptName: string,
  date: string,
  totalAmount: number,
  discount: number,
  totalValue: number,
  receiptDetail: ReceiptDetail[]
}

export interface ReceiptData {
  date: string,
  totalAmount: number,
  discount: number,
  totalValue: number,
  receiptDetail: ReceiptDetailData[]
}



