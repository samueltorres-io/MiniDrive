import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

const API = 'http://localhost:8080/api';

export interface UserResponse {
  id: number;
  username: string;
  createdAt: string;
}

export interface ChildFolderResponse {
  id: number;
  name: string;
  createdAt: string;
}

export interface FolderResponse {
  id: number;
  name: string;
  parentId: number | null;
  createdAt: string;
  subFolders: ChildFolderResponse[];
}

export interface FileResponse {
  id: number;
  name: string;
  extension: string | null;
  size: number | null;
  status: string;
  folderId: number | null;
  createdAt: string;
  updatedAt: string | null;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private http: HttpClient) {}

  // ── Users ─────────────────────────────────────────────────────────────────

  createUser(username: string): Observable<UserResponse> {
    return this.http.post<UserResponse>(`${API}/users`, { username });
  }

  getUser(params: { id?: number; username?: string }): Observable<UserResponse> {
    let p = new HttpParams();
    if (params.id) p = p.set('id', params.id);
    if (params.username) p = p.set('username', params.username);
    return this.http.get<UserResponse>(`${API}/users`, { params: p });
  }

  // ── Folders ───────────────────────────────────────────────────────────────

  createFolder(userId: number, name: string, parentId?: number): Observable<FolderResponse> {
    return this.http.post<FolderResponse>(`${API}/folders`, {
      userId,
      name,
      parentId: parentId ?? null,
    });
  }

  getFolders(userId: number, folderId?: number): Observable<FolderResponse> {
    let p = new HttpParams().set('userId', userId);
    if (folderId != null) p = p.set('folderId', folderId);
    return this.http.get<FolderResponse>(`${API}/folders`, { params: p });
  }

  deleteFolder(id: number, userId: number): Observable<void> {
    return this.http.delete<void>(`${API}/folders/${id}`, {
      params: new HttpParams().set('userId', userId),
    });
  }

  // ── Files ─────────────────────────────────────────────────────────────────

  listFiles(userId: number, folderId?: number): Observable<FileResponse[]> {
    let p = new HttpParams().set('userId', userId);
    if (folderId != null) p = p.set('folderId', folderId);
    return this.http.get<FileResponse[]>(`${API}/files`, { params: p });
  }

  uploadFile(userId: number, file: File, folderId?: number): Observable<FileResponse> {
    const form = new FormData();
    form.append('userId', String(userId));
    form.append('file', file);
    if (folderId != null) form.append('folderId', String(folderId));
    return this.http.post<FileResponse>(`${API}/files`, form);
  }

  downloadFile(id: number, userId: number): Observable<Blob> {
    return this.http.get(`${API}/files/${id}/download`, {
      params: new HttpParams().set('userId', userId),
      responseType: 'blob',
    });
  }

  deleteFile(id: number, userId: number): Observable<void> {
    return this.http.delete<void>(`${API}/files/${id}`, {
      params: new HttpParams().set('userId', userId),
    });
  }
}