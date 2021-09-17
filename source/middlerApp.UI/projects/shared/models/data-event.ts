export interface DataEvent<T=any> {
    Subject: string;
    Action: "Created" | "Updated" | "Deleted"
    Payload: T 

    MetaData: {[key:string]: any}
}