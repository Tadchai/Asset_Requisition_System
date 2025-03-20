export interface Employee {
  employeeId: number,
  fullName: string
}
export interface RequisitionedItem {
  assetId: string;
  itemCategoryId: number;
  itemCategoryName: string;
  itemClassificationId: number;
  itemClassificationName: string;
  instanceId: number;
  requisitonDate: string;
  requisitionId: number;
}

export interface GetByIdEmployeeResponse {
  employeeId: number;
  fullName: string;
  requisitionedItems: RequisitionedItem[];
}




