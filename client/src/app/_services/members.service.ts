import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { of } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class MembersService {

  baseUrl = environment.apiUrl;
  members: Member[] = [];

  constructor(private http: HttpClient) { }

  getmembers() {
    if (this.members.length > 0) {
      return of(this.members);      
    }
    return this.http.get<Member[]>(`${this.baseUrl}/users`)
    .pipe(
      map(members => {
        this.members = members;
        return members;
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
