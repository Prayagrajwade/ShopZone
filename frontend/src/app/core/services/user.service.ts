import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { environment } from "src/environments/environment";

@Injectable({ providedIn: 'root' })
export class UserService {
    
  private baseUrl = `${environment.apiUrl}/admin/users`;
  constructor(private http: HttpClient) {}

  getUsers(search = '', page = 1) {
    return this.http.get<any>(`${this.baseUrl}?search=${search}&page=${page}`);
  }

  create(data: any) {
    return this.http.post(this.baseUrl, data);
  }

  update(id: number, data: any) {
    return this.http.put(`${this.baseUrl}/${id}`, data);
  }

  delete(id: number) {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }

  forceLogout(id: number) {
    return this.http.post(`${this.baseUrl}/${id}/force-logout`, {});
  }
}