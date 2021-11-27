import { EndpointAction } from '../../models/endpoint-action';
import { ScriptModalComponent } from './script-modal.component';


export class ScriptParameters {
    Language: "JavaScript" | "TypeScript" | "PowerShellCore" = "TypeScript";
    SourceCode: string = '';
}

export class ScriptAction extends EndpointAction<ScriptParameters> {

    ActionType = "Script"
    Parameters = new ScriptParameters()

    Terminating = true
}
