import { SimpleClaim } from '../../models/simple-claim';
import { MRoleDto } from '../../models/m-role-dto';

export class MUserDto {
    Id!: string; 
    UserName!: string;
    Email!: string;
    Firstname!: string;
    Lastname!: string;
    PhoneNumber!: string;

    EmailConfirmed: boolean = false;
    PhoneNumberConfirmed: boolean = false;
    TwoFactorEnabled: boolean = false;
    LockoutEnabled: boolean = false;

    ExpiresOn?: Date | null;

    Active: boolean = false;
   
    Claims: Array<SimpleClaim> = [];
    Roles: Array<MRoleDto> = [];

    HasPassword: boolean = false;
    //Logins: Array<any>;
    //Secrets: Array<any>;

    
}

