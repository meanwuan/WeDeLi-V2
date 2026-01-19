import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError, EMPTY } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const tokenInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
    const authService = inject(AuthService);
    const token = authService.getAccessToken();

    // Skip auth endpoints
    if (req.url.includes('/auth/login') ||
        req.url.includes('/auth/register') ||
        req.url.includes('/auth/forgot-password') ||
        req.url.includes('/auth/reset-password') ||
        req.url.includes('/auth/refresh-token')) {
        return next(req);
    }

    // Add token to request
    let authReq = req;
    if (token) {
        authReq = req.clone({
            setHeaders: {
                Authorization: `Bearer ${token}`
            }
        });
    }

    return next(authReq).pipe(
        catchError((error: HttpErrorResponse) => {
            if (error.status === 401) {
                // Try to refresh token
                return authService.refreshToken().pipe(
                    switchMap(response => {
                        if (response.success && response.data) {
                            // Retry original request with new token
                            const newReq = req.clone({
                                setHeaders: {
                                    Authorization: `Bearer ${response.data.accessToken}`
                                }
                            });
                            return next(newReq);
                        }
                        return throwError(() => error);
                    }),
                    catchError(() => {
                        // Refresh failed, force logout
                        authService.forceLogout();
                        return throwError(() => error);
                    })
                );
            }
            return throwError(() => error);
        })
    );
};
