import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { Pagination } from 'src/app/_models/pagination';
import { Member } from '../../_models/member';
import { MembersService } from '../../_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {

  members: Member[];
  pagination: Pagination;
  pageNumber = 1;
  pageSize = 5;

  members$ : Observable<Member[]>; 

  constructor(private memberService : MembersService) { }

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers(){
    this.memberService.getmembers(this.pageNumber, this.pageSize)
      .subscribe(response => {
        this.members = response.result;
        this.pagination = response.pagination;
      })
  }
}
