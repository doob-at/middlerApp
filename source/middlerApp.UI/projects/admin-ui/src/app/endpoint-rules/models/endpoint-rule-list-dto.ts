export class EndpointRuleListDto {
    Id!: string;
    Name!: string;
    Scheme!: Array<string>;
    Hostname!: string;
    Path!: string;
    HttpMethods!: Array<string>;
    Actions!: Array<any>;
    Order!: number;
    Enabled!: boolean;
}
