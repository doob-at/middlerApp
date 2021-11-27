export class PermissionEntry {
    public Id: string = '';
    public Order: number = 0;
    public PrincipalName: string = '';
    public Type: string = '';
    public AccessMode: string | null = null;
    public Client: string | null = null;
    public SourceAddress: string | null = null;
}

export function PermissionEntrySortByOrder(a: PermissionEntry, b: PermissionEntry) {
    if (a.Order < b.Order) {
        return -1;
    }
    if (a.Order > b.Order) {
        return 1;
    }
    return 0;
}