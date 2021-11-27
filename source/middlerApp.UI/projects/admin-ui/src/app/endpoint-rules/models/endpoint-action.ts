export class EndpointAction<T = any> {
    Id!: string;
    Order!: number;
    ActionType!: string;
    Enabled: boolean = true;

    Terminating: boolean = false
    Parameters?: T
}


