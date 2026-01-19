import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ApiResponse, PaginatedResponse, PaginationRequest } from '../models/common.model';

@Injectable({
    providedIn: 'root'
})
export class ApiService {
    private http = inject(HttpClient);

    private readonly baseUrl = 'http://localhost:5009/api/v1';

    // GET request
    get<T>(endpoint: string, params?: Record<string, any>): Observable<T> {
        const httpParams = this.buildParams(params);
        return this.http.get<T>(`${this.baseUrl}${endpoint}`, { params: httpParams })
            .pipe(catchError(this.handleError));
    }

    // GET with API response wrapper
    getWithResponse<T>(endpoint: string, params?: Record<string, any>): Observable<ApiResponse<T>> {
        return this.get<ApiResponse<T>>(endpoint, params);
    }

    // GET paginated
    getPaginated<T>(endpoint: string, pagination: PaginationRequest, filters?: Record<string, any>): Observable<PaginatedResponse<T>> {
        const params = {
            pageNumber: pagination.pageNumber,
            pageSize: pagination.pageSize,
            sortBy: pagination.sortBy,
            sortOrder: pagination.sortOrder,
            ...filters
        };
        return this.get<PaginatedResponse<T>>(endpoint, params);
    }

    // POST request
    post<T>(endpoint: string, body: any): Observable<T> {
        return this.http.post<T>(`${this.baseUrl}${endpoint}`, body)
            .pipe(catchError(this.handleError));
    }

    // POST with API response wrapper
    postWithResponse<T>(endpoint: string, body: any): Observable<ApiResponse<T>> {
        return this.post<ApiResponse<T>>(endpoint, body);
    }

    // PUT request
    put<T>(endpoint: string, body: any): Observable<T> {
        return this.http.put<T>(`${this.baseUrl}${endpoint}`, body)
            .pipe(catchError(this.handleError));
    }

    // PATCH request
    patch<T>(endpoint: string, body: any): Observable<T> {
        return this.http.patch<T>(`${this.baseUrl}${endpoint}`, body)
            .pipe(catchError(this.handleError));
    }

    // DELETE request
    delete<T>(endpoint: string): Observable<T> {
        return this.http.delete<T>(`${this.baseUrl}${endpoint}`)
            .pipe(catchError(this.handleError));
    }

    // GET blob (for file downloads)
    getBlob(endpoint: string, params?: Record<string, any>): Observable<Blob> {
        const httpParams = this.buildParams(params);
        return this.http.get(`${this.baseUrl}${endpoint}`, {
            params: httpParams,
            responseType: 'blob'
        }).pipe(catchError(this.handleError));
    }

    // Build HTTP params from object
    private buildParams(params?: Record<string, any>): HttpParams {
        let httpParams = new HttpParams();
        if (params) {
            Object.keys(params).forEach(key => {
                const value = params[key];
                if (value !== null && value !== undefined && value !== '') {
                    httpParams = httpParams.set(key, value.toString());
                }
            });
        }
        return httpParams;
    }

    // Error handler
    private handleError(error: HttpErrorResponse): Observable<never> {
        let errorMessage = 'Đã xảy ra lỗi không xác định';

        if (error.error instanceof ErrorEvent) {
            // Client-side error
            errorMessage = error.error.message;
        } else {
            // Server-side error
            if (error.error?.message) {
                errorMessage = error.error.message;
            } else if (error.status === 0) {
                errorMessage = 'Không thể kết nối đến server';
            } else if (error.status === 401) {
                errorMessage = 'Phiên đăng nhập đã hết hạn';
            } else if (error.status === 403) {
                errorMessage = 'Bạn không có quyền thực hiện thao tác này';
            } else if (error.status === 404) {
                errorMessage = 'Không tìm thấy dữ liệu';
            } else if (error.status === 500) {
                errorMessage = 'Lỗi server, vui lòng thử lại sau';
            }
        }

        console.error('API Error:', error);
        return throwError(() => new Error(errorMessage));
    }
}
