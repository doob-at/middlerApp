import { SimpleClaim } from "./simple-claim.model";

export class LoggedInUser {
    UserName!: string;
    FirstName?: string;
    LastName?: string;
    Email!: string;
    DisplayName!: string;
    Roles?: Array<string>
    Claims?: Array<SimpleClaim>;
    IsAdmin: boolean = false;
}