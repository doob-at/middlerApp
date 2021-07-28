export class LoginInputModel {
    public Username!: string;
    public Password!: string;
    public RememberLogin: boolean = false;
    public ReturnUrl?: string;
    public Provider!: string;


}