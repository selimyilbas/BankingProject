/**
 * API yanıt modelleri: standart ApiResponse ve sayfalı sonuç yapısı.
 */
export interface ApiResponse<T> {
    success: boolean;
    message: string;
    data?: T;
    errors: string[];
  }
  
  export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
  }