import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService, UserResponse } from '../../services/api.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  @Output() loggedIn = new EventEmitter<UserResponse>();

  username = '';
  error = '';
  loading = false;
  mode: 'login' | 'register' = 'login';

  constructor(private api: ApiService) {}

  submit() {
    const name = this.username.trim();
    if (!name) return;

    this.loading = true;
    this.error = '';

    if (this.mode === 'login') {
      this.api.getUser({ username: name }).subscribe({
        next: (user) => { this.loading = false; this.loggedIn.emit(user); },
        error: () => { this.loading = false; this.error = 'User not found. Try registering.'; },
      });
    } else {
      this.api.createUser(name).subscribe({
        next: (user) => { this.loading = false; this.loggedIn.emit(user); },
        error: (e) => {
          this.loading = false;
          this.error = e.error?.error ?? 'Could not create user.';
        },
      });
    }
  }

  toggle() {
    this.mode = this.mode === 'login' ? 'register' : 'login';
    this.error = '';
  }
}