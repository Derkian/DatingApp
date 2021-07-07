import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { getPaginationHeaders, getPaginateResult } from './paginationHelper';
import { Message } from '../_models/message';

@Injectable({
  providedIn: 'root'
})
export class MessageService {

  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getMessages(pageNumber, pageSize, container){
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append("Container", container);
    return getPaginateResult<Message[]>(`${this.baseUrl}/messages`,params, this.http);
  }

  getMessageThread(username: string){
    return this.http.get<Message[]>(`${this.baseUrl}/messages/thread/${username}`);
  }

  sendMessage(username: string, content: string){
    return this.http.post<Message>(`${this.baseUrl}/messages`, { recipientUsername: username, content : content });
  }

  deleteMessage(id: number){
    return this.http.delete(`${this.baseUrl}/messages/${id}`);
  }
  
}
