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
    ResourceGroupId?: string;
    ResourceId?: string;
    SubscriptionId?: string;
    UserId:string;
    GroupId?: string;
    ApplicationId?: string;
    ImgUrl?: string;
    ResourceType?:string;
    TagId?:string;
}