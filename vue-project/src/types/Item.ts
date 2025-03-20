export interface Item {
  itemCategoryId: number,
  name: string,
}

export interface FreeItemResponse {
  assetId: string,
  classificationName: string,
  categoryName: string,
  itemInstanceId: number
}

export interface Category {
  id: number | null,
  name: string,
}
export interface ItemCategory<T> extends Category {
  itemClassifications: ItemClassifications<T>[],
}

export interface ItemClassifications<T> {
  id: number | null,
  name: string,
  itemInstances: T[],
}

export interface ItemInstances {
  id: number | null,
  assetId: string,
}

export interface ItemInstancesWithEmployee extends ItemInstances {
  requisitionEmployeeId: number | null,
  requisitionEmployeeName: string | null
}

export interface SearchItemsResponse {
  data: Category[],
  pageIndex: number,
  pageSize: number,
  rowCount: number
}

export interface CategoryReceipt {
  categoryId: number,
  categoryName: string,
}

export interface ClassificationReceipt {
  classificationId: number,
  classificationName: string,
}

export interface InstanceReceipt {
  instanceId: number,
  assetId: string,
}
