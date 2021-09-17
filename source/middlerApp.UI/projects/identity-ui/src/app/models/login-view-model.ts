import { LoginInputModel } from './login-input-model';

export class LoginViewModel extends LoginInputModel {
    public AllowRememberLogin: boolean = false;
    public EnableLocalLogin: boolean = true;

    public ExternalProviders: any;
    public LocalProviders: any;
    public DefaultProvider: string = 'local';

}
