// src/app/models/api-response.model.ts
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