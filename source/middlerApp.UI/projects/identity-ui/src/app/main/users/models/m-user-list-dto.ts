export class MUserListDto {
    Id!: string;
    UserName!: string;
    Email!: string;
    Firstname?: string;
    Lastname?: string;

    EmailConfirmed: boolean = false;
    Active: boolean = false;
    
    HasPassword: boolean = false;
    Logins: Array<string> = [];
    Deleted: boolean = false;
}



export function MUserListDtoSortByName(a: MUserListDto, b: MUserListDto) {
    if (a.UserName?.toLowerCase() < b.UserName?.toLowerCase()) {
        return -1;
    }
    if (a.UserName?.toLowerCase() > b.UserName?.toLowerCase()) {
        return 1;
    }
    return 0;
}