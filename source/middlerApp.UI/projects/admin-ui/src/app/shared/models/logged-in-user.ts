export class LoggedInUser {
    DN!: string;
    FirstName?: string;
    LastName?: string;
    UserName!: string;
    DisplayName!: string;
    Groups: Array<string> = [];

    IsAdmin: boolean = false;
}