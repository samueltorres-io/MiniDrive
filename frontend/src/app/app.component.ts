import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoginComponent } from './components/login/login.component';
import { DriveComponent } from './components/drive/drive.component';
import { UserResponse } from './services/api.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, LoginComponent, DriveComponent],
  template: `
    <app-login *ngIf="!user" (loggedIn)="onLogin($event)" />
    <app-drive *ngIf="user" [user]="user!" />
  `,
})
export class AppComponent {
  user: UserResponse | null = null;

  onLogin(user: UserResponse) {
    this.user = user;
  }
}