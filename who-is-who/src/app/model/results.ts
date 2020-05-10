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
    Department?: any;
    Mail?: any;
    Surname?: any;
    UserType?: any;
    DeepLink: string;
    ResourceGroupId?: any;
    SubscriptionId?: any;
}