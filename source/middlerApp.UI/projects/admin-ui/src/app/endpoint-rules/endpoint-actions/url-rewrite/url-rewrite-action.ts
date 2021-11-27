import { EndpointAction } from '../../models/endpoint-action';
import { UrlRewriteModalComponent } from './url-rewrite-modal.component';

export class UrlRewriteParameters {
    RewriteTo: string = "";
}

export class UrlRewriteAction extends EndpointAction<UrlRewriteParameters> {

    ActionType = "UrlRewrite"
    Parameters = new UrlRewriteParameters()
    Terminating = false
}
