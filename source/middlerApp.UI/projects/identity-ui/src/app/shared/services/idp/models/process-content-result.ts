import { ConsentViewModel } from './consent-view-model';

export interface ProcessConsentResult {
        IsRedirect: boolean;
        RedirectUri: string;
        ClientId: string;

        ShowView: boolean;
        ViewModel: ConsentViewModel;

        HasValidationError: boolean;
        ValidationError: string;
    }
