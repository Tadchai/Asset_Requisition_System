export interface ApiResponse {
  id: number | null,
  statusCode: number;
  message: string;
}

export interface PageResponse<T> {
  data: T[],
  pageIndex: number,
  pageSize: number,
  rowCount: number
}

export interface Search {
  name: string
  page: number
  pageSize: number
}
