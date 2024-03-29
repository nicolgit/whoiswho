export interface results_response {
    "@odata.context": string;
    value: Value[];
}

export interface Value {
    "@search.score": number;
    PartitionKey: string;
    RowKey: string;
    ETag: string;
    Timestamp: Date;
    Key: string;
    GroupType?: any;
    Name: string;
    Type: string;
    Department?: string;
    Mail?: string;
    Surname?: string;
    UserType?: string;
    DeepLink?: string;
    ImgUrl?: string;
    ResourceType?:string;
    TagId?:string;

    ParentPartitionKey?:string;
    ParentRowKey?:string;
    ChildPartitionKey?:string;
    ChildRowKey?:string;
}