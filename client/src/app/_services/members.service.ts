import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { PaginatedResult } from 'src/app/_models/pagination';
import { Member } from '../_models/member';
import { of } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class MembersService {

  baseUrl = environment.apiUrl;
  members: Member[] = [];
  paginatedResult: PaginatedResult<Member[]> = new PaginatedResult<Member[]>();

  constructor(private http: HttpClient) { }

  getmembers(page?: number, itemsPerPage?: number) {    
    let params = new HttpParams();

    if (page !== null && itemsPerPage !== null) {
      params = params.append('pageNumber', page.toString());
      params = params.append('pageSize', itemsPerPage.toString());      
    }

    return this.http.get<Member[]>(`${this.baseUrl}/users`, {observe: 'response', params})
    .pipe(
      map(response => {
        this.paginatedResult.result = response.body;

        if (response.headers.get('Pagination') !== null) {
          this.paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
        }

        return this.paginatedResult;
      })
    );
  }

  getMember(username){
    const member = this.members.find(user => user.userName === username);
    
    if (member !== undefined) {
      return of(member);      
    }

    return this.http.get<Member>(`${this.baseUrl}/users/${username}`);
  }

  updateMember(member : Member){
    return this.http.put(`${this.baseUrl}/users`, member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = member;
      })
    );

  }

  setMainPhoto(photoId: number){
    return this.http.put(`${this.baseUrl}/users/set-main-photo/${photoId}`, {});
  }

  deletePhoto(photoId: number){
    return this.http.delete(`${this.baseUrl}/users/delete-photo/${photoId}`);
  }
}
