import { LoginInputModel } from './login-input-model';

export class LoginViewModel extends LoginInputModel {
    public AllowRememberLogin: boolean = false;
    public EnableLocalLogin: boolean = false;

    public ExternalProviders: Array<any> = [];
    public LocalProviders: Array<any> = [];
    public DefaultProvider!: string;

}
