import { Injectable, signal } from '@angular/core';

export interface ToastMessage {
  id: string;
  message: string;
  type: 'success' | 'error' | 'info';
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  // Lista de notificaciones activas usando Signals de Angular
  toasts = signal<ToastMessage[]>([]);

  success(message: string, durationMs = 4000) {
    this.add(message, 'success', durationMs);
  }

  error(message: string, durationMs = 5000) {
    this.add(message, 'error', durationMs);
  }

  info(message: string, durationMs = 3500) {
    this.add(message, 'info', durationMs);
  }

  remove(id: string) {
    this.toasts.update(list => list.filter(t => t.id !== id));
  }

  private add(message: string, type: 'success' | 'error' | 'info', durationMs: number) {
    const id = Math.random().toString(36).substring(2, 9);
    this.toasts.update(list => [...list, { id, message, type }]);

    setTimeout(() => {
      this.remove(id);
    }, durationMs);
  }
}
