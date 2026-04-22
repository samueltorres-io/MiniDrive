import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  ApiService,
  UserResponse,
  ChildFolderResponse,
  FileResponse,
} from '../../services/api.service';

interface BreadcrumbItem {
  id: number | null;
  name: string;
}

@Component({
  selector: 'app-drive',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './drive.component.html',
  styleUrl: './drive.component.css',
})
export class DriveComponent implements OnInit {
  @Input() user!: UserResponse;

  folders: ChildFolderResponse[] = [];
  files: FileResponse[] = [];
  breadcrumb: BreadcrumbItem[] = [{ id: null, name: 'My Drive' }];

  newFolderName = '';
  showNewFolder = false;
  loadingFolders = false;
  loadingFiles = false;
  uploading = false;
  error = '';
  toast = '';
  private toastTimer: any;

  get currentFolderId(): number | null {
    return this.breadcrumb[this.breadcrumb.length - 1].id;
  }

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.load();
  }

  load() {
    const fid = this.currentFolderId ?? undefined;
    this.loadingFolders = true;
    this.loadingFiles = true;
    this.error = '';

    this.api.getFolders(this.user.id, fid).subscribe({
      next: (res) => {
        this.folders = res.subFolders ?? [];
        this.loadingFolders = false;
      },
      error: () => { this.loadingFolders = false; },
    });

    this.api.listFiles(this.user.id, fid).subscribe({
      next: (res) => { this.files = res; this.loadingFiles = false; },
      error: () => { this.loadingFiles = false; },
    });
  }

  navigateTo(folder: ChildFolderResponse) {
    this.breadcrumb.push({ id: folder.id, name: folder.name });
    this.load();
  }

  navigateToCrumb(index: number) {
    this.breadcrumb = this.breadcrumb.slice(0, index + 1);
    this.load();
  }

  // ── Create Folder ─────────────────────────────────────────────────────────

  createFolder() {
    const name = this.newFolderName.trim();
    if (!name) return;
    const parentId = this.currentFolderId ?? undefined;

    this.api.createFolder(this.user.id, name, parentId).subscribe({
      next: (folder) => {
        this.folders.push({ id: folder.id, name: folder.name, createdAt: folder.createdAt });
        this.newFolderName = '';
        this.showNewFolder = false;
        this.notify('Folder created!');
      },
      error: (e) => { this.error = e.error?.error ?? 'Could not create folder.'; },
    });
  }

  cancelNewFolder() {
    this.showNewFolder = false;
    this.newFolderName = '';
  }

  deleteFolder(folder: ChildFolderResponse, event: Event) {
    event.stopPropagation();
    if (!confirm(`Delete folder "${folder.name}" and all its contents?`)) return;

    this.api.deleteFolder(folder.id, this.user.id).subscribe({
      next: () => {
        this.folders = this.folders.filter((f) => f.id !== folder.id);
        this.notify('Folder deleted.');
      },
      error: (e) => { this.error = e.error?.error ?? 'Could not delete folder.'; },
    });
  }

  // ── Upload File ───────────────────────────────────────────────────────────

  triggerUpload() {
    document.getElementById('file-input')?.click();
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    const file = input.files[0];
    input.value = '';

    this.uploading = true;
    const fid = this.currentFolderId ?? undefined;

    this.api.uploadFile(this.user.id, file, fid).subscribe({
      next: (f) => {
        this.files.unshift(f);
        this.uploading = false;
        this.notify(`"${f.name}" uploaded!`);
      },
      error: (e) => {
        this.uploading = false;
        this.error = e.error?.error ?? 'Upload failed.';
      },
    });
  }

  // ── Download File ─────────────────────────────────────────────────────────

  downloadFile(file: FileResponse) {
    this.api.downloadFile(file.id, this.user.id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = file.extension ? `${file.name}.${file.extension}` : file.name;
        a.click();
        URL.revokeObjectURL(url);
        this.notify(`Downloading "${file.name}"…`);
      },
      error: () => { this.error = 'Download failed.'; },
    });
  }

  deleteFile(file: FileResponse) {
    if (!confirm(`Delete "${file.name}"?`)) return;

    this.api.deleteFile(file.id, this.user.id).subscribe({
      next: () => {
        this.files = this.files.filter((f) => f.id !== file.id);
        this.notify('File deleted.');
      },
      error: (e) => { this.error = e.error?.error ?? 'Could not delete file.'; },
    });
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  formatSize(bytes: number | null): string {
    if (bytes == null) return '—';
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    if (bytes < 1024 * 1024 * 1024) return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
    return `${(bytes / 1024 / 1024 / 1024).toFixed(2)} GB`;
  }

  fileIcon(ext: string | null): string {
    const e = ext?.toLowerCase() ?? '';
    if (['jpg','jpeg','png','gif','webp','svg'].includes(e)) return 'img';
    if (['mp4','mov','avi','mkv'].includes(e)) return 'video';
    if (['mp3','wav','ogg','flac'].includes(e)) return 'audio';
    if (['pdf'].includes(e)) return 'pdf';
    if (['zip','tar','gz','rar'].includes(e)) return 'archive';
    if (['txt','md'].includes(e)) return 'text';
    if (['json','ts','js','py','cs','html','css'].includes(e)) return 'code';
    if (['docx','doc'].includes(e)) return 'doc';
    if (['xlsx','csv'].includes(e)) return 'sheet';
    return 'generic';
  }

  dismissError() { this.error = ''; }

  private notify(msg: string) {
    this.toast = msg;
    clearTimeout(this.toastTimer);
    this.toastTimer = setTimeout(() => (this.toast = ''), 3000);
  }
}