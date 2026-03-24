import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserService } from 'src/app/core/services/user.service';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,         
    ReactiveFormsModule ],
  templateUrl: './user-manage.component.html',
  styleUrls: ['./user-manage.component.scss']
})

export class UserManagementComponent implements OnInit {
  users: any[] = [];
  search = '';
  page = 1;
  total = 0;

  showModal = false;
  selectedUser: any = null;
  userForm!: FormGroup;
  error = '';

  constructor(private userService: UserService, private fb: FormBuilder) {}

  ngOnInit() {
    this.initForm();
    this.loadUsers();
  }

  initForm() {
    this.userForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', Validators.required],
      password: [''],
      role: ['user', Validators.required]
    });
  }

  loadUsers() {
    this.userService.getUsers(this.search, this.page).subscribe(res => {
      this.users = res.data;
      this.total = res.total;
    });
  }

  onSearch() {
    this.page = 1;
    this.loadUsers();
  }

  openCreate() {
    this.selectedUser = null;
    this.initForm();
    this.showModal = true;
  }

  openEdit(user: any) {
    this.selectedUser = user;
    this.userForm.patchValue(user);
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
    this.error = '';
  }

  saveUser() {
    if (this.selectedUser) {
      this.userService.update(this.selectedUser.id, this.userForm.value)
        .subscribe({
          next: () => {
            this.closeModal();
            this.loadUsers();
          },
          error: () => this.error = 'Update failed'
        });
    } else {
      this.userService.create(this.userForm.value)
        .subscribe({
          next: () => {
            this.closeModal();
            this.loadUsers();
          },
          error: () => this.error = 'Create failed'
        });
    }
  }

  deleteUser(id: number) {
    if (!confirm('Are you sure?')) return;

    this.userService.delete(id).subscribe(() => this.loadUsers());
  }

  forceLogout(id: number) {
    this.userService.forceLogout(id).subscribe(() => {
      alert('User logged out');
    });
  }
}
